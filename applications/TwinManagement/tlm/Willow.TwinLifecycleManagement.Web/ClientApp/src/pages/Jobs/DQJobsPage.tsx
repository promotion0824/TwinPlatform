import { useMemo, useState } from 'react';
import { GridColDef, useGridApiRef } from '@mui/x-data-grid-pro';
import { CustomToolBar } from '../../components/Grid/CustomToolBar';
import useGetDQValidationJobs from './hooks/useGetDQValidationJobs';
import {
  AsyncJobStatus,
  ITwinValidationJobSummaryDetails,
  TwinValidationJobSummaryDetailErrors,
} from '../../services/Clients';
import { AsyncJobStatusChip } from '../../components/AsyncJobStatusChip';
import { StyledHeader } from '../../components/Common/StyledComponents';
import { usePersistentGridState } from '../../hooks/usePersistentGridState';
import { DataGrid } from '@willowinc/ui';

/**
 * Table for displaying DQ validation jobs
 */
export default function DQJobsTable() {
  const apiRef = useGridApiRef();
  const { savedState } = usePersistentGridState(apiRef, 'dq-jobs');

  const { data, isLoading } = useGetDQValidationJobs();

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'jobId',
        headerName: 'ID',
        flex: 0.35,
        // cellClassName: 'idColumn',
        valueGetter: (params) => params.row.jobId,
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
        headerName: 'Start Time',
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

      {
        field: 'errorsPercent',
        headerName: 'Errors %',
        flex: 0.1,
        valueGetter: (params) => {
          let details = params.row?.summaryDetails;
          let percent = getErrorPercent(details);
          return percent !== undefined ? (percent * 100).toFixed(2) : '--';
        },
      },
      {
        field: 'statusMessage',
        headerName: 'Status Details',
        flex: 0.4,
        valueGetter: (params) => {
          if (params.row.details.status === AsyncJobStatus.Error)
            // At the moment the only server-side status we have for DQ is an error message
            return params.row.details.statusMessage ?? '';

          let details = params.row?.summaryDetails;
          return details ? getJobStatusMsg(details) : params.row.statusMessage ?? '';
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
      <StyledHeader variant="h1">Data Quality Validation Jobs</StyledHeader>
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

type TVJSDE = Partial<TwinValidationJobSummaryDetailErrors>;

function getErrorsRollup(jobSummary: ITwinValidationJobSummaryDetails): TVJSDE | undefined {
  const reducer = (acc: TVJSDE, cur: TVJSDE): TVJSDE => ({
    numOK: acc.numOK! + cur.numOK!,
    numPropertyErrors: acc.numPropertyErrors! + cur.numPropertyErrors!,
    numRelationshipErrors: acc.numRelationshipErrors! + cur.numRelationshipErrors!,
    numUnitErrors: acc.numUnitErrors! + cur.numUnitErrors!,
  });

  if (!jobSummary?.errorsByModel) return undefined;

  const init: TVJSDE = {
    numOK: 0,
    numPropertyErrors: 0,
    numRelationshipErrors: 0,
    numUnitErrors: 0,
  };

  return Object.values(jobSummary!.errorsByModel!).reduce(reducer, init);
}

function getErrorPercent(jobSummary: ITwinValidationJobSummaryDetails): number | undefined {
  const allModelsErrs = getErrorsRollup(jobSummary);
  if (!allModelsErrs) return undefined;
  const allErrCount =
    allModelsErrs.numPropertyErrors! + allModelsErrs.numRelationshipErrors! + allModelsErrs.numUnitErrors!;
  const total = allErrCount + allModelsErrs.numOK!;
  return total ? allErrCount / total : 0;
}

function getJobStatusMsg(jobSummary: ITwinValidationJobSummaryDetails): string | undefined {
  const allModelsErrs = getErrorsRollup(jobSummary);
  if (!allModelsErrs) return undefined;
  // Removed the display of NumOk twins as it's confusing to the user's that Ok's+Err's don't sum m to twins due to multiple rules and checkTypes'
  const msg = `Checked ${jobSummary.processedEntities} Twins:  ${allModelsErrs.numPropertyErrors} Prop Errors, ${allModelsErrs.numRelationshipErrors} Rel Errors`;
  return msg;
}
