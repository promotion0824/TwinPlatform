import { FaCreativeCommonsNd, FaFan, FaFire, FaQuestionCircle, FaRegLightbulb, FaRegObjectGroup, FaRoad } from 'react-icons/fa';
import { BiPowerOff, BiWater, BiWifi2 } from 'react-icons/bi';
import { CgBowl, CgSmartHomeHeat } from 'react-icons/cg';
import { MdGrid4X4, MdOutlineGarage, MdOutlineSensors, MdSpaceBar } from 'react-icons/md';
import { IoDiscOutline, IoThermometer, IoSettings } from 'react-icons/io5';
import { GiBoatPropeller, GiElectric, GiIBeam, GiServerRack, GiSteamBlast, GiValve, GiVerticalFlip, GiWaterTank, GiWireCoil } from 'react-icons/gi';
import { RiDeleteBinLine } from 'react-icons/ri';
import { IconPropsShort } from './IconForModel'
import { TiDocument } from 'react-icons/ti';
import { BsFillStopFill, BsSlashCircle, BsSpeedometer2, BsSun } from 'react-icons/bs';
import { HiOutlineOfficeBuilding } from 'react-icons/hi';
import { VscDebugStart, VscSettings } from 'react-icons/vsc';
import { AiOutlineSecurityScan } from 'react-icons/ai';

const IconForModelS = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "SafetyLighting": return (<FaRegLightbulb size={size} />);
    case "SecurityEquipment": return (<AiOutlineSecurityScan size={size} />);
    case "Sensor": return (<MdOutlineSensors size={size} />);
    case "SensorEquipment": return (<GiServerRack size={size} />);
    case "ServiceContract": return (<TiDocument size={size} />);
    case "Setpoint": return (<MdOutlineSensors size={size} />);
    case "Shower": return (<BiWater size={size} />);
    case "ShowerHead": return (<BiWater size={size} />);
    case "SignalStrengthSensor": return (<BiWifi2 size={size} />);
    case "Sink": return (<CgBowl size={size} />);
    case "Slab": return (<FaRoad size={size} />);
    case "SmokeDamper": return (<BsSlashCircle size={size} />);
    case "SmokeDetector": return (<BsSlashCircle size={size} />);
    case "SolarIrradianceSensor": return (<BsSun size={size} />);
    case "SolarInverter": return (<BsSun size={size} />);
    case "SoundPressureLevelSensor": return (<FaCreativeCommonsNd size={size} />);
    case "Space": return (<MdSpaceBar size={size} />);
    case "SpeedSensor": return (<BsSpeedometer2 size={size} />);
    case "Specification": return (<TiDocument size={size} />);
    case "SprinklerBackflowPreventer": return (<GiValve size={size} />);
    case "SprinklerBalancingValve": return (<GiValve size={size} />);
    case "SprinklerCheckValve": return (<GiValve size={size} />);
    case "SprinklerPressureReducingValve": return (<GiValve size={size} />);
    case "SprinklerSolenoidValve": return (<GiValve size={size} />);
    case "SprinklerGlobeValve": return (<GiValve size={size} />);
    case "SprinklerShutOffValve": return (<GiValve size={size} />);
    case "SprinklerHeatTracing": return (<GiWireCoil size={size} />);
    case "SprinklerPressureReducingStation": return (<GiWireCoil size={size} />);
    case "SprinklerSystem": return (<FaFire size={size} />);
    case "SprinklerEquipment": return (<FaFire size={size} />);
    case "SprinklerTank": return (<GiWaterTank size={size} />);
    case "SprinklerValve": return (<GiValve size={size} />);
    case "StairPressurizationFan": return (<FaFan size={size} />);
    case "StandbyHeatingSetpoint": return (<IoThermometer size={size} />);
    case "StartStopSensor": return (<BiPowerOff size={size} />);
    case "StartLevelActuator": return (<BiPowerOff size={size} />);
    case "StartLevelState": return (<BiPowerOff size={size} />);
    case "StaticPressureSensor": return (<FaCreativeCommonsNd size={size} />);
    case "StaticPressureSetpoint": return (<VscSettings size={size} />);
    case "StateSensor": return (<BiPowerOff size={size} />);
    case "StateSetpoint": return (<BiPowerOff size={size} />);
    case "StatusSensor": return (<BiPowerOff size={size} />);
    case "SteamCondensatePump": return (<GiBoatPropeller size={size} />);
    case "SteamSystem": return (<FaRegObjectGroup size={size} />);
    case "SteamUnitHeater": return (<CgSmartHomeHeat size={size} />);
    case "SteamMassFlowSensor": return (<GiSteamBlast size={size} />);
    case "SteamMassSensor": return (<GiSteamBlast size={size} />);
    case "StormDrainageSystem": return (<IoDiscOutline size={size} />);
    case "StopActuator": return (<BsFillStopFill size={size} />);
    case "StopSensor": return (<BsFillStopFill size={size} />);
    case "StartActuator": return (<VscDebugStart size={size} />);
    case "StorageSystem": return (<RiDeleteBinLine size={size} />);
    case "StorageEquipment": return (<RiDeleteBinLine size={size} />);
    case "Structure": return (<HiOutlineOfficeBuilding size={size} />);
    case "StructuralColumn": return (<GiVerticalFlip size={size} />);
    case "StructuralBeam": return (<GiIBeam size={size} />);
    case "StructuralBuildingComponent": return (<GiIBeam size={size} />);
    case "SubBuilding": return (<HiOutlineOfficeBuilding size={size} />);
    case "Substructure": return (<MdOutlineGarage size={size} />);
    case "SumpPump": return (<GiBoatPropeller size={size} />);
    case "SupplyFan": return (<FaFan size={size} />);
    case "SupplyAirRegister": return (<MdGrid4X4 size={size} />);
    case "SupplyAirGrille": return (<MdGrid4X4 size={size} />);
    case "SupplyAirDiffuser": return (<MdGrid4X4 size={size} />);
    case "Switchboard": return (<GiElectric size={size} />);
    case "Switchgear": return (<GiElectric size={size} />);
    case "System": return (<RiDeleteBinLine size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelS;
