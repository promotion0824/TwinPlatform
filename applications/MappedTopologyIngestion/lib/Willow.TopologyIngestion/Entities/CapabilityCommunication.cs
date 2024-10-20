namespace Willow.TopologyIngestion.Entities
{
    /// <summary>
    /// A enum that represents the various possible BACnet object types.
    /// </summary>
    public enum BACnetCapabilityObjectType
    {
        /// <summary>
        /// An analog input object type.
        /// </summary>
        AI = 0,

        /// <summary>
        /// An analog output object type.
        /// </summary>
        AO = 1,

        /// <summary>
        /// An analog value object type.
        /// </summary>
        AV = 2,

        /// <summary>
        /// A binary input object type.
        /// </summary>
        BI = 3,

        /// <summary>
        /// A binary output object type.
        /// </summary>
        BO = 4,

        /// <summary>
        /// A binary value object type.
        /// </summary>
        BV = 5,

        /// <summary>
        /// A calendar object type.
        /// </summary>
        CAL = 6,

        /// <summary>
        /// A command object type.
        /// </summary>
        DEV = 8,

        /// <summary>
        /// A Multi-state input object type.
        /// </summary>
        MSI = 13,

        /// <summary>
        /// A Multi-state output object type.
        /// </summary>
        MSO = 14,

        /// <summary>
        /// A schedule object type.
        /// </summary>
        SCHED = 17,

        /// <summary>
        /// A Multi-state value object type.
        /// </summary>
        MSV = 19,

        /// <summary>
        /// An accumulator object type.
        /// </summary>
        ACC = 23,
    }

    /// <summary>
    /// A class that represents the various communication properties for a twin.
    /// </summary>
    public class CapabilityCommunication
    {
        /// <summary>
        /// Gets or sets the protocol used for communication.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string protocol { get; set; } = string.Empty;
#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Gets or sets the Bacnet properties for the communication.
        /// </summary>
        public BACnetCapability? BACnet { get; set; }
    }

    /// <summary>
    /// A class that represents the various BACnet properties for a twin.
    /// </summary>
    public class BACnetCapability
    {
        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public int deviceID { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Gets or sets the object ID.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public int objectID { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter

        /// <summary>
        /// Gets or sets the object type.
        /// </summary>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string objectType { get; set; } = string.Empty;
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
