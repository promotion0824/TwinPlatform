import { FaQuestionCircle } from 'react-icons/fa';
import { GiFlowerTwirl, GiPoisonGas } from 'react-icons/gi';
import { HiOutlineSwitchHorizontal } from 'react-icons/hi';
import { IconPropsShort } from './IconForModel'
import { MdPower } from 'react-icons/md';
import { RiEmotionNormalLine } from 'react-icons/ri';
import { FcOvertime } from 'react-icons/fc';

const IconForModelN = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "NaturalGasSystem": return (<GiFlowerTwirl size={size} />);
    case "NaturalGasVolumeSensor": return (<GiFlowerTwirl size={size} />);
    case "NetworkSecurityEquipment": return (<HiOutlineSwitchHorizontal size={size} />);
    case "NeutralCurrentMagnitudeSensor": return (<MdPower size={size} />);
    case "NO2AirQualitySensor": return (<GiPoisonGas size={size} />);
    case "NormalState": return (<RiEmotionNormalLine size={size} />);
    case "NormalScheduleOccupiedState": return (<FcOvertime size={size} />);
    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelN;
