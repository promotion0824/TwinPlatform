import { Box, Typography } from '@mui/material';
import { Stack } from '@mui/material';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { Tooltip } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, FileResponse, InsightDtoBatchDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { GetInsightStatusFilter, InsightStatusFormatter } from '../insights/InsightStatusFormatter';
import { InsightFaultyFormatter, InsightLastFaultedFormatter, InsightLinkFormatterOnRuleName } from '../LinkFormatters';
import { FormatLocations } from '../StringOptions';
import { buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, singleSelectOperators, stringOperators } from './GridFunctions';

/**
 * Properties for a grid of insights
 * */
interface InsightsGridProps {
  invokeQuery: (request: BatchRequestDto) => Promise<InsightDtoBatchDto>,
  downloadCsv: (request: BatchRequestDto) => Promise<FileResponse>,
  /**
   * The id of the model or equipment that should be used for caching the response
   */
  key: string,
  /**
   * The id of the page that is making the request so that sort and filters are remembered per page
   */
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

/**
 * Displays a grid of insights
 * */
const InsightsGrid = ({ query }: { query: InsightsGridProps }) => {

  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_InsightsGrid_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_${query.key}_InsightsGrid_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_${query.key}_InsightsGrid_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_InsightsGrid_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'LastFaultedDate', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, {});
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const createRequest = function () {
    const request = new BatchRequestDto();
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1;//MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const fetchInsights = async () => {
    const request = createRequest();
    return query.invokeQuery(request);
  }

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return query.downloadCsv(request);
    },
    createBatchRequest: createRequest
  };

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['insightswithfilter', query.key, paginationModel, sortModel, filters], async () => await fetchInsights(), { keepPreviousData: true })

  const columns: GridColDef[] = [
    {
      field: 'IsFaulty', headerName: 'Faulty', width: 70, type: 'boolean', renderCell: (params: any) => { return InsightFaultyFormatter(params.row!); }
    },
    {
      field: 'LastFaultedDate', headerName: 'Last Faulted', width: 160, filterable: false,
      renderCell: (params: any) => { return InsightLastFaultedFormatter(params.row!); }
    },
    {
      field: 'Status', headerName: 'Status', width: 100, type: "singleSelect", filterOperators: singleSelectOperators(), valueOptions: () => { return GetInsightStatusFilter(); },
      renderCell: (params: any) => { return InsightStatusFormatter(params.row!); }
    },
    {
      field: 'RuleName', headerName: 'Insight', flex: 3, minWidth: 300,
      renderCell: (params: any) => { return InsightLinkFormatterOnRuleName(params.row!); },
      filterOperators: stringOperators()
    },
    {
      field: 'EquipmentName', headerName: 'Equipment', flex: 2, minWidth: 250,
      valueGetter: (params: any) => params.row.equipmentName,
      filterOperators: stringOperators()
    },
    {
      field: 'TwinLocations', headerName: 'Location', flex: 2, minWidth: 300, sortable: false,
      renderCell: (params: any) => {
        var location = FormatLocations(params.row.locations);
        return (
          <Tooltip label={location} position='bottom' multiline>
            <Typography variant='body2'>{location}</Typography>
          </Tooltip>);
      },
      filterOperators: stringOperators()
    },
    {
      field: 'TimeZone', headerName: 'TimeZone', flex: 1, minWidth: 180,
      valueGetter: (params: any) => params.row.timeZone,
      filterOperators: stringOperators()
    },
    {
      field: 'Text', headerName: 'Description', flex: 3, minWidth: 250, filterOperators: stringOperators(),
      valueGetter: (params: any) => params.row.text,
    }
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
    </Box>);
}

export default InsightsGrid;
