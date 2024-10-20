using Willow.Model.Requests;

namespace Willow.TwinLifecycleManagement.Web.Helpers.Adapters
{
	public class GitRequestAdapter : IBaseRequestAdapter<UpgradeModelsRepoRequest, GitRepoRequest>
	{
		private readonly string _owner;
		private readonly string _repo;
		private readonly string _defaultRef;
		private readonly string _path;

		private readonly string _building;
		private readonly string _airport;

		public GitRequestAdapter(IConfiguration config)
		{
			_owner = config.GetValue<string>("GitModelsImporting:RepositoryOwner");
			_repo = config.GetValue<string>("GitModelsImporting:RepositoryBase");
			_defaultRef = config.GetValue<string>("GitModelsImporting:DefaultRef");
			_path = config.GetValue<string>("GitModelsImporting:RepositoryFolderPath");

			// Repo suffixes for individial verticals (keys are private contract with the front-end, defined in config.js)
			_building = config.GetValue<string>("GitModelsImporting:RepositoryBuildingElement");
			_airport = config.GetValue<string>("GitModelsImporting:RepositoryAirportElement");
		}

		public UpgradeModelsRepoRequest AdaptData(GitRepoRequest input) =>
			new()
			{
				Owner = _owner,
				Path = _path,
				Ref = input.BranchRef == _defaultRef ? string.Empty : input.BranchRef,
				Repository = _repo + input.FolderPath switch
				{
					"Building" => _building,
					"Airport" => _airport,
					_ => throw new ArgumentOutOfRangeException($"Unknown GitHub repo key for model upload: {input.FolderPath}")
				},
				Submodules = input.FolderPath switch
				{
					"Building" => null,
					"Airport" => new List<string>() { "Ontology/.building" },
					_ => throw new ArgumentOutOfRangeException($"Unknown GitHub repo key for model upload: {input.FolderPath}")
				},
			};
	}
}
