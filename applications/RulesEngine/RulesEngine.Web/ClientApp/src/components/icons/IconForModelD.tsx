import { FaBox, FaCreativeCommonsNd, FaFan, FaQuestionCircle, FaRegObjectGroup, FaTemperatureHigh, FaTemperatureLow } from 'react-icons/fa';
import { VscDebugDisconnect, VscRunAbove } from 'react-icons/vsc';
import { HiOutlineServer } from 'react-icons/hi';
import { BsArrowBarDown, BsDoorClosed, BsDownload, BsSlashCircle, BsSlashCircleFill } from 'react-icons/bs';
import { IoThermometer, IoWaterOutline } from 'react-icons/io5';
import { WiHumidity, WiWindBeaufort0, WiWindBeaufort1 } from 'react-icons/wi';
import { GiBoatPropeller, GiChemicalTank, GiDew, GiDuration, GiOrganigram, GiPipes, GiRecycle, GiSilence, GiStraightPipe, GiWashingMachine, GiWaterDrop } from 'react-icons/gi';
import { TiDocument } from 'react-icons/ti';
import { IconPropsShort } from './IconForModel'
import { BiCabinet, BiWater } from 'react-icons/bi';

const IconForModelD = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "DamperPositionSensor": return (<BsSlashCircleFill size={size} />);
    case "DamperPositionSetpoint": return (<BsSlashCircle size={size} />);
    case "DamperActuator": return (<BsSlashCircle size={size} />);
    case "DamperPositionActuator": return (<BsSlashCircleFill size={size} />);
    case "DamperStatusSensor": return (<BsSlashCircle size={size} />);
    case "DamperOpenActuator": return (<BsSlashCircle size={size} />);
    case "DamperOpenSensor": return (<BsSlashCircle size={size} />);
    case "DataNetworkEquipment": return (<HiOutlineServer size={size} />);
    case "DCElectricalSystem": return (<FaRegObjectGroup size={size} />);
    case "DedicatedOutdoorAirSystem": return (<FaBox size={size} />);
    case "DefrostRunActuator": return (<VscRunAbove size={size} />);
    case "DefrostRunState": return (<VscRunAbove size={size} />);
    case "DeltaPressureSensor": return (<FaCreativeCommonsNd size={size} />);
    case "DeltaAirPressureSensor": return (<FaCreativeCommonsNd size={size} />);
    case "DeltaChilledWaterTemperatureSensor": return (<IoThermometer size={size} />);
    case "DeltaWaterPressureSensor": return (<FaCreativeCommonsNd size={size} />);
    case "DeltaWaterPressureSetpoint": return (<FaCreativeCommonsNd size={size} />);
    case "DeltaAirPressureSetpoint": return (<FaCreativeCommonsNd size={size} />);
    case "Department": return (<GiOrganigram size={size} />);
    case "DesignDrawing": return (<TiDocument size={size} />);
    case "DewPointTemperatureSensor": return (<GiDew size={size} />);
    case "DilutionTank": return (<GiChemicalTank size={size} />);
    case "Dishwasher": return (<GiWashingMachine size={size} />);
    case "DishwashingEquipment": return (<GiWashingMachine size={size} />);
    case "DischargeAirDamperPositionSensor": return (<BsSlashCircle size={size} />);
    case "DischargeAirDamperPositionActuator": return (<BsSlashCircleFill size={size} />);
    case "DischargeAirFlowSensor": return (<WiWindBeaufort0 size={size} />);
    case "DischargeAirFlowSetpoint": return (<WiWindBeaufort1 size={size} />);
    case "DischargeAirHumiditySensor": return (<WiHumidity size={size} />);
    case "DischargeAirStaticPressureSensor": return (<BsDownload size={size} />);
    case "DischargeAirHumiditySetpoint": return (<WiHumidity size={size} />);
    case "DischargeAirStaticPressureSetpoint": return (<BsDownload size={size} />);
    case "DischargeAirTemperatureSensor": return (<FaTemperatureHigh size={size} />);
    case "DischargeAirTemperatureSetpoint": return (<FaTemperatureLow size={size} />);
    case "DischargeAirDeltaPressureSensor": return (<FaCreativeCommonsNd size={size} />);
    case "DischargeFanRunState": return (<FaFan size={size} />);
    case "DischargeFanRunSensor": return (<FaFan size={size} />);
    case "DischargeFanRunActuator": return (<FaFan size={size} />);
    case "DischargeFanRunLevelState": return (<FaFan size={size} />);
    case "DischargeFanVFDRunLevelActuator": return (<FaFan size={size} />);
    case "DischargeFanVFDRunLevelState": return (<FaFan size={size} />);
    case "DisconnectSwitch": return (<VscDebugDisconnect size={size} />);
    case "DishwasherRunState": return (<VscDebugDisconnect size={size} />);
    case "DistributionAsset": return (<GiPipes size={size} />);
    case "DistributionConnector": return (<GiPipes size={size} />);
    case "Document": return (<TiDocument size={size} />);
    case "DomesticColdWaterSystem": return (<IoWaterOutline size={size} />);
    case "DomesticHotWaterRecircSystem": return (<GiRecycle size={size} />);
    case "DomesticHotWaterCirculatingPump": return (<BiWater size={size} />);
    case "DomesticHotWaterSystem": return (<IoWaterOutline size={size} />);
    case "DomesticNonPotableWaterSystem": return (<GiWaterDrop size={size} />);
    case "DomesticWaterPump": return (<GiBoatPropeller size={size} />);
    case "DomesticWaterSystem": return (<GiWaterDrop size={size} />);
    case "Door": return (<BsDoorClosed size={size} />);
    case "DoorHardware": return (<BsDoorClosed size={size} />);
    case "Drain": return (<BsArrowBarDown size={size} />);
    case "DrainCleanout": return (<BsArrowBarDown size={size} />);
    case "DrainageSystem": return (<BsArrowBarDown size={size} />);
    case "Drawing": return (<TiDocument size={size} />);
    case "DrinkingFountain": return (<BiWater size={size} />);
    case "DryFoodDisplayCase": return (<BiCabinet size={size} />);
    case "DuctConnection": return (<GiStraightPipe size={size} />);
    case "DuctSilencer": return (<GiSilence size={size} />);
    case "DurationSensor": return (<GiDuration size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelD;
