import { FaCarBattery, FaQuestionCircle } from 'react-icons/fa';
import { CgSmartHomeHeat } from 'react-icons/cg';
import { IoSpeedometerOutline, IoThermometer } from 'react-icons/io5';
import { GiBowlingPin, GiElectric } from 'react-icons/gi';
import { IconPropsShort } from './IconForModel'
import { BsBoxArrowDownRight, BsBoxArrowInDownRight } from 'react-icons/bs';

const IconForModelU = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "UnitHeater": return (<CgSmartHomeHeat size={size} />);
    case "UniqueEnteringPeopleCountSensor": return (<BsBoxArrowInDownRight size={size} />);
    case "UniqueLeavingPeopleCountSensor": return (<BsBoxArrowDownRight size={size} />);
    case "UnoccupiedCoolingSetpoint": return (<IoThermometer size={size} />);
    case "UnoccupiedHeatingSetpoint": return (<IoThermometer size={size} />);
    case "UnoccupiedHeatingZoneAirTemperatureSetpoint": return (<IoThermometer size={size} />);
    case "UnoccupiedCoolingZoneAirTemperatureSetpoint": return (<IoThermometer size={size} />);
    case "UPS": return (<FaCarBattery size={size} />);
    case "Urinal": return (<GiBowlingPin size={size} />);
    case "UrinalWaterless": return (<IoSpeedometerOutline size={size} />);
    case "UrinalFlushometer": return (<IoSpeedometerOutline size={size} />);
    case "UtilityAccount": return (<GiElectric size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelU;
