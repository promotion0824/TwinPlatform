namespace Willow.Expressions;

public partial class Unit
{
    private static bool Acidity(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 14;
    }

    private static bool Apparent_Energy_Power(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 1000;
    }

    private static bool Concentration(string modelId, double pointValue)
    {
        // Julien: we could add CO2 sensors on there. The thresholds that I’m using in the data quality rule
        // itself are 310 / 1850 ppm. The 310ppm is basically avg outside CO2 level minus some
        // calibration variation on the sensor (they often come with +/ -100ppm calibration),
        // and the 1850ppm is because sensors generally go up to 2000ppm.There could be sensors
        // going above that though, but I haven’t seen them in buildings myself.
        if (modelId.EndsWith("CO2AirQualitySensor;1") && (pointValue is < 310 or > 1850)) return true;

        return pointValue is < 0 or > 100000;
    }

    private static bool Current(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 1500;
    }

    private static bool Degree_Angle(string modelId, double pointValue)
    {
        // Most are 0-360 but some ...
        return pointValue is < -720 or > 720;
    }

    private static bool Efficiency(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 2;
    }

    private static bool Energy_Btu(string modelId, double pointValue)
    {
        // enthalpy can go negative
        return pointValue is < -100 or > 10000000;
    }

    private static bool Energy_kWh(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 10000000000;
    }

    private static bool Energy_Wh(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 10000000000000;
    }

    private static bool Enthalpy_Btu(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 70000;
    }

    private static bool Enthalpy_KJ(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 160000;
    }

    private static bool FlowRate_Cfh(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 1700;
    }

    private static bool FlowRate_Cfm_Gph(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 100000;
    }

    private static bool FlowRate_Lph(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 170000000;
    }

    private static bool FlowRate_Lpm(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 2800000;
    }

    private static bool FlowRate_Lps(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 45000;
    }

    private static bool FlowRate_Cmph(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 170000;
    }

    private static bool Frequency(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 95;
    }

    private static bool Illuminance(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 1000000;
    }

    private static bool Length_In(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 18000;
    }

    private static bool Length_ft(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 1500;
    }

    private static bool Magnetic_Field_Strength(string modelId, double pointValue)
    {
        return pointValue is < -1000 or > 10000;
    }

    private static bool Mass_Hg(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 100000;
    }

    private static bool MassFlow_Pph(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 10000000;
    }

    private static bool People(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 100000;
    }

    private static bool Percentage(string modelId, double pointValue)
    {
        // Signal strengths aren't really percentages
        if (modelId.EndsWith("SignalStrengthSensor;1")) return pointValue > 200;

        // many % sensors go negative unfortunately
        return pointValue is < -101 or > 101;
    }

    private static bool Power_Kw(string modelId, double pointValue)
    {
        return pointValue is < -10 or > 10000000;
    }

    private static bool Power_Tr(string modelId, double pointValue)
    {
        return pointValue is < -1 or > 10000;
    }

    private static bool Power_W(string modelId, double pointValue)
    {
        return pointValue is < -100 or > 1000000000;
    }

    private static bool Pressure_Bar(string modelId, double pointValue)
    {
        return pointValue is < -0.35 or > 35;
    }

    private static bool Pressure_Hpa(string modelId, double pointValue)
    {
        return pointValue is < -35 or > 3500;
    }

    private static bool Pressure_InH2O(string modelId, double pointValue)
    {
        return pointValue is < -150 or > 15000;
    }

    private static bool Pressure_Kpa(string modelId, double pointValue)
    {
        return pointValue is < -3.5 or > 350;
    }

    private static bool Pressure_Pa(string modelId, double pointValue)
    {
        return pointValue is < -3500 or > 350000;
    }

    private static bool Pressure_Psi(string modelId, double pointValue)
    {
        return pointValue is < -5 or > 500;
    }

    private static bool Power_Density(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 1000;
    }

    private static bool Power_Factor(string modelId, double pointValue)
    {
        return pointValue is < -1 or > 1;
    }

    private static bool Power_Reactive(string modelId, double pointValue)
    {
        return pointValue is < -1000 or > 1000;
    }

    private static bool Rotation(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 5000;
    }

    private static bool Relative_Humidity(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 100;
    }

    private static bool Signal_Strength(string modelId, double pointValue)
    {
        return pointValue is < -100 or > 0;
    }

    private static bool Temperature_C(string modelId, double pointValue)
    {
        return pointValue is < -73 or > 288;
    }

    private static bool Temperature_F(string modelId, double pointValue)
    {
        return pointValue is < -100 or > 550;
    }

    private static bool Thermal_Efficiency(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 15;
    }

    private static bool Velocity_Kph(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 1200;
    }

    private static bool Velocity_Ms(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 350;
    }

    private static bool Velocity_Mph(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 1000;
    }

    private static bool Voltage(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 100000;  // 100kV, have seen 12kV
    }

    private static bool Volume_Gal(string modelId, double pointValue)
    {
        //make the max really big for now. This should be testing a delta instead of absolute values
        return pointValue is < 0 or > 10000000000;
    }

    private static bool Volume_Kl(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 10000000000;
    }

    private static bool Volume_L(string modelId, double pointValue)
    {
        return pointValue is < 0 or > 10000000000;
    }
}
