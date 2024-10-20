using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging.Testing;
using System.Security.Claims;
using Willow.Telemetry;
using WillowRules.Logging;
using System.Collections.Generic;

namespace WillowRules.Test.Logging;

[TestClass]
public class AuditLoggerExtensionsTests
{
	#region Plumbing

	public AuditLoggerExtensionsTests()
	{
		FakeLoggerProvider = new FakeLoggerProvider();
		AuditLoggerFactory = new AuditLoggerFactory(LoggerFactory.Create(config => config.AddProvider(FakeLoggerProvider)));
		AuditLogger = new AuditLogger<AuditLoggerExtensionsTests>(AuditLoggerFactory);
	}

	public FakeLoggerProvider FakeLoggerProvider { get; private set; }

	public AuditLoggerFactory AuditLoggerFactory { get; private set; }

	public IAuditLogger<AuditLoggerExtensionsTests> AuditLogger { get; private set; }

	#endregion

	[TestMethod]
	public void LogInformation_ClaimsPricipalWithEmailAndName_UserIdIsEmail()
	{
		var user = new ClaimsPrincipal(
			new ClaimsIdentity([
				new Claim(ClaimTypes.Email, "test@willowinc.com"),
				new Claim(ClaimTypes.NameIdentifier, "Test User")
			])
		);

		AuditLogger.LogInformation(user, "Login");

		var logMessage = FakeLoggerProvider.Collector.LatestRecord;

		Assert.AreEqual("Login by test@willowinc.com", logMessage.Message);

		Assert.AreEqual(1, logMessage.Scopes.Count);

		var scope = logMessage.Scopes[0]! as Dictionary<string, object?>;
		{
			Assert.IsNotNull(scope);
			Assert.AreEqual(scope["Audit"], true);
		}
	}

	[TestMethod]
	public void LogInformation_ClaimsPricipalWithNoEmail_UserIdIsName()
	{
		var user = new ClaimsPrincipal(
			new ClaimsIdentity([
				new Claim(ClaimTypes.NameIdentifier, "Test User")
			])
		);

		AuditLogger.LogInformation(user, "Login");

		var logMessage = FakeLoggerProvider.Collector.LatestRecord;

		Assert.AreEqual("Login by Test User", logMessage.Message);
	}

	[TestMethod]
	public void LogInformation_WithCustomScope_CustomScopeIsLoggedAlongWithDefaultScope()
	{
		var user = new ClaimsPrincipal(
			new ClaimsIdentity([
				new Claim(ClaimTypes.Email, "test@willowinc.com"),
				new Claim(ClaimTypes.NameIdentifier, "Test User")
			])
		);

		AuditLogger.LogInformation(user, new() { ["Permissions"] = "CanReadRules, CanEditRules" }, "Login");

		var logMessage = FakeLoggerProvider.Collector.LatestRecord;

		Assert.AreEqual(2, logMessage.Scopes.Count);

		var scope0 = logMessage.Scopes[0]! as Dictionary<string, object?>;
		{
			Assert.IsNotNull(scope0);
			Assert.AreEqual(scope0["Permissions"], "CanReadRules, CanEditRules");
		}

		var scope1 = logMessage.Scopes[1]! as Dictionary<string, object?>;
		{
			Assert.IsNotNull(scope1);
			Assert.AreEqual(scope1["Audit"], true);
		}
	}

	[TestMethod]
	public void LogInformation_StringUserIdWithCustomScope_CustomScopeIsLoggedAlongWithDefaultScope()
	{
		AuditLogger.LogInformation("test@willowinc.com", new Dictionary<string, object> { ["Permissions"] = "CanReadRules, CanEditRules" }, "Login");

		var logMessage = FakeLoggerProvider.Collector.LatestRecord;

		Assert.AreEqual("Login by test@willowinc.com", logMessage.Message);

		Assert.AreEqual(2, logMessage.Scopes.Count);

		var scope0 = logMessage.Scopes[0]! as Dictionary<string, object?>;
		{
			Assert.IsNotNull(scope0);
			Assert.AreEqual(scope0["Permissions"], "CanReadRules, CanEditRules");
		}

		var scope1 = logMessage.Scopes[1]! as Dictionary<string, object?>;
		{
			Assert.IsNotNull(scope1);
			Assert.AreEqual(scope1["Audit"], true);
		}
	}
}
