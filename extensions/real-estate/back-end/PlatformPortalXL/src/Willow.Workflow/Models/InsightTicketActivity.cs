using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Willow.Workflow.Models;

public class InsightTicketActivity
{
	public Guid TicketId { get; set; }

    public string TicketSummary { get; set; }

    public string ActivityType { get; set; }

	public DateTime ActivityDate { get; set; }

	public Guid SourceId { get; set; }

	public TicketSourceType SourceType { get; set; }

	public List<KeyValuePair<string, string>> Activities { get; set; }
}

