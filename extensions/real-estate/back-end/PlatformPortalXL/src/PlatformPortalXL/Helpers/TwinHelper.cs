using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.Helpers
{
    public static class TwinHelper
    {
        public readonly static string[] Hidden = new []
        {
            "/registrationID",
            "/registrationKey",
            "/lastValue",
            "/lastValueTime",
            "/mappedIds"
        };

        public readonly static string[] ReadOnly = new []
        {
            "/id",
            "/uniqueID",
            "/siteID",
            "/installedByRef",
            "/manufacturedByRef",
            "/serviceProviderRef",
            "/serviceResponsibilityRef",
            "/commissionedByRef",
            "/connectorID",
            "/detected",
            "/registrationID",
            "/registrationKey",
            "/customTags",
            "/customProperties",
            "/categorizationProperties",
            "/categorizationProperties/alarm",
            "/categorizationProperties/assetComponent",
            "/categorizationProperties/demand",
            "/categorizationProperties/effective",
            "/categorizationProperties/electricalPhase",
            "/categorizationProperties/externalIds",
            "/categorizationProperties/HVACMode",
            "/categorizationProperties/limit",
            "/categorizationProperties/name",
            "/categorizationProperties/occupancyMode",
            "/categorizationProperties/phenomenon",
            "/categorizationProperties/position",
            "/categorizationProperties/stage",
            "/categorizationProperties/position",
            "/communication",
            "/communication/BACnet",
            "/communication/BACnet/deviceId",
            "/communication/BACnet/objectType",
            "/communication/BACnet/objectId",
            "/communication/protocol",
            "/communication/API",
            "/communication/API/externalId",
            "/communication/IoTHub",
            "/communication/IoTHub/externalId",
            "/communication/Modbus",
            "/communication/OPCDA",
            "/communication/OPCUA",
            "/tags",
            "/trendID",
            "/trendInterval",
            "/lastValue",
            "/lastValueTime",
            "/scaleFactor",
            "/CO2",
            "/CO2/CO2Delta",
            "/CO2/CO2Sensor",
            "/CO2/CO2Setpoint",
            "/humidity",
            "/humidity/humidityDelta",
            "/humidity/humiditySensor",
            "/humidity/humiditySetpoint",
            "/occupancy",
            "/occupancy/dwellTimeAverage",
            "/occupancy/entranceDwellTime",
            "/occupancy/entranceRate",
            "/occupancy/exitDwellTime",
            "/occupancy/exitRate",
            "/occupancy/hasInferredOccupancy",
            "/occupancy/isOccupied",
            "/occupancy/peopleCount",
            "/temperature",
            "/temperature/temperatureDelta",
            "/temperature/temperatureSensor",
            "/temperature/temperatureSetpoint",
            "/accessEventType",
            "/ownedByRef"
        };

        static TwinHelper()
        {
            // Sanitize assumes that all the paths in Hidden have a single
            // slash, at the start, and don't contain tildes (which are used to
            // escape backslashes in JSON pointers). So we validate that on
            // startup.
            foreach (var path in Hidden)
            {
               if (
                   path[0] != '/'
                   || path.Count(c => c == '/') != 1
                   || path.Any(c => c == '~'))
               {
                  throw new ArgumentException("we only support super basic JSON pointers");
               }
            }
        }

        /// <summary>
        /// Remove hidden fields from `twin`
        /// </summary>
        public static void Sanitize(JToken twin)
        {
            foreach (var path in Hidden)
            {
               twin.SelectToken(path.Substring(1), errorWhenNoMatch: false)?.Parent?.Remove();
            }
        }
    }
}
