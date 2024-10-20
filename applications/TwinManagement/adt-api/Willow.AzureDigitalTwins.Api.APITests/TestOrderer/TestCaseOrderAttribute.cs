using System;

namespace Willow.AzureDigitalTwins.Api.APITests.TestOrderer
{

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class TestCaseOrderAttribute : Attribute
	{
		public TestCaseOrderAttribute(int order)
		{
			Order = order;
		}

		public int Order { get; private set; }
	}
}
