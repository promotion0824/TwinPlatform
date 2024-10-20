namespace Willow.CognitiveSearch;

using Azure.Search.Documents.Indexes.Models;

/// <summary>
/// Standard Willow Synonym maps.
/// </summary>
internal class SearchSynonymMap
{
    /// <summary>
    /// Gets the standard synonyn map for twin names.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "This is a long line that needs to be broken up for readability.")]
    public static SynonymMap Willow { get; } = new SynonymMap(
            "twinnames",
            "floor mount=>floormount\n" + // prevent floor mount from being floor
            "level actuator=>levelactuator\n" +

            // If we => ARROW this, niveau ONLY matches Floor (!)
            "level, floor, étage, Étage, etage, niveau\n" +
            "building, batiment\n" +
            "basement, cellar\n" +
            "stile, turnstile, turnstyle, style\n" +
            "AmazonGo, Amazon Go\n" + // BRF special
            "AHU, air handling unit, airhandlingunit\n" +
            "FV, flush valve\n" +
            "PF, power factor\n" +
            "PG, pressure gauge\n" +
            "PWR, power\n" +
            "MAT, mixed air temperature\n" +
            "RAT, return air temperature\n" +
            "SAT, supply air temperature\n" +
            "OAT, outside air temperature\n" +
            "DAT, discharge air temperature\n" +
            "DAH, discharge air humidity\n" +
            "ZNT, zone temperature\n" +
            "ZNH, zone humidity\n" +
            "DWG, drawing\n" +
            "FEC, extinguisher cabinet\n" +
            "ACCH, Air Cooled Chiller\n" +
            "AD, Access Door\n" +
            "AHU, Air Handling Unit\n" +
            "ALD, Automatic Louver Damper\n" +
            "AS, Air Separator\n" +
            "ATS, Automatic Transfer Switch\n" +
            "BDD, Backdraft Damper\n" +
            "CAV, Constant Air Volume\n" +
            "CC, Cooling Coil\n" +
            "CDW, Condenser Water\n" +
            "CDWP, Condernser Water Pump\n" +
            "CDWR, Condenser Water Return\n" +
            "CDWS, Condenser Water Supply\n" +
            "CH, Chiller\n" +
            "CHW, Chilled Water\n" +
            "CHWP, Chilled Water Pump\n" +
            "CHWR, Chilled Water Return\n" +
            "CHWS, Chilled Water Supply\n" +
            "CLG, Cooling\n" +
            "COGEN, Cogeneration\n" +
            "CO-GEN, Cogeneration\n" +
            "CP, Condensate Pump\n" +
            "CRAC, Computer Room Air Conditioning Unit\n" +
            "CT, Cooling Tower\n" +
            "CUH, Cabinet Unit Heater\n" +
            "CW, Cold Water\n" +
            "CW, Condenser Water\n" +
            "CWP, Cold Water Pump\n" +
            "CWP, Condernser Water Pump\n" +
            "CWR, Condenser Water Return\n" +
            "CWS, Condenser Water Supply\n" +
            "DA, Discharge Air\n" +
            "DAH, Discharge Air Humidity\n" +
            "DAT, Discharge Air Temperature\n" +
            "DB, Dry Bulb\n" +
            "DCW, Domestic Cold Water\n" +
            "DHW, Domestic Hot Water\n" +
            "DOAS, Dedicated Outside Air Unit\n" +
            "DW, Domestic Water\n" +
            "DX, Direct Expansion\n" +
            "EA, Exhaust Air\n" +
            "EAH, Exhaust Air Humidity\n" +
            "EAT, Entering Air Temperature\n" +
            "EAT, Exhaust Air Temperature\n" +
            "EF, Exhaust Fan\n" +
            "EM, Emergency\n" +
            "EMR, Elevator Machine Room\n" +
            "EPF, Elevator Pressurization Fan\n" +
            "ET, Expansion Tank\n" +
            "EV, Electric Vehicle\n" +
            "EWT, Entering Water Temperature\n" +
            "EX, Exhaust\n" +
            "EXF, Exhaust Fan\n" +
            "FCU, Fan Coil Unit\n" +
            "FD, Fire Damper\n" +
            "FO, Fuel Oil\n" +
            "FOP, Fuel Oil Pump\n" +
            "FOR, Fuel Oil Return\n" +
            "FOS, Fuel Oil Supply\n" +
            "FOV, Fuel Oil Vent\n" +
            "FPB, Fan Powered Box\n" +
            "FSD, Fire-Smoke Damper\n" +
            "GEN, Generator\n" +
            "GSX, Garage Smoke Exhaust\n" +
            "GX, Garage Exhaust\n" +
            "GX, General Exhaust\n" +
            "GXF, Garage Exhaust Fan\n" +
            "GXF, General Exhaust Fan\n" +
            "HC, Heating Coil\n" +
            "HHW, Heating Hot Water\n" +
            "HHWP, Heating Hot Water Pump\n" +
            "HHWR, Heating Hot Water Return\n" +
            "HHWS, Heating Hot Water Supply\n" +
            "HPS, High Pressure Steam\n" +
            "HTG, Heating\n" +
            "HUM, Humidifier\n" +
            "HV, Heating and Ventilation Unit\n" +
            "HW, Hot Water\n" +
            "HWP, Hot Water Pump\n" +
            "HWR, Hot Water Return\n" +
            "HWS, Hot Water Supply\n" +
            "HX, Heat Exchanger\n" +
            "KX, Kitchen Exhaust\n" +
            "KXF, Kitchen Exhaust Fan\n" +
            "LAT, Leaving Air Temperature\n" +
            "LPS, Low Pressure Steam\n" +
            "LTG, Lighting\n" +
            "LWT, Leaving Water Temperature\n" +
            "MA, Mixed Air\n" +
            "MAH, Mixed Air Humidity\n" +
            "MAT, Mixed Air Temperature\n" +
            "MAU, Makeup Air Unit\n" +
            "MER, Mechanical Equipment Room\n" +
            "MPS, Medium Pressure Steam\n" +
            "NC, Normally Closed\n" +
            "NO, Normally Open\n" +
            "OA, Outside Air\n" +
            "OAH, Outside Air Humidity\n" +
            "OAT, Outside Air Temperature\n" +
            "PC, Preheat Coil\n" +
            "PC, Pumped Condensate\n" +
            "PCDW, Primary Condenser Water\n" +
            "PCDW, Process Condenser Water\n" +
            "PCDWP, Primary Condenser Water Pump\n" +
            "PCDWP, Process Condenser Water Pump\n" +
            "PCDWR, Primary Condenser Water Return\n" +
            "PCDWR, Process Condenser Water Return\n" +
            "PCDWS, Primary Condenser Water Supply\n" +
            "PCDWS, Process Condenser Water Supply\n" +
            "PCHW, Primary Chilled Water\n" +
            "PCHW, Process Chilled Water\n" +
            "PCHWP, Primary Chilled Water Pump\n" +
            "PCHWP, Process Chilled Water Pump\n" +
            "PCHWR, Primary Chilled Water Return\n" +
            "PCHWR, Process Chilled Water Return\n" +
            "PCHWS, Primary Chilled Water Supply\n" +
            "PCHWS, Process Chilled Water Supply\n" +
            "PCW, Primary Condenser Water\n" +
            "PCW, Process Condenser Water\n" +
            "PCWP, Primary Condenser Water Pump\n" +
            "PCWP, Process Condenser Water Pump\n" +
            "PCWR, Primary Condenser Water Return\n" +
            "PCWR, Process Condenser Water Return\n" +
            "PCWS, Primary Condenser Water Supply\n" +
            "PCWS, Process Condenser Water Supply\n" +
            "PF, Pressurization Fan\n" +
            "PFHX, Plate and Frame Heat Exchanger\n" +
            "PHC, Preheat Coil\n" +
            "PHHW, Primary Heating Hot Water\n" +
            "PHHWP, Primary Heating Hot Water Pump\n" +
            "PHHWR, Primary Heating Hot Water Return\n" +
            "PHHWS, Primary Heating Hot Water Supply\n" +
            "PV, Photovoltaic\n" +
            "PWR, Power\n" +
            "RA, Return Air\n" +
            "RAH, Return Air Humidity\n" +
            "RAT, Return Air Temperature\n" +
            "RC, Reheat Coil\n" +
            "RF, Return Fan\n" +
            "RH, Relative Humidity\n" +
            "RHC, Reheat Coil\n" +
            "RTU, Rooftop Unit\n" +
            "SA, Supply Air\n" +
            "SAH, Supply Air Humidity\n" +
            "SAT, Supply Air Temperature\n" +
            "SCDW, Secondary Condenser Water\n" +
            "SCDWP, Secondary Condenser Water Pump\n" +
            "SCDWR, Secondary Condenser Water Return\n" +
            "SCDWS, Secondary Condenser Water Supply\n" +
            "SCHW, Secondary Chilled Water\n" +
            "SCHWP, Secondary Chilled Water Pump\n" +
            "SCHWR, Secondary Chilled Water Return\n" +
            "SCHWS, Secondary Chilled Water Supply\n" +
            "SCW, Secondary Condenser Water\n" +
            "SCWP, Secondary Condenser Water Pump\n" +
            "SCWR, Secondary Condenser Water Return\n" +
            "SCWS, Secondary Condenser Water Supply\n" +
            "SD, Smoke Damper\n" +
            "SF, Supply Fan\n" +
            "SHHW, Secondary Heating Hot Water\n" +
            "SHHWP, Secondary Heating Hot Water Pump\n" +
            "SHHWR, Secondary Heating Hot Water Return\n" +
            "SHHWS, Secondary Heating Hot Water Supply\n" +
            "SPF, Stair Pressurization Fan\n" +
            "SSF, Sidestream Filtration\n" +
            "ST, Storm Water\n" +
            "STHX, Shell and Tube Heat Exchanger\n" +
            "SX, Smoke Exhaust\n" +
            "SXP, Spoke Exhaust Fan\n" +
            "TCDW, Tritiary Condenser Water\n" +
            "TCDWP, Tritiary Condenser Water Pump\n" +
            "TCDWR, Tritiary Condenser Water Return\n" +
            "TCDWS, Tritiary Condenser Water Supply\n" +
            "TCHW, Tritiary Chilled Water\n" +
            "TCHWP, Tritiary Chilled Water Pump\n" +
            "TCHWR, Tritiary Chilled Water Return\n" +
            "TCHWS, Tritiary Chilled Water Supply\n" +
            "TCW, Tritiary Condenser Water\n" +
            "TCWP, Tritiary Condenser Water Pump\n" +
            "TCWR, Tritiary Condenser Water Return\n" +
            "TCWS, Tritiary Condenser Water Supply\n" +
            "TF, Transfer Fan\n" +
            "TU, Terminal Unit\n" +
            "TX, Toilet Exhaust\n" +
            "TX, Transformer\n" +
            "TXF, Toilet Exhaust Fan\n" +
            "UH, Unit Heater\n" +
            "VA, Ventilation Air\n" +
            "VAH, Ventilation Air Humidity\n" +
            "VAT, Ventilation Air Temperature\n" +
            "VAV, Variable Air Volume\n" +
            "VD, Volume Damper\n" +
            "VFC, VRF Fan Coil Unit\n" +
            "VFD, Variable Frequency Drive\n" +
            "VRF, Variable Refrigerant Flow\n" +
            "WB, Wet Bulb\n" +
            "WCCH, Water Cooled Chiller\n" +
            "WH, Water Heater\n" +
            "XT, Expansion Tank\n" +

            // deal with badly named levels
            "01,1\n02,2\n03,3\n04,4\n05,5\n06,6\n07,7\n08,8\n09,9");
}
