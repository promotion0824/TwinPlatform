import { AsyncJobStatus } from '../services/Clients';
import { CheckboxGroup, Checkbox } from '@willowinc/ui';

const AsyncJobStatusGroupCheckbox = ({ onChange, value }: { onChange: (value: string[]) => void; value: string[] }) => {
  return (
    <CheckboxGroup onChange={onChange} value={value} label="Job Statuses">
      {Object.values(AsyncJobStatus).map((status) => {
        return <Checkbox key={status} label={status} value={status} />;
      })}
    </CheckboxGroup>
  );
};

export default AsyncJobStatusGroupCheckbox;
