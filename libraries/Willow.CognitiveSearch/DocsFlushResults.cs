namespace Willow.CognitiveSearch;

/// <summary>
/// Results of flushing documents to the search index.
/// </summary>
/// <param name="pendingDocsCount">The count of pending documents.</param>
/// <param name="insertedDocsCount">The count of inserted documents.</param>
/// <param name="deletedDocsCount">The count of deleted documents.</param>
/// <param name="failedDocsCount">The count of failed documents.</param>
public record DocsFlushResults(long pendingDocsCount, long insertedDocsCount, long deletedDocsCount, long failedDocsCount);
