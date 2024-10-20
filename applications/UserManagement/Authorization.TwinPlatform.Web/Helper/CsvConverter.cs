using CsvHelper;
using System.Globalization;
using System.IO.Compression;

namespace Authorization.TwinPlatform.Web.Helper;

/// <summary>
/// Static class for helping with reading and writing Csv file 
/// </summary>
public static class CsvConverter
{
	public static IEnumerable<T> ConvertFromCSVStream<T>(Stream fileStream)
	{
		using var streamReader = new StreamReader(fileStream, leaveOpen: false);
		using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture, leaveOpen: false);
		return csvReader.GetRecords<T>().ToList();
	}

	public static async Task<byte[]> ConvertToCSVBytes<T>(List<T> records) where T : notnull
	{
		using var memStream = new MemoryStream();
		using var streamWriter = new StreamWriter(memStream, leaveOpen: false);
		using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture, leaveOpen: false);
		await csv.WriteRecordsAsync(records);
		await csv.FlushAsync();
		memStream.Position = 0;
		return memStream.ToArray();
	}

    public static async Task CopyRecordsToZipAsync<T>(List<T> records, ZipArchive zipArchive, string fileName) where T : notnull
    {
        var recordBytes = await ConvertToCSVBytes(records);
        using var recordStream = new MemoryStream(recordBytes);
        // append file extension to file name
        fileName += ".csv";
        await using var entryStream = zipArchive.CreateEntry(fileName).Open();
        await recordStream.CopyToAsync(entryStream);
    }
}
