import { FaFan, FaQuestionCircle, FaRegObjectGroup, FaStreetView, FaTemperatureHigh, FaTemperatureLow } from 'react-icons/fa';
import { BiBuilding, BiDoorOpen } from 'react-icons/bi';
import { CgToggleOff } from 'react-icons/cg';
import { BsFillPersonCheckFill, BsSlashCircleFill } from 'react-icons/bs';
import { IoThermometer } from 'react-icons/io5';
import { WiHumidity, WiWindBeaufort0 } from 'react-icons/wi';
import { GiChickenOven, GiLevelEndFlag, GiManualMeatGrinder } from 'react-icons/gi';
import { IconPropsShort } from './IconForModel'
import { MdSpaceBar } from 'react-icons/md';
import { RiLightbulbLine } from 'react-icons/ri';
import { FcOvertime } from 'react-icons/fc';

const IconForModelO = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "OccupancySensor": return (<FaStreetView size={size} />);
    case "OccupiedState": return (<FaStreetView size={size} />);
    case "OccupancySensorEquipment": return (<FaStreetView size={size} />);
    case "OccupancySetpoint": return (<FaStreetView size={size} />);
    case "OccupancyZone": return (<BiBuilding size={size} />);
    case "OccupiedActuator": return (<BsFillPersonCheckFill size={size} />);
    case "OccupiedCoolingSetpoint": return (<IoThermometer size={size} />);
    case "OccupiedHeatingSetpoint": return (<FaTemperatureLow size={size} />);
    case "OccupiedHeatingZoneAirTemperatureSetpoint": return (<FaTemperatureHigh size={size} />);
    case "OccupiedCoolingZoneAirTemperatureSetpoint": return (<FaTemperatureLow size={size} />);
    case "OccupiedModeActuator": return (<FaStreetView size={size} />);
    case "OmniDirectionalApproachLightFixture": return (<RiLightbulbLine size={size} />);
    case "OnLevelActuator": return (<GiLevelEndFlag size={size} />);
    case "OnLevelSensor": return (<GiLevelEndFlag size={size} />);
    case "OnLevelState": return (<GiLevelEndFlag size={size} />);
    case "OnSensor": return (<CgToggleOff size={size} />);
    case "OnOffActuator": return (<CgToggleOff size={size} />);
    case "OnOffSensor": return (<CgToggleOff size={size} />);
    case "OnOffState": return (<CgToggleOff size={size} />);
    case "OreProcessingSystem": return (<GiManualMeatGrinder size={size} />);
    case "OpenActuator": return (<BiDoorOpen size={size} />);
    case "OpenCloseActuator": return (<BiDoorOpen size={size} />);
    case "OpenSensor": return (<BiDoorOpen size={size} />);
    case "OpenState": return (<BiDoorOpen size={size} />);
    case "OpenCloseSensor": return (<BiDoorOpen size={size} />);
    case "OpenCloseState": return (<BiDoorOpen size={size} />);
    case "Organization": return (<FaRegObjectGroup size={size} />);
    case "OutdoorArea": return (<MdSpaceBar size={size} />);
    case "OutsideAirDamperPositionActuator": return (<BsSlashCircleFill size={size} />);
    case "OutsideAirDamperPositionSensor": return (<BsSlashCircleFill size={size} />);
    case "OutsideAirEnthalpySensor": return (<WiWindBeaufort0 size={size} />);
    case "OutsideAirFan": return (<FaFan size={size} />);
    case "OutsideAirFlowSensor": return (<WiWindBeaufort0 size={size} />);
    case "OutsideAirFlowSetpoint": return (<WiWindBeaufort0 size={size} />);
    case "OutsideAirHumiditySensor": return (<WiHumidity size={size} />);
    case "OutsideAirTemperatureSensor": return (<FaTemperatureLow size={size} />);
    case "OutsideAirTemperatureSetpoint": return (<FaTemperatureLow size={size} />);
    case "OutsideAirTemperatureCoolingLockoutSetpoint": return (<FaTemperatureLow size={size} />);
    case "OutsideFanRunActuator": return (<FaFan size={size} />);
    case "Oven": return (<GiChickenOven size={size} />);
    case "OvertimeScheduleOccupiedState": return (<FcOvertime size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelO;
