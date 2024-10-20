using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.Extensions.Logging;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.ServiceBus;
using Willow.Storage.Blobs;

namespace Willow.AzureDigitalTwins.Api.Services;

public interface IAsyncService<T>
{
    Task TriggerAsyncProcess(T asyncJob, Topic topicOptions);
    Task StoreAsyncJob(string folder, T asyncJob, CancellationToken cancellationToken = default);
    Task<T> GetLatestAsyncJob(string folder, AsyncJobStatus? status = null);
    Task<string> StoreRequest<R>(string folder, string jobId, R request);
    Task<R> GetRequest<R>(string path, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsyncJobs(string folder, string jobId = null, AsyncJobStatus? status = null, string userId = null, DateTime? from = null, DateTime? to = null, bool fullDetails = true);
    Task DeleteBlob(string folder, string id, bool includeRequestBlob = false, CancellationToken cancellationToken = default);
    Task DeleteBlobFolder(string folder, CancellationToken cancellationToken = default);

}

public class AsyncService<T> : IAsyncService<T> where T : AsyncJob
{
    private readonly IMessageSender _messageSender;
    private readonly IBlobService _blobService;
    private readonly ILogger<AsyncService<T>> _logger;

    private const string _jobsPath = "jobs";
    private const string _requestPath = "requests";
    private const string _targetTag = "target";
    private readonly string _container;
    private const string _statusTag = "status";
    private const string _createTag = "createtime";
    private const string _folderTag = "folder";
    private const string _jobIdTag = "jobid";
    private const string _userIdTag = "userid";


    public AsyncService(IMessageSender messageSender,
        IBlobService blobService,
        IConfiguration configuration,
        ILogger<AsyncService<T>> logger)
    {
        _messageSender = messageSender;
        _blobService = blobService;
        _container = configuration.GetValue<string>("BlobStorage:AsyncContainer");
        _logger = logger;
    }

    public async Task TriggerAsyncProcess(T asyncJob, Topic topicOptions)
    {
        await _messageSender.Send(topicOptions.ServiceBusName, topicOptions.TopicName, asyncJob, messageId: asyncJob.JobId);
        _logger.LogInformation($"TriggerAsyncProcess: Message to Service bus: {topicOptions.ServiceBusName}, Topic: {topicOptions.TopicName}, JobId: {asyncJob.JobId}");
    }

    public async Task DeleteBlob(string folder, string id, bool includeRequestBlob = false, CancellationToken cancellationToken = default)
    {
        var deleteTasks = new List<Task>();
        if (includeRequestBlob)
            deleteTasks.Add(_blobService.DeleteBlob(_container, $"{folder}/{_requestPath}/{id}", cancellationToken));

        deleteTasks.Add(_blobService.DeleteBlob(_container, $"{folder}/{_jobsPath}/{id}", cancellationToken));

        await Task.WhenAll(deleteTasks);
    }

    public async Task DeleteBlobFolder(string folder, CancellationToken cancellationToken = default)
    {
        await _blobService.DeleteAllFilesInBlobFolder(_container, $"{folder}/{_jobsPath}", cancellationToken);
        _logger.LogInformation($"DeleteBlobFolder: Container {_container}, Folder {folder}/{_jobsPath}");
    }

    public async Task<IEnumerable<T>> FindAsyncJobs(string folder,
        string jobId = null,
        AsyncJobStatus? status = null,
        string userId = null,
        DateTime? from = null,
        DateTime? to = null,
        bool fullDetails = true)
    {
        var filterTags = new List<(string, string, string)> { (_folderTag, "=", folder) };
        if (jobId is not null)
            filterTags.Add((_jobIdTag, "=", ConvertToNumbers(jobId)));
        if (status.HasValue)
            filterTags.Add((_statusTag, "=", status.Value.ToString()));
        if (userId is not null)
            filterTags.Add((_userIdTag, "=", ConvertToNumbers(userId)));
        if (from.HasValue)
            filterTags.Add((_createTag, ">=", from.Value.Ticks.ToString()));
        if (to.HasValue)
            filterTags.Add((_createTag, "<=", to.Value.Ticks.ToString()));

        var matchingBlobs = await _blobService.GetBlobNameByTags(_container, filterTags);
        var asyncJobs = new ConcurrentBag<T>();
        var tasks = new List<Task>();

        if (fullDetails)
        {
            tasks.AddRange(matchingBlobs.Select(async x =>
            {
                var blobStream = await _blobService.DownloadFile(_container, x);
                asyncJobs.Add(JsonSerializer.Deserialize<T>(new StreamReader(blobStream).ReadToEnd()));
            }));
        }
        else
        {
            tasks.AddRange(matchingBlobs.Select(async x =>
            {
                var tags = await _blobService.GetBlobTags(_container, x);

                var job = Activator.CreateInstance<T>();
                job.JobId = ConvertFromNumbers(tags.Tags[_jobIdTag]);
                job.UserId = ConvertFromNumbers(tags.Tags[_userIdTag]);
                job.Details = new AsyncJobDetails
                {
                    Status = (AsyncJobStatus)Enum.Parse(typeof(AsyncJobStatus), tags.Tags[_statusTag])
                };
                job.CreateTime = new DateTime((long)Convert.ToDouble(tags.Tags[_createTag]));
                job.Target = tags.Tags.ContainsKey(_targetTag) ? tags.Tags[_targetTag].Split("-").Select(x => (EntityType)Enum.Parse(typeof(EntityType), x)).ToList() : null;
                asyncJobs.Add(job);
            }));
        }

        await Task.WhenAll(tasks);

        return asyncJobs.OrderByDescending(x => x.CreateTime);
    }

    private string ConvertToNumbers(string value)
    {
        var numbers = string.Empty;
        foreach (char c in value)
        {
            numbers += String.Format("{0}", Convert.ToByte(c)).PadLeft(3, '0');
        }
        return numbers;
    }

    private string ConvertFromNumbers(string value)
    {
        var result = new StringBuilder();
        var temp = new StringBuilder();
        for (int n = 0; n < value.Length; ++n)
        {
            temp.Append(value[n]);
            if (((n + 1) % 3 == 0) && int.TryParse(temp.ToString(), out int x))
            {
                result.Append(Convert.ToChar(x));
                temp.Clear();
            }
        }
        return result.ToString();
    }

    public async Task StoreAsyncJob(string folder, T asyncJob, CancellationToken cancellationToken = default)
    {
        var start = Stopwatch.StartNew();

        await MeasureExecutionTime.ExecuteTimed(async () =>
        {
            var blobPath = await UploadToStorage(
                                                    folder,
                                                    _jobsPath,
                                                    asyncJob.JobId,
                                                    asyncJob,
                                                    overwrite: true,
                                                    cancellationToken);
            _logger.LogDebug($"StoreAsyncJob: Path: {_jobsPath} JobId: {asyncJob.JobId}");

            // TODO: Move progress reporting from the asyncJob payload to this meta-data so we don't need to read the BLOB itself?
            // Note that most of this data does not change - would avoid this call most of the time if we only updated when changed
            await _blobService.MergeTags(_container, blobPath, new Dictionary<string, string>
            {
                { _statusTag, asyncJob.Details.Status.ToString() },
                { _userIdTag, asyncJob.UserId != null ? ConvertToNumbers(asyncJob.UserId) : "" },
                { _jobIdTag, ConvertToNumbers(asyncJob.JobId) },
                { _createTag, asyncJob.CreateTime.Ticks.ToString() },
                { _folderTag, folder },
                { _targetTag, string.Join("-", asyncJob.Target.Select(x => x.ToString()).ToArray()) }
            }, cancellationToken);
            return Task.FromResult(true);
        },
            (res, ms) =>
            {
                _logger.LogDebug($"Time to update async job: {ms} ms");
            });
    }

    private async Task<string> UploadToStorage<U>(string folder, string path, string id, U entity, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        // Note that we can get a CollectionModified exception during serialization as the serializer enumerates collections
        //   So either lock{} here or make sure we're serializing static or concurrent collections
        stream.FromString(JsonSerializer.Serialize(entity));

        var blobPath = $"{folder}/{path}/{id}";

        await _blobService.UploadFile(_container, blobPath, stream, overwrite, cancellationToken);

        return blobPath;
    }

    public async Task<string> StoreRequest<R>(string folder, string jobId, R request)
    {
        _logger.LogInformation($"Store request folder: {folder}, JobId: {jobId}");

        return await UploadToStorage(folder, _requestPath, jobId, request);
    }

    public async Task<R> GetRequest<R>(string path, CancellationToken cancellationToken = default)
    {
        var blob = await _blobService.DownloadFile(_container, path, cancellationToken);
        var reader = new StreamReader(blob);
        return JsonSerializer.Deserialize<R>(reader.ReadToEnd());
    }
    public async Task<T> GetLatestAsyncJob(string folder, AsyncJobStatus? status = null)
    {
        var blobs = await _blobService.GetBlobItems(_container, $"{folder}/{_jobsPath}");
        if (blobs.Any())
        {
            foreach (var x in blobs.OrderByDescending(x => x.Properties.CreatedOn.Value))
            {
                var blobTags = await _blobService.GetBlobTags(_container, x.Name);
                if (!status.HasValue || (
                         blobTags.Tags != null
                        && blobTags.Tags.ContainsKey(_statusTag)
                        && blobTags.Tags[_statusTag] == status.ToString()))
                {
                    var blob = await _blobService.DownloadFile(_container, x.Name);

                    var reader = new StreamReader(blob);
                    return JsonSerializer.Deserialize<T>(reader.ReadToEnd());
                }
            }
        }

        return default(T);
    }
}
