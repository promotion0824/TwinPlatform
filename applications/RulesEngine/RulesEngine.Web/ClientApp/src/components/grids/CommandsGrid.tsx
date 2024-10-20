import { Box, Stack } from '@mui/material';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, CommandDtoBatchDto, FileResponse } from '../../Rules';
import compareStringsLexNumeric from '../AlphaNumericSorter';
import { GetCommandTypeFilter, GetCommandTypeText } from '../commands/CommandTypeFormatter';
import { ExportToCsv } from '../ExportToCsv';
import { CommandLinkFormatter, CommandSyncFormatter, DateFormatter, IsTriggeredFormatter, IsValidTriggerFormatter, RuleLinkFormatter, TwinLinkFormatterById } from '../LinkFormatters';
import { buildCacheKey, doubleOperators, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, singleSelectOperators, stringOperators } from './GridFunctions';


/**
 * Properties for a grid of insights
 * */
interface ICommandsGridProps {
  invokeQuery: (request: BatchRequestDto) => Promise<CommandDtoBatchDto>,
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

const CommandsGrid = ({ query }: { query: ICommandsGridProps }) => {
  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_CommandsTable_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_${query.key}_CommandsTable_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_${query.key}_CommandsTable_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_CommandsTable_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'RuleId', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { Id: false, CommandId: false, LastSyncDate: false, TwinId: false, EquipmentId: false, ConnectorId: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const [columnsLoaded, setColumnsLoaded] = useState(false);

  const createRequest = function () {
    const request = new BatchRequestDto();
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1; //MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const fetchCommands = async () => {
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
  } = useQuery(['commands', query.key, paginationModel, sortModel, filters], async () => fetchCommands(), { keepPreviousData: true })

  // Define columns for command
  let initialColumns: GridColDef[] =
    [
      {
        field: 'IsValid', type: 'boolean', headerName: 'Valid', width: 100, cellClassName: "MuiDataGrid-cell--textCenter",
        renderCell: (params: any) => { return IsValidTriggerFormatter(params.row!.isValid); },
      },
      {
        field: 'IsTriggered', type: 'boolean', headerName: 'Triggered', width: 100, cellClassName: "MuiDataGrid-cell--textCenter",
        renderCell: (params: any) => { return IsTriggeredFormatter(params.row!.isTriggered); },
      },
      {
        field: 'CommandName', headerName: 'Command', flex: 1, minWidth: 100,
        sortComparator: (v1: any, v2: any, _param1: any, _param2: any) =>
          compareStringsLexNumeric(v1?.toString() ?? "", v2?.toString() ?? ""),
        renderCell: (params: any) => { return CommandLinkFormatter(params.row!); },
        filterOperators: stringOperators()
      },
      {
        field: 'EquipmentName', headerName: 'Equipment', flex: 1, minWidth: 150,
        valueGetter: (params: any) => params.row.twinId,
        renderCell: (params: any) => { return TwinLinkFormatterById(params.row!.equipmentId, params.row!.equipmentName); },
        filterOperators: stringOperators()
      },
      {
        field: 'RuleName', headerName: 'Skill', flex: 2, minWidth: 250,
        sortComparator: (v1: any, v2: any, _param1: any, _param2: any) =>
          compareStringsLexNumeric(v1?.toString() ?? "", v2?.toString() ?? ""),
        renderCell: (params: any) => { return RuleLinkFormatter(params.row!.ruleId, params.row!.ruleName); },
        filterOperators: stringOperators()
      },
      {
        field: 'EquipmentId', headerName: 'Equipment Id', flex: 2, minWidth: 250,
        valueGetter: (params: any) => params.row.equipmentId,
        filterOperators: stringOperators()
      },
      {
        field: 'CommandType', headerName: 'Type', width: 100, type: "singleSelect", filterOperators: singleSelectOperators(), valueOptions: () => { return GetCommandTypeFilter(); },
        renderCell: (params: any) => {
          return GetCommandTypeText(params.row!.commandType);
        }
      },
      {
        field: 'Value', headerName: 'Value', width: 100,
        valueGetter: (params: any) => params.row!.value,
        filterOperators: doubleOperators()
      },
      {
        field: 'Unit', headerName: 'Units', width: 100, sortable: false,
        valueGetter: (params: any) => params.row!.unit,
        filterOperators: stringOperators()
      },
      {
        field: 'StartTime', headerName: 'Set Start time', flex: 1, minWidth: 100, filterable: false,
        renderCell: (params: any) => {
          return DateFormatter(params.row!.startTime);
        }
      },
      {
        field: 'EndTime', headerName: 'Set End time', flex: 1, minWidth: 100, filterable: false,
        renderCell: (params: any) => {
          return DateFormatter(params.row!.endTime);
        }
      },
      {
        field: 'LastSyncDate', headerName: 'Last Synced', flex: 2, minWidth: 250, filterable: false,
        renderCell: (params: any) => {
          return DateFormatter(params.row!.lastSyncDate);
        }
      },
      {
        field: 'Enabled', headerName: 'Sync', width: 65, type: 'boolean',
        renderCell: (params: any) => { return CommandSyncFormatter(params.row!.enabled); }
      },
      {
        field: 'TwinName', headerName: 'Point', flex: 2, minWidth: 250,
        valueGetter: (params: any) => params.row.twinId,
        renderCell: (params: any) => { return TwinLinkFormatterById(params.row!.twinId, params.row!.twinName); },
        filterOperators: stringOperators()
      },
      {
        field: 'ConnectorId', headerName: 'Connector Id', minWidth: 150,
        valueGetter: (params: any) => params.row.connectorId,
        filterOperators: stringOperators()
      },
      {
        field: 'ExternalId', headerName: 'External Id', flex: 2, minWidth: 250,
        valueGetter: (params: any) => params.row.externalId,
        filterOperators: stringOperators()
      },
      {
        field: 'TwinId', headerName: 'Twin Id', flex: 2, minWidth: 250,
        valueGetter: (params: any) => params.row.twinId,
        filterOperators: stringOperators()
      },
      {
        field: 'CommandId', headerName: 'Command Id', flex: 2, minWidth: 250,
        valueGetter: (params: any) => params.row.commandId,
        filterOperators: stringOperators()
      },
      {
        field: 'Id', headerName: 'Id', flex: 2, minWidth: 250,
        valueGetter: (params: any) => params.row.id,
        filterOperators: stringOperators()
      }
    ];

  const [columns, setColumnState] = useState(initialColumns);

  useEffect(() => {
    if (data === undefined) {
      return;
    }

    if (columnsLoaded === false) {
      setColumnsLoaded(true);
      setColumnState(initialColumns);
      setColumnVisibilityModel(columnVisibilityModel);
    }
  }, [data]);

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

export default CommandsGrid;
