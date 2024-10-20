﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CachelessMigrationTests.Twins
{
    public class QueryRealEstateTest : BaseTest
    {
		protected override string UrlFormat => $"{{0}}/admin/sites/{UatData.SiteId1MW}/twins/query/realestate";
		private object Payload = new { RestrictToSite = false, RootModels = new List<string> { "Zone" } };
		
		public static QueryRealEstateTest Create()
		{
			return new QueryRealEstateTest();
		}

		protected override Result GetCachlessDtCoreResult(Caller caller)
		{
			var localTask = caller.Post(string.Format(UrlFormat, Urls.LocalUrl), Payload);
			return localTask.GetAwaiter().GetResult();
		}

		protected override Result GetCurrentDtCoreResult(Caller caller)
		{
			var uatTask = caller.Post(string.Format(UrlFormat, Urls.UatUrl), Payload);
			return uatTask.GetAwaiter().GetResult();
		}
	}
}
