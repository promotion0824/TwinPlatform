using System;
using System.Collections.Concurrent;

namespace Willow.Expressions;

/// <summary>Unit output type</summary>
public enum UnitOutputType
{
    /// <summary>Unknown type</summary>
    Undefined = 0,

    /// <summary>Analog type</summary>
    Analog = 1,

    /// <summary>Binary type for boolean</summary>
    Binary = 2
}

public partial class Unit
{
    /// <summary>
    /// Predefined collection of units
    /// </summary>
    private static Unit[] predefinedUnits;

    /// <summary>
    /// Gets the predefined units
    /// </summary>
    public static Unit[] PredefinedUnits => predefinedUnits;

    static Unit()
    {
        predefinedUnits = new Unit[]
        {
            degC,
            degF,
            percentage,
            percentage100,
            rpm,
            cfm,
            lps,
            iwc,
            watt,
            kW,
            GW,
            kWh,
            amp,
            mA,
            amph,
            mAh,
            volt,
            kilogram,
            gram,
            mg,
            pascal,
            kPa,
            USD,
            USD2,
            scalar,
            second,
            minute,
            month,
            week,
            hour,
            day,
            array,
            boolean,
            new Unit("ph") { OutOfRange = Acidity },
            new Unit("kvah") { OutOfRange = Apparent_Energy_Power },
            new Unit("kva") { OutOfRange = Apparent_Energy_Power },
            new Unit("ppm") { OutOfRange = Concentration },
            new Unit("amps") { OutOfRange = Current },
            new Unit("angle") { OutOfRange = Degree_Angle },
            new Unit("deg") { OutOfRange = Degree_Angle },
            new Unit("degd'angle") { OutOfRange = Degree_Angle },
            new Unit("degAngular") { OutOfRange = Degree_Angle },
            new Unit("kw/ton") { OutOfRange = Efficiency },
            new Unit("btu") { OutOfRange = Energy_Btu },
            new Unit("kbtu"),
            new Unit("wh") { OutOfRange = Energy_Wh },
            new Unit("btu/lb") { OutOfRange = Enthalpy_Btu },
            new Unit("kj/kg") { OutOfRange = Enthalpy_KJ },
            new Unit("cfh") { OutOfRange = FlowRate_Cfh },
            new Unit("ft3/min") { OutOfRange = FlowRate_Cfm_Gph },
            new Unit("gph") { OutOfRange = FlowRate_Cfm_Gph },
            new Unit("l/h") { OutOfRange = FlowRate_Lph },
            new Unit("l/hr") { OutOfRange = FlowRate_Lph },
            new Unit("l/min") { OutOfRange = FlowRate_Lpm },
            new Unit("m3/h") { OutOfRange = FlowRate_Cmph },
            new Unit("m3/hr") { OutOfRange = FlowRate_Cmph },
            new Unit("hz") { OutOfRange = Frequency },
            new Unit("lux") { OutOfRange = Illuminance },
            new Unit("in") { OutOfRange = Length_In },
            new Unit("ft") { OutOfRange = Length_ft },
            new Unit("hg") { OutOfRange = Mass_Hg },
            new Unit("lbs-hr") { OutOfRange = MassFlow_Pph },
            new Unit("lbs/hr") { OutOfRange = MassFlow_Pph },
            new Unit("ppl") { OutOfRange = People },
            new Unit("btu/h") { OutOfRange = Energy_Btu },
            new Unit("btu/hr") { OutOfRange = Energy_Btu },
            new Unit("btu-h") { OutOfRange = Energy_Btu },
            new Unit("kbtuh-h"),
            new Unit("kbtu-h"),
            new Unit("tr") { OutOfRange = Power_Tr },
            new Unit("bar") { OutOfRange = Pressure_Bar },
            new Unit("hpa") { OutOfRange = Pressure_Hpa },
            new Unit("inh2o") { OutOfRange = Pressure_InH2O },
            new Unit("in.wc") { OutOfRange = Pressure_InH2O },
            new Unit("in/wc") { OutOfRange = Pressure_InH2O },
            new Unit("psi") { OutOfRange = Pressure_Psi },
            new Unit("w/m2") { OutOfRange = Power_Density },
            new Unit("pf") { OutOfRange = Power_Factor },
            new Unit("kvar") { OutOfRange = Power_Reactive },
            new Unit("%rh") { OutOfRange = Relative_Humidity },
            new Unit("rssi") { OutOfRange = Signal_Strength },
            new Unit("cop") { OutOfRange = Thermal_Efficiency },
            new Unit("km/h") { OutOfRange = Velocity_Kph },
            new Unit("km/hr") { OutOfRange = Velocity_Kph },
            new Unit("m/s") { OutOfRange = Velocity_Ms },
            new Unit("mph") { OutOfRange = Velocity_Mph },
            new Unit("gal") { OutOfRange = Volume_Gal },
            new Unit("kl") { OutOfRange = Volume_Kl },
            new Unit("l") { OutOfRange = Volume_L },
        };

        allUnits = new ConcurrentDictionary<string, Unit>(StringComparer.OrdinalIgnoreCase);

        //Add predefined units and aliases to the dictionary
        foreach (var unit in predefinedUnits)
        {
            allUnits[unit.Name] = unit;
            if (unit.Aliases != null)
            {
                foreach (var alias in unit.Aliases)
                {
                    allUnits[alias] = unit;
                }
            }
        }
    }

    /// <summary>
    /// Seconds
    /// </summary>
    public static readonly Unit second = new Unit("s");

    /// <summary>
    /// Seconds
    /// </summary>
    public static readonly Unit minute = new Unit("m", "minute", "minutes", "min");

    /// <summary>
    /// Seconds
    /// </summary>
    public static readonly Unit hour = new Unit("h", "hour", "hours", "hrs", "hr");

    /// <summary>
    /// Seconds
    /// </summary>
    public static readonly Unit day = new Unit("d", "day", "days");

    /// <summary>
    /// Month
    /// </summary>
    public static readonly Unit month = new Unit("Mth");

    /// <summary>
    /// Week
    /// </summary>
    public static readonly Unit week = new Unit("wk");

    /// <summary>
    /// Array of objects/values
    /// </summary>
    public static readonly Unit array = new Unit("array");

    /// <summary>
    /// Boolean
    /// </summary>
    public static readonly Unit boolean = new Unit("bool");

    /// <summary>
    /// Scalar
    /// </summary>
    /// <remarks>
    /// Not used any more
    /// </remarks>
    public static readonly Unit scalar = new Unit("scalar");

    /// <summary>
    /// Inches water column
    /// </summary>
    public static readonly Unit iwc = new Unit("iwc")
    {
        OutOfRange = Pressure_InH2O
    };

    /// <summary>
    /// Pascal (pressure)
    /// </summary>
    public static readonly Unit pascal = new Unit("Pa")
    {
        OutOfRange = Pressure_Pa
    };

    /// <summary>
    /// USD
    /// </summary>
    public static readonly Unit USD = new Unit("USD");

    /// <summary>
    /// Pascal (pressure)
    /// </summary>
    public static readonly Unit USD2 = new Unit("$");

    /// <summary>
    /// Kilopascal
    /// </summary>
    public static readonly Unit kPa = new Unit("kPa")
    {
        //to pascal
        BaseConversion = (v) => v / 1000,
        OutOfRange = Pressure_Kpa
    };

    /// <summary>
    /// Kilogram
    /// </summary>
    public static readonly Unit kilogram = new Unit("kg");

    /// <summary>
    /// Gram
    /// </summary>
    public static readonly Unit gram = new Unit("g")
    {
        //to kilogram
        BaseConversion = (v) => v / 1000
    };

    /// <summary>
    /// Milligram
    /// </summary>
    public static readonly Unit mg = new Unit("mg")
    {
        //to kilogram
        BaseConversion = (v) => v / 1000 / 1000,
        OutOfRange = Magnetic_Field_Strength
    };

    /// <summary>
    /// Degrees celsius
    /// </summary>
    public static readonly Unit degC = new Unit("degC", "C", "°C", "celsius", "degrees-celsius")
    {
        //to kelvin
        BaseConversion = (v) => v + 273.15d,
        OutOfRange = Temperature_C
    };

    /// <summary>
    /// Degrees fahrenheit
    /// </summary>
    public static readonly Unit degF = new Unit("degF", "F", "°F", "fahrenheit", "degrees-fahrenheit")
    {
        //to kelvin
        BaseConversion = (v) => ((v - 32d) * 5 / 9) + 273.15d,
        OutOfRange = Temperature_F
    };

    /// <summary>
    /// Percentage
    /// </summary>
    public static readonly Unit percentage = new Unit("%")
    {
        OutOfRange = Percentage
    };

    /// <summary>
    /// Percentage 100
    /// </summary>
    public static readonly Unit percentage100 = new Unit("%100", "%00")
    {
        //to standard percentage
        BaseConversion = (v) => v * 100d
    };

    /// <summary>
    /// RPM
    /// </summary>
    public static readonly Unit rpm = new Unit("rpm")
    {
        OutOfRange = Rotation
    };

    /// <summary>
    /// CFM
    /// </summary>
    public static readonly Unit cfm = new Unit("cfm")
    {
        OutOfRange = FlowRate_Cfm_Gph
    };

    /// <summary>
    /// Liters per second
    /// </summary>
    public static readonly Unit lps = new Unit("l/s")
    {
        //to cfm
        BaseConversion = (v) => v * 2.1188799727597,
        OutOfRange = FlowRate_Lps
    };

    /// <summary>
    /// Watt
    /// </summary>
    public static readonly Unit watt = new Unit("W")
    {
        OutOfRange = Power_W
    };

    /// <summary>
    /// Kilowatt
    /// </summary>
    public static readonly Unit kW = new Unit("kW")
    {
        //to watt
        BaseConversion = (v) => v / 1000d,
        OutOfRange = Power_Kw
    };

    /// <summary>
    /// Gigawatt
    /// </summary>
    public static readonly Unit GW = new Unit("GW")
    {
        //to watt
        BaseConversion = (v) => v * 1e+9
    };

    /// <summary>
    /// Kilowatt-hour
    /// </summary>
    public static readonly Unit kWh = new Unit("kWh")
    {
        OutOfRange = Energy_kWh
    };

    /// <summary>
    /// Amp
    /// </summary>
    public static readonly Unit amp = new Unit("A")
    {
        OutOfRange = Current
    };

    /// <summary>
    /// Milliamp
    /// </summary>
    public static readonly Unit mA = new Unit("mA")
    {
        //to amp
        BaseConversion = (v) => v / 1000
    };

    /// <summary>
    /// Aamp-hour
    /// </summary>
    public static readonly Unit amph = new Unit("Ah");

    /// <summary>
    /// Milliamp-hour
    /// </summary>
    public static readonly Unit mAh = new Unit("mAh")
    {
        //to amp hour
        BaseConversion = (v) => v / 1000
    };

    /// <summary>
    /// Volt
    /// </summary>
    public static readonly Unit volt = new Unit("V")
    {
        OutOfRange = Voltage
    };
}
