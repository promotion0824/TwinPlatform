using AssetCoreTwinCreator.Constants;
using System.IO;

namespace AssetCoreTwinCreator.Helper
{
	public static class MimeTypeHelper
	{
		public static string GetMimeType(string fileName)
		{
			var fileExtension = Path.GetExtension(fileName)?.TrimStart('.');

			if (string.IsNullOrWhiteSpace(fileName))
			{
				return MimeTypeConstants.Unknown;
			}

			switch(fileExtension.ToLower())
			{
				case FileExtensionConstants.Bmp:
					return MimeTypeConstants.Bmp;
				case FileExtensionConstants.Csv:
					return MimeTypeConstants.Csv;
				case FileExtensionConstants.Gif:
					return MimeTypeConstants.Gif;
				case FileExtensionConstants.Png:
					return MimeTypeConstants.Png;
				case FileExtensionConstants.Jpeg:
				case FileExtensionConstants.Jpg:
					return MimeTypeConstants.Jpeg;
				case FileExtensionConstants.Json:
					return MimeTypeConstants.Json;
				case FileExtensionConstants.MsWord:
					return MimeTypeConstants.MsWord;
				case FileExtensionConstants.Pdf:
					return MimeTypeConstants.Pdf;
				case FileExtensionConstants.Text:
					return MimeTypeConstants.Text;
				case FileExtensionConstants.Xml:
					return MimeTypeConstants.Xml;
				case FileExtensionConstants.Zip:
					return MimeTypeConstants.Zip;
				default:
					return MimeTypeConstants.Unknown;
			}
		}
	}
}
