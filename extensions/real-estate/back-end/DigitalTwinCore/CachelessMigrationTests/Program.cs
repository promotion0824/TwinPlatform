using System;
using System.Collections.Generic;
using System.Linq;
using CachelessMigrationTests.Assets;
using CachelessMigrationTests.Devices;
using CachelessMigrationTests.Livedata;
using CachelessMigrationTests.Models;
using CachelessMigrationTests.Points;
using CachelessMigrationTests.Twins;

namespace CachelessMigrationTests
{
	class Program
	{
		static void Main(string[] args)
		{
            Console.WriteLine();
			// Initial call so localhost can load models and does not impact in metrics
			Console.WriteLine("Initial call to localhost to load models in memory...");
			var localTask = new Caller().Get($"{Urls.LocalUrl}/admin/sites/{UatData.SiteId1MW}/models");
			localTask.Wait();

			if (!localTask.Result.Success)
			{
				Console.WriteLine($"Loading models failed (Error: {localTask.Result.Error}), press any key to finish.");
				Console.ReadLine();
				return;
			}

			Console.WriteLine($"Done loading models in memory in {Math.Round(localTask.Result.Span.TotalSeconds, 2)}s...");
			Console.WriteLine();

            var testGroups = new Dictionary<string, IEnumerable<IRunnable>>
            {
                {
                    "assets", new List<IRunnable>
                    {
						GetAssetByUniqueIdTest.Create(),
						GetAssetByTwinIdTest.Create(),
						GetAssetByGeometryIdTest.Create(),
						GetAssetPageTest.Create(),
						GetAssetsOnFloorPageTest.Create(),
						GetAssetsOnFloorByCategoryTest.Create(),
						GetAssetsOnFloorByCategoryAndSearchWordTest.Create(),
						GetLivedataAssetsOnFloorTest.Create(),
						GetExtraPropAssetsOnFloorTest.Create(),
						GetLivedataExtraPropAssetsOnFloorTest.Create(),
						GetAssetDocumentsTest.Create(),
						GetAssetRelationshipsTest.Create(),
						GetAssetsTest.Create(),
						GetAssets_LiveDataOnly_Test.Create(),
						GetAssets_IncludeExtraProperties_Test.Create(),
						GetAllAssetsOnFloorTest.Create(),
						GetAssetTree_WithCategories_Test.Create(),
						GetCategoriesOfAllAssetsTest.Create(),
						GetAssetsPage_LiveDataOnly_IncludeExtraProperties_Test.Create(),
						GetAssetsPage_IncludeExtraProperties_Test.Create()
					}
                },
				{
					"livedata", new List<IRunnable>
					{
						GetSitePointsTest.Create(),
						PostPointLiveDataTest.Create()
					}
				},
				{
					"models", new List<IRunnable>
					{
						GetModelsTest.Create(),
						GetModelByIdTest.Create(),
						SearchModelNamesTest.Create(),
						CreateDeleteModelTest.Create()
					}
				},
				{
					"twins", new List<IRunnable>
					{
						GetTwinTest.Create(),
						GetUncachedTwinTest.Create(),
						GetTwinRelationshipsTest.Create(),
						GetTwinIncomingRelationshipsTest.Create(),
						QueryRealEstateTest.Create(),
						QueryRealEstateIdsTest.Create(),
						GetTwinsTest.Create(),
						GetTwinsWithRelationshipsTest.Create()
					}
				},
				{
					"points", new List<IRunnable>
					{
						GetPointsByConnectorTest.Create(),
						GetPointsByConnectorWithAssetsTest.Create(),
						GetPointsByConnectorCountTest.Create(),
						GetPointByUniqueIdTest.Create(),
						GetPointByTrendIdTest.Create(),
						GetPointsByTrendIdsTest.Create(),
                        //GetPointsPagedTest.Create(),
                        //GetPointsPagedWithAssetsTest.Create(),
                        GetPointsTest.Create(),
						GetPointsByTagTest.Create(),
						GetPointsCountTest.Create(),
						GetPointsWithAssetsTest.Create()
					}
				},
				{
					"devices", new List<IRunnable>
					{
						GetDeviceByUniqueIdTest.Create(),
						GetDeviceByExternalPointIdTest.Create(),
						GetDevicesTest.Create(),
						GetDevicesWithPointsTest.Create(),
						GetDevicesByConnectorTest.Create(),
						GetDevicesByConnectorWithPointsTest.Create()
					}
				}
			};

            var runType = Array.IndexOf(args, "--type");
            var specificTest = Array.IndexOf(args, "--test");

            var testsToRun = new List<IRunnable>();

            if (specificTest > -1)
            {
                var testToRun = testGroups.SelectMany(x => x.Value).SingleOrDefault(x => x.GetType().Name.ToLower() == args[specificTest + 1].Trim().ToLower());
                if(testToRun == null)
                {
                    Console.WriteLine($"Invalid test to run ({args[specificTest + 1].Trim().ToLower()})");
                    Console.ReadLine();
                    return;
                }

                testsToRun.Add(testToRun);
            }
            else if (runType > -1)
            {
                var testTypesToRun = args[runType + 1].Trim().ToLower().Split(",").ToList();

                if (testTypesToRun.All(x => !testGroups.ContainsKey(x.Trim())))
                {
                    Console.WriteLine($"Invalid test type to run ({testTypesToRun}), valid types: {string.Join(", ", testGroups.Keys)}");
                    Console.ReadLine();
                    return;
                }

                testTypesToRun.ForEach(x => { if (testGroups.ContainsKey(x.Trim())) testsToRun.AddRange(testGroups[x.Trim()]); });
            }
            else
                testsToRun.AddRange(testGroups.SelectMany(x => x.Value));

            var testsCount = testsToRun.Count;
            Console.WriteLine($"{testsCount} tests found to run.");
            Console.WriteLine();
            var testIndex = 1;
            foreach (var test in testsToRun)
            {
                Console.WriteLine($"[{testIndex}/{testsCount}]");
                test.Run(args);
                testIndex++;
            }

            TestOutput.GenerateOutput(args);
            Console.WriteLine("Done running tests, press any key to finish...");
			Console.ReadLine();
		}
	}
}
