import { useMemo, useEffect } from 'react';
import {
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarExport,
  GridToolbarFilterButton,
  GridColumnVisibilityModel,
} from '@mui/x-data-grid-pro';
import { AsyncJobStatus } from '../../../services/Clients';
import { AsyncJobStatusChip } from '../../../components/AsyncJobStatusChip';
import { usePersistentGridState } from '../../../hooks/usePersistentGridState';
import { GridColDef, Link } from '@willowinc/ui';
import styled from '@emotion/styled';
import { StyledDataGrid } from '../../../components/Grid/StyledDataGrid';
import { useJobs } from '../JobsProvider';
import LinearProgress from '@mui/material/LinearProgress';
import useUserinfo from '../../../hooks/useUserInfo';
import useLoader from '../../../hooks/useLoader';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

export default function JobsTable() {
  const navigate = useNavigate();
  const { getJobsQuery, tableApiRef, selectedRowsState, selectedJobState } = useJobs();
  const { savedState } = usePersistentGridState(tableApiRef, 'unified-jobs-v4');
  const { userEmail } = useUserinfo();

  const { unifiedJobsQuery, paginationState } = getJobsQuery;
  const { data = { jobs: [], totalCount: 0 }, isLoading, isFetching } = unifiedJobsQuery;

  const [showLoader, hideLoader] = useLoader();

  useEffect(() => {
    if (isFetching) {
      showLoader();
    } else {
      hideLoader();
    }

    return () => hideLoader();
  }, [hideLoader, isFetching, showLoader]);

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'jobId',
        headerName: 'ID',
        flex: 0.5,
        renderCell: ({ value }) => <Link title={value}>{value}</Link>,
      },
      {
        field: 'jobType',
        headerName: 'Job Type',
        flex: 0.5,
      },
      {
        field: 'jobSubtype',
        headerName: 'Job Subtype',
        flex: 0.5,
      },
      {
        field: 'status',
        headerName: 'Status',
        flex: 0.5,
        type: 'singleSelect',
        valueOptions: Object.values(AsyncJobStatus),
        renderCell: ({ value }: any) => <AsyncJobStatusChip value={value} />,
      },
      {
        field: 'progressStatusMessage',
        headerName: 'Progress Status Message',
        flex: 1,
      },
      {
        field: 'timeCreated',
        headerName: 'Time Created',
        flex: 0.5,
        sortComparator: (x, y) => new Date(x).getTime() - new Date(y).getTime(),
        valueFormatter: (params: any) => {
          return new Date(params.value).toLocaleDateString('en-US', {
            month: 'numeric',
            day: 'numeric',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
          });
        },
      },
      {
        field: 'timeLastUpdated',
        headerName: 'Time Last Updated',
        flex: 0.5,
        sortComparator: (x, y) => new Date(x).getTime() - new Date(y).getTime(),
        valueFormatter: (params: any) => {
          return new Date(params.value).toLocaleDateString('en-US', {
            month: 'numeric',
            day: 'numeric',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
          });
        },
      },
      {
        field: 'userMessage',
        headerName: 'Comment',
        flex: 1,
      },
      {
        field: 'processingStartTime',
        headerName: 'Processing Start Time',
        flex: 1,
        valueFormatter: (params: any) => {
          return new Date(params.value).toLocaleDateString('en-US', {
            month: 'numeric',
            day: 'numeric',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
          });
        },
      },
      {
        field: 'processingEndTime',
        headerName: 'Processing End Time',
        flex: 1,
        valueFormatter: (params: any) => {
          return new Date(params.value).toLocaleDateString('en-US', {
            month: 'numeric',
            day: 'numeric',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true,
          });
        },
      },
      { field: 'progressCurrentCount', headerName: 'Progress Current Count', flex: 1 },
      { field: 'userId', headerName: 'User ID', flex: 1 },
      { field: 'isDeleted', headerName: 'IsDeleted', flex: 1 },
      { field: 'isExternal', headerName: 'IsExternal', flex: 1 },
    ],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  // Default column visibility settings. Display/hide columns fields.
  const [columnVisibilityModel, setColumnVisibilityModel] = useState<GridColumnVisibilityModel>({
    jobId: true,
    jobType: true,
    jobSubtype: true,
    status: true,
    timeCreated: true,
    timeLastUpdated: true,
    progressStatusMessage: true,
    userMessage: false,
    processingStartTime: false,
    processingEndTime: false,
    progressCurrentCount: false,
    userId: false,
    isDeleted: false,
    isExternal: false,
  });

  const { columnVisibilityModel: columnVisibilityModelState } = savedState?.columns || {};
  const {
    jobId = true,
    jobType = true,
    jobSubtype = true,
    status = true,
    timeCreated = true,
    timeLastUpdated = true,
    progressStatusMessage = true,
    userMessage = false,
    processingStartTime = false,
    processingEndTime = false,
    progressCurrentCount = false,
    userId = false,
    isDeleted = false,
    isExternal = false,
  } = columnVisibilityModelState || {};

  useEffect(() => {
    setColumnVisibilityModel({
      jobId,
      jobType,
      jobSubtype,
      status,
      timeCreated,
      timeLastUpdated,
      progressStatusMessage,
      userMessage,
      processingStartTime,
      processingEndTime,
      progressCurrentCount,
      userId,
      isDeleted,
      isExternal,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const [paginationModel, setPaginationModel] = paginationState;

  const { jobs = [], totalCount } = data;

  return (
    <StyledDataGrid
      apiRef={tableApiRef}
      initialState={savedState}
      getRowId={(row) => row.jobId}
      onCellClick={(cell) => {
        if (cell.field === 'jobId') {
          selectedJobState[1](cell.row);
          navigate(`./${cell.id}/details`, { replace: false });
        }
      }}
      rows={jobs}
      rowCount={totalCount}
      checkboxSelection
      onRowSelectionModelChange={(ids) => {
        selectedRowsState[1](ids);
      }}
      rowSelectionModel={selectedRowsState[0]}
      columns={columns}
      columnVisibilityModel={columnVisibilityModel}
      onColumnVisibilityModelChange={(newModel) => setColumnVisibilityModel(newModel)}
      pagination
      paginationMode="server"
      paginationModel={paginationModel}
      onPaginationModelChange={setPaginationModel}
      pageSizeOptions={[100, 250, 1000]}
      slots={{ toolbar: CustomToolBar, loadingOverlay: LinearProgress }}
      loading={isLoading}
      getRowClassName={(params) => {
        if (params.row.userId === userEmail) {
          return 'my-job';
        }
        return '';
      }}
    />
  );
}

function CustomToolBar() {
  return (
    <Flex>
      <StyledToolBarContainer>
        <GridToolbarColumnsButton />
        <GridToolbarFilterButton />
        <GridToolbarExport />
      </StyledToolBarContainer>
      {/* <div>
        <AuthHandler requiredPermissions={[AppPermissions.CanDeleteJobs]}>
          <DeleteJobsButton />
        </AuthHandler>
      </div> */}
    </Flex>
  );
}

const Flex = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  width: '100%',
  justifyContent: 'space-between',
  '&:last-child': { padding: '0 10px' },
});
const StyledToolBarContainer = styled(GridToolbarContainer)({ gap: 10, flexWrap: 'nowrap', padding: '4px 3px' });
