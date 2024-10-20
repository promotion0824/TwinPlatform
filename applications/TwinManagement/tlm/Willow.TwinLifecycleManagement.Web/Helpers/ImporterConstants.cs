namespace Willow.TwinLifecycleManagement.Web.Helpers
{
	public static class ImporterConstants
	{
		public static class Columns
		{
			public const string DocumentColumName = "#file";
			public const string ModelColumn = "model";
			public const string IdColumn = "id";
			public const string SiteIdColumn = "siteID";
		}

		public static class FileExtension
		{
			public const string CsvExtensionConstant = ".csv";
			public const string ExcelExtensionConstant = ".xlsx";
			public const string OlderExcelExtensionConstant = ".xls";
			public const string CsvFileName = "Csv";
			public const string ExcelFileName = "Excel";
		}

		public static class Git
		{
			public static readonly string BaseUrl = "https://github.com";
			public static readonly string AllowedOwner = "WillowInc";
			public static readonly string[] AllowedRepos = { "opendigitaltwins-building" };
			public static readonly string[] AllowedRepoPathStarts = { "tree/main/Ontology" };
		}
		public static class UserConstants
		{
			public static readonly string UserHeader = "User-Data";
		}
	}
}
