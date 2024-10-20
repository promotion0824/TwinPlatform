using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace DigitalTwinCore.Test.Infrastructure
{
    public static class FileHelper
    {
        public static T LoadFile<T>(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(fileName));

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var result = reader.ReadToEnd();

            var modelData = JsonConvert.DeserializeObject<T>(result);
            return modelData;
        }
    }
}
