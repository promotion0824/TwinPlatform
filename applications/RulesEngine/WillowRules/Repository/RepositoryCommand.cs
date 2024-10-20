using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Repository;

/// <summary>
/// A repository for <see cref="Command"/>
/// </summary>
public interface IRepositoryCommand : IRepositoryBase<Command>
{
	/// <summary>
	/// Enable syncing to to Command And Control
	/// </summary>
	Task<int> EnableSync(string commandId, bool enabled);

	/// <summary>
	/// Gets a unique list of command ids and it's command enabled flag
	/// </summary>
	Task<IEnumerable<(string id, bool enabled, bool isTriggered)>> GetCommandValues();

	/// <summary>
	/// Removes all commands for a given rule Id
	/// </summary>
	Task<int> RemoveAllCommandsForRule(string ruleId);

}

/// <summary>
/// A repository for <see cref="Command"/>
/// </summary>
public class RepositoryCommand : RepositoryBase<Command>, IRepositoryCommand
{
    /// <summary>
	/// Creates a new <see cref="RepositoryCommand"/>
	/// </summary>
    public RepositoryCommand(
            IDbContextFactory<RulesContext> dbContextFactory,
            RulesContext rulesContext,
            WillowEnvironmentId willowEnvironment,
            IMemoryCache memoryCache,
            IEpochTracker epochTracker,
            ILogger<RepositoryCommand> logger,
            IOptions<CustomerOptions> customerOptions) 
        : base(
            dbContextFactory, 
            rulesContext, 
            rulesContext.Commands, 
            willowEnvironment, 
            memoryCache, 
            epochTracker, 
            logger, 
            customerOptions)
    {
    }

    /// <inheritdoc/>
	protected override IQueryable<Command> ApplySort(IQueryable<Command> queryable, SortSpecificationDto[] sortSpecifications)
	{
		bool first = true;
        IOrderedQueryable<Command>? result = null;
        foreach (var sortSpecification in sortSpecifications)
        {
            switch (sortSpecification.field)
			{
				case nameof(Command.Value):
					{
						result = AddSort(queryable, result!, first, x => x.Value, sortSpecification.sort);
						break;
					}
				case nameof(Command.IsValid):
					{
						result = AddSort(queryable, result!, first, x => x.IsValid, sortSpecification.sort);
						break;
					}
				case nameof(Command.TwinId):
                    {
                        result = AddSort(queryable, result!, first, x => x.TwinId, sortSpecification.sort);
                        break;
                    }
				case nameof(Command.IsTriggered):
					{
						result = AddSort(queryable, result!, first, x => x.IsTriggered, sortSpecification.sort);
						break;
					}
				case nameof(Command.RuleId):
					{
						result = AddSort(queryable, result!, first, x => x.RuleId, sortSpecification.sort);
						break;
					}
				case nameof(Command.RuleName):
					{
						result = AddSort(queryable, result!, first, x => x.RuleName, sortSpecification.sort);
						break;
					}
				case nameof(Command.CommandId):
					{
						result = AddSort(queryable, result!, first, x => x.CommandId, sortSpecification.sort);
						break;
					}
				case nameof(Command.CommandName):
					{
						result = AddSort(queryable, result!, first, x => x.CommandName, sortSpecification.sort);
						break;
					}
				case nameof(Command.CommandType):
					{
						result = AddSort(queryable, result!, first, x => x.CommandType, sortSpecification.sort);
						break;
					}
				case nameof(Command.StartTime):
					{
						result = AddSort(queryable, result!, first, x => x.StartTime, sortSpecification.sort);
						break;
					}
				case nameof(Command.EndTime):
					{
						result = AddSort(queryable, result!, first, x => x.EndTime, sortSpecification.sort);
						break;
					}
				case nameof(Command.Enabled):
					{
						result = AddSort(queryable, result!, first, x => x.Enabled, sortSpecification.sort);
						break;
					}
				case nameof(Command.LastSyncDate):
					{
						result = AddSort(queryable, result!, first, x => x.LastSyncDate, sortSpecification.sort);
						break;
					}
				case nameof(Command.EquipmentId):
					{
						result = AddSort(queryable, result!, first, x => x.EquipmentId, sortSpecification.sort);
						break;
					}
				case nameof(Command.EquipmentName):
					{
						result = AddSort(queryable, result!, first, x => x.EquipmentName, sortSpecification.sort);
						break;
					}
				case nameof(Command.TwinName):
					{
						result = AddSort(queryable, result!, first, x => x.TwinName, sortSpecification.sort);
						break;
					}
				case nameof(Command.ExternalId):
					{
						result = AddSort(queryable, result!, first, x => x.ExternalId, sortSpecification.sort);
						break;
					}
				case nameof(Command.ConnectorId):
					{
						result = AddSort(queryable, result!, first, x => x.ConnectorId, sortSpecification.sort);
						break;
					}
				default:
                case nameof(Command.Id):
                    {
                        result = AddSort(queryable, result!, first, x => x.Id, sortSpecification.sort);
                        break;
                    }
            }
            first = false;
        }

        return result ?? queryable;
	}

    /// <inheritdoc/>
	protected override Expression<Func<Command, bool>>? GetExpression(FilterSpecificationDto filter, IFormatProvider? formatProvider)
	{
		switch (filter.field)
        {
            case nameof(Command.Id):
                {
                    return filter.CreateExpression((Command c) => c.Id, filter.ToString(formatProvider));
                }
            case nameof(Command.CommandType):
                {
                    return filter.CreateExpression((Command c) => c.CommandType, filter.ToEnum<CommandType>(formatProvider));
                }
            case nameof(Command.TwinId):
                {
                    return filter.CreateExpression((Command c) => c.TwinId, filter.ToString(formatProvider));
				}
			case nameof(Command.TwinName):
				{
					return filter.CreateExpression((Command c) => c.TwinName, filter.ToString(formatProvider));
				}
			case nameof(Command.Unit):
                {
                    return filter.CreateExpression((Command c) => c.Unit, filter.ToString(formatProvider));
                }
            case nameof(Command.RuleId):
                {
                    return filter.CreateExpression((Command c) => c.RuleId, filter.ToString(formatProvider));
				}
			case nameof(Command.IsTriggered):
				{
					return filter.CreateExpression((Command c) => c.IsTriggered, filter.ToBoolean(formatProvider));
				}
			case nameof(Command.RuleName):
				{
					return filter.CreateExpression((Command c) => c.RuleName, filter.ToString(formatProvider));
				}
			case nameof(Command.CommandName):
				{
					return filter.CreateExpression((Command c) => c.CommandName, filter.ToString(formatProvider));
				}
			case nameof(Command.CommandId):
				{
					return filter.CreateExpression((Command c) => c.CommandId, filter.ToString(formatProvider));
				}
			case nameof(Command.EquipmentId):
				{
					return filter.CreateExpression((Command c) => c.EquipmentId, filter.ToString(formatProvider));
				}
			case nameof(Command.EquipmentName):
				{
					return filter.CreateExpression((Command c) => c.EquipmentName, filter.ToString(formatProvider));
				}
			case nameof(Command.ExternalId):
				{
					return filter.CreateExpression((Command c) => c.ExternalId, filter.ToString(formatProvider));
				}
			case nameof(Command.ConnectorId):
				{
					return filter.CreateExpression((Command c) => c.ConnectorId, filter.ToString(formatProvider));
				}
			case nameof(Command.Enabled):
				{
					return filter.CreateExpression((Command c) => c.Enabled, filter.ToBoolean(formatProvider));
				}
			case nameof(Command.IsValid):
				{
					return filter.CreateExpression((Command c) => c.IsValid, filter.ToBoolean(formatProvider));
				}
			default:
                {
                    return null;
                }
        }
	}

	public async Task<int> EnableSync(string commandId, bool enabled)
	{
		logger.LogInformation($"Set command {commandId} to sync: {enabled}");
		int result = await this.rulesContext.Database
			.ExecuteSqlInterpolatedAsync($"UPDATE [Commands] SET Enabled={enabled} WHERE [Commands].[Id]={commandId}");
		InvalidateOne(commandId);
		return result;
	}

	public async Task<IEnumerable<(string id, bool enabled, bool isTriggered)>> GetCommandValues()
	{
		var items = await (from p in this.dbSet
						   select new
						   {
							   id = p.Id,
							   enabled = p.Enabled,
							   isTriggered = p.IsTriggered
						   })
				.ToListAsync();

		return items.Select(v => (v.id, v.enabled, v.isTriggered));
	}

	/// <inheritdoc/>
	public override IQueryable<Command> WithArrays(IQueryable<Command> input)
	{
		return input;
	}

	public async Task<int> RemoveAllCommandsForRule(string ruleId)
	{
		logger.LogInformation($"Remove all commands for rule {ruleId}", ruleId);

		int result = 0;

		await ExecuteAsync(async () =>
		{
			// No index on RuleId but that's OK
			result = await this.rulesContext.Database
				.ExecuteSqlInterpolatedAsync($"DELETE FROM [Commands] WHERE [Commands].[RuleId]={ruleId}");

			this.InvalidateCache();
		});

		return result;
	}
}
