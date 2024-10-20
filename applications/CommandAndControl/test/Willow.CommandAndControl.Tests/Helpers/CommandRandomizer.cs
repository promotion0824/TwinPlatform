using Bogus;
using Willow.CommandAndControl.Data.Models;

namespace Willow.CommandAndControl.Tests.Helpers;

public static class CommandRandomizer
{
    public static RequestedCommand GetRequestedCommand(string externalId,
                                                       double value,
                                                       string commandName,
                                                       DateTimeOffset startTime,
                                                       DateTimeOffset? endTime,
                                                       DateTimeOffset? createdDate = null,
                                                       bool isApproved = true)
    {
        var faker = new Faker<RequestedCommand>()
            .RuleFor(o => o.Id, f => Guid.NewGuid())
            .RuleFor(o => o.ConnectorId, f => Guid.NewGuid().ToString())
            .RuleFor(o => o.CommandName, f => commandName)
            .RuleFor(o => o.ExternalId, f => externalId)
            .RuleFor(o => o.Value, f => value)
            .RuleFor(o => o.Unit, f => "F")
            .RuleFor(o => o.StartTime, f => startTime)
            .RuleFor(o => o.EndTime, f => endTime)
            .RuleFor(o => o.Status, f => isApproved ? Data.Enums.RequestedCommandStatus.Approved : Data.Enums.RequestedCommandStatus.Pending)
            .RuleFor(o => o.CreatedDate, f => createdDate ?? DateTimeOffset.UtcNow)
            .RuleFor(o => o.LastUpdated, f => DateTimeOffset.UtcNow);
        return faker.Generate(1).Single();
    }

    public static (object?, RequestedCommand) GetRequestedCommandWithSource(string externalId,
                                                        double value,
                                                        string commandName,
                                                        DateTimeOffset startTime,
                                                        DateTimeOffset? endTime,
                                                        bool isApproved = true)
    {
        var faker = new Faker<RequestedCommand>()
            .RuleFor(o => o.Id, f => Guid.NewGuid())
            .RuleFor(o => o.ConnectorId, f => Guid.NewGuid().ToString())
            .RuleFor(o => o.CommandName, f => commandName)
            .RuleFor(o => o.ExternalId, f => externalId)
            .RuleFor(o => o.Value, f => value)
            .RuleFor(o => o.Unit, f => "F")
            .RuleFor(o => o.StartTime, f => startTime)
            .RuleFor(o => o.EndTime, f => endTime)
            .RuleFor(o => o.Status, f => isApproved ? Data.Enums.RequestedCommandStatus.Approved : Data.Enums.RequestedCommandStatus.Pending)
            .RuleFor(o => o.CreatedDate, f => DateTimeOffset.UtcNow)
            .RuleFor(o => o.LastUpdated, f => DateTimeOffset.UtcNow);
        return (null, faker.Generate(1).Single());
    }
}
