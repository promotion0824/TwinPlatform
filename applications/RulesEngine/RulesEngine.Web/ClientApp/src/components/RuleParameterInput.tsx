import { RuleParameterDto } from '../Rules';
import { Input } from '@mui/material';

interface RuleParameterInputProps {
  parameter: RuleParameterDto;
}

const RuleParameterInput: React.FC<RuleParameterInputProps> = ({ parameter }) => {
  return (
    <div>
      <span>{parameter.name}</span>
      <Input id={parameter.name} placeholder={parameter.pointExpression} value={parameter.pointExpression} className="rule-expression" />
    </div>
  );
}

export default RuleParameterInput;
