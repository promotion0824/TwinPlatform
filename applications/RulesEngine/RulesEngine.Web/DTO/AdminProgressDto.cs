using System;
using Willow.Rules.Model;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace RulesEngine.Web;

/// <summary>
/// Progress records for the admin page
/// </summary>
public class AdminProgressDto
{
    /// <summary>
    /// When the progress Dto was created
    /// </summary>
    public DateTimeOffset Now { get; }

    /// <summary>
    /// The progress records from the database
    /// </summary>
    public IList<ProgressDto> Progresses { get; }

    /// <summary>
    /// Creates a new <see cref="AdminProgressDto" />
    /// </summary>
    public AdminProgressDto(IEnumerable<Progress> progresses, IEnumerable<RuleExecutionRequest> requests)
    {
        var all = progresses
            .Select(v => new ProgressDto(v)).ToList()
            .Concat(requests.Select(v => new ProgressDto(v)).ToList());

        var inprogress = all
            .Where(v => v.Status == ProgressStatus.InProgress || v.IsRealtime)
            .OrderBy(v => v.IsRealtime)
            .ThenBy(v => v.Id);

        var queued = all
            .Except(inprogress)
            .Where(v => v.Queued)
            .OrderBy(v => v.LastUpdated);

        var finished = all
            .Except(inprogress)
            .Except(queued)
            .OrderByDescending(v => v.LastUpdated);

        this.Progresses = inprogress.Concat(queued).Concat(finished).ToList();

        this.Now = DateTimeOffset.Now;
    }
}
