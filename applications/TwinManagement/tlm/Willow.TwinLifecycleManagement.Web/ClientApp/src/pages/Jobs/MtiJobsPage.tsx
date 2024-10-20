import { useMemo, useState } from 'react';
import { GridColDef, useGridApiRef } from '@mui/x-data-grid-pro';
import { CustomToolBar } from '../../components/Grid/CustomToolBar';
import useGetMtiJobs from './hooks/useGetMtiJobs';
import { AsyncJobStatus, MtiAsyncJobType } from '../../services/Clients';
import { AsyncJobStatusChip } from '../../components/AsyncJobStatusChip';
import { StyledHeader } from '../../components/Common/StyledComponents';
import { usePersistentGridState } from '../../hooks/usePersistentGridState';
import { DataGrid } from '@willowinc/ui';

/**
 * Table for displaying MTI jobs
 */
export default function DQJobsTable() {
  const apiRef = useGridApiRef();
  const { savedState } = usePersistentGridState(apiRef, 'mti-jobs');

  const { data, isLoading } = useGetMtiJobs();

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'jobId',
        headerName: 'ID',
        flex: 0.35,
        valueGetter: (params) => params.row.jobId,
      },
      {
        field: 'jobType',
        headerName: 'Job Type',
        flex: 0.25,
        valueGetter: (params) => JobTypeLabelMap(params.row.jobType),
      },
      {
        field: 'status',
        headerName: 'Status',
        type: 'singleSelect',
        valueOptions: Object.values(AsyncJobStatus),
        valueGetter: (params) => params.row.details.status,
        renderCell: ({ value }) => <AsyncJobStatusChip value={value} />,
      },
      {
        field: 'createTime',
        headerName: 'Create Time',
        flex: 0.25,
        sortComparator: (x, y) => new Date(x).getTime() - new Date(y).getTime(),
        valueFormatter: (params: any) => {
          return new Date(params.value).toLocaleDateString('en-US', {
            weekday: 'short',
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: 'numeric',
            minute: 'numeric',
            timeZoneName: 'short',
          });
        },
      },
      {
        field: 'lastUpdateTime',
        headerName: 'Last Update Time',
        flex: 0.25,
        sortComparator: (x, y) => new Date(x).getTime() - new Date(y).getTime(),
        valueFormatter: (params: any) => {
          return new Date(params.value).toLocaleDateString('en-US', {
            weekday: 'short',
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: 'numeric',
            minute: 'numeric',
            timeZoneName: 'short',
          });
        },
      },
    ],
    []
  );

  const [paginationModel, setPaginationModel] = useState({
    pageSize: 250,
    page: 0,
  });

  return (
    <>
      <StyledHeader variant="h1">Mapped Topology Ingestion Jobs</StyledHeader>
      <div style={{ height: '86vh', width: '100%', backgroundColor: '#242424' }}>
        <DataGrid
          apiRef={apiRef}
          initialState={savedState}
          getRowHeight={() => 'auto'}
          rows={data || []}
          getRowId={(row) => row.jobId}
          loading={isLoading}
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          slots={{ toolbar: CustomToolBar }}
          columns={columns}
        />
      </div>
    </>
  );
}

function JobTypeLabelMap(jobType: MtiAsyncJobType) {
  const map = {
    [MtiAsyncJobType.SyncOrganization]: 'Sync Organization',
    [MtiAsyncJobType.SyncSpatial]: 'Sync Spatial Resources',
    [MtiAsyncJobType.SyncConnectors]: 'Sync Connectors',
    [MtiAsyncJobType.SyncAssets]: 'Sync Assets',
    [MtiAsyncJobType.SyncCapabilities]: 'Sync Capabilities',
    [MtiAsyncJobType.Ingest]: 'Ingest',
    [MtiAsyncJobType.PushToMapped]: 'Push to Mapped',
    default: 'Unknown',
  };

  return map[jobType] || map.default;
}
