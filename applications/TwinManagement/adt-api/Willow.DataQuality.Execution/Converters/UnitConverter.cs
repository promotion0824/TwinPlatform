namespace Willow.DataQuality.Execution.Converters;

internal class UnitConverter
{
    private static readonly Dictionary<string, Dictionary<string, double>> unitsConversions = new Dictionary<string, Dictionary<string, double>>
        {
            { "britishThermalUnitPerHour", new Dictionary<string, double>
                                                 {
                                                    { "kilowattHour", 2.5673 }
                                                 }
            }
        };


    public static bool ConvertToUnit(string unitFrom, string unitTo, double value, out double result)
    {
        if (unitsConversions.ContainsKey(unitFrom))
        {
            var units = unitsConversions[unitFrom];
            if (units.ContainsKey(unitTo))
            {
                result = value * units[unitTo];
                return true;
            }
        }
        result = 0;
        return false;
    }
}
