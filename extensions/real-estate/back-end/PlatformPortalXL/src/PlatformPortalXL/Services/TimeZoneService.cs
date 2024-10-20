using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using TimeZoneNames;

namespace PlatformPortalXL.Services
{
    public interface ITimeZoneService
    {
        string GetTimeZoneType(string timeZoneId);
        Dictionary<string, string> GetRegionTimeZone(string timeZoneId);
        Task<List<TimeZoneDto>> GetTimeZones();
    }

    public class TimeZoneService : ITimeZoneService
    {
        private const string XmlFileName = @".WindowsZones.xml";
        private readonly Dictionary<string, string> _timeZoneDictionary;
        private readonly Dictionary<string, Dictionary<string, string>> _regionTimeZoneDictionary;

        public TimeZoneService()
        {
            var xmlFileContent = ReadXmlFile();
            _timeZoneDictionary = ParseXml(xmlFileContent);
            _regionTimeZoneDictionary = ParseXmlForRegion(xmlFileContent);
        }

        public string GetTimeZoneType(string timeZoneId)
        {
            _timeZoneDictionary.TryGetValue(ConvertTimeZone(timeZoneId), out string ret);
            return ret;
        }

        private static Dictionary<string, string> ParseXml(string xmlFileContent)
        {
            var dict = new Dictionary<string, string>();
            var xml = new XmlDocument();
            xml.LoadXml(xmlFileContent);
            var nodes = xml.SelectNodes(@"supplementalData/windowsZones/mapTimezones//mapZone[@territory='001']");
            foreach (XmlNode node in nodes)
            {
                var key = node.Attributes["other"].Value;
                var value = node.Attributes["type"].Value;
                dict.Add(key, value);
            }

            return dict;
        }

        private static string ReadXmlFile()
        {
            var assembly = typeof(TimeZoneService).Assembly;
            var resourceName = assembly.GetManifestResourceNames().Single(s => s.EndsWith(XmlFileName, System.StringComparison.InvariantCulture));
            var resourceContent = string.Empty;
            var stream = assembly.GetManifestResourceStream(resourceName);
            using (StreamReader reader = new StreamReader(stream))
            {
                resourceContent = reader.ReadToEnd();
            }

            return resourceContent;

        }

        private static Dictionary<string, Dictionary<string, string>> ParseXmlForRegion(string xmlFileContent)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> timezone = new Dictionary<string, string>();
            var key = string.Empty;
            var other = string.Empty;
            var xml = new XmlDocument();
            xml.LoadXml(xmlFileContent);
            var nodes = xml.SelectNodes(@"supplementalData/windowsZones/mapTimezones//mapZone");
            foreach (XmlNode node in nodes)
            {
                key = node.Attributes["other"].Value;
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, timezone);
                    if (timezone.Count > 0)
                    {
                        dict[other] = timezone;
                    }
                    other = key;
                    timezone = new Dictionary<string, string>();
                    var territory = node.Attributes["territory"].Value;
                    var type = node.Attributes["type"].Value;
                    timezone.Add(territory, type);
                }
                else
                {
                    var territory = node.Attributes["territory"].Value;
                    var type = node.Attributes["type"].Value;
                    timezone.Add(territory, type);
                }
            }
            dict[key] = timezone;
            return dict;
        }

        public Dictionary<string, string> GetRegionTimeZone(string timeZoneId)
        {
            _regionTimeZoneDictionary.TryGetValue(ConvertTimeZone(timeZoneId), out Dictionary<string, string> ret);
            return ret;
        }

        private static string ConvertTimeZone(string timeZoneId)
        {
            return TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out string windowsId) ? windowsId : timeZoneId;
        }

        public async Task<List<TimeZoneDto>> GetTimeZones()
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones();
            var dtos = new List<TimeZoneDto>();
            var processedIds = new List<string>();

            await Task.Run(() =>
            {
                foreach (TimeZoneInfo tzInfo in timeZones)
                {
                    // TimeZoneInfo rely on time zone data of the operating system.
                    // On Linux/Unix, the IANA id is used as ID, so we need to convert this to ensure consistency.
                    var id = TimeZoneInfo.TryConvertIanaIdToWindowsId(tzInfo.Id, out string windowsId) ? windowsId : tzInfo.Id;

                    if (!processedIds.Contains(id))
                    {
                        dtos.Add(new TimeZoneDto
                        {
                            Id = id,
                            // Ensure consistent display name on Linux, Unix, Windows. Future work to support other languages.
                            DisplayName = TZNames.GetDisplayNameForTimeZone(id, "en"),
                            Offset =
                                $"{(tzInfo.BaseUtcOffset.Hours < 0 ? "-" : "+")}{tzInfo.BaseUtcOffset.ToString(@"hh\:mm")}",
                            RegionTimeZone = GetRegionTimeZone(id)
                        });
                        processedIds.Add(id);
                    }
                }
            });

            return dtos;
        }
    }
}
