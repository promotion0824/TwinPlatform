using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.CommandAndControlAPI.SDK.Client;
using Willow.CommandAndControlAPI.SDK.Dtos;
using Willow.Extensions.Logging;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Model;

namespace Willow.Rules.Services;

/// <summary>
/// Service to send data to the Willow Command and Control Api
/// </summary>
public interface ICommandService
{
	/// <summary>
	/// Queue rules engine commands using a channel buffered writer
	/// </summary>
	Task QueueCommand(RequestedCommandDto request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the reader channel (used by the background service)
	/// </summary>
	ChannelReader<RequestedCommandDto> Reader { get; }

	/// <summary>
	/// Sends a command update to Command and Control
	/// </summary>
	Task<HttpStatusCode> SendCommandUpdate(PostRequestedCommandsDto request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Tries to aquire an auth token for the api
	/// </summary>
	Task TryAcquireToken();
}

/// <summary>
/// Command service extensions
/// </summary>
public static class CommandServiceExtensions
{

	/// <summary>
	/// Creates a command dto to send to Command and Control
	/// </summary>
	/// <exception cref="InvalidOperationException">If the command is not valid</exception>
	public static RequestedCommandDto CreateRequestedCommandDto(this Command command)
	{
		if (!command.CanSync())
		{
			throw new InvalidOperationException($"Command is not able to sync. Id {command.Id}");
		}

		return new RequestedCommandDto()
		{
			Type = command.CommandType.ToString(),
			CommandName = command.CommandName,
			ExternalId = command.ExternalId,
			Value = command.Value,
			Unit = command.Unit,
			StartTime = command.StartTime,
			EndTime = command.EndTime,
			RuleId = command.RuleId,
			TwinId = command.TwinId,
			ConnectorId = command.ConnectorId,
			Relationships = command.Relationships.Select(v => new RelationshipDto()
			{
				ModelId = v.ModelId,
				RelationshipType = v.RelationshipType,
				TwinId = v.TwinId,
				TwinName = v.TwinName
			}).ToArray()
		};
	}

	/// <summary>
	/// Creates a command dto to send to Command and Control
	/// </summary>
	/// <exception cref="InvalidOperationException">If the command is not valid</exception>
	public static PostRequestedCommandsDto CreateCommandPostRequest(this Command command)
	{
		return new Command[] { command }.CreateCommandPostRequest();
	}

	/// <summary>
	/// Creates a command dto to send to Command and Control
	/// </summary>
	/// <exception cref="InvalidOperationException">If the command is not valid</exception>
	public static PostRequestedCommandsDto CreateCommandPostRequest(this IEnumerable<Command> commands)
	{
		var commandRequests = new List<RequestedCommandDto>();

		foreach (var command in commands)
		{
			commandRequests.Add(command.CreateRequestedCommandDto());
		}

		return new PostRequestedCommandsDto()
		{
			Commands = commandRequests
		};
	}
}

/// <summary>
/// Service to send data to the Willow Command and Control Api
/// </summary>
public class CommandService : ICommandService
{
	private readonly Channel<RequestedCommandDto> messageQueue;

	public const int MaxQueueCapacity = 100000;

	private readonly IHttpClientFactory httpClientFactory;
	private readonly DefaultAzureCredential credential;
	private readonly ILogger<CommandService> logger;
	private readonly ILogger throttledLogger;
	private readonly IMemoryCache memoryCache;
	private readonly CommandAndControlApiOption options;
	private readonly HealthCheckCommandApi healthCheckCommandApi;

	/// <summary>
	/// Creates a new <see cref="CommandService" />
	/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public CommandService(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		IOptions<CustomerOptions> customerOptions,
		DefaultAzureCredential credential,
		HealthCheckCommandApi healthCheckCommandApi,
		IHttpClientFactory httpClientFactory,
		IMemoryCache memoryCache,
		ILogger<CommandService> logger)
	{
		this.messageQueue = Channel.CreateBounded<RequestedCommandDto>(new BoundedChannelOptions(MaxQueueCapacity)
		{
			//Increased performance
			SingleReader = true
		});

		if (customerOptions is null) throw new ArgumentNullException(nameof(customerOptions));

		this.healthCheckCommandApi = healthCheckCommandApi ?? throw new ArgumentNullException(nameof(healthCheckCommandApi));

		this.credential = credential ?? throw new ArgumentNullException(nameof(credential));
		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

		var options = customerOptions.Value ?? throw new ArgumentNullException("Missing rules options configuration");

		if (options.CommandApi is null || string.IsNullOrWhiteSpace(options.CommandApi.Uri) || string.IsNullOrEmpty(options.CommandApi.Audience))
		{
			logger.LogWarning("CommandService is not configured and will be skipped");

			this.httpClientFactory = null!;

			healthCheckCommandApi.Current = HealthCheckCommandApi.NotConfigured;
		}
		else
		{
			this.options = options.CommandApi;
			this.httpClientFactory = httpClientFactory;
			healthCheckCommandApi.Current = HealthCheckCommandApi.Healthy;
		}

		this.logger = logger;
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(30));
	}

	private HttpClient GetClient(string token)
	{
		if(httpClientFactory is null)
		{
			throw new ArgumentNullException(nameof(httpClientFactory));
		}

		var httpClient = httpClientFactory.CreateClient("CommandApi");

		httpClient.BaseAddress = new Uri(this.options.Uri);
		httpClient.DefaultRequestHeaders.Add("Authorization-Scheme", "AzureAd");
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		return httpClient;
	}

	private async Task<string?> EnsureToken()
	{
		if (options is null || httpClientFactory is null) return "";

		var accessToken = await memoryCache.GetOrCreateAsync("CommandApiToken", async (cacheEntry) =>
		{
			AccessToken? token = null;

			logger.LogInformation("Command Service getting access token");

			try
			{
				var scope = $"{options.Audience}/.default";

				token = await credential.GetTokenAsync(new TokenRequestContext(new[]
				{
					scope
				}));

				cacheEntry.AbsoluteExpiration = token.Value.ExpiresOn.AddMinutes(-1.0);

				healthCheckCommandApi.Current = HealthCheckCommandApi.Healthy;
			}
			catch (Exception ex)
			{
				cacheEntry.AbsoluteExpiration = DateTime.Now.AddMinutes(10);
				logger.LogError(ex, "Command Service Failed to retrieve token for command api");
				healthCheckCommandApi.Current = HealthCheckCommandApi.FailingCalls;
			}

			return token;
		});

		return accessToken?.Token;
	}

	public ChannelReader<RequestedCommandDto> Reader => messageQueue.Reader;
	public ChannelWriter<RequestedCommandDto> Writer => messageQueue.Writer;

	public async Task QueueCommand(RequestedCommandDto request, CancellationToken cancellationToken = default)
	{
		await Writer.WriteAsync(request, cancellationToken);
	}

	public async Task<HttpStatusCode> SendCommandUpdate(PostRequestedCommandsDto request, CancellationToken cancellationToken = default)
	{
		try
		{
			var token = await EnsureToken();

			if (string.IsNullOrEmpty(token))
			{
				return HttpStatusCode.OK;
			}

			var httpClient = GetClient(token);

			var client = new CommandAndControlClient(httpClient);

			await client.PostRequestedCommands(request, cancellationToken);

			healthCheckCommandApi.Current = HealthCheckCommandApi.Healthy;

			return HttpStatusCode.OK;
		}
		catch (HttpRequestException ex)
		{
			healthCheckCommandApi.Current = HealthCheckCommandApi.FailingCalls;
			throttledLogger.LogError(ex, "Failed to set Commands, status={code}. {m1}, {m2}", ex.StatusCode, ex.Message, ex.InnerException?.Message);
			return ex.StatusCode ?? HttpStatusCode.InternalServerError;
		}
		catch (Exception ex)
		{
			healthCheckCommandApi.Current = HealthCheckCommandApi.FailingCalls;
			throttledLogger.LogError(ex, "Failed to set Commands");
			return HttpStatusCode.InternalServerError;
		}
	}

	public async Task TryAcquireToken()
	{
		await EnsureToken();
	}
}
