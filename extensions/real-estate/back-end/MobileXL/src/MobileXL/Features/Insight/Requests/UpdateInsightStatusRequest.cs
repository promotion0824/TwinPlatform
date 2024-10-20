using MobileXL.Models;
using System.ComponentModel.DataAnnotations;

namespace MobileXL.Features.Insight.Requests;

public class UpdateInsightStatusRequest
{
	[Required(ErrorMessage = "the insight status is required")]
	public InsightStatus? Status { get; set; }
}
