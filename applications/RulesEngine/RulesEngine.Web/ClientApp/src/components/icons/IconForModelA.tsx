import { FaAudioDescription, FaBox, FaCompressAlt, FaCreativeCommonsNd, FaQuestionCircle, FaRegObjectGroup, FaRoad, FaTemperatureHigh, FaTemperatureLow } from 'react-icons/fa';
import { BiAlarm, BiArea, BiBarcodeReader } from 'react-icons/bi';
import { VscDebugDisconnect } from 'react-icons/vsc';
import { FcElectricalSensor } from 'react-icons/fc/';
import { HiOutlineMinusCircle, HiOutlineOfficeBuilding } from 'react-icons/hi';
import { IoColorFilterOutline, IoSnowSharp, IoTrailSignOutline } from 'react-icons/io5';
import { WiHumidity, WiWindBeaufort0, WiWindBeaufort1 } from 'react-icons/wi';
import { Gi3DStairs, GiAirplaneArrival, GiAirplaneDeparture, GiClockwiseRotation, GiDoorHandle, GiElectric, GiIBeam, GiTheaterCurtains } from 'react-icons/gi';
import { RiDeleteBinLine, RiLightbulbLine, RiProjector2Line, RiRefreshFill } from 'react-icons/ri';
import { IconPropsShort } from './IconForModel'
import { TiDocument } from 'react-icons/ti';
import { CgDisplayFullwidth } from 'react-icons/cg';
import { FiServer } from 'react-icons/fi';
import { BsHddRack, BsPercent } from 'react-icons/bs';
import { MdGrid4X4, MdPower, MdTextRotationAngledown } from 'react-icons/md';

const IconForModelA = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "AbsentState": return (<HiOutlineMinusCircle size={size} />);
    case "AbsoluteHumiditySensor": return (<WiHumidity size={size} />);
    case "AbsoluteHumiditySetpoint": return (<WiHumidity size={size} />);
    case "ACElectricalSystem": return (<FaRegObjectGroup size={size} />);
    case "AccessControlEquipment": return (<GiDoorHandle size={size} />);
    case "AccessReader": return (<BiBarcodeReader size={size} />);
    case "AccessControlPanel": return (<BiBarcodeReader size={size} />);
    case "AccessPanel": return (<BsHddRack size={size} />);
    case "ActiveElectricalPowerSensor": return (<GiElectric size={size} />);
    case "ActiveElectricalEnergySensor": return (<GiElectric size={size} />);
    case "Actuator": return (<BsPercent size={size} />);
    case "ActiveChilledBeam": return (<GiIBeam size={size} />);
    case "AirCooledRefrigerationCondenser": return (<IoSnowSharp size={size} />);
    case "AirCO2Sensor": return (<RiRefreshFill size={size} />);
    case "AirCO2Setpoint": return (<RiRefreshFill size={size} />);
    case "AirCompressor": return (<FaCompressAlt size={size} />);
    case "AirCompressorDryer": return (<FaCompressAlt size={size} />);
    case "AircraftBoardingChair": return (<Gi3DStairs size={size} />);
    case "AircraftBoardingEquipment": return (<Gi3DStairs size={size} />);
    case "AirfieldLightingEquipment": return (<RiLightbulbLine size={size} />);
    case "AirfieldPavement": return (<FaRoad size={size} />);
    case "AirfieldSignageEquipment": return (<IoTrailSignOutline size={size} />);
    case "AirCurtain": return (<GiTheaterCurtains size={size} />);
    case "AirFilter": return (<IoColorFilterOutline size={size} />);
    case "AirFlowSensor": return (<WiWindBeaufort0 size={size} />);
    case "AirFlowSetpoint": return (<WiWindBeaufort1 size={size} />);
    case "AirHandlingUnit": return (<FaBox size={size} />);
    case "AirHumiditySensor": return (<WiHumidity size={size} />);
    case "AirHumiditySetpoint": return (<WiHumidity size={size} />);
    case "AirInletsOutlets": return (<MdGrid4X4 size={size} />);
    case "AirDiffuser": return (<MdGrid4X4 size={size} />);
    case "AirRegister": return (<MdGrid4X4 size={size} />);
    case "AirGrille": return (<MdGrid4X4 size={size} />);
    case "Airport": return (<GiAirplaneDeparture size={size} />);
    case "AirportAsset": return (<GiAirplaneDeparture size={size} />);
    case "AirportEquipment": return (<GiAirplaneDeparture size={size} />);
    case "AirportICTEquipment": return (<FiServer size={size} />);
    case "AirportAudioVisualEquipment": return (<RiProjector2Line size={size} />);
    case "AirportDisplay": return (<CgDisplayFullwidth size={size} />);
    case "AirportStructure": return (<HiOutlineOfficeBuilding size={size} />);
    case "AudioVisualEquipment": return (<RiProjector2Line size={size} />);
    case "AirportTerminal": return (<GiAirplaneArrival size={size} />);
    case "AirPressureSensor": return (<FaCreativeCommonsNd size={size} />);
    case "AirPressureSetpoint": return (<FaCreativeCommonsNd size={size} />);
    case "AirTemperatureSensor": return (<FaTemperatureHigh size={size} />);
    case "AirDeltaPressureSensor": return (<FaCreativeCommonsNd size={size} />);
    case "AirStaticPressureSensor": return (<FaCreativeCommonsNd size={size} />);
    case "AirStaticPressureSetpoint": return (<FaCreativeCommonsNd size={size} />);
    case "AirTemperatureSetpoint": return (<FaTemperatureLow size={size} />);
    case "AlarmSensor": return (<BiAlarm size={size} />);
    case "AlarmState": return (<BiAlarm size={size} />);
    case "AngleSensor": return (<MdTextRotationAngledown size={size} />);
    case "AngleSetpoint": return (<MdTextRotationAngledown size={size} />);
    case "AngularVelocitySensor": return (<GiClockwiseRotation size={size} />);
    case "AngularVelocitySetpoint": return (<GiClockwiseRotation size={size} />);
    case "ApparentElectricalEnergySensor": return (<MdPower size={size} />);
    case "ApparentElectricalPowerSensor": return (<MdPower size={size} />);
    case "ApproachLightFixture": return (<RiLightbulbLine size={size} />);
    case "Agent": return (<FaRegObjectGroup size={size} />);
    case "AreaSensor": return (<BiArea size={size} />);
    case "ArchitecturalAsset": return (<HiOutlineOfficeBuilding size={size} />);
    case "AsBuiltDrawing": return (<TiDocument size={size} />);
    case "Asset": return (<FaRegObjectGroup size={size} />);
    case "AssetComponent": return (<FaRegObjectGroup size={size} />);
    case "AssetCollection": return (<RiDeleteBinLine size={size} />);
    case "AudioAmplifier": return (<FaAudioDescription size={size} />);
    case "AutomaticTransferSwitch": return (<VscDebugDisconnect size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelA;
