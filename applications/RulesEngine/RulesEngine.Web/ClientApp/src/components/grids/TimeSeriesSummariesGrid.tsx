import { Box, Stack, Typography } from '@mui/material';
import { DataGridPro, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { Tooltip } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, TimeSeriesDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { DateFormatter, ModelFormatter2 } from '../LinkFormatters';
import { FormatLocations } from '../StringOptions';
import StyledLink from '../styled/StyledLink';
import { GetTimeSeriesStatusFilter, TimeSeriesStatusFormatter, TimeSeriesStatusFormatterLegend } from '../TimeSeriesStatusFormatter';
import { buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, numberOperators, singleSelectOperators, stringOperators } from './GridFunctions';

interface ITimeSeriesSummariesGridProps {
  key: string,
  pageId: string
}

// Link to the twin if we know it, otherwise link to search
const LinkForDtId = (a: TimeSeriesDto) =>
  a.dtId ?
    (<StyledLink to={"/equipment/" + encodeURIComponent(a.dtId!)}>{a.dtId}</StyledLink>) :
    (<StyledLink to={"/search/?query=" + encodeURIComponent(a.externalId!)}>Missing</StyledLink >);

const LinkForId = (a: TimeSeriesDto) =>
  a.dtId ?
    (<StyledLink to={"/equipment/" + encodeURIComponent(a.dtId!)}> {a.id!}</StyledLink>) :
    (<StyledLink to={"/search/?query=" + encodeURIComponent(a.externalId!)}> {a.externalId!}</StyledLink >);

const stuckFormatter = (a: TimeSeriesDto, child: string) =>
  (Math.abs(a.minValue! - a.maxValue!) < 0.00001) ? <span style={{ color: 'red' }}>{child}</span> : <span>{child}</span>;

const timePeriodFormatter = (seconds: number | undefined) =>
  (seconds === null || seconds === undefined) ? '--' :
    seconds < 90 ? `${seconds.toFixed(1)} s` :
      seconds < 90 * 60 ? `${(seconds / 60.0).toFixed(1)} min` :
        seconds < 90 * 60 * 3 ? `${(seconds / 60.0 / 60.0).toFixed(1)} hr` :
        `${(seconds / 60.0 / 60.0 / 24).toFixed(1)} d`;

const zeroWarnFormatter = (a: number) =>
  (a === 0) ? <span style={{ color: 'red' }}>{a}</span> : <span>{a}</span>;

const colorFormatter = (b: boolean, child: string) =>
  (b) ? <span style={{ color: 'red' }}>{child}</span> :
    <span>{child}</span>;

const PeriodFormatter = (p: any) => colorFormatter(p.row.isPeriodOutOfRange, timePeriodFormatter(p.row.estimatedPeriod));
const TrendIntervalFormatter = (p: any) => timePeriodFormatter(p.row.trendInterval);

const MinFormatter = (p: any) => stuckFormatter(p.row, p.row.minValue.toFixed(1));
const AverageFormatter = (p: any) => p.row.averageValue ? stuckFormatter(p.row, p.row.averageValue.toFixed(1)) : '-';
const MaxFormatter = (p: any) => stuckFormatter(p.row, p.row.maxValue.toFixed(1));
const TotalValuesProcessedFormatter = (p: any) => zeroWarnFormatter(p.row.totalValuesProcessed);
const UnitOfMeasureFormatter = (p: any) => p.row.unitOfMeasure;

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

/**
 * Displays a grid of time series summaries
 * */
const TimeSeriesSummariesGrid = ({ query }: { query: ITimeSeriesSummariesGridProps }) => {

  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_TimeSeriesSummariesGrid_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_TimeSeriesSummariesGrid_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_TimeSeriesSummariesGrid_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_TimeSeriesSummariesGrid_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'id', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { Id: false, maxTimeToKeep: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const apiclient = useApi();

  const createRequest = function () {
    const request = new BatchRequestDto();
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1;//MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const fetchInstances = async () => {
    const request = createRequest();
    return apiclient.getTimeSeriesSummaries(request);
  }

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportTimeSeriesSummaries(request);
    },
    createBatchRequest: createRequest
  };

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['timeSeriesSummaries', paginationModel, sortModel, filters], async () => fetchInstances(), { keepPreviousData: true })

  const columns = [
    {
      field: 'Status', headerName: 'Status', width: 100, type: "singleSelect", cellClassName: "MuiDataGrid-cell--textCenter",
      valueOptions: () => { return GetTimeSeriesStatusFilter(); }, filterOperators: singleSelectOperators(),
      renderCell: (p: any) => (TimeSeriesStatusFormatter(p.row!))
    },
    {
      field: 'ModelId', headerName: 'Model', flex: 2, minWidth: 320,
      renderCell: (params: any) => ModelFormatter2(params.row),
      filterOperators: stringOperators()
    },
    {
      field: 'DtId', headerName: 'DtId', flex: 2, minWidth: 320, filterOperators: stringOperators(),
      renderCell: (p: any) => { return LinkForDtId(p.row); },
    },
    {
      field: 'Id', headerName: 'Id', flex: 2, minWidth: 320, filterOperators: stringOperators(),
      renderCell: (p: any) => { return LinkForId(p.row); },
    },
    {
      field: 'LastSeen', headerName: 'Last Seen', width: 160, filterable: false,
      renderCell: (params: any) => { return DateFormatter(params.row!.endTime); }
    },
    {
      field: 'TwinLocations', headerName: 'Location', flex: 2, minWidth: 300, sortable: false,
      renderCell: (params: any) => {
        var location = FormatLocations(params.row.twinLocations);
        return (
          <Tooltip label={location} position='bottom' multiline>
            <Typography variant='body2'>{location}</Typography>
          </Tooltip>);
      },
      filterOperators: stringOperators()
    },
    { field: 'TotalValuesProcessed', headerName: 'Count', flex: 1, minWidth: 160, renderCell: TotalValuesProcessedFormatter, filterOperators: numberOperators() },
    { field: 'EstimatedPeriod', headerName: 'Period', flex: 1, minWidth: 160, filterable: false, renderCell: PeriodFormatter },
    { field: 'Latency', headerName: 'Latency', flex: 1, minWidth: 160, filterable: false, renderCell: (params: any) => timePeriodFormatter(params.row!.latency) },
    { field: 'TrendInterval', headerName: 'Expected', flex: 1, minWidth: 160, filterable: false, renderCell: TrendIntervalFormatter },
    { field: 'MinValue', headerName: 'Min', flex: 1, minWidth: 160, renderCell: MinFormatter, filterOperators: numberOperators() },
    { field: 'AverageValue', headerName: 'Average', flex: 1, minWidth: 160, renderCell: AverageFormatter, filterOperators: numberOperators() },
    { field: 'MaxValue', headerName: 'Max', flex: 1, minWidth: 160, renderCell: MaxFormatter, filterOperators: numberOperators() },
    { field: 'UnitOfMeasure', headerName: 'Unit', flex: 1, minWidth: 160, renderCell: UnitOfMeasureFormatter, filterOperators: stringOperators() },
    { field: 'maxTimeToKeep', headerName: 'TimeToKeep', flex: 1, minWidth: 160, filterable: false, renderCell: (params: any) => timePeriodFormatter(params.row!.maxTimeToKeep)  }
  ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  const handleFilterChange = (newModel: GridFilterModel) => {
    setFilters(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(data?.total || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => data?.total !== undefined ? data?.total : prevRowCountState);
  }, [data?.total, setRowCountState]);

  return (
    <div>
      <Box sx={{ flex: 1 }}>
        <DataGridPro
          autoHeight
          loading={isLoading || isFetching}
          rows={data?.items || []}
          rowCount={rowCountState}
          pageSizeOptions={gridPageSizes()}
          columns={columns}
          pagination
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          paginationMode="server"
          sortingMode="server"
          filterMode="server"
          onSortModelChange={handleSortModelChange}
          onFilterModelChange={handleFilterChange}
          sortingOrder={sortingOrder}
          columnVisibilityModel={columnVisibilityModel}
          onColumnVisibilityModelChange={(newModel: any) => setColumnVisibilityModel(newModel)}
          disableRowSelectionOnClick
          hideFooterSelectedRowCount
          initialState={{
            filter: {
              filterModel: { ...filters },
            },
            sorting: {
              sortModel: [...sortModel]
            }
          }}
          slots={{
            toolbar: () => (
              <GridToolbarContainer>
                <Box sx={{ display: 'flex', flexGrow: 1, gap: 2 }}>
                  <GridToolbarColumnsButton />
                  <GridToolbarFilterButton />
                  <ExportToCsv source={csvExport} />
                </Box>
              </GridToolbarContainer>
            ),
            noRowsOverlay: () => (
              <Stack margin={2}>
                {isError ? 'An error occured...' : 'No rows to display.'}
              </Stack>
            ),
          }}
          sx={{
            '& .MuiDataGrid-row:hover': {
              backgroundColor: "transparent"
            },
            '& .MuiDataGrid-cell:focus': {
              outline: ' none'
            }
          }}
        />
      </Box>
      <TimeSeriesStatusFormatterLegend />
    </div>);
}

export default TimeSeriesSummariesGrid;
