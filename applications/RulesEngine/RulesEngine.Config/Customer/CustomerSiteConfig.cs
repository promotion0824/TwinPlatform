using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Rules.Configuration;

/// <summary>
/// Configuration for customer sites
/// </summary>
public class CustomerSiteConfig
{
	/// <summary>
	/// Constructor
	/// </summary>
	public CustomerSiteConfig(string url, string name, bool isProd, bool isSingleTenant, bool isDeprecated = false)
	{
		Name = name;
		Url = url;
		IsProd = isProd;
		IsSingleTenant = isSingleTenant;
		IsDeprecated = isDeprecated;
	}

	/// <summary>
	/// Name of the customer
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Base url of the customer
	/// </summary>
	public string Url { get; set; }

	/// <summary>
	/// Prd vs NonPrd env
	/// </summary>
	public bool IsProd { get; set; }

	/// <summary>
	/// Prd vs NonPrd env
	/// </summary>
	public bool IsDeprecated { get; set; }

	/// <summary>
	/// Whether the site is single tenant
	/// </summary>
	public bool IsSingleTenant { get; set; }

}
