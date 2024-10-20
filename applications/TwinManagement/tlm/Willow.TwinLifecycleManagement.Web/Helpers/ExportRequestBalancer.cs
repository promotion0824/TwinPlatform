using Willow.TwinLifecycleManagement.Web.Models;

namespace Willow.TwinLifecycleManagement.Web.Helpers
{
	/// <summary>
	/// An ADT API Twins query that is supposed to be executed sequentially with other
	/// <see cref="SequentialQuery"/> objects.
	/// </summary>
	internal sealed class SequentialQuery
	{
		/// <summary>
		/// <see cref="ParallelRequests"/> representing requests to the ADT API to be processed in parallel.
		/// </summary>
		public List<TwinsRequest> ParallelRequests { get; init; }
	}

	/// <summary>
	/// Represents a single request to the ADT API, containing a collection or <see cref="InterfaceTwinsInfo"/> Models
	/// that are all sent in the same request.
	/// </summary>
	internal sealed class TwinsRequest
	{
		public InterfaceTwinsInfo[] Models { get; init; }
	}

	/// <summary>
	/// Uses best effort to prevent Twin exporting process from causing OOM exceptions by converting a collection
	/// of <see cref="InterfaceTwinsInfo"/> Models into a collection of <see cref="SequentialQuery"/> objects.
	/// Each <see cref="SequentialQuery"/> contains a collection of <see cref="TwinsRequest"/> which, when executed in parallel,
	/// will not collectively consume more memory than specified in <see cref="ExportRequestBalancer"/> constructor.
	/// </summary>
	/// <notes>
	/// If a Model represented by <see cref="InterfaceTwinsInfo"/> has more Twins that can fit into a <see cref="TwinsRequest"/>
	/// object, that Model gets put into its own exclusive <see cref="SequentialQuery"/> to avoid it and other, potentially
	/// sizable requests, from consuming too much RAM.
	/// </notes>
	/// <remarks>
	/// This is a temporary solution until we implement request splitting on the frontend.
	/// </remarks>
	internal sealed class ExportRequestBalancer
	{
		private readonly int _parallelRequestCount;
		private readonly uint _maxMemoryInBytes;
		private readonly int _twinSizeInBytes;

		/// <summary>
		/// A collection of Models that does not exceed a specified memory constraint when
		/// executed in parallel with other Requests.
		/// </summary>
		private sealed record Request(int TwinCapacity)
		{
			public int TwinCount { get; set; }
			public InterfaceModel[] Models { get; set; }
			public InterfaceTwinsInfo[] ToInterfaceTwinsInfoArray() => Models.Select(r => r.Model).ToArray();
		}

		/// <summary>
		/// Contains information about a Model, it's twin count and size.
		/// </summary>
		private sealed record InterfaceModel(InterfaceTwinsInfo Model, int TwinCount, int SizeInBytes);

		/// <summary>
		/// Creates an instance of the balancer.
		/// </summary>
		/// <param name="parallelRequests">Maximum amount of requests that will be executed in parallel in each <see cref="SequentialQuery"/>. Default is 5.</param>
		/// <param name="maxMemoryInBytes">Maximum amount of memory in bytes each <see cref="SequentialQuery"/> is allowed to consume. Default is 1 GB.</param>
		/// <param name="twinSizeInBytes">Average Twin size in bytes, used to calculate memory consumption of each <see cref="Request"/>. Default is 10 KB.</param>
		public ExportRequestBalancer(int parallelRequests = 5, uint maxMemoryInBytes = 1_073_741_824, int twinSizeInBytes = 10_240)
		{
			_parallelRequestCount = parallelRequests;
			_maxMemoryInBytes = maxMemoryInBytes;
			_twinSizeInBytes = twinSizeInBytes;
		}

		/// <summary>
		/// Balances a collection of Models for processing.
		/// </summary>
		/// <param name="models">A collection of <see cref="InterfaceTwinsInfo"/> obtained from ADT API.</param>
		/// <returns>A collection of sequential queries that use best-effort to conform to memory constraints.</returns>
		public List<SequentialQuery> Balance(IEnumerable<InterfaceTwinsInfo> models)
		{
			var requestTwinCapacity = GetRequestCapacity();

			var allModels = ToInterfaceModels(models);
			var regularModels = allModels.Where(r => r.TwinCount <= requestTwinCapacity).ToList();
			var heavyModels = allModels.Where(r => r.TwinCount > requestTwinCapacity).ToList();
			var regularModelsTwinCount = regularModels.Sum(r => r.TwinCount);

			var sequentialQueries = new List<SequentialQuery>();
			var requests = GetRequests(requestTwinCapacity, regularModelsTwinCount);

			PackModelsToRequests(requests, regularModels);
			GroupRequestsToSequentialQueries(sequentialQueries, requests);
			GroupHeavyModelsToSequentialQueries(sequentialQueries, heavyModels);

			return sequentialQueries;
		}

		private int GetRequestCapacity()
		{
			var requestSizeInBytes = (uint) Math.Floor((decimal) _maxMemoryInBytes / _parallelRequestCount);
			var requestTwinCapacity = (int) Math.Floor((decimal) requestSizeInBytes / _twinSizeInBytes);

			if (requestTwinCapacity <= 0)
			{
				throw new ArgumentException(
					$"Unable to properly split models into requests: calculated request twin capacity is 0.");
			}

			return requestTwinCapacity;
		}

		private List<InterfaceModel> ToInterfaceModels(IEnumerable<InterfaceTwinsInfo> models)
		{
			return models.Select(model => new InterfaceModel(
				model,
				model.TotalCount,
				model.TotalCount * _twinSizeInBytes)).ToList();
		}

		private static List<Request> GetRequests(int chunkTwinCapacity, int totalTwinCount)
		{
			var requests = new List<Request>();
			var requestCount = (int) Math.Ceiling((decimal) totalTwinCount / chunkTwinCapacity);

			for (var i = 0; i < requestCount; i++)
			{
				requests.Add(new Request(chunkTwinCapacity));
			}

			return requests;
		}

		private static void PackModelsToRequests(List<Request> requests, IEnumerable<InterfaceModel> models)
		{
			var weightOrderedModels = models.OrderByDescending(r => r.TwinCount / r.SizeInBytes).ToList();

			foreach (var request in requests)
			{
				PackRequest(request, weightOrderedModels);
			}
		}

		private void GroupRequestsToSequentialQueries(ICollection<SequentialQuery> sequentialQueries,
			IEnumerable<Request> requests)
		{
			var requestChunks = requests.Where(r => r.TwinCount > 0).Chunk(_parallelRequestCount);
			foreach (var chunk in requestChunks)
			{
				var query = new SequentialQuery
				{
					ParallelRequests = chunk.Select( request =>
						new TwinsRequest { Models = request.ToInterfaceTwinsInfoArray() }).ToList()
				};

				sequentialQueries.Add(query);
			}
		}

		private static void GroupHeavyModelsToSequentialQueries(ICollection<SequentialQuery> sequentialQueries,
			List<InterfaceModel> heavyModels)
		{
			foreach (var heavyModel in heavyModels)
			{
				sequentialQueries.Add(new SequentialQuery
				{
					ParallelRequests = new List<TwinsRequest>
					{
						new TwinsRequest { Models = new []{ heavyModel.Model } }
					}
				});
			}
		}

		/// <summary>
		/// Packages as much Models into a Request as possible.
		/// Uses https://en.wikipedia.org/wiki/Knapsack_problem#Greedy_approximation_algorithm
		/// </summary>
		private static void PackRequest(Request request, List<InterfaceModel> weightOrderedModels)
		{
			var modelsIndex = 0;

			if (!weightOrderedModels.Any())
				return;

			var solutionA = 0;
			while (solutionA <= request.TwinCapacity)
			{
				if (modelsIndex == weightOrderedModels.Count)
				{
					modelsIndex--;
					break;
				}

				var currentTwinCount = weightOrderedModels[modelsIndex].TwinCount;

				if (solutionA + currentTwinCount > request.TwinCapacity)
				{
					break;
				}

				solutionA += currentTwinCount;
				modelsIndex++;
			}

			var solutionB = weightOrderedModels[modelsIndex].TwinCount;

			if (solutionA > solutionB)
			{
				var modelCount = modelsIndex + 1;
				var modelsToPack = new InterfaceModel[modelCount];
				weightOrderedModels.CopyTo(0, modelsToPack, 0, modelCount);
				weightOrderedModels.RemoveRange(0, modelCount);

				request.TwinCount = solutionA;
				request.Models = modelsToPack;
			}
			else
			{
				var modelsToPack = new InterfaceModel[1];
				weightOrderedModels.CopyTo(modelsIndex, modelsToPack, 0, 1);
				weightOrderedModels.RemoveRange(modelsIndex, 1);

				request.TwinCount = solutionB;
				request.Models = modelsToPack;
			}
		}
	}
}