using DigitalTwinCore.Models.Connectors;
using System;

namespace DigitalTwinCore.Dto
{
    [Serializable]
    public class PointCommunicationDto
    {
        public PointCommunicationProtocol Protocol { get; set; }
        public ApiPointProperties Api { get; set; }
        public BacNetPointProperties BacNet { get; set; }
        public ModbusPointProperties Modbus { get; set; }
        public OpcDaPointProperties OpcDa { get; set; }
        public OpcUaPointProperties OpcUa { get; set; }
        public IotHubPointProperties IotHub { get; set; }

        internal static PointCommunicationDto MapFrom(PointCommunication model)
        {
            if (model == null)
            {
                return null;
            }

            return new PointCommunicationDto
            {
                Api = model.Api,
                BacNet = model.BacNet,
                IotHub = model.IotHub,
                Modbus = model.Modbus,
                OpcDa = model.OpcDa,
                OpcUa = model.OpcUa,
                Protocol = model.Protocol
            };
        }
    }
}
