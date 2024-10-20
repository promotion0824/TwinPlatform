using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Willow.Rules.Web.Pages
{
	/// <summary>
	/// Error model
	/// </summary>
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public class ErrorModel : PageModel
	{
		private readonly ILogger<ErrorModel> logger;

		/// <summary>
		/// Creates a new <see cref="ErrorModel"/>
		/// </summary>
		/// <param name="_logger"></param>
		public ErrorModel(ILogger<ErrorModel> _logger)
		{
			logger = _logger;
		}

		/// <summary>
		/// The request Id
		/// </summary>
		public string RequestId { get; set; }

		/// <summary>
		/// Show the request id
		/// </summary>
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

		/// <summary>
		/// On get callback
		/// </summary>
		public void OnGet()
		{
			RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
		}
	}
}
