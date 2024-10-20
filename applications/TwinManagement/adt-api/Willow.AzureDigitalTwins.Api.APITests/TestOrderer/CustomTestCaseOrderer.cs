using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Willow.AzureDigitalTwins.Api.APITests.TestOrderer
{
	public class CustomTestCaseOrderer : ITestCaseOrderer
	{
		public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
		{
			var sortedTestCases = new SortedDictionary<int, List<TTestCase>>();

			foreach (var testCase in testCases)
			{
				int order = GetHightestPriority<TTestCase>(testCase);
				GetOrCreate(sortedTestCases, order).Add(testCase);
			}

			foreach (var list in sortedTestCases.Keys.Select(priority => sortedTestCases[priority]))
			{
				list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
				foreach (TTestCase testCase in list)
					yield return testCase;
			}
		}

		private int GetHightestPriority<TTestCase>(TTestCase testCase) where TTestCase : ITestCase
		{
			int localOrder = 0;
			var attributeInfos = testCase.TestMethod.Method.GetCustomAttributes(typeof(TestCaseOrderAttribute).AssemblyQualifiedName);
			foreach (var attributeInfo in attributeInfos)
			{
				var order = attributeInfo.GetNamedArgument<int>(nameof(TestCaseOrderAttribute.Order));
				if(order > localOrder)
					localOrder = order;
			}
			return localOrder;
		}
		private TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{

			if (dictionary.TryGetValue(key, out TValue result)) return result;

			result = new TValue();
			dictionary[key] = result;

			return result;
		}
	}
}
