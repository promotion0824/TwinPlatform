import { FaCar, FaFan, FaHandHoldingWater, FaQuestionCircle, FaRegLightbulb, FaRegObjectGroup, FaTemperatureHigh, FaTemperatureLow } from 'react-icons/fa';
import { BiCylinder, BiExit, BiPowerOff, BiWater } from 'react-icons/bi';
import { VscCircuitBoard } from 'react-icons/vsc';
import { CgSmartHomeHeat } from 'react-icons/cg';
import { BsBoxArrowInDownRight, BsPlug } from 'react-icons/bs';
import { WiHumidity, WiWindBeaufort0, WiWindBeaufort1 } from 'react-icons/wi';
import { GiBoatPropeller, GiElectric, GiElevator, GiEscalator, GiSupersonicArrow } from 'react-icons/gi';
import { HiOutlineSwitchHorizontal } from 'react-icons/hi';
import { TiFlowSwitch } from 'react-icons/ti';
import { IconPropsShort } from './IconForModel'
import { RiDeleteBinLine } from 'react-icons/ri';
import { MdOutlineIndeterminateCheckBox } from 'react-icons/md';
import { FiTruck } from 'react-icons/fi';

const IconForModelE = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "EdgeOfDockLoadingDockLeveler": return (<FiTruck size={size} />);
    case "EffectiveZoneAirTemperatureSetpoint": return (<FaTemperatureLow size={size} />);
    case "EffectiveCoolingZoneAirTemperatureSetpoint": return (<FaTemperatureLow size={size} />);
    case "EffectiveHeatingZoneAirTemperatureSetpoint": return (<FaTemperatureHigh size={size} />);
    case "EjectorPump": return (<GiBoatPropeller size={size} />);
    case "ElectricalBus": return (<GiElectric size={size} />);
    case "ElectricalCable": return (<GiElectric size={size} />);
    case "ElectricalCircuit": return (<GiElectric size={size} />);
    case "ElectricalCircuit1Pole": return (<GiElectric size={size} />);
    case "ElectricalCircuit2Pole": return (<GiElectric size={size} />);
    case "ElectricalCircuit3Pole": return (<GiElectric size={size} />);
    case "ElectricalDistributionEquipment": return (<VscCircuitBoard size={size} />);
    case "ElectricalEnergySensor": return (<GiElectric size={size} />);
    case "ElectricalMeter": return (<GiElectric size={size} />);
    case "ElectricalReceptacle": return (<BsPlug size={size} />);
    case "ElectricalPanelboard": return (<VscCircuitBoard size={size} />);
    case "ElectricalPanelboardMCB": return (<VscCircuitBoard size={size} />);
    case "ElectricalPanelboardMLO": return (<VscCircuitBoard size={size} />);
    case "ElectricalPowerSensor": return (<GiElectric size={size} />);
    case "ElectricalEquipment": return (<GiElectric size={size} />);
    case "ElectricalSystem": return (<GiElectric size={size} />);
    case "ElectricalSwitch": return (<BiPowerOff size={size} />);
    case "ElectricUnitHeater": return (<CgSmartHomeHeat size={size} />);
    case "ElectricTanklessWaterHeater": return (<MdOutlineIndeterminateCheckBox size={size} />);
    case "ElectricTankWaterHeater": return (<BiCylinder size={size} />);
    case "ElectricVehicleChargingStation": return (<FaCar size={size} />);
    case "Elevator": return (<GiElevator size={size} />);
    case "ElevatorMachine": return (<GiElevator size={size} />);
    case "EmergencyLight": return (<FaRegLightbulb size={size} />);
    case "EmergencyLightingPowerInverter": return (<FaRegLightbulb size={size} />);
    case "EnableActuator": return (<TiFlowSwitch size={size} />);
    case "EnableState": return (<TiFlowSwitch size={size} />);
    case "EnableSensor": return (<TiFlowSwitch size={size} />);
    case "EnergySensor": return (<GiElectric size={size} />);
    case "EnteringChilledWaterFlowSensor": return (<BiWater size={size} />);
    case "EnteringCondenserWaterTemperatureSensor": return (<FaHandHoldingWater size={size} />);
    case "EnteringChilledWaterTemperatureSensor": return (<FaHandHoldingWater size={size} />);
    case "EnteringHotWaterTemperatureSensor": return (<BiWater size={size} />);
    case "EnteringPeopleCountSensor": return (<BsBoxArrowInDownRight size={size} />);
    case "EnthalpySensor": return (<CgSmartHomeHeat size={size} />);
    case "EthernetSwitch": return (<HiOutlineSwitchHorizontal size={size} />);
    case "Escalator": return (<GiEscalator size={size} />);
    case "Equipment": return (<FaRegObjectGroup size={size} />);
    case "EquipmentCollection": return (<RiDeleteBinLine size={size} />);
    case "EquipmentGroup": return (<RiDeleteBinLine size={size} />);
    case "ExhaustAirFlowSensor": return (<WiWindBeaufort0 size={size} />);
    case "ExhaustAirFlowSetpoint": return (<WiWindBeaufort1 size={size} />);
    case "ExhaustAirHumiditySensor": return (<WiHumidity size={size} />);
    case "ExhaustAirDamperPositionActuator": return (<GiSupersonicArrow size={size} />);
    case "ExhaustAirDamperPositionSensor": return (<GiSupersonicArrow size={size} />);
    case "ExhaustAirDamperPositionSetpoint": return (<GiSupersonicArrow size={size} />);
    case "ExhaustFan": return (<FaFan size={size} />);
    case "ExhaustFanRunState": return (<FaFan size={size} />);
    case "ExhaustFanRunActuator": return (<FaFan size={size} />);
    case "ExhaustFanRunSensor": return (<FaFan size={size} />);
    case "ExhaustFanVFDRunLevelActuator": return (<FaFan size={size} />);
    case "ExitSign": return (<BiExit size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelE;
