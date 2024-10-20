import { Alert, styled } from '@mui/material';
import { useDataQaulity } from '../DQProvider';
import { AsyncJobStatus, ITwinsValidationJob } from '../../../services/Clients';
import { AsyncJobStatusChip } from '../../../components/AsyncJobStatusChip';
import useGetLatestDQValidationJob from '../hooks/useGetLatestDQValidationJob';
import { AsyncValue } from '../../../components/AsyncValue';
/**
 * This component displays information about the latest twin validation job (i.e. userId, last updated time, and status).
 */
export default function SummaryOfLastScan({ showInProgressWarning = false }: { showInProgressWarning?: boolean }) {
  const { latestValidationJobQuery, mutateDQTwinValidation } = useDataQaulity();
  const { data, isSuccess, isFetching } = latestValidationJobQuery;

  const { data: latestValidationJobInProcess } = useGetLatestDQValidationJob(
    { status: AsyncJobStatus.Processing },
    { enabled: showInProgressWarning, cacheTime: 0 }
  );

  const validationJobsInProgress = latestValidationJobInProcess;

  const { data: twinValidationResponse, isLoading: isTwinValidationLoading } = mutateDQTwinValidation;

  // After we successfully scanned, show the latest validation job. We use the response of mutateDQTwinValidation instead of validationJobsQuery
  // as there is a latency delay between the creation of the twin validation job and the jobs being returned in the validationJobsQuery.
  const latestValidationJob = twinValidationResponse ? twinValidationResponse : data;

  return (
    <SummaryScanContainer>
      <LastScan
        isLoading={isFetching || isTwinValidationLoading}
        isSuccess={isSuccess}
        latestValidationJob={latestValidationJob}
        showInProgressWarning={showInProgressWarning && !!validationJobsInProgress && !isFetching}
      />
    </SummaryScanContainer>
  );
}

const LastScan = ({
  isLoading,
  isSuccess,
  latestValidationJob,
  showInProgressWarning = false,
}: {
  isLoading: boolean;
  isSuccess: boolean;
  latestValidationJob?: ITwinsValidationJob;
  showInProgressWarning?: boolean;
}) => {
  const lastUpdateTime = latestValidationJob?.lastUpdateTime?.toLocaleDateString('en-US', {
    weekday: 'short',
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
    timeZoneName: 'short',
  });

  const summaryOfLastScan = latestValidationJob
    ? `${lastUpdateTime} by ${latestValidationJob?.userId}`
    : 'No scans have been run';

  return (
    <LastScanContainer>
      <FieldContainer>
        <Text>Last Scan: </Text>
        {/* TODO: add link to DQ job details view */}
        <AsyncValue sx={{ marginTop: '5px !important' }} value={summaryOfLastScan} isLoading={isLoading} />
        {isSuccess && !isLoading && !!latestValidationJob && (
          <FieldContainer>
            <AsyncJobStatusChip value={latestValidationJob?.details?.status || AsyncJobStatus.Error} />
          </FieldContainer>
        )}
      </FieldContainer>
      {showInProgressWarning && (
        <Alert severity="warning">There is currently a scan in progress - results may be partial until complete</Alert>
      )}
    </LastScanContainer>
  );
};

const Text = styled('div')({ fontWeight: 'bold' });

const SummaryScanContainer = styled('div')({
  display: 'flex',
  flexDirection: 'column',
  gap: 10,
  width: '100%',
  overflow: 'auto',
  maxHeight: '28.7vh',
  padding: '0 1em 1em 0',
});

const LastScanContainer = styled('div')({
  display: 'flex',
  flexDirection: 'column',
  gap: 10,
  width: '100%',
});

const FieldContainer = styled('div')({ display: 'flex', gap: '1rem', alignItems: 'center' });
