import { RuleParameterDto } from '../Rules';
import RuleParameterInput from './RuleParameterInput';

interface RuleInputListProps {
  parameters: RuleParameterDto[];
  title: string;
}

const RuleInputList: React.FC<RuleInputListProps> = ({ parameters, title }) => {
  if (parameters) {
    return (
      <div>
        <h2>{title}</h2>
        {parameters.map((parameter: RuleParameterDto, i: number) =>
          <div key={i}>
            <RuleParameterInput parameter={parameter} />
          </div>
        )}
      </div>
    );
  }
  else {
    return (<></>);
  }
}

export default RuleInputList;