using DigitalTwinCore.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CachelessMigrationTests.Livedata
{
	public class GetSitePointsTest : BaseTest
	{
        protected override bool AdxImplemented => true;

        protected override string UrlFormat => $"{{0}}/livedataingest/sites/{UatData.SiteId1MW}/points";
        protected override ICollection<KeyValuePair<string, string>> QueryParam => new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("ids", "9f291c5b-106f-4ee2-93bb-003659ee8ada"),
				new KeyValuePair<string, string>("ids", "239b4377-80a9-4d20-971a-0015516da5d1"),
				new KeyValuePair<string, string>("ids", "5425621e-c457-43d5-afd8-002cb3bead9a"),
				new KeyValuePair<string, string>("ids", "9e43af56-3487-428b-a469-000dc07a5b7f"),
				new KeyValuePair<string, string>("ids", "abf1ad92-e185-45d9-84af-0022a9e2b035"),
				new KeyValuePair<string, string>("ids", "3936f21b-be85-4943-9f0e-00013799a668"),
				new KeyValuePair<string, string>("ids", "33cc29c8-7405-4548-8da2-00144043bd19"),
				new KeyValuePair<string, string>("ids", "48bb8341-c47a-4aac-aa2b-001825f17411"),
				new KeyValuePair<string, string>("ids", "08b7cfad-3eae-47c8-840b-003a3c8c6eb0"),
				new KeyValuePair<string, string>("ids", "1ad1a94d-2be3-4b0f-9faf-003d5780f0bc"),
				new KeyValuePair<string, string>("ids", "09e04642-da85-482c-9e79-005ef3756562"),
				new KeyValuePair<string, string>("ids", "694aee30-c7a2-4231-b0cc-0049ed2135c7"),
				new KeyValuePair<string, string>("ids", "2fb817ba-8f11-4d5b-ac8e-00525afc086d"),
				new KeyValuePair<string, string>("ids", "f75277ae-032d-4393-9811-0044541a61fa"),
				new KeyValuePair<string, string>("ids", "1e20d207-ecf9-49c9-8404-00547338b599"),
				new KeyValuePair<string, string>("ids", "07e14f98-f17f-4196-9cfd-0046f0413179"),
				new KeyValuePair<string, string>("ids", "cda6d668-0952-49a5-9f68-00548f5cc673"),
				new KeyValuePair<string, string>("ids", "196d949e-b0a9-4879-8fac-005e034fc49b"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-TX_4_1_RUNNINGINBYPASS.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-CRITICAL-AC_67_1FAIL.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC10_01ECN_PID_OUTPUT_1.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC61_01WARNING_DISPLAY_8.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-ATS_29_Current_PhaseB.PV"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC54_01WARN_VFD_IN_BYP_8.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-CRITICAL-SCR_Base_H_Fault_LowAlarm_ReactTankLvl.PV"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC_27_1_OUTPUTTORQUE.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-CPCRITICAL-EF_C_03_04HiSuctionPressAlarm.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC_4_6MINAIRFLOWALARM.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC_18_1_OUTPUTSPEEDPCT.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-HV_68_2_4_OUTPUTCURRENT.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC_23_1ENTERINGAIRENTHALPY.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC32_01SUP_AIR_TEMP_1_7.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-ATS_18_DI_TSLockout.PV"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-BTU_CW_L_VOLUMETOTAL.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-SP_70_2_OVTKWHCURHR_.PresentValue"),
				new KeyValuePair<string, string>("externalIds", "-FACILITY-MANWEST-AC58_01CLG_STG_ON_1.PresentValue"),
				new KeyValuePair<string, string>("trendIds", "1626c13a-fbcf-4cd1-81e1-087393cd1b94"),
				new KeyValuePair<string, string>("trendIds", "094f29b3-36aa-4004-8ebe-302e2ffffe42"),
				new KeyValuePair<string, string>("trendIds", "486865be-90a4-4818-9336-cd12223a7fea"),
				new KeyValuePair<string, string>("trendIds", "dc0aaf1f-83c7-46a0-8ce4-7fef1ed28aa6"),
				new KeyValuePair<string, string>("trendIds", "fc9bb2cb-71f9-424b-9850-f586c43333f5"),
				new KeyValuePair<string, string>("trendIds", "1c566927-60d0-48c3-b96d-eaeac58d5a94"),
				new KeyValuePair<string, string>("trendIds", "6a28cdf0-4943-4fe1-bc86-05cd73756b59"),
				new KeyValuePair<string, string>("trendIds", "e21b7080-e6ce-45a4-8a17-8478521a9004"),
				new KeyValuePair<string, string>("trendIds", "cdbf1124-6219-475a-bad0-6bc97008fc34"),
				new KeyValuePair<string, string>("trendIds", "84530707-07f5-498f-ac01-6cf74021e192"),
				new KeyValuePair<string, string>("trendIds", "4e28ba7e-bc0d-4074-8a94-f1bbc098782e"),
				new KeyValuePair<string, string>("trendIds", "d070702b-ddac-444c-974f-3fa1c23c6558"),
				new KeyValuePair<string, string>("trendIds", "3d6d5876-5fc0-401b-a1cc-099a9173e32c"),
				new KeyValuePair<string, string>("trendIds", "8c652138-33cf-4c80-857b-8202ce203016"),
				new KeyValuePair<string, string>("trendIds", "0122fd74-c0a3-41bb-b4f8-f7c3d14dd3b1"),
				new KeyValuePair<string, string>("trendIds", "074d0f60-22d9-4fff-9c61-385575466520"),
				new KeyValuePair<string, string>("trendIds", "9bb13376-2fa7-4807-88a3-3ac8361802da"),
				new KeyValuePair<string, string>("trendIds", "a038a529-1bdb-4144-9e83-e6b336d4e51a")
			};

		public static GetSitePointsTest Create()
		{
			return new GetSitePointsTest();
		}

        protected override void CompareResponseContent(Result oldRes, Result newRes)
        {
			var pointOld = JsonConvert.DeserializeObject<IEnumerable<LiveDataIngestPointDto>>(oldRes.Content).OrderBy(x => x.UniqueId);
			var pointNew = JsonConvert.DeserializeObject<IEnumerable<LiveDataIngestPointDto>>(newRes.Content).OrderBy(x => x.UniqueId);

			Console.WriteLine("Comparing content...");
			Console.WriteLine($"Amount: old {pointOld.Count()} - new {pointNew.Count()}");
			Console.WriteLine($"Ids: {(Enumerable.SequenceEqual(pointOld.Select(x => x.UniqueId), pointNew.Select(x => x.UniqueId)) ? "Match" : $"Different")}");
			Console.WriteLine($"Assets: {(Enumerable.SequenceEqual(pointOld.Select(x => x.AssetId), pointNew.Select(x => x.AssetId)) ? "Match" : $"Different")}");
			Console.WriteLine($"TrendIds: {(Enumerable.SequenceEqual(pointOld.Select(x => x.TrendId), pointNew.Select(x => x.TrendId)) ? "Match" : $"Different")}");
			Console.WriteLine($"Extternal Ids: {(Enumerable.SequenceEqual(pointOld.Select(x => x.ExternalId), pointNew.Select(x => x.ExternalId)) ? "Match" : $"Different")}");
			Console.WriteLine("Done comparing content...");
		}
	}
}
