using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.TwinLifecycleManagement.Web.Helpers;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Policies
{
	public class TlmUserEmailTests
	{

		[Fact]
		public async Task TlmUserEmailShouldExtractValidHostname()
		{
			string expectedHostName = "willowinc.com";

			List<string> emails = new()
			{
				 "milos@willowinc.com",
				 "test1.1@willowinc.com" ,

			};

			foreach (var email in emails)
			{
				Assert.Equal(expectedHostName, new TlmUserEmail(email).Host);
			}
		}

		[Fact]
		public async Task TlmUserEmailShouldThrowExceptionForInvalidEmailFormat()
		{
			List<string> emails = new()
			{
				 "Abc.example.com",
				 "A@b@c@example.com",
				 "ab(c)d,e:f;g<h>i[jk]l@example.com",
				 "just\"not\"right@example.com",
				 "this isnt\"\\allowed@example.com",
				 "this\\ still\\\"notallowed@example.com"
			};

			foreach (var email in emails)
			{
				Assert.ThrowsAny<FormatException>(() => new TlmUserEmail(email).Host);
			}
		}
	}
}
