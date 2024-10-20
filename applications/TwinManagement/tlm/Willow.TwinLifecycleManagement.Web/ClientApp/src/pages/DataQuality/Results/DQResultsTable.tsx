import { useState, useEffect, useMemo } from 'react';
import { GridColDef, gridClasses, useGridApiRef } from '@mui/x-data-grid-pro';
import { Box, styled, Paper, CircularProgress } from '@mui/material';
import { CustomToolBar } from '../../../components/Grid/CustomToolBar';
import { useDataQaulity } from '../DQProvider';
import { parseResultInfo } from './utils/parseResultInfo';
import { Result } from '../../../services/Clients';
import { DqResultTypeChip } from '../../../components/DqResultTypeChip';
import useOntology from '../../../hooks/useOntology/useOntology';
import useLocations from '../../../hooks/useLocations';
import { usePersistentGridState } from '../../../hooks/usePersistentGridState';
import { DataGrid } from '@willowinc/ui';

/**
 * This component displays the active validation results table.
 */
export default function DQResultsTable() {
  const { data: ontology, isLoading: isOntologyLoading, isSuccess: isOntologySuccess } = useOntology();
  const { data: locations, isSuccess: isLocationsSuccess } = useLocations();
  const isDependenciesLoaded = isLocationsSuccess && isOntologySuccess;
  const apiRef = useGridApiRef();
  const { savedState } = usePersistentGridState(apiRef, 'dq-results', isDependenciesLoaded);

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'ruleId',
        headerName: 'Rule Id',
        flex: 0.35,
        renderCell: ({ value }) => <StyledBox title={value}>{value}</StyledBox>,
      },
      {
        field: 'twinDtId',
        headerName: 'Twin Id',
        flex: 0.4,
        renderCell: ({ value }) => <StyledBox title={value}>{value}</StyledBox>,
      },
      {
        field: 'modelId',
        headerName: 'Model',
        flex: 0.35,
        valueGetter: ({ value }) => ontology?.getModelById(value)?.name || '',
        renderCell: ({ value }) => <StyledBox title={value}>{value}</StyledBox>,
      },
      {
        field: 'name',
        headerName: 'Twin Name',
        flex: 0.4,
        renderCell: ({ value }) => <StyledBox title={value}>{value}</StyledBox>,
        valueGetter: (params) => params.row?.twinInfo?.name,
      },
      {
        field: 'uniqueID',
        headerName: 'Twin UniqueID',
        flex: 0.4,
        renderCell: ({ value }) => <StyledBox title={value}>{value}</StyledBox>,
        valueGetter: (params) => {
          return params.row?.twinIdentifiers?.uniqueId ?? params.row?.twinInfo?.locations?.uniqueID;
        },
      },
      {
        field: 'siteName',
        headerName: 'Site Name',
        flex: 0.4,
        valueGetter: (params: any) =>
          locations?.getLocationById(params.row?.twinInfo?.locations?.siteId)?.twin?.name || '',
        sortable: false,
      },
      {
        field: 'siteId',
        headerName: 'Site ID',
        flex: 0.4,
        renderCell: ({ value }) => <StyledBox title={value}>{value}</StyledBox>,
        valueGetter: (params) => params.row?.twinInfo?.locations?.siteId,
      },

      {
        field: 'resultType',
        headerName: 'Result Type',
        flex: 1,
        sortable: false,
        renderCell: ({ row }) => <DqResultTypeChip row={row} />,
        type: 'singleSelect',
        valueOptions: Object.values(Result),
      },
      {
        field: 'resultInfo',
        headerName: 'Result Details',
        flex: 2,
        type: 'text',
        sortable: false,
        renderCell: ({ row }) => {
          const resultInfo = parseResultInfo(row);
          return (
            <StyledBox>
              <StyledUl>
                {resultInfo.map((errors, i) => (
                  <StyledLi key={`${i}`}>{errors}</StyledLi>
                ))}
              </StyledUl>
            </StyledBox>
          );
        },
      },
      {
        field: 'checkTime',
        headerName: 'Check Time',
        flex: 0.4,
        sortComparator: (x, y) => new Date(x).getTime() - new Date(y).getTime(),
        valueGetter: (params) =>
          new Date(params.row?.runInfo.checkTime)?.toLocaleDateString('en-US', {
            weekday: 'short',
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: 'numeric',
            minute: 'numeric',
            timeZoneName: 'short',
          }),
      },
    ],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [isOntologySuccess, isLocationsSuccess]
  );

  const {
    getDQResultsQuery,
    continuationToken,
    setContinuationToken,
    errorsOnly,
    setDQResultsPageSize,
    DQResultsPageSize,
    selectedFilterLocation,
    selectedFilterModels,
    searchString,
  } = useDataQaulity();

  const { data, isLoading: isDQResultsLoading } = getDQResultsQuery;

  const resultsData = data?.content ?? [];

  const [page, setPage] = useState(0);
  const [rowCount, setRowCount] = useState<number>(0);

  let parsedContinuationToken =
    continuationToken !== ''
      ? JSON.parse(continuationToken)
      : data?.continuationToken && JSON.parse(data.continuationToken);

  const { Total } = parsedContinuationToken || {};

  useEffect(() => {
    setRowCount((prevRowCount: number | undefined) => {
      return Total ? Total : resultsData?.length;
    });
  }, [Total, resultsData?.length]);

  // Reset page when filters is changed
  useEffect(() => {
    setPage(0);
  }, [errorsOnly, DQResultsPageSize, selectedFilterLocation, selectedFilterModels, searchString]);

  useEffect(() => {
    // todo: abort previous requests when new page is selected.
    if (!!parsedContinuationToken) {
      parsedContinuationToken.NextPage = page;

      setContinuationToken(JSON.stringify(parsedContinuationToken));
    } else {
      // case when we're on the last page, endpoint does not return continuationToken, so use previous state to get the previous page
      setContinuationToken((prevState: string) => {
        return prevState;
      });
    }
    setPage(page);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page]);

  const isGridLoading = isOntologyLoading || isDQResultsLoading;
  return (
    <div style={{ height: '63vh', width: '100%', backgroundColor: '#242424', marginTop: 10 }}>
      {isDependenciesLoaded ? (
        <StripedDataGrid
          apiRef={apiRef}
          initialState={savedState}
          getEstimatedRowHeight={() => 100}
          getRowHeight={() => 'auto'}
          rows={resultsData}
          columns={columns}
          slots={{ toolbar: CustomToolBar }}
          loading={isGridLoading}
          pagination
          paginationMode="server"
          pageSizeOptions={[250, 500, 1000]}
          paginationModel={{ page, pageSize: DQResultsPageSize }}
          onPaginationModelChange={(pageModel) => {
            let { page, pageSize } = pageModel;
            setDQResultsPageSize(pageSize);
            setPage(page);
          }}
          rowCount={rowCount}
          getRowClassName={(params) => (params.indexRelativeToCurrentPage % 2 === 0 ? 'even' : 'odd')}
        />
      ) : (
        <Paper
          variant="outlined"
          sx={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
        >
          <CircularProgress />
        </Paper>
      )}
    </div>
  );
}

const StyledUl = styled('ul')({ padding: 0, margin: 0 });
const StyledLi = styled('li')({
  display: 'flex',
  '&:before': {
    content: '"•"', // ■  • ☒
    marginRight: '1em',
  },
});

const StyledBox = styled(Box)({ overflowWrap: 'anywhere', width: '100%' });

const StripedDataGrid = styled(DataGrid)(({ theme }) => ({
  [`& .${gridClasses.row}.even`]: {
    backgroundColor: '#333539',
  },
}));
