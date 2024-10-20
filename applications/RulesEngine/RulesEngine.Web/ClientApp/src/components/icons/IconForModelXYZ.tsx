import { FaQuestionCircle, FaTemperatureHigh, FaTemperatureLow } from 'react-icons/fa';
import { BsTextareaResize } from 'react-icons/bs';
import { WiHumidity } from 'react-icons/wi';
import { IconPropsShort } from './IconForModel'

const IconForModelXYZ = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "Zone": return (<BsTextareaResize size={size} />);
    case "ZoneAirHumiditySensor": return (<WiHumidity size={size} />);
    case "ZoneAirHumiditySetpoint": return (<WiHumidity size={size} />);
    case "ZoneAirTemperatureSensor": return (<FaTemperatureHigh size={size} />);
    case "ZoneAirTemperatureSetpoint": return (<FaTemperatureLow size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelXYZ;
