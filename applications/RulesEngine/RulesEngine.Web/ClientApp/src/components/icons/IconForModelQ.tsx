import { FaQuestionCircle } from 'react-icons/fa';
import { IconPropsShort } from './IconForModel'
import { GiStack } from 'react-icons/gi';

const IconForModelQ = ({ shortModelId, size }: IconPropsShort): JSX.Element => {
  switch (shortModelId) {
    case "QuantitySensor": return (<GiStack size={size} />);
    case "QuantitySetpoint": return (<GiStack size={size} />);

    default: return (<FaQuestionCircle size={size} />);
  }
}

export default IconForModelQ;
