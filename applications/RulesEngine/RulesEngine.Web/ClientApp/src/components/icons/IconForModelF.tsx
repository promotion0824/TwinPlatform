import { FaFan, FaFire, FaFireExtinguisher, FaGripfire, FaQuestionCircle, FaToilet } from 'react-icons/fa';
import { BiCabinet, BiError, BiWater } from 'react-icons/bi';
import { CgDisplayFullwidth, CgPushChevronRightO, CgSmartHomeHeat } from 'react-icons/cg';
import { SiCommonworkflowlanguage } from 'react-icons/si';
import { BsBoxArrowUpRight, BsSlashCircle } from 'react-icons/bs';
import { IoDiscOutline, IoFastFoodOutline, IoSnowSharp } from 'react-icons/io5';
import { WiWindBeaufort0 } from 'react-icons/wi';
import { GiComputerFan, GiElectric, GiLightningFrequency, GiOilPump, GiSnowflake1, GiStoneWall } from 'react-icons/gi';
import { TiFlowSwitch } from 'react-icons/ti';
import { RiBuilding2Fill } from 'react-icons/ri';
import { IconPropsShort } from './IconForModel'
import { IoIosColorFilter } from 'react-icons/io';

const IconForModelF = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "Facade": return (<GiStoneWall size={size} />);
    case "Fan": return (<FaFan size={size} />);
    case "FanActuator": return (<GiComputerFan size={size} />);
    case "FanCoilUnit": return (<IoSnowSharp size={size} />);
    case "FanCoilUnitReheat": return (<CgSmartHomeHeat size={size} />);
    case "FanCurrentSensor": return (<GiElectric size={size} />);
    case "FanPoweredBox": return (<BsBoxArrowUpRight size={size} />);
    case "FanPoweredBoxReheat": return (<BsBoxArrowUpRight size={size} />);
    case "FanVFDRunLevelActuator": return (<BsBoxArrowUpRight size={size} />);
    case "FanRunActuator": return (<FaFan size={size} />); // DFW uses these
    case "FanRunState": return (<GiComputerFan size={size} />); // ??
    case "FanRunSensor": return (<GiComputerFan size={size} />);
    case "FanRunLevelActuator": return (<GiComputerFan size={size} />); // ??
    case "FanRunLevelSetpoint": return (<GiComputerFan size={size} />); // ??
    case "FanRunLevelState": return (<GiComputerFan size={size} />); // ??
    case "FanRunLevelSensor": return (<WiWindBeaufort0 size={size} />); // DFW uses these (%)
    case "FanSpeedActuator": return (<FaFan size={size} />);
    case "FanStatusSensor": return (<GiComputerFan size={size} />);
    case "Faucet": return (<BiWater size={size} />);
    case "FaultResetActuator": return (<TiFlowSwitch size={size} />);
    case "FaultSensor": return (<BiError size={size} />);
    case "FaultState": return (<BiError size={size} />);
    case "FilterAirDeltaPressureSensor": return (<IoIosColorFilter size={size} />);
    case "FireAlarmControlPanel": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmEquipment": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmFlowSwitch": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmNotificationAppliance": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmSpeaker": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmSpeakerStrobe": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmBell": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmChime": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmChimeStrobe": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmHorn": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmHornStrobe": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmStrobe": return (<FaFireExtinguisher size={size} />);
    case "FireAlarmPullStation": return (<FaFireExtinguisher size={size} />);
    case "FireDamper": return (<BsSlashCircle size={size} />);
    case "FireExtinguisher": return (<FaFireExtinguisher size={size} />);
    case "FireExtinguisherRemovedSensor": return (<FaFireExtinguisher size={size} />);
    case "FireHydrant": return (<FaFireExtinguisher size={size} />);
    case "FireExtinguisherTroubleSensor": return (<FaFireExtinguisher size={size} />);
    case "FireProtectionEquipment": return (<FaFireExtinguisher size={size} />);
    case "FireProtectionEquipmentGroup": return (<FaGripfire size={size} />);
    case "FireAlarmSystem": return (<FaGripfire size={size} />);
    case "FireAlarmInitiatingDevice": return (<FaGripfire size={size} />);
    case "FireDetector": return (<FaGripfire size={size} />);
    case "FireProtectionSystem": return (<FaGripfire size={size} />);
    case "FireSuppressionSystem": return (<FaGripfire size={size} />);
    case "FirePump": return (<CgPushChevronRightO size={size} />);
    case "FireSmokeDamper": return (<BsSlashCircle size={size} />);
    case "FireSprinklerHead": return (<FaFire size={size} />);
    case "FireSuppressionEquipment": return (<FaFire size={size} />);
    case "FlightInformationDisplay": return (<CgDisplayFullwidth size={size} />);
    case "Floor": return (<RiBuilding2Fill size={size} />);
    case "FloorCommonArea": return (<RiBuilding2Fill size={size} />);
    case "FloorDrain": return (<IoDiscOutline size={size} />);
    case "FlowSensor": return (<SiCommonworkflowlanguage size={size} />);
    case "FlushometerValve": return (<FaToilet size={size} />);
    case "FoodStorageEquipment": return (<IoFastFoodOutline size={size} />);
    case "FoodPreparationEquipment": return (<IoFastFoodOutline size={size} />);
    case "FoodDisplayCase": return (<BiCabinet size={size} />);
    case "FoodStorageRack": return (<IoFastFoodOutline size={size} />);
    case "FrequencySensor": return (<GiLightningFrequency size={size} />);
    case "FuelOilSystem": return (<GiOilPump size={size} />);
    case "Furniture": return (<BiCabinet size={size} />);
    case "Freezer": return (<GiSnowflake1 size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelF;
