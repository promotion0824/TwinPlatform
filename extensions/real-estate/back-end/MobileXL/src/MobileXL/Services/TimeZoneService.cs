using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MobileXL.Services
{
    public interface ITimeZoneService
    {
        string GetTimeZoneType(string timeZoneId);
    }

    public class TimeZoneService : ITimeZoneService
    {
        private const string XmlFileName = @".WindowsZones.xml";
        private readonly Dictionary<string, string> _timeZoneDictionary;

        public TimeZoneService()
        {
            var xmlFileContent = ReadXmlFile();
            _timeZoneDictionary = ParseXml(xmlFileContent);
        }

        public string GetTimeZoneType(string timeZoneId)
        {
            _timeZoneDictionary.TryGetValue(timeZoneId, out string ret);
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
    }
}
