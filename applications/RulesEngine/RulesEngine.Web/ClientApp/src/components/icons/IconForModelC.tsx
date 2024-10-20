import { FaBox, FaCompressAlt, FaFan, FaQuestionCircle, FaRegCopyright, FaRegObjectGroup } from 'react-icons/fa';
import { CgSleep } from 'react-icons/cg';
import { BsBoxArrowUpRight } from 'react-icons/bs';
import { IoLogoCapacitor, IoSnowSharp, IoThermometer } from 'react-icons/io5';
import { GiBelt, GiBoatPropeller, GiBrainFreeze, GiChickenOven, GiConcentricCrescents, GiElectric, GiHeatHaze, GiIBeam, GiOfficeChair, GiPoisonGas, GiPowerGenerator, GiServerRack, GiStraightPipe, GiValve, GiWatchtower, GiWeightCrush } from 'react-icons/gi';
import { RiRefreshFill } from 'react-icons/ri';
import { IconPropsShort } from './IconForModel'
import { TiDocument } from 'react-icons/ti';
import { BiAddToQueue, BiDoorOpen } from 'react-icons/bi';
import { MdOutlineSensors, MdRoofing } from 'react-icons/md';

const IconForModelC = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "Capability": return (<MdOutlineSensors size={size} />);
    case "CapacitySensor": return (<IoLogoCapacitor size={size} />);
    case "Ceiling": return (<MdRoofing size={size} />);
    case "CeilingFan": return (<FaFan size={size} />);
    case "CAVBox": return (<BsBoxArrowUpRight size={size} />);
    case "Chair": return (<GiOfficeChair size={size} />);
    case "ChilledBeam": return (<GiIBeam size={size} />);
    case "ChilledWaterFlowSensor": return (<GiValve size={size} />);
    case "ChilledWaterValvePositionActuator": return (<GiValve size={size} />);
    case "ChilledWaterTemperatureSensor": return (<IoThermometer size={size} />);
    case "ChilledWaterTemperatureSetpoint": return (<IoThermometer size={size} />);
    case "Chiller": return (<IoSnowSharp size={size} />);
    case "ChillerGroup": return (<IoSnowSharp size={size} />);
    case "ChilledWaterPlant": return (<IoSnowSharp size={size} />);
    case "ChilledWaterValvePositionSensor": return (<GiValve size={size} />);
    case "CloseSensor": return (<BiDoorOpen size={size} />);
    case "CondensatePump": return (<GiBoatPropeller size={size} />);
    case "ContactSensor": return (<BiDoorOpen size={size} />);
    case "CoolingDegreeDays": return (<GiHeatHaze size={size} />);
    case "CoolingTowerGroup": return (<IoSnowSharp size={size} />);
    case "CoolingZoneAirTemperatureSetpoint": return (<GiHeatHaze size={size} />);
    case "CoolingAirVolumeFlowSetpointLowLimit": return (<GiHeatHaze size={size} />);
    case "CoolingAirVolumeFlowSetpointHighLimit": return (<GiHeatHaze size={size} />);
    case "CO2AirQualitySensor": return (<RiRefreshFill size={size} />);
    case "CO2Sensor": return (<RiRefreshFill size={size} />);
    case "CO2Setpoint": return (<RiRefreshFill size={size} />);
    case "COAirQualitySensor": return (<GiPoisonGas size={size} />);
    case "Cogenerator": return (<GiPowerGenerator size={size} />);
    case "Collection": return (<FaRegCopyright size={size} />);
    case "ComputerRoomAirHandlingUnit": return (<FaBox size={size} />);
    case "Component": return (<FaRegObjectGroup size={size} />);
    case "CompressedAir": return (<FaCompressAlt size={size} />);
    case "Company": return (<FaRegCopyright size={size} />);
    case "CompressorRunActuator": return (<GiBoatPropeller size={size} />);
    case "CompressorRunSensor": return (<GiBoatPropeller size={size} />);
    case "ComputerRoomAirConditioningUnit": return (<IoSnowSharp size={size} />);
    case "ContractDocument": return (<TiDocument size={size} />);
    case "Conduit": return (<GiStraightPipe size={size} />);
    case "CondensingUnit": return (<CgSleep size={size} />);
    case "Controller": return (<GiServerRack size={size} />);
    case "ConcentrationSystem": return (<GiConcentricCrescents size={size} />);
    case "ConveyanceEquipment": return (<GiBelt size={size} />);
    case "ConveyanceEquipmentGroup": return (<GiBelt size={size} />);
    case "CookingEquipment": return (<GiChickenOven size={size} />);
    case "CoolingActuator": return (<GiBrainFreeze size={size} />);
    case "CoolingLevelActuator": return (<GiBrainFreeze size={size} />);
    case "CoolingLevelSensor": return (<GiBrainFreeze size={size} />);
    case "CoolingTower": return (<GiWatchtower size={size} />);
    case "CountSensor": return (<BiAddToQueue size={size} />);
    case "CurrentSensor": return (<GiElectric size={size} />);
    case "Crusher": return (<GiWeightCrush size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelC;
