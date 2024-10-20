using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Data;
using Willow.Rules.Model;

namespace RulesEngine.Processor;

/// <summary>
/// Log extensions
/// </summary>
public static class LogExtensions
{
	/// <summary>
	/// Adds a Serilog Sql Sink
	/// </summary>
	public static ILoggingBuilder AddSerilogSqlSink(this ILoggingBuilder builder,
		IConfiguration configuration,
		string connection,
		string tableName)
	{
		var sinkOptions = new MSSqlServerSinkOptions();
		sinkOptions.TableName = tableName;
		//EF core will create the table. logs will start syncing as soon as table exists
		//sinkOptions.AutoCreateSqlTable = true;

		var columnOptions = new ColumnOptions()
		{
			AdditionalColumns = new Collection<SqlColumn>
			{
				new SqlColumn
				{
					ColumnName = "CorrelationId",
					PropertyName = "CorrelationId",
					DataType = SqlDbType.VarChar,
					DataLength = 36
				},
				new SqlColumn
				{
					ColumnName = "ProgressId",
					PropertyName = "ProgressId",
					DataType = SqlDbType.VarChar,
					DataLength = 450
				},
			}
		};

		//better performance without identity column
		columnOptions.Store.Remove(StandardColumn.Id);
		//not interesting
		columnOptions.Store.Remove(StandardColumn.MessageTemplate);
		// we don't need XML data
		columnOptions.Store.Remove(StandardColumn.Properties);
		// we do want JSON data
		columnOptions.Store.Add(StandardColumn.LogEvent);

		columnOptions.Level.DataLength = 100;

		columnOptions.TimeStamp.ConvertToUtc = true;
		var serilogger = new LoggerConfiguration()
			.WriteTo
			.MSSqlServer(
				connectionString: connection,
				sinkOptions: sinkOptions,
				columnOptions: columnOptions,
				logEventFormatter: new CustomLogEventFormatter()
			)
			.ReadFrom
			.Configuration(configuration)
			.CreateLogger();

		builder.AddSerilog(serilogger);

		//use this for debugging serilog issues
		//Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

		return builder;
	}

	/// <summary>
	/// Begins a logging scope with the properties ("columns") required by the Logs table
	/// </summary>
	public static IDisposable BeginRequestScope(this Microsoft.Extensions.Logging.ILogger logger, RuleExecutionRequest request)
	{
		return BeginRequestScope(logger, request.ProgressId, request.CorrelationId, request.RequestedBy);
	}

	/// <summary>
	/// Begins a logging scope with the properties ("columns") required by the Logs table
	/// </summary>
	public static IDisposable BeginRequestScope(this Microsoft.Extensions.Logging.ILogger logger, string progressId, string correlationId, string requestedBy)
	{
		//ProgressId and CorrelationId must be in scope for serilog to write to column. RequestedBy will be in LogEvent payload
		return logger.BeginScope("Running {ProgressId}. CorrelationId {CorrelationId}. Requested by {RequestedBy}.",
			progressId, correlationId, requestedBy);
	}
}
