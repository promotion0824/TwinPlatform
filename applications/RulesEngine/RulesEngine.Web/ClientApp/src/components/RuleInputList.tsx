import { RuleUIElementDto } from '../Rules';
import RuleInput from './RuleInput';

interface RuleInputListProps {
  elements: RuleUIElementDto[];
  title: string;
}

const RuleInputList: React.FC<RuleInputListProps> = ({ elements, title }) => {
  if (elements) {
    return (
      <div>
        <h2>{title}</h2>
        {elements.map((element: RuleUIElementDto, i: number) =>
          <div key={i}>
            <RuleInput element={element} />
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