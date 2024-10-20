using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Willow.Rules.Repository;
public static class DbInitializer
{
	private static readonly object lockObject = new();

	public static void Initialize(RulesContext context, ILogger logger)
	{
		try
		{
			// Shouldn't happen but let's make sure it doesn't (lock)
			lock (lockObject)
			{
				// Migrations may be slow
				context.Database.SetCommandTimeout(60 * 60 /* 1 hour! */);
				context.Database.Migrate();
				context.SaveChanges();
			}
		}
		catch (MsalClientException ex)
		{
			logger.LogWarning(ex, "Failed to initialize SQL DB");
			Thread.Sleep(20000);
			throw;
		}
		catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.StartsWith("Login failed"))
		{
			logger.LogWarning(ex, "Failed to login to database");
			Thread.Sleep(20000);
			throw;
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Failed to initialize SQL DB");
			throw;
		}
	}
}
