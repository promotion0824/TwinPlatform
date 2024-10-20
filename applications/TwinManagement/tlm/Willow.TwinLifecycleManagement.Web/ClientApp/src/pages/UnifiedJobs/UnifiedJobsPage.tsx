import PageLayout from '../../components/PageLayout';
import JobsTable from './components/JobsTable';
import JobsFilters from './components/JobsFilters';
import JobOnDemandTriggerButton from './components/JobOnDemandTriggerButton';
import JobsProvider, { useJobs } from './JobsProvider';
import { Button, Loader, Icon } from '@willowinc/ui';
import styled from '@emotion/styled';
import { useParams } from 'react-router-dom';
import JobDetailsPage from './JobDetailsPage';

export default function JobsPage() {
  const { jobId } = useParams();
  return <JobsProvider>{jobId ? <JobDetailsPage /> : <JobsTablePage />}</JobsProvider>;
}

function JobsTablePage() {
  return (
    <PageLayout>
      <PageLayout.Header pageTitleItems={[{ title: 'Jobs' }]}>
        <PageLayout.Header.ActionBar>
          <ActionButtons />
        </PageLayout.Header.ActionBar>
      </PageLayout.Header>

      <PageLayout.Sidebar>
        <JobsFilters />
      </PageLayout.Sidebar>

      <PageLayout.MainContent>
        <JobsTable />
      </PageLayout.MainContent>
    </PageLayout>
  );
}
function ActionButtons() {
  return (
    <Flex>
      <JobOnDemandTriggerButton />
      <DeleteJobButton />
    </Flex>
  );
}

function DeleteJobButton() {
  const { handleDeleteBulk, isDeleting, selectedRowsState } = useJobs();

  const shouldDisableDeleteButton = isDeleting || selectedRowsState[0].length === 0;

  return (
    <Button
      onClick={handleDeleteBulk}
      disabled={shouldDisableDeleteButton}
      prefix={isDeleting ? <Loader /> : <StyledIcon icon="info" />}
      kind="negative"
    >
      Delete
    </Button>
  );
}

const Flex = styled('div')({ display: 'flex' });
const StyledIcon = styled(Icon)({ fontVariationSettings: `'FILL' 1,'wght' 400,'GRAD' 200,'opsz' 20 !important` });
