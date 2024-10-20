import { FaFan, FaQuestionCircle, FaTemperatureHigh, FaTemperatureLow, FaThermometerFull, FaToilet } from 'react-icons/fa';
import { BiBuilding, BiCylinder, BiRotateLeft } from 'react-icons/bi';
import { VscDebugDisconnect } from 'react-icons/vsc';
import { MdOutlineIndeterminateCheckBox, MdTransform } from 'react-icons/md';
import { GiChemicalTank, GiElectric, GiHeatHaze, GiPoisonCloud, GiRoad, GiTable, GiTowel, GiTurnstile } from 'react-icons/gi';
import { FiClock, FiCrosshair } from 'react-icons/fi';
import { IconPropsShort } from './IconForModel'
import { TiDocument } from 'react-icons/ti';
import { FcElectricalSensor } from 'react-icons/fc';
import { BsBoxArrowDownRight, BsBoxArrowInDownRight } from 'react-icons/bs';

const IconForModelT = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "Table": return (<GiTable size={size} />);
    case "Tank": return (<GiChemicalTank size={size} />);
    case "TanklessWaterHeater": return (<MdOutlineIndeterminateCheckBox size={size} />);
    case "TankWaterHeater": return (<BiCylinder size={size} />);
    case "TemperatureSensor": return (<FaTemperatureHigh size={size} />);
    case "TemperatureSetpoint": return (<FaTemperatureLow size={size} />);
    case "TenantUnit": return (<BiBuilding size={size} />);
    case "TerminalUnit": return (<FiCrosshair size={size} />);
    case "TestReport": return (<TiDocument size={size} />);
    case "ThermalEnergySensor": return (<GiElectric size={size} />);
    case "ThermalPowerSensor": return (<GiElectric size={size} />);
    case "ThermalMeter": return (<GiHeatHaze size={size} />);
    case "ThermostatEquipment": return (<FaThermometerFull size={size} />);
    case "TimeSensor": return (<FiClock size={size} />);
    case "TimeSetpoint": return (<FiClock size={size} />);
    case "Toilet": return (<FaToilet size={size} />);
    case "ToiletAccessory": return (<FaToilet size={size} />);
    case "ToiletPartition": return (<FaToilet size={size} />);
    case "ToiletPaperDispenser": return (<FaToilet size={size} />);
    case "ToiletSeactCoverDispenser": return (<FaToilet size={size} />);
    case "ToiletTank": return (<FaToilet size={size} />);
    case "ToiletFlushometer": return (<FaToilet size={size} />);
    case "TorqueSensor": return (<BiRotateLeft size={size} />);
    case "TotalImportReactiveElectricalEnergySensor": return (<GiElectric size={size} />);
    case "TotalActiveElectricalPowerSensor": return (<FcElectricalSensor size={size} />);
    case "TotalEnteringPeopleCountSensor": return (<BsBoxArrowInDownRight size={size} />);
    case "TotalLeavingPeopleCountSensor": return (<BsBoxArrowDownRight size={size} />);
    case "TotalNetActiveElectricalEnergySensor": return (<GiElectric size={size} />);
    case "TotalNetActiveElectricalPowerSensor": return (<GiElectric size={size} />);
    case "TotalImportActiveElectricalPowerSensor": return (<GiElectric size={size} />);
    case "TowelBar": return (<GiTowel size={size} />);
    case "TowelRack": return (<GiTowel size={size} />);
    case "TowelStorageAsset": return (<GiTowel size={size} />);
    case "TowelRing": return (<GiTowel size={size} />);
    case "TransferFan": return (<FaFan size={size} />);
    case "TransferSwitch": return (<VscDebugDisconnect size={size} />);
    case "Transformer": return (<MdTransform size={size} />);
    case "Turnstile": return (<GiTurnstile size={size} />);
    case "Tunnel": return (<GiRoad size={size} />);
    case "TVOCSensor": return (<GiPoisonCloud size={size} />);
    case "TVOCAirQualitySensor": return (<GiPoisonCloud size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelT;
