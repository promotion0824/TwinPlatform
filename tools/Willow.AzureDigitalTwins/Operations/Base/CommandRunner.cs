using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Willow.AzureDigitalTwins.BackupRestore.Log;
using Willow.AzureDigitalTwins.BackupRestore.Results;

namespace Willow.AzureDigitalTwins.BackupRestore.Operations.Base
{
	public abstract class CommandRunner
	{
		public abstract Task<ProcessResult> ProcessCommand();
		public abstract List<string> GetValidationErrors();

		protected async Task<int> ProcessWithRetry<T>(string entityType,
			ConcurrentDictionary<string, string> errors,
			Func<T, string> getId, Func<T, Task> processEntity,
			Options settings,
			IInteractiveLogger interactiveLogger,
			ConcurrentDictionary<string, T> entities,
			Func<Task> finalStep = null)
		{
			var succeeded = 0;
			var retries = 0;
			var previousCount = 0;
			var entityCount = 0;
			var processedCount = 0;

			while (entities.Any() && (retries++ < settings.MaxRetries || entities.Count < previousCount))
			{
				previousCount = entities.Count;
				if (retries > 1)
				{
					interactiveLogger.NewLine();
					interactiveLogger.LogLine($"[{retries}] attempt for {previousCount} failed {entityType}...");
				}

				entityCount = entities.Count;
				interactiveLogger.LogLine($"{entityCount} {entityType} to process, now processing...");

				var process = Task.Run(() => Parallel.ForEach(entities,
					new ParallelOptions { MaxDegreeOfParallelism = settings.ProcessingThreads },
					x =>
					{
						try
						{
							processEntity(x.Value).Wait();
							Interlocked.Increment(ref succeeded);
							entities.TryRemove(x.Key, out T value);
						}
						catch (RequestFailedException requestFailedException)
						{
							if (requestFailedException.Status != (int)HttpStatusCode.TooManyRequests)
							{
								entities.TryRemove(x.Key, out T value);
								errors.TryAdd(x.Key, requestFailedException.Message);
							}
						}
						catch (Exception ex)
						{
							entities.TryRemove(x.Key, out T value);
							errors.TryAdd(x.Key, ex.Message);
						}

						Interlocked.Increment(ref processedCount);
						interactiveLogger.Log($"Processed {processedCount} {entityType}...", true, false);
					}));

				await process;

				if (finalStep != null)
					await finalStep();

				Interlocked.Exchange(ref processedCount, 0);
			}

			interactiveLogger.NewLine();
			interactiveLogger.LogLine($"Done with {entityType}...");

			return succeeded;
		}
	}
}
