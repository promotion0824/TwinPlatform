namespace Willow.CognitiveSearch;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Willow.Extensions.Logging;

/// <summary>
/// Tracks documents waiting to be uploaded to Search service.
/// </summary>
/// <remarks>
/// Remember to call flush afterwards.
/// </remarks>
public partial class PendingDocsQueue
{
    private static readonly SemaphoreSlim Semaphore = new (1, 1);
    private const int BatchSize = 200;   // reduced to 200, Investa was throwing out of memory
    private readonly ConcurrentQueue<(IndexActionType actionType, UnifiedItemDto document)> pendingDocs = new ();
    private readonly ILogger logger;
    private readonly ILogger throttledLogger;
    private readonly ILogger throttledErrorLogger;
    private readonly HealthCheckSearch healthCheckSearch;
    private int totalInserted = 0;
    private int totalDeleted = 0;
    private int failed = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PendingDocsQueue"/> class.
    /// </summary>
    /// <param name="healthCheckSearch">The health check search.</param>
    /// <param name="logger">The logger.</param>
    public PendingDocsQueue(HealthCheckSearch healthCheckSearch, ILogger logger)
    {
        this.healthCheckSearch = healthCheckSearch ?? throw new ArgumentNullException(nameof(healthCheckSearch));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        throttledLogger = logger.Throttle(TimeSpan.FromSeconds(20));

        // Errors get their own throttled logger so they don't compete with regular logs
        throttledErrorLogger = logger.Throttle(TimeSpan.FromSeconds(1));
        totalInserted = 0;
        totalDeleted = 0;
    }

    // Allow only letters, numbers, dashes ("-"), and underscores ("_")
    [GeneratedRegex("^[A-Za-z0-9\\-_]+$")]
    private static partial Regex keyRegex();

    /// <summary>
    /// Adds one SearchDocumentDto to the pending uploads queue and does an upload
    /// as necessary.
    /// </summary>
    /// <param name="client">The search client.</param>
    /// <param name="doc">The search document.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An asynchronous task.</returns>
    /// <exception cref="ArgumentException">Key must be a valid Azure Cognitive Search Key.</exception>
    public async Task Upload(SearchClient client, UnifiedItemDto doc, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(doc.Key))
        {
            logger.LogDebug("Why is key empty on {doc}", JsonConvert.SerializeObject(doc));
            return;
        }

        if (!keyRegex().IsMatch(doc.Key))
        {
            throw new ArgumentException("Key cannot contain special characters.");
        }

        pendingDocs.Enqueue((IndexActionType.MergeOrUpload, doc));
        await SendBatchIfAtLeast(client, BatchSize, cancellationToken);
    }

    /// <summary>
    /// Delete document from the Index.
    /// </summary>
    /// <param name="client">Instance of SearchClient.</param>
    /// <param name="doc">Instance of SearchDocumentDto.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task that can be awaited.</returns>
    public async Task Delete(SearchClient client, UnifiedItemDto doc, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(doc.Key);
        if (string.IsNullOrEmpty(doc.Key))
        {
            logger.LogError("Document key cannot be empty. {object}", JsonConvert.SerializeObject(doc));
            return;
        }

        pendingDocs.Enqueue((IndexActionType.Delete, doc));
        await SendBatchIfAtLeast(client, BatchSize, cancellationToken);
    }

    /// <summary>
    /// Flush buffer to search engine.
    /// </summary>
    /// <param name="searchClient">Instance of SearchClient.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<DocsFlushResults> Flush(SearchClient searchClient, CancellationToken cancellationToken = default)
    {
        await SendBatchIfAtLeast(searchClient, 1, cancellationToken);
        logger.LogInformation("Search final counts: pending {pendingCount} done:inserted {doneInsertedCount} done:deleted {doneDeletedCount}, failed {failed}", pendingDocs.Count, totalInserted, totalDeleted, failed);
        return new DocsFlushResults(pendingDocs.Count, totalInserted, totalDeleted, failed);
    }

    /// <summary>
    /// Flush buffer if above a given size.
    /// </summary>
    /// <param name="searchClient">Search Client.</param>
    /// <param name="count"> Minimum count of documents before pushing to ACS.</param>
    /// <param name="cancellationToken"> Cancellation Token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task SendBatchIfAtLeast(SearchClient searchClient, int count, CancellationToken cancellationToken = default)
    {
        int countInBatch = 0;
        try
        {
            await Semaphore.WaitAsync(cancellationToken);

            if (pendingDocs.Count < count || pendingDocs.IsEmpty)
            {
                return;
            }

            IndexDocumentsOptions options = new ()
            {
                ThrowOnAnyError = false,
            };
            IndexDocumentsBatch<UnifiedItemDto>? batch = null;
            List<(IndexActionType actionType, UnifiedItemDto document)> docs = new ();
            while (pendingDocs.TryDequeue(out var item))
            {
                docs.Add(item);
            }

            countInBatch = docs.Count; // in case it fails
            totalInserted += docs.Count(w => w.actionType == IndexActionType.Merge || w.actionType == IndexActionType.Upload || w.actionType == IndexActionType.MergeOrUpload);
            totalDeleted += docs.Count(w => w.actionType == IndexActionType.Delete);
            throttledLogger.LogInformation("Search docs: {batchCount}, pending {pendingCount} done:inserted {doneInsertedCount} done:deleted {doneDeletedCount}, failed {failed}", docs.Count, pendingDocs.Count, totalInserted, totalDeleted, failed);

            batch = IndexDocumentsBatch.Create(
                docs.Select(x =>
                {
                    return x.actionType switch
                    {
                        IndexActionType.Merge => IndexDocumentsAction.Merge(x.document),
                        IndexActionType.Upload => IndexDocumentsAction.Upload(x.document),
                        IndexActionType.MergeOrUpload => IndexDocumentsAction.MergeOrUpload(x.document),
                        IndexActionType.Delete => IndexDocumentsAction.Delete(x.document),
                        _ => throw new NotImplementedException(), // Default will never get hit, unless there is any new action.
                    };
                })
                .Where(x => x.Document is not null) // Sometimes we get nulls here, why?
                .ToArray());

            if (batch is not null && batch.Actions.Count > 0)
            {
                _ = await searchClient.IndexDocumentsAsync(batch, options, cancellationToken: cancellationToken);

                // Only set healthy after a successful batch upload
                healthCheckSearch.Current = HealthCheckSearch.Healthy;
            }

            // TODO: Check results and log warnings
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 403)
        {
            throttledErrorLogger.LogError(ex, "Search docs failed to upload/delete, authorization error.");
            healthCheckSearch.Current = HealthCheckResult.Unhealthy("Authorization failure");
            failed += countInBatch;
            throw; // cannot recover
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            throttledErrorLogger.LogError(ex, "Search docs failed to upload/delete, request failed 404 error.");
            healthCheckSearch.Current = HealthCheckResult.Unhealthy(ex.InnerException?.Message ?? ex.Message);
            failed += countInBatch;

            //runonce = true;  // allow full rebuild if we get a 404 - so you can delete the inded in Portal and recover
            throw;  // cannot recover
        }
        catch (Azure.RequestFailedException ex)
        {
            throttledErrorLogger.LogError(ex, "Search docs failed to upload/delete, request failed.");
            healthCheckSearch.Current = HealthCheckResult.Unhealthy(ex.InnerException?.Message ?? ex.Message);
            failed += countInBatch;
        }
        catch (Exception ex)
        {
            throttledErrorLogger.LogError(ex, "Error while updating/deleting search docs.");
            healthCheckSearch.Current = HealthCheckResult.Unhealthy(ex.Message);
            failed += countInBatch;
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
