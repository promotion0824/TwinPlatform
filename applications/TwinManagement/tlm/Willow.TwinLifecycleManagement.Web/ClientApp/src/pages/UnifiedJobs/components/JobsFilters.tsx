import styled from '@emotion/styled';
import { Select, Loader, TagsInput, Checkbox } from '@willowinc/ui';
import { AsyncJobStatus } from '../../../services/Clients';
import { useJobs } from '../JobsProvider';
import useGetJobType from '../hooks/useGetJobType';

export default function JobsFilters() {
  const { getJobsQuery } = useJobs();
  const { selectedStatusesState, selectedDateState, selectedJobTypesState, justMyJobsCheckboxState, hideSytemJobs } = getJobsQuery;

  return (
    <FiltersContainer>
      <JobTypesSelector
        value={selectedJobTypesState[0]}
        onChange={(value: string[]) => selectedJobTypesState[1](value)}
      />
      <JobStatusesSelector
        onChange={(value: string[]) => selectedStatusesState[1](value as AsyncJobStatus[])}
        value={selectedStatusesState[0]}
      />
      <DateSelector value={selectedDateState[0]} onChange={(value: string | null) => selectedDateState[1](value)} />

      <JustMyJobsSelector
        value={justMyJobsCheckboxState[0]}
        onChange={() => justMyJobsCheckboxState[1]((prev) => !prev)}
      />
      <HideSystemJobsSelector
        value={hideSytemJobs[0]}
        onChange={() => hideSytemJobs[1]((prev) => !prev)}
      />
    </FiltersContainer>
  );
}

const FiltersContainer = styled('div')({
  display: 'flex',
  flexDirection: 'column',
  gap: 16,
  padding: 16,
});

function DateSelector({ value, onChange }: { value: string | null; onChange: (value: string | null) => void }) {
  return (
    <Select
      value={value}
      onChange={onChange}
      label="Time Created"
      placeholder="Select Date"
      clearable
      data={[
        {
          label: 'Last 24 Hours',
          value: '1',
        },
        {
          label: 'Last 7 Days',
          value: '7',
        },
        {
          label: 'Last 30 Days',
          value: '30',
        },
        {
          label: 'Last Year',
          value: '365',
        },
        {
          label: 'Last Two Years',
          value: '730',
        },
      ]}
    />
  );
}

function JobTypesSelector({ value, onChange }: { value: string[]; onChange: (value: string[]) => void }) {
  const { data = [], isLoading } = useGetJobType();
  return (
    <TagsInput
      label="Job Types"
      placeholder={value.length === 0 ? 'Select Job Types' : undefined}
      data={data}
      value={value}
      onChange={onChange}
      clearable
      rightSection={isLoading ? <Loader /> : undefined}
      disabled={isLoading || data.length === 0}
    />
  );
}

function JobStatusesSelector({ value, onChange }: { value: string[]; onChange: (value: string[]) => void }) {
  return (
    <TagsInput
      label="Job Statuses"
      placeholder={value.length === 0 ? 'Select Job Statuses' : undefined}
      data={Object.values(AsyncJobStatus)}
      value={value}
      onChange={onChange}
      clearable
    />
  );
}

function JustMyJobsSelector({ value, onChange }: { value: boolean; onChange: () => void }) {
  return <Checkbox label="Just My Jobs" checked={value} onChange={onChange} />;
}

function HideSystemJobsSelector({ value, onChange }: { value: boolean; onChange: () => void }) {
  return <Checkbox label="Hide System Jobs" checked={value} onChange={onChange} />;
}
