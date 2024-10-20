import { LinearProgress, Button } from '@mui/material';
import { useQuery, useQueryClient } from 'react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { AsyncJobDetails } from '../../types/AsyncJobDetails';
import LinearWithValueLabel from '../../components/Common/LinearProgressWithLabel';
import { useState, useEffect } from 'react';
import useApi from '../../hooks/useApi';
import { useActionComment } from '../../hooks/useActionComment';
import { ApiException, ErrorResponse, AsyncJobStatus, TimeSeriesImportJob } from '../../services/Clients';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import useUserInfo from '../../hooks/useUserInfo';
import JobErrorsTable from '../Jobs/JobsTable/JobErrorsTable';

export default function TimeSeriesJobResults() {
  const navigate = useNavigate();
  const api = useApi();
  const userInfo = useUserInfo();
  const { jobId } = useParams();
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();
  const [refetchCount, setRefetchCount] = useState(0);
  const [actionComment, setActionComment] = useActionComment(undefined);
  const maxRefetchCount = 5;
  const [jobDone, setJobDone] = useState(false);
  const [lastFetchWasError, setLastFetchWasError] = useState(false);

  const [progress, setProgress] = useState<number>(0);

  const refetchInterval = () => {
    if (jobDone) return 0;
    var miliseconds = (refetchCount + 1 + (refetchCount ^ 2) / 2) * 1000;
    var topFlatLimit = 10000;
    return miliseconds > topFlatLimit ? topFlatLimit : miliseconds;
  };

  const isDone = (job: TimeSeriesImportJob) => {
    if (!job) {
      console.warn('Null job returned from successful job response');
      return false;
    }
    let isItDone = job.details?.status !== 'Processing' && job.details?.status !== 'Queued';
    setJobDone(isItDone);
    return isItDone;
  };

  const queryClient = useQueryClient();
  const {
    data = undefined,
    error = undefined,
    isFetching = true,
    refetch,
  } = useQuery(
    'getJobDetails',
    async () => {
      return api
        .timeSeries(jobId as string)
        .then((response) => {
          setRefetchCount(refetchCount + 1);
          if (response && response.processedEntities && response.totalEntities) {
            setProgress(Math.round((response.processedEntities / response.totalEntities) * 100));
          }
          setLastFetchWasError(false);
          return response;
        })
        .catch((error: ErrorResponse | ApiException) => {
          setRefetchCount(refetchCount + 1);
          if (
            refetchCount >= maxRefetchCount ||
            (typeof error == typeof ErrorResponse && (error as ErrorResponse).statusCode !== 404) ||
            (typeof error == typeof ApiException && (error as ApiException).status !== 404) ||
            (typeof error == typeof '' && String(error).match(/\d{3}/g)?.join('') !== '404')
          ) {
            console.warn('Got 404 from job response', error);
            setLastFetchWasError(true);
            setRefetchCount(refetchCount + 1);
          }
        });
    },
    {
      refetchInterval: refetchInterval, // 1, 2.5, 5, 8.5, 10, 10, 10* sec
      onSuccess: (res: TimeSeriesImportJob) => {
        console.log('Job:', res);
        if (isDone(res)) {
          setRefetchCount(maxRefetchCount);
        }
        setActionComment(refineComment(res?.userData));
      },
    }
  );

  const cancelImportJob = () => {
    if (!jobId) {
      alert('Job ID is not set!');
    } else {
      api
        .cancelTimeSeriesImport(jobId, userInfo.userEmail)
        .then((_res) => {
          alert('Import job cancelled successfully!');
        })
        .catch((err) => {
          setErrorMessage(err);
          setShowPopUp(true);
          setOpenPopUp(true);
        });
    }
  };

  const refineComment = (wholeTxt: string | undefined): string[] => {
    if (!wholeTxt) return ['', ''];
    var index = wholeTxt.indexOf(']');
    return [wholeTxt.slice(1, index), wholeTxt.slice(index + 2)];
  };

  let nErrors = data?.entitiesError ? Object.keys(data.entitiesError).length : 0;

  return (
    <div>
      <Button variant="contained" data-cy="refresh" onClick={() => refetch()} disabled={refetchCount < maxRefetchCount}>
        Refresh
      </Button>
      <Button
        variant="contained"
        data-cy="cancelJob"
        sx={{ float: 'right' }}
        color="error"
        onClick={() => cancelImportJob()}
        hidden={data?.details?.status !== 'Processing' && data?.details?.status !== 'Queued'}
      >
        Cancel Job
      </Button>
      <br />
      <p>
        <br />
        <b>Job {jobId}</b>
      </p>
      <b>Status:</b> {data?.details?.status ?? '???'}
      {data?.details ? (
        <>
          {isFetching && <LinearProgress style={{ marginTop: '20px' }} />}
          <p></p>
          <p>
            <b>Message:</b> {(data.details as AsyncJobDetails).statusMessage}
          </p>
          <p>
            <b>Processed entities: </b>
            {data.processedEntities ?? '0'} of {data.totalEntities ?? '?'} total
            {data.details.status !== 'Processing' ? ` (${nErrors} errors)` : ' (Errors pending)'}
          </p>
          <>
            {data.details.status === 'Processing' && data.processedEntities && data.totalEntities && (
              <LinearWithValueLabel value={progress} />
            )}
          </>
          <div className="container" style={{ height: '90vh' }}>
            <>{!isFetching && !error && data.entitiesError && Object.keys(data.entitiesError).length > 0 && <JobErrorsTable entitiesError={data.entitiesError} />}</>
          </div>
        </>
      ) : (
        isFetching && <LinearProgress style={{ marginTop: '20px' }} />
      )}
      {showPopUp ? (
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      ) : (
        <></>
      )}
    </div>
  );
}
