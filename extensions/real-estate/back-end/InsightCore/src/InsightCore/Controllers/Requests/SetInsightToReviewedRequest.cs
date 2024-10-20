using System;
using System.ComponentModel.DataAnnotations;

namespace InsightCore.Controllers.Requests
{
    public class SetInsightToReviewedRequest
	{
		[Required]
	    public Guid? UserId { get; set; }
		
	}
}
