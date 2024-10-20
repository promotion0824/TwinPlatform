import { createContext, useContext, useState, useEffect } from 'react';
import { ApiException, ErrorResponse, JobsEntry } from '../../services/Clients';
import useGetUnifiedJobs, { IGetJobs } from './hooks/useGetUnifiedJobs';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import { GridRowId, useGridApiRef, GridRowModelUpdate } from '@mui/x-data-grid-pro';
import { useMutation } from 'react-query';
import { useSnackbar } from '../../providers/SnackbarProvider/SnackbarProvider';
import useApi from '../../hooks/useApi';
import { useParams } from 'react-router-dom';

type JobsContextType = {
  tableApiRef: ReturnType<typeof useGridApiRef>;
  getJobsQuery: IGetJobs;
  handleDeleteBulk: () => Promise<void>;
  isDeleting: boolean;
  selectedRowsState: [GridRowId[], React.Dispatch<React.SetStateAction<GridRowId[]>>];
  selectedJobState: [JobsEntry | null, React.Dispatch<React.SetStateAction<JobsEntry | null>>];
};

const JobsContext = createContext<JobsContextType | undefined>(undefined);

export function useJobs() {
  const context = useContext(JobsContext);
  if (context == null) {
    throw new Error('useJobs must be used within a JobsProvider');
  }

  return context;
}

/**
 * JobsProvider is a wrapper component that manage jobs.
 */
export default function JobsProvider({ children }: { children: JSX.Element }) {
  const api = useApi();
  const tableApiRef = useGridApiRef();
  // states used for error handling
  const [openPopUp, setOpenPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();
  const getJobsQuery = useGetUnifiedJobs({
    onError: (error) => {
      setErrorMessage(error);
      setOpenPopUp(true);
    },
  });

  const snackbar = useSnackbar();

  const selectedJobState = useState<JobsEntry | null>(null);
  const selectedRowsState = useState<GridRowId[]>([]);

  const { jobId } = useParams();

  useEffect(() => {
    // if jobId is null, set selectedJobState to null
    if (!jobId) {
      selectedJobState[1](null);
    }
  }, [jobId, selectedJobState]);

  const deleteBulkMutate = useMutation(({ ids }: { ids: string[] }) => api.deleteJobEntries(false, ids), {});

  const { mutateAsync: deleteBulkMutateAsync, isLoading: isDeleting } = deleteBulkMutate;

  const handleDeleteBulk = async () => {
    let selectedRows = Array.from(tableApiRef.current.getSelectedRows().values());

    if (selectedRows.length === 0) return;

    let selectedRowsIds = selectedRows.map((row) => row.jobId);

    const numOfSuccessfulDeleteRecords = await deleteBulkMutateAsync({ ids: selectedRowsIds });

    selectedRowsState[1]([]); // clear selected rows

    const gridRowModelUpdate = selectedRowsIds.map((id) => ({ jobId: id, _action: 'delete' } as GridRowModelUpdate));

    tableApiRef.current.updateRows(gridRowModelUpdate);

    snackbar.show(`${numOfSuccessfulDeleteRecords} records deleted successfully`);
  };

  return (
    <JobsContext.Provider
      value={{
        tableApiRef,
        getJobsQuery,
        handleDeleteBulk,
        isDeleting,
        selectedRowsState,
        selectedJobState,
      }}
    >
      {children}

      {
        // todo: remove when global error handling is implemented
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      }
    </JobsContext.Provider>
  );
}
