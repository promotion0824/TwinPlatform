using System;
using System.Collections.Generic;
using System.Text;

namespace CachelessMigrationTests.Models
{
    public class SearchModelNamesTest : BaseTest
    {
        protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/models/search";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new Dictionary<string, string> { { "query", "dtmi:digitaltwins:rec_3_3:core:Cap" } };

        public static SearchModelNamesTest Create()
        {
            return new SearchModelNamesTest();
        }
	}
}
