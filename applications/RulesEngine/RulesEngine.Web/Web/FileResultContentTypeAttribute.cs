using System;

namespace Willow.Rules.Web;

/// <summary>
/// Indicates swashbuckle should expose the result of the method as a file in open api (see https://swagger.io/docs/specification/describing-responses/)
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class FileResultContentTypeAttribute : Attribute
{
	/// <summary>
	/// Constructs the attribute with the content type
	/// </summary>
	/// <param name="contentType"></param>
	public FileResultContentTypeAttribute(string contentType)
	{
		ContentType = contentType;
	}

	/// <summary>
	/// Content type of the file e.g. image/png
	/// </summary>
	public string ContentType { get; }
}
