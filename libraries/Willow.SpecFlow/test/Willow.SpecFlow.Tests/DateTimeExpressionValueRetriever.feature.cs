﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Willow.SpecFlow.Tests
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class TestDateTimeExpressionsValueRetrieverFeature : object, Xunit.IClassFixture<TestDateTimeExpressionsValueRetrieverFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "DateTimeExpressionValueRetriever.feature"
#line hidden
        
        public TestDateTimeExpressionsValueRetrieverFeature(TestDateTimeExpressionsValueRetrieverFeature.FixtureData fixtureData, Willow_SpecFlow_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "Test DateTime Expressions Value Retriever", null, ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Test DateTime Expressions Value Retriever")]
        [Xunit.TraitAttribute("FeatureTitle", "Test DateTime Expressions Value Retriever")]
        [Xunit.TraitAttribute("Description", "Test DateTime Expressions Value Retriever")]
        public void TestDateTimeExpressionsValueRetriever()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test DateTime Expressions Value Retriever", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 3
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
 testRunner.Given("the current time is \'2024-04-18 14:24:00Z\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                            "CurrentDateTime"});
                table1.AddRow(new string[] {
                            "NOW"});
                table1.AddRow(new string[] {
                            "NOW + 1d"});
                table1.AddRow(new string[] {
                            "NOW + 1day"});
                table1.AddRow(new string[] {
                            "NOW + 1days"});
                table1.AddRow(new string[] {
                            "NOW - 1d"});
                table1.AddRow(new string[] {
                            "NOW - 1day"});
                table1.AddRow(new string[] {
                            "NOW - 1days"});
                table1.AddRow(new string[] {
                            "NOW + 1h"});
                table1.AddRow(new string[] {
                            "NOW + 1hour"});
                table1.AddRow(new string[] {
                            "NOW + 1hours"});
                table1.AddRow(new string[] {
                            "NOW - 1h"});
                table1.AddRow(new string[] {
                            "NOW - 1hour"});
                table1.AddRow(new string[] {
                            "NOW - 1hours"});
                table1.AddRow(new string[] {
                            "NOW + 1m"});
                table1.AddRow(new string[] {
                            "NOW + 1min"});
                table1.AddRow(new string[] {
                            "NOW + 1mins"});
                table1.AddRow(new string[] {
                            "NOW - 1m"});
                table1.AddRow(new string[] {
                            "NOW - 1min"});
                table1.AddRow(new string[] {
                            "NOW - 1mins"});
                table1.AddRow(new string[] {
                            "NOW + 1s"});
                table1.AddRow(new string[] {
                            "NOW + 1sec"});
                table1.AddRow(new string[] {
                            "NOW + 1secs"});
                table1.AddRow(new string[] {
                            "NOW - 1s"});
                table1.AddRow(new string[] {
                            "NOW - 1sec"});
                table1.AddRow(new string[] {
                            "NOW - 1secs"});
                table1.AddRow(new string[] {
                            "TODAY"});
                table1.AddRow(new string[] {
                            "TODAY + 1d"});
                table1.AddRow(new string[] {
                            "TODAY + 1day"});
                table1.AddRow(new string[] {
                            "TODAY + 1days"});
                table1.AddRow(new string[] {
                            "TODAY - 1d"});
                table1.AddRow(new string[] {
                            "TODAY - 1day"});
                table1.AddRow(new string[] {
                            "TODAY - 1days"});
                table1.AddRow(new string[] {
                            "TODAY + 1h"});
                table1.AddRow(new string[] {
                            "TODAY + 1hour"});
                table1.AddRow(new string[] {
                            "TODAY + 1hours"});
                table1.AddRow(new string[] {
                            "TODAY - 1h"});
                table1.AddRow(new string[] {
                            "TODAY - 1hour"});
                table1.AddRow(new string[] {
                            "TODAY - 1hours"});
                table1.AddRow(new string[] {
                            "TODAY + 1m"});
                table1.AddRow(new string[] {
                            "TODAY + 1min"});
                table1.AddRow(new string[] {
                            "TODAY + 1mins"});
                table1.AddRow(new string[] {
                            "TODAY - 1m"});
                table1.AddRow(new string[] {
                            "TODAY - 1min"});
                table1.AddRow(new string[] {
                            "TODAY - 1mins"});
                table1.AddRow(new string[] {
                            "TODAY + 1s"});
                table1.AddRow(new string[] {
                            "TODAY + 1sec"});
                table1.AddRow(new string[] {
                            "TODAY + 1secs"});
                table1.AddRow(new string[] {
                            "TODAY - 1s"});
                table1.AddRow(new string[] {
                            "TODAY - 1sec"});
                table1.AddRow(new string[] {
                            "TODAY - 1secs"});
                table1.AddRow(new string[] {
                            ""});
#line 5
 testRunner.And("I have test objects", ((string)(null)), table1, "And ");
#line hidden
                TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                            "CurrentDateTime"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-19 14:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-19 14:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-19 14:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 14:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 14:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 14:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 15:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 15:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 15:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 13:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 13:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 13:24:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:25:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:25:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:25:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:23:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:23:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:23:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:24:01"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:24:01"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:24:01"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:23:59"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:23:59"});
                table2.AddRow(new string[] {
                            "2024-04-18 14:23:59"});
                table2.AddRow(new string[] {
                            "2024-04-18 00:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-19 00:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-19 00:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-19 00:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 00:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 00:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 00:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 01:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 01:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 01:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:00:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 00:01:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 00:01:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 00:01:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:59:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:59:00"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:59:00"});
                table2.AddRow(new string[] {
                            "2024-04-18 00:00:01"});
                table2.AddRow(new string[] {
                            "2024-04-18 00:00:01"});
                table2.AddRow(new string[] {
                            "2024-04-18 00:00:01"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:59:59"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:59:59"});
                table2.AddRow(new string[] {
                            "2024-04-17 23:59:59"});
                table2.AddRow(new string[] {
                            ""});
#line 58
 testRunner.Then("the objects\' CurrentDateTime should be", ((string)(null)), table2, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Test DateTimeOffset Expressions Value Retriever")]
        [Xunit.TraitAttribute("FeatureTitle", "Test DateTime Expressions Value Retriever")]
        [Xunit.TraitAttribute("Description", "Test DateTimeOffset Expressions Value Retriever")]
        public void TestDateTimeOffsetExpressionsValueRetriever()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Test DateTimeOffset Expressions Value Retriever", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 112
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 113
 testRunner.Given("the current time is \'2024-04-18 14:24:00Z\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                            "CurrentDateTime"});
                table3.AddRow(new string[] {
                            "NOW"});
                table3.AddRow(new string[] {
                            "NOW + 1d"});
                table3.AddRow(new string[] {
                            "NOW + 1day"});
                table3.AddRow(new string[] {
                            "NOW + 1days"});
                table3.AddRow(new string[] {
                            "NOW - 1d"});
                table3.AddRow(new string[] {
                            "NOW - 1day"});
                table3.AddRow(new string[] {
                            "NOW - 1days"});
                table3.AddRow(new string[] {
                            "NOW + 1h"});
                table3.AddRow(new string[] {
                            "NOW + 1hour"});
                table3.AddRow(new string[] {
                            "NOW + 1hours"});
                table3.AddRow(new string[] {
                            "NOW - 1h"});
                table3.AddRow(new string[] {
                            "NOW - 1hour"});
                table3.AddRow(new string[] {
                            "NOW - 1hours"});
                table3.AddRow(new string[] {
                            "NOW + 1m"});
                table3.AddRow(new string[] {
                            "NOW + 1min"});
                table3.AddRow(new string[] {
                            "NOW + 1mins"});
                table3.AddRow(new string[] {
                            "NOW - 1m"});
                table3.AddRow(new string[] {
                            "NOW - 1min"});
                table3.AddRow(new string[] {
                            "NOW - 1mins"});
                table3.AddRow(new string[] {
                            "NOW + 1s"});
                table3.AddRow(new string[] {
                            "NOW + 1sec"});
                table3.AddRow(new string[] {
                            "NOW + 1secs"});
                table3.AddRow(new string[] {
                            "NOW - 1s"});
                table3.AddRow(new string[] {
                            "NOW - 1sec"});
                table3.AddRow(new string[] {
                            "NOW - 1secs"});
                table3.AddRow(new string[] {
                            "TODAY"});
                table3.AddRow(new string[] {
                            "TODAY + 1d"});
                table3.AddRow(new string[] {
                            "TODAY + 1day"});
                table3.AddRow(new string[] {
                            "TODAY + 1days"});
                table3.AddRow(new string[] {
                            "TODAY - 1d"});
                table3.AddRow(new string[] {
                            "TODAY - 1day"});
                table3.AddRow(new string[] {
                            "TODAY - 1days"});
                table3.AddRow(new string[] {
                            "TODAY + 1h"});
                table3.AddRow(new string[] {
                            "TODAY + 1hour"});
                table3.AddRow(new string[] {
                            "TODAY + 1hours"});
                table3.AddRow(new string[] {
                            "TODAY - 1h"});
                table3.AddRow(new string[] {
                            "TODAY - 1hour"});
                table3.AddRow(new string[] {
                            "TODAY - 1hours"});
                table3.AddRow(new string[] {
                            "TODAY + 1m"});
                table3.AddRow(new string[] {
                            "TODAY + 1min"});
                table3.AddRow(new string[] {
                            "TODAY + 1mins"});
                table3.AddRow(new string[] {
                            "TODAY - 1m"});
                table3.AddRow(new string[] {
                            "TODAY - 1min"});
                table3.AddRow(new string[] {
                            "TODAY - 1mins"});
                table3.AddRow(new string[] {
                            "TODAY + 1s"});
                table3.AddRow(new string[] {
                            "TODAY + 1sec"});
                table3.AddRow(new string[] {
                            "TODAY + 1secs"});
                table3.AddRow(new string[] {
                            "TODAY - 1s"});
                table3.AddRow(new string[] {
                            "TODAY - 1sec"});
                table3.AddRow(new string[] {
                            "TODAY - 1secs"});
                table3.AddRow(new string[] {
                            ""});
#line 114
 testRunner.And("I have test offset objects", ((string)(null)), table3, "And ");
#line hidden
                TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                            "CurrentDateTime"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-19 14:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-19 14:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-19 14:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 14:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 14:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 14:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 15:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 15:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 15:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 13:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 13:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 13:24:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:25:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:25:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:25:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:23:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:23:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:23:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:24:01Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:24:01Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:24:01Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:23:59Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:23:59Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 14:23:59Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 00:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-19 00:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-19 00:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-19 00:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 00:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 00:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 00:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 01:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 01:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 01:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:00:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 00:01:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 00:01:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 00:01:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:59:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:59:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:59:00Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 00:00:01Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 00:00:01Z"});
                table4.AddRow(new string[] {
                            "2024-04-18 00:00:01Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:59:59Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:59:59Z"});
                table4.AddRow(new string[] {
                            "2024-04-17 23:59:59Z"});
                table4.AddRow(new string[] {
                            ""});
#line 167
 testRunner.Then("the offset objects\' CurrentDateTime should be", ((string)(null)), table4, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                TestDateTimeExpressionsValueRetrieverFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                TestDateTimeExpressionsValueRetrieverFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
