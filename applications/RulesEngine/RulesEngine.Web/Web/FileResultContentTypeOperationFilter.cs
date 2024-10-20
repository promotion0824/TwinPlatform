using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Willow.Rules.Web;

/// <summary>
/// Updates the operation to specifcy the provided file content type.
/// https://stackoverflow.com/questions/43844261/what-is-the-correct-way-to-download-a-file-via-the-nswag-code-generator-angular
/// </summary>
public class FileResultContentTypeOperationFilter : IOperationFilter
{
	/// <summary>
	/// Applies the new content type
	/// </summary>
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var requestAttribute = context.MethodInfo.GetCustomAttributes(typeof(FileResultContentTypeAttribute), false)
			.Cast<FileResultContentTypeAttribute>()
			.FirstOrDefault();

		if (requestAttribute == null) return;

		operation.Responses.Clear();
		operation.Responses.Add("200", new OpenApiResponse
		{
			Content = new Dictionary<string, OpenApiMediaType>
			{
				{
					requestAttribute.ContentType, new OpenApiMediaType
					{
						Schema = new OpenApiSchema
						{
							Type = "string",
							Format = "binary"
						}
					}
				}
			}
		});
	}
}
