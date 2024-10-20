using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Willow.Rules.Web;

/// <summary>
/// A general set of extensions for the Web
/// </summary>
public static class WebExtensions
{
    private static char[] MoreInvalidChars = new char[] { ';' };

    /// <summary>
    /// Gets the user's name or email claim
    /// </summary>
    public static string UserName(this ClaimsPrincipal claims)
    {
        return claims.FindFirstValue(ClaimTypes.Name)
            ?? claims.FindFirstValue("name")//this one seems to have the name claim. might be custom from ab2c
            ?? claims.FindFirstValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Gets the user's email claim
    /// </summary>
    public static string Email(this ClaimsPrincipal claims)
    {
        return claims.FindFirstValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Generates a csv file result
    /// </summary>
    public static FileStreamResult CsvResult<T>(IEnumerable<T> data, string filename)
    {
        //remove any invalid chars
        filename = string.Join("_", filename.Split(Path.GetInvalidFileNameChars().Union(MoreInvalidChars).ToArray())).Trim('_');

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            InjectionOptions = InjectionOptions.Escape
        };

        byte[] buffer;

        using (var memoryStream = new MemoryStream())
        using (var streamWriter = new StreamWriter(memoryStream))
        using (var csvWriter = new CsvWriter(streamWriter, config))
        {
            csvWriter.WriteRecords(data);
            streamWriter.Flush();
            buffer = memoryStream.ToArray();
        }

        return new FileStreamResult(new MemoryStream(buffer), MediaTypeNames.Application.Octet) { FileDownloadName = filename };
    }

    /// <summary>
	/// Generates a csv file result suitable for dynamic columns
	/// </summary>
    public static async Task<FileStreamResult> CsvResultWithDynamicHeaders<T>(IAsyncEnumerable<T> data, string filename)
    {
        // Remove any invalid chars
        filename = string.Join("_", filename.Split(Path.GetInvalidFileNameChars().Union(MoreInvalidChars).ToArray())).Trim('_');

        string randomName = Path.ChangeExtension(Path.GetRandomFileName(), "csv");
        string tmpDirectory = Path.GetTempPath();
        string filePath = Path.Combine(tmpDirectory, filename);
        string tempFilePath = Path.Combine(tmpDirectory, randomName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            InjectionOptions = InjectionOptions.Escape
        };

        var headers = new HashSet<string>();

        //first write the body to a temp file
        using (var memoryStream = new FileStream(tempFilePath, FileMode.Append))
        using (var streamWriter = new StreamWriter(memoryStream))
        using (var csvWriter = new CsvWriter(streamWriter, config))
        {
            await foreach (var entry in data)
            {
                if (entry is IDictionary<string, object> expandoDict)
                {
                    foreach (var key in expandoDict.Keys)
                    {
                        //record headers as we go
                        headers.Add(key);
                    }

                    foreach (var header in headers)
                    {
                        expandoDict.TryGetValue(header, out var value);
                        csvWriter.WriteField(value);
                    }
                }

                csvWriter.NextRecord();
            }

            streamWriter.Flush();
        }

        //now write to the main file but first writing the headers
        using (var sw = new StreamWriter(filePath))
        {
            sw.WriteLine(string.Join(",", headers));

            foreach (var line in await File.ReadAllLinesAsync(tempFilePath))
            {
                sw.WriteLine(line);
            }
        }

        File.Delete(tempFilePath);

        var file = File.Open(filePath, FileMode.Open, FileAccess.Read);

        return new FileStreamResult(file, "text/csv") { FileDownloadName = filename };
    }
}
