namespace OntologyGraphTool.Models;

public class Rules
{
    public static readonly List<(string a, string b)> IllegalAncestorCombinations = new List<(string a, string b)>()
    {
        ("dtmi:org:brickschema:schema:Brick:Space;1", "dtmi:com:willowinc:Asset;1"),
        ("dtmi:mapped:core:Device;1", "dtmi:com:willowinc:Space;1"),
        // Point covers Sensor and Setpoint
        ("dtmi:org:brickschema:schema:Brick:Point;1", "dtmi:com:willowinc:Equipment;1"),
        ("dtmi:mapped:core:Device;1", "dtmi:com:willowinc:Document;1"),
        ("dtmi:org:brickschema:schema:Brick:Command;1", "dtmi:com:willowinc:Sensor;1"),
        ("dtmi:mapped:core:Place;1", "dtmi:com:willowinc:Equipment;1"),
        ("dtmi:org:brickschema:schema:Brick:Sensor;1", "dtmi:com:willowinc:Actuator;1"),
        ("dtmi:org:brickschema:schema:Brick:System;1","dtmi:com:willowinc:Capability;1"),
        ("dtmi:org:brickschema:schema:Brick:Point;1", "dtmi:com:willowinc:Collection;1"),
        ("dtmi:org:brickschema:schema:Brick:Space;1", "dtmi:com:willowinc:Collection;1"),
        ("dtmi:org:brickschema:schema:Brick:HVAC_Equipment;1","dtmi:com:willowinc:CompressedAirEquipment;1"),
        ("dtmi:org:brickschema:schema:Brick:HVAC_Equipment;1","dtmi:com:willowinc:Capability;1"),
        ("dtmi:org:brickschema:schema:Brick:HVAC_Equipment;1", "dtmi:com:willowinc:BuildingComponent;1"),        ("dtmi:mapped:core:Person;1","dtmi:com:willowinc:Event;1"),
        // not ancestry ("dtmi:mapped:core:Grease_Trap;1", "dtmi:com:willowinc:HydronicSteamTrap;1"),
        ("dtmi:org:brickschema:schema:Brick:Sensor;1", "dtmi:com:willowinc:AssetComponent;1"),
        ("dtmi:org:brickschema:schema:Brick:Alarm;1", "dtmi:com:willowinc:Setpoint;1"),
        ("dtmi:org:brickschema:schema:Brick:Fan;1","dtmi:com:willowinc:ElectricalEquipment;1"),
        ("dtmi:org:brickschema:schema:Brick:Alarm;1", "dtmi:com:willowinc:Sensor;1"),
        ("dtmi:mapped:core:Place;1","dtmi:com:willowinc:Event;1"),
        ("dtmi:org:brickschema:schema:Brick:Command;1", "dtmi:com:willowinc:State;1"),
        ("dtmi:mapped:core:Thing;1", "dtmi:com:willowinc:Collection;1"),
        ("dtmi:org:brickschema:schema:Brick:Filter;1", "dtmi:com:willowinc:HVACFan;1"),
        ("dtmi:org:brickschema:schema:Brick:Parameter;1","dtmi:com:willowinc:Sensor;1"),
        ("dtmi:org:brickschema:schema:Brick:Point;1", "dtmi:com:willowinc:Component;1"),
        ("dtmi:org:brickschema:schema:Brick:Alarm;1","dtmi:com:willowinc:Component;1"),
        ("dtmi:org:brickschema:schema:Brick:Parameter;1","dtmi:com:willowinc:Setpoint;1"),
        ("dtmi:org:brickschema:schema:Brick:Setpoint;1", "dtmi:com:willowinc:Asset;1"),
        ("dtmi:org:brickschema:schema:Brick:Status;1", "dtmi:com:willowinc:Setpoint;1"),
        ("dtmi:org:brickschema:schema:Brick:HVAC_Equipment;1", "dtmi:com:willowinc:RetailEquipment;1"),
        ("dtmi:org:brickschema:schema:Brick:Air_Quality_Sensor;1", "dtmi:com:willowinc:PressureSensor;1"),
        ("dtmi:org:brickschema:schema:Brick:Pressure_Sensor;1", "dtmi:com:willowinc:TemperatureSensor;1"),
        ("dtmi:org:brickschema:schema:Brick:Voltage_Sensor;1", "dtmi:com:willowinc:TemperatureSensor;1")
    };

    public static readonly List<(string a, string b)> IllegalCombinations = new List<(string a, string b)>()
    {
        ("Sensor", "Setpoint"),
        ("Command", "Zone"),
        ("Command", "Controller"),
        ("Command", "Equipment"),
        ("Setpoint", "Actuator"),
        ("Luminance", "Pressure"),
        ("Max", "Min")
    };

    public static readonly List<(string a, string b)> KnownMappings = new List<(string a, string b)>(){
        ("dtmi:mapped:core:Thing;1", "dtmi:com:willowinc:Asset;1"),
        ("dtmi:mapped:core:Device;1", "dtmi:com:willowinc:Equipment;1"),
        ("dtmi:org:brickschema:schema:Brick:Sensor;1", "dtmi:com:willowinc:Sensor;1"),
        ("dtmi:org:brickschema:schema:Brick:HVAC_Equipment;1", "dtmi:com:willowinc:HVACEquipment;1"),
        ("dtmi:org:brickschema:schema:Brick:Space;1", "dtmi:com:willowinc:Space;1"),
        ("dtmi:org:brickschema:schema:Brick:Security_Equipment;1", "dtmi:com:willowinc:SecurityEquipment;1"),
        ("dtmi:org:brickschema:schema:Brick:Parameter;1", "dtmi:com:willowinc:Parameter;1"),
        ("dtmi:org:brickschema:schema:Brick:Parking_Space;1", "dtmi:com:willowinc:ParkingSpot;1"),
        ("dtmi:org:brickschema:schema:Brick:Pressure_Sensor;1", "dtmi:com:willowinc:PressureSensor;1"),
    };

}
