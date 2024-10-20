import RefreshOutlinedIcon from '@mui/icons-material/RefreshOutlined';
import { Box, Button, Stack, Typography } from '@mui/material';
import { DataGridPro, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { Tooltip } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { useQuery, useQueryClient } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from "../../hooks/useLocalStorage";
import { BatchRequestDto, CalculatedPointDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { CapbilityValidFormatter, DateFormatter, ModelFormatter2, TwinLinkFormatterById } from '../LinkFormatters';
import { ADTActionRequiredLookup, ADTActionStatusLookup, CalculatedPointSourceLookup } from '../Lookups';
import { FormatLocations } from '../StringOptions';
import StyledLink from '../styled/StyledLink';
import { buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, singleSelectOperators, stringOperators } from './GridFunctions';

interface CalculatedPointsGridProps {
  ruleId: string,
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const CalculatedPointRefFormatter = (re: CalculatedPointDto) =>
  (<StyledLink to={"/calculatedpoint/" + encodeURIComponent(re.id!)}> {re.id}</StyledLink >);

const CalculatedPointsGrid = ({ query }: { query: CalculatedPointsGridProps }) => {

  const ruleId = query.ruleId;

  const filterKey = buildCacheKey(`${query.pageId}_${ruleId}_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'Id', sort: 'asc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { ActionRequired: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const createRequest = function () {
    const request = new BatchRequestDto();
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1;//MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const api = useApi();
  const queryClient = useQueryClient();

  const fetchCalculatedPoints = async () => {
    const request = createRequest();
    return api.getCalculatedPointsAfter(ruleId, request);
  };

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return api.exportCalculatedPointsAfter(ruleId, request);
    },
    createBatchRequest: createRequest
  };

  const handleRefresh = () => {
    queryClient.invalidateQueries(['calculatedPoints'], { exact: true });
    refetch();
  };

  const {
    isLoading,
    data,
    isFetching,
    isError,
    refetch 
  } = useQuery(['calculatedPoints', ruleId, paginationModel, sortModel, filters], async () => await fetchCalculatedPoints(), { keepPreviousData: true })

  // Define columns
  const columns = [
    {
      field: 'Valid', headerName: 'Valid', width: 100, filterable: false, cellClassName: "MuiDataGrid-cell--textCenter",
      renderCell: (params: any) => { return CapbilityValidFormatter(params.row!); }
    },
    {
      field: 'TriggerCount', headerName: 'Triggers', width: 100, filterable: false, sortable: false,
      renderCell: (params: any) => { return params.row!.triggerCount; }
    },
    {
      field: 'Id', headerName: 'Id', flex: 1, minWidth: 300,
      renderCell: (params: any) => { return CalculatedPointRefFormatter(params.row!); },
      filterOperators: stringOperators()
    },
    {
      field: 'Name', headerName: 'Skill', flex: 1, minWidth: 300,
      renderCell: (params: any) => { return (params.row!.ruleId ? <StyledLink to={"/rule/" + encodeURIComponent(params.row!.ruleId!)}>{params.row!.name}</StyledLink> : '-') },
      filterOperators: stringOperators()
    },
    {
      field: 'ModelId', headerName: 'Model', flex: 1, minWidth: 150,
      valueGetter: (params: any) => params.row.modelId,
      renderCell: (params: any) => { return params.row.modelId ? ModelFormatter2({ modelId: params.row.modelId }) : '-'; },
      filterOperators: stringOperators()
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
    {
      field: 'IsCapabilityOf', headerName: 'Is Capability Of', flex: 1, minWidth: 150,
      valueGetter: (params: any) => params.row.isCapabilityOf,
      renderCell: (params: any) => { return params.row.isCapabilityOf ? TwinLinkFormatterById(params.row.isCapabilityOf, params.row.isCapabilityOf) : '-'; },
      filterOperators: stringOperators()
    },
    {
      field: 'Unit', headerName: 'Unit', width: 100,
      renderCell: (params: any) => { return params.row.unit },
      filterOperators: stringOperators()
    },
    {
      field: 'TimeZone', headerName: 'Time Zone', flex: 1, minWidth: 150,
      renderCell: (params: any) => { return params.row.timeZone },
      filterOperators: stringOperators()
    },
    {
      field: 'LastSyncDateUTC', headerName: 'Last Sync Date UTC', width: 160, filterable: false,
      renderCell: (params: any) => { return DateFormatter(params.row.lastSyncDateUTC!); }
    },
    {
      field: 'Source', headerName: 'Source', flex: 1, minWidth: 100, type: 'singleSelect', filterOperators: singleSelectOperators(),
      valueOptions: () => { return CalculatedPointSourceLookup.GetSourceFilter(); },
      renderCell: (params: any) => { return CalculatedPointSourceLookup.getSourceString(params.row.source); }
    },
    {
      field: 'ActionRequired', headerName: 'ADT Action', width: 100, type: 'singleSelect', filterOperators: singleSelectOperators(),
      valueOptions: () => { return ADTActionRequiredLookup.GetActionFilter(); },
      renderCell: (params: any) => { return ADTActionRequiredLookup.getActionString(params.row.actionRequired); }
    },
    {
      field: 'ActionStatus', headerName: 'ADT Status', flex: 1, minWidth: 100, type: 'singleSelect', filterOperators: singleSelectOperators(),
      valueOptions: () => { return ADTActionStatusLookup.GetStatusFilter(); },
      renderCell: (params: any) => { return ADTActionStatusLookup.getStatusString(params.row.actionStatus); }
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
  }, [data, data?.total, setRowCountState]);

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
                <Button variant="text" onClick={() => handleRefresh()} disabled={isLoading || isFetching} color="primary">
                  <RefreshOutlinedIcon sx={{ mr: 1, fontSize: "medium" }} />
                  Refresh
                </Button>
              </Box>
            </GridToolbarContainer>
          ),
          noRowsOverlay: () => (
            <Stack margin={2}>
              {isError ? 'An error occured...' : 'No rows to display. For new points run an ADT Cache Update and a Rebuild Points afterwards.'}
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


export default CalculatedPointsGrid;
