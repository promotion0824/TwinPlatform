import { FaBox, FaFan, FaQuestionCircle, FaRuler, FaTemperatureHigh } from 'react-icons/fa';
import { BiAlignMiddle, BiBuilding, BiCabinet } from 'react-icons/bi';
import { BsArrowBarDown, BsBatteryHalf, BsBoxArrowUpRight, BsDownload, BsSlashCircleFill } from 'react-icons/bs';
import { IoExitOutline, IoSnowSharp } from 'react-icons/io5';
import { TiFlowSwitch } from 'react-icons/ti';
import { VscDebugStart, VscGroupByRefType, VscRunAbove } from 'react-icons/vsc';
import { IconPropsShort } from './IconForModel'
import { GiBoatPropeller, GiCheckboxTree, GiElectric } from 'react-icons/gi';
import { HiOutlineOfficeBuilding } from 'react-icons/hi';
import { MdAddRoad, MdGrid4X4 } from 'react-icons/md';
import { WiHumidity } from 'react-icons/wi';

const IconForModelR = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "RadiantHeatingManifold": return (<GiCheckboxTree size={size} />);
    case "ReactiveElectricalEnergySensor": return (<GiElectric size={size} />);
    case "ReactiveElectricalPowerSensor": return (<GiElectric size={size} />);
    case "ReplaceBatteryState": return (<BsBatteryHalf size={size} />);
    case "RefrigerationEquipment": return (<IoSnowSharp size={size} />);
    case "RefrigeratedFoodDisplayCase": return (<BiCabinet size={size} />);
    case "RefrigerationCompressorGroup": return (<VscGroupByRefType size={size} />);
    case "RefrigerationCircuit": return (<VscGroupByRefType size={size} />);
    case "RefrigerationCondenser": return (<GiBoatPropeller size={size} />);
    case "Region": return (<HiOutlineOfficeBuilding size={size} />);
    case "ReheatActuator": return (<BsBoxArrowUpRight size={size} />);
    case "RelativeHumiditySensor": return (<WiHumidity size={size} />);
    case "ReliefAirHumiditySensor": return (<WiHumidity size={size} />);
    case "RenewableImportActiveElectricalPowerSensor": return (<GiElectric size={size} />);
    case "RequestToExitDevice": return (<IoExitOutline size={size} />);
    case "ResetActuator": return (<TiFlowSwitch size={size} />);
    case "ResetSensor": return (<TiFlowSwitch size={size} />);
    case "ReturnAirDiffuser": return (<MdGrid4X4 size={size} />);
    case "ReturnAirGrille": return (<MdGrid4X4 size={size} />);
    case "ReturnAirRegister": return (<MdGrid4X4 size={size} />);
    case "ReturnAirDamperPositionActuator": return (<BsSlashCircleFill size={size} />);
    case "ReturnAirDamperPositionSensor": return (<BsSlashCircleFill size={size} />);
    case "ReturnAirTemperatureSensor": return (<FaTemperatureHigh size={size} />);
    case "ReturnAirTemperatureSetpoint": return (<FaTemperatureHigh size={size} />);
    case "ReturnAirEnthalpySensor": return (<FaTemperatureHigh size={size} />);
    case "ReturnAirFlowSetpoint": return (<MdGrid4X4 size={size} />);
    case "ReturnAirFlowSensor": return (<MdGrid4X4 size={size} />);
    case "ReturnAirHumiditySensor": return (<WiHumidity size={size} />);
    case "ReturnAirStaticPressureSensor": return (<BsDownload size={size} />);
    case "ReturnFan": return (<FaFan size={size} />);
    case "RoofLevel": return (<BiAlignMiddle size={size} />);
    case "RoofDrain": return (<BsArrowBarDown size={size} />);
    case "RooftopUnit": return (<FaBox size={size} />);
    case "Room": return (<BiBuilding size={size} />);
    case "RunActuator": return (<VscRunAbove size={size} />);
    case "RunSensor": return (<VscRunAbove size={size} />);
    case "RunLevelSensor": return (<VscRunAbove size={size} />);
    case "RunStopSensor": return (<VscRunAbove size={size} />);
    case "RunLevelState": return (<VscRunAbove size={size} />);
    case "RunLevelSetpoint": return (<VscRunAbove size={size} />);
    case "RunLevelActuator": return (<VscRunAbove size={size} />);
    case "RunStopActuator": return (<VscDebugStart size={size} />);
    case "Rule": return (<FaRuler size={size} />);
    case "RunwayPavementMarking": return (<MdAddRoad size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelR;
