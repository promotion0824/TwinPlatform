using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CachelessMigrationTests.Models
{
    public class CreateDeleteModelTest : IRunnable
    {
		public void Run(string[] args)
		{
			var caller = new Caller();
			var modelId = "dtmi:digitaltwins:rec_3_3:asset:HVACCoolingMethod2;1";

			var model = "{ \"@id\":\"dtmi:digitaltwins:rec_3_3:asset:HVACCoolingMethod2;1\",\"@type\":\"Interface\",\"contents\":[{ \"@type\":\"Property\",\"displayName\":{ \"en\":\"type\"},\"name\":\"type\",\"dtmi:dtdl:property:schema;2\":{ \"@type\":\"Enum\",\"dtmi:dtdl:property:enumValues;2\":[{ \"enumValue\":\"DX\",\"name\":\"DX\"},{ \"enumValue\":\"ChilledWater\",\"name\":\"ChilledWater\"}],\"valueSchema\":\"string\"},\"writable\":true},{ \"@type\":\"Property\",\"displayName\":{ \"en\":\"refrigerant type\"},\"name\":\"refrigerantType\",\"schema\":\"string\",\"writable\":true},{ \"@type\":\"Property\",\"displayName\":{ \"en\":\"outside diameter\"},\"name\":\"outsideDiameter\",\"schema\":\"double\",\"writable\":true}],\"displayName\":{ \"en\":\"HVAC cooling method\"},\"extends\":\"dtmi:digitaltwins:rec_3_3:asset:Component;1\",\"@context\":[\"dtmi:dtdl:context;2\"] }";
			var jsonModel = JsonDocument.Parse(model);

			var localCreateTask = caller.Post($"{Urls.LocalUrl}/admin/sites/{UatData.SiteId1MW}/models", jsonModel.RootElement);
			var localCreateRes = localCreateTask.GetAwaiter().GetResult();

			var o = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(localCreateRes.Content);
			o.Property("uploadTime").Remove();
			localCreateRes.Content = JsonConvert.SerializeObject(o);

			var localDeleteTask = caller.Delete($"{Urls.LocalUrl}/admin/sites/{UatData.SiteId1MW}/models/{modelId}");			
			var localDeleteRes = localDeleteTask.GetAwaiter().GetResult();

			var uatCreateTask = caller.Post($"{Urls.UatUrl}/admin/sites/{UatData.SiteId1MW}/models", jsonModel.RootElement);
			var uatCreateRes = uatCreateTask.GetAwaiter().GetResult();

			o = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(uatCreateRes.Content);
			o.Property("uploadTime").Remove();
			uatCreateRes.Content = JsonConvert.SerializeObject(o);

			var uatDeleteTask = caller.Delete($"{Urls.UatUrl}/admin/sites/{UatData.SiteId1MW}/models/{modelId}");						
			var uatDeleteRes = uatDeleteTask.GetAwaiter().GetResult();

			TestOutput.Process("CreateModelTest", uatCreateRes, localCreateRes);
			TestOutput.Process("DeleteModelTest", uatDeleteRes, localDeleteRes);
		}

        private CreateDeleteModelTest()
        {
        }

		public static CreateDeleteModelTest Create()
		{
			return new CreateDeleteModelTest();
		}
    }
}
