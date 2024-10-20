using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CachelessMigrationTests.Assets
{
	//public static class GetSpaceAssetTreeTest
	//{
	//	public static void Run()
	//	{
	//		var caller = new Caller();

	//		var siteId = "4e5fc229-ffd9-462a-882b-16b4a63b2a8a"; // 1MW
	//		string continuationToken = null;
	//		var contents = new List<object>();

	//		do
	//		{
	//			var res = caller.Get($"{Urls.LocalUrl}/sites/{siteId}/assets/assettree/paged", new Dictionary<string, string>
	//			{
	//				{ "modelNames", "Space" }
	//				//{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" }
	//			}).GetAwaiter().GetResult();

	//			var tokenRes = JsonSerializer.Deserialize<TokenResult>(res.Content);
	//			if (tokenRes.content != null && tokenRes.content.Length > 0)
	//			{
	//				contents.AddRange(tokenRes.content);
	//			}
				
	//			continuationToken = tokenRes.continuationToken;
	//		}
	//		while (continuationToken != null);

	//		var uatTask = caller.Get($"{Urls.UatUrl}/sites/{siteId}/assets/assettree", new Dictionary<string, string>
	//		{
	//			//{ "floorId", "9d3dd93d-bcee-4686-a1a0-264146118dd2" },
	//			//{ "pageSize", "100" }
	//		});

	//		var uatRes = uatTask.GetAwaiter().GetResult();

	//		TestOutput.Process(nameof(GetSpaceAssetTreeTest), uatRes, new Result { Content = JsonSerializer.Serialize(contents) });
	//	}
	//}

	//public class TokenResult
	//{
	//	public string continuationToken { get; set; }
	//	public object[] content { get; set; } = new object[] { };
	//}
}
