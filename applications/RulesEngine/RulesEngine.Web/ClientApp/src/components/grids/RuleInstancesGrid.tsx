import { Box, Stack, Typography } from '@mui/material';
import { DataGridPro, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { Tooltip } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, FileResponse, RuleInstanceDto, RuleInstanceDtoBatchDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { ValidFormatter } from '../LinkFormatters';
import { RuleInstanceStatusLookup } from '../Lookups';
import { FormatLocations } from '../StringOptions';
import StyledLink from '../styled/StyledLink';
import { buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, singleSelectOperators, stringOperators } from './GridFunctions';

interface RuleInstancesProps {
  invokeQuery: (request: BatchRequestDto) => Promise<RuleInstanceDtoBatchDto>,
  downloadCsv: (request: BatchRequestDto) => Promise<FileResponse>,
  key: string,
  /**
   * The id of the page that is making the request so that sort and filters are remembered per page
   */
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];
const LinkToRuleInstanceFormatter = (a: RuleInstanceDto) => (<StyledLink to={"/ruleinstance/" + encodeURIComponent(a.id!)}> {a.id}</StyledLink >);
const LinkToInsightFormatter = (a: RuleInstanceDto) => (<StyledLink to={"/insight/" + encodeURIComponent(a.id!)}> {a.id}</StyledLink >);

/**
 * Displays a grid of skill instances
 * */
const RuleInstancesGrid = ({ query }: { query: RuleInstancesProps }) => {

  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_RuleInstancesGrid_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_${query.key}_RuleInstancesGrid_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_${query.key}_RuleInstancesGrid_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_RuleInstancesGrid_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'EquipmentId', sort: 'desc' }]);
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

  const fetchRuleInstances = async () => {
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
  } = useQuery(['ruleInstances', query.key, paginationModel, filters, sortModel], async () => fetchRuleInstances(), { keepPreviousData: true })

  const columns = [
    {
      field: 'Status', headerName: 'Status', width: 100, type: "singleSelect", cellClassName: "MuiDataGrid-cell--textCenter",
      valueOptions: () => { return RuleInstanceStatusLookup.GetStatusFilter(); },
      filterOperators: singleSelectOperators(),
      renderCell: (p: any) => { return ValidFormatter(p.row); }
    },
    {
      field: 'Id', headerName: 'Id', flex: 2, minWidth: 300,
      renderCell: (p: any) => { return LinkToRuleInstanceFormatter(p.row); },
      filterOperators: stringOperators(),
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
      field: 'insight', headerName: 'Insight', flex: 2, minWidth: 300, filterable: false,
      renderCell: (p: any) => { return LinkToInsightFormatter(p.row); },
    },
    { field: 'TriggerCount', headerName: 'Triggers', flex: 1, minWidth: 160 }
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

export default RuleInstancesGrid;
