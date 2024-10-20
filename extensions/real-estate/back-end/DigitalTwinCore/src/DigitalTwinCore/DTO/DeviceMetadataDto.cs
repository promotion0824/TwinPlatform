using DigitalTwinCore.Models;
using System;

namespace DigitalTwinCore.Dto
{
    [Serializable]
    public class DeviceMetadataDto
    {
        public string Address { get; set; }
        public string MacAddress { get; set; }

        //Modbus
        public string IpAddress { get; set; }

        //PointGrab
        public string AttachmentState { get; set; }
        public string ConnectionStatus { get; set; }
        public string FwVersion { get; set; }
        public string GeoPosition { get; set; }
        public string Height { get; set; }
        public string MetricPosition { get; set; }
        public string Rotation { get; set; }
        public string SerialNo { get; set; }

        internal static DeviceMetadataDto MapFrom(Twin twin)
        {
            if (twin == null)
            {
                return null;
            }

            return new DeviceMetadataDto
            {
                MacAddress = twin.GetStringProperty("MACAddress"),
                IpAddress = twin.GetStringProperty("IPAddress"),

                //TODO: This field is used by BACNet and needs to be defined in Asset/Device model 
                Address = twin.GetStringProperty("Address"),

                //TODO: These fields exist in PointGrab metadata, but may not be used, but map to Asset property
                SerialNo = twin.GetStringProperty("serialNumber"),

                //TODO: These fields exist in PointGrab metadata, but may not be used. If needed they need to be defined in Asset/Device model
                AttachmentState = twin.GetStringProperty("AttachmentState"),
                ConnectionStatus = twin.GetStringProperty("ConnectionStatus"),
                FwVersion = twin.GetStringProperty("FwVersion"),
                GeoPosition = twin.GetStringProperty("GeoPosition"),
                Height = twin.GetStringProperty("Height"),
                MetricPosition = twin.GetStringProperty("MetricPosition"),
                Rotation = twin.GetStringProperty("Rotation"),
            };
        }
    }
}
