using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Willow.Common.Configuration;

// Handles reading .env files to be able to support using telepresence configuration output
public class DotEnvVariablesConfigurationProvider : ConfigurationProvider
{
    private readonly string _filePath;

    public DotEnvVariablesConfigurationProvider(string? filePath)
    {
        _filePath = filePath ?? Path.Combine(Directory.GetCurrentDirectory(), ".env");
    }

    public override void Load()
    {
        if (File.Exists(_filePath))
        {
            var regex = new Regex(@"([^=]*)=(.*)");
            foreach (var line in File.ReadAllLines(_filePath))
            {
                var match = regex.Match(line);
                if (match.Success && match.Groups.Count == 3)
                {
                    Data[NormalizeKey(match.Groups[1].Value)] = match.Groups[2].Value;
                }
            }
        }
    }


    private static string NormalizeKey(string key) => key.Replace("__", ConfigurationPath.KeyDelimiter);
}
