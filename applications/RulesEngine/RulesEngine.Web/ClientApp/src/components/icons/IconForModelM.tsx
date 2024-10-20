import { FaBox, FaQuestionCircle, FaTemperatureLow } from 'react-icons/fa';
import { CgArrowLongDownE, CgArrowLongUpE } from 'react-icons/cg';
import { WiHumidity } from 'react-icons/wi';
import { GiCheckboxTree, GiFlowerTwirl, GiSpeedometer, GiWeight } from 'react-icons/gi';
import { IconPropsShort } from './IconForModel'
import { MdModeStandby, MdOutlineMotionPhotosOn, MdOutlineMoving } from 'react-icons/md';
import { BiAlignMiddle, BiCabinet, BiPowerOff } from 'react-icons/bi';
import { IoSnowSharp } from 'react-icons/io5';

const IconForModelM = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "Manifold": return (<GiCheckboxTree size={size} />);
    case "MassSensor": return (<GiWeight size={size} />);
    case "MassFlowSensor": return (<GiWeight size={size} />);
    case "MassSetpoint": return (<GiWeight size={size} />);
    case "MassFlowSetpoint": return (<GiWeight size={size} />);
    case "MakeupAirUnit": return (<FaBox size={size} />);
    case "MediumTemperatureRefrigeratedFoodDisplayCase": return (<BiCabinet size={size} />);
    case "MediumTemperatureRefrigerationCompressor": return (<IoSnowSharp size={size} />);
    case "MezzanineLevel": return (<BiAlignMiddle size={size} />);
    case "ModeSensor": return (<MdModeStandby size={size} />);
    case "ModeActuator": return (<MdModeStandby size={size} />);
    case "MotorController": return (<MdOutlineMoving size={size} />);
    case "MovingWalkway": return (<MdOutlineMoving size={size} />);
    case "MaxLimit": return (<CgArrowLongUpE size={size} />);
    case "MinLimit": return (<CgArrowLongDownE size={size} />);
    case "MaxDischargeAirFlowSetpoint": return (<GiFlowerTwirl size={size} />);
    case "MaxDischargeFanAirFlowSetpoint": return (<GiFlowerTwirl size={size} />);
    case "MeterEquipment": return (<GiSpeedometer size={size} />);
    case "MinDischargeAirFlowSetpoint": return (<GiFlowerTwirl size={size} />);
    case "MinDischargeFanAirFlowSetpoint": return (<GiFlowerTwirl size={size} />);
    case "MinOutsideAirFlowSensor": return (<GiFlowerTwirl size={size} />);
    case "MinOutsideAirFlowSetpoint": return (<GiFlowerTwirl size={size} />);
    case "MixedAirTemperatureSensor": return (<FaTemperatureLow size={size} />);
    case "MixedAirHumiditySensor": return (<WiHumidity size={size} />);
    case "ModeState": return (<BiPowerOff size={size} />);
    case "MotionSensor": return (<MdOutlineMotionPhotosOn size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelM;
