import { FaCompass, FaQuestionCircle } from 'react-icons/fa';
import { IoIosSpeedometer } from 'react-icons/io';
import { GiCctvCamera, GiComputerFan, GiElectric, GiHood, GiMovementSensor, GiTruck, GiValve } from 'react-icons/gi';
import { IconPropsShort } from './IconForModel'
import { BiCylinder } from 'react-icons/bi';
import { IoSnowSharp } from 'react-icons/io5';
import { CgSmartHomeHeat } from 'react-icons/cg';

const IconForModelV = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "Valve": return (<GiValve size={size} />);
    case "ValvePosition": return (<GiValve size={size} />);
    case "ValveOpenActuator": return (<GiValve size={size} />);
    case "ValveOpenSensor": return (<GiValve size={size} />);
    case "ValvePositionSensor": return (<GiValve size={size} />);
    case "ValvePositionActuator": return (<GiValve size={size} />);
    case "VariableFrequencyDrive": return (<IoIosSpeedometer size={size} />);
    case "VAVBox": return (<FaCompass size={size} />);
    case "VAVBoxReheat": return (<FaCompass size={size} />);
    case "Vehicle": return (<GiTruck size={size} />);
    case "VehicleEquipment": return (<GiTruck size={size} />);
    case "VentilationHood": return (<GiHood size={size} />);
    case "VerticalFanCoilUnit": return (<IoSnowSharp size={size} />);
    case "VerticalFanCoilUnitReheat": return (<CgSmartHomeHeat size={size} />);
    case "VerticalStackFanCoilUnit": return (<IoSnowSharp size={size} />);
    case "VerticalStackFanCoilUnitReheat": return (<CgSmartHomeHeat size={size} />);
    case "VFDActuator": return (<GiComputerFan size={size} />);
    case "VFDRunLevelActuator": return (<GiComputerFan size={size} />); // ??
    case "VFDRunLevelSensor": return (<GiComputerFan size={size} />); // ??
    case "VFDRunLevelState": return (<GiComputerFan size={size} />); // ??
    case "VibrationSensorEquipment": return (<GiMovementSensor size={size} />);
    case "VideoSurveillanceCamera": return (<GiCctvCamera size={size} />);
    case "VoltageSensor": return (<GiElectric size={size} />);
    case "VoltageImbalanceSensor": return (<GiElectric size={size} />);
    case "VolumeSensor": return (<BiCylinder size={size} />);
    case "VolumeSetpoint": return (<BiCylinder size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelV;
