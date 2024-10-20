import { RuleUIElementDto, RuleUIElementType } from '../Rules';
import { Input } from '@mui/material';

interface RuleInputProps {
  element: RuleUIElementDto;
}

const RuleInput: React.FC<RuleInputProps> = ({ element }) => {
  switch (element.elementType) {
    case RuleUIElementType._1:
      return (
        <>
          {(element.valueDouble) ?
            <div>
              <span>{element.name}</span>
              <Input id={element.id} placeholder={element.valueDouble.toString()} className="rule-template-value" />
              <span>{element.units}</span>
            </div> : <></>
          }
        </>
      );
    case 2:
      return (
        <>
          {(element.valueInt) ?
            <div>
              <span>{element.name}</span>
              <Input id={element.id} placeholder={element.valueInt.toString()} className="rule-template-value" />
              <span>{element.units}</span>
            </div> : <></>
          }
        </>
      );
    case 3:
      return (
        <>
          {(element.valueString) ?
            <div>
              <span>{element.name}</span>
              <Input id={element.id} placeholder={element.valueString.toString()} className="rule-template-value" />
              <span>{element.units}</span>
            </div> : <></>
          }
        </>
      );
    default:
      return (<></>);
  }
}

export default RuleInput;
