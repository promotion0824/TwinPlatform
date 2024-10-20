import { Box, Stack, Typography } from '@mui/material';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, GridComparatorFn, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { Tooltip } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, TwinDtoContentType } from '../../Rules';
import compareStringsLexNumeric from '../AlphaNumericSorter';
import { ExportToCsv } from '../ExportToCsv';
import { ModelFormatter2, RelatedEntityFormatter } from '../LinkFormatters';
import { FormatLocations } from '../StringOptions';
import { buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, numberOperators, stringOperators } from './GridFunctions';

interface TwinsByModelGridProps {
  modelId: string
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const TwinsByModelGrid = ({ props }: { props: TwinsByModelGridProps }) => {
  const filterKey = buildCacheKey(`${props.pageId}_${props.modelId}_TwinsByModelGrid_FilterModel`);
  const sortKey = buildCacheKey(`${props.pageId}_TwinsByModelGrid_SortModel`);
  const colsKey = buildCacheKey(`${props.pageId}_TwinsByModelGrid_ColumnModel`);
  const paginationKey = buildCacheKey(`${props.pageId}_TwinsByModelGrid_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'name', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, {});
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const api = useApi();

  const createRequest = function () {
    const request = new BatchRequestDto();
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1;//MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const fetchTwins = async () => {
    const request = createRequest();
    return await api.twinsByModel(props.modelId, request);
  }

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['TwinsByModelGrid', props.modelId, paginationModel, sortModel, filters], async () => await fetchTwins(), { keepPreviousData: true })

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return api.exportTwinsByModel(props.modelId, request);
    },
    createBatchRequest: createRequest
  };

  const nameSortComparator: GridComparatorFn<string> = (v1, v2) =>
    compareStringsLexNumeric(v1?.toString() ?? "", v2?.toString() ?? "");

  const initialColumns: GridColDef[] =
    [
      {
        field: 'name', headerName: 'Name', flex: 2, minWidth: 250,
        renderCell: (params: any) => { return RelatedEntityFormatter(params.row); },
        sortComparator: nameSortComparator, filterOperators: stringOperators()
      },
      {
        field: 'Id', headerName: 'Id', flex: 2, minWidth: 250, valueGetter: (params: any) => params.row.id, filterOperators: stringOperators()
      },
      {
        field: 'ModelId', headerName: 'ModelId', flex: 2, minWidth: 240,
        renderCell: (params: any) => { return ModelFormatter2(params.row); },
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
        field: 'position', headerName: 'Position', flex: 1, minWidth: 100, filterOperators: stringOperators()
      },
      {
        field: 'description', headerName: 'Description', flex: 2, minWidth: 250, filterOperators: stringOperators()
      },
      {
        field: 'unit', headerName: 'Unit', flex: 1, minWidth: 100, filterOperators: stringOperators()
      },
      {
        field: 'TimeZone', headerName: 'Time Zone', minWidth: 150, filterOperators: stringOperators(), valueGetter: (params: any) => params.row.timezone
      }
    ];

  const [columnsLoaded, setColumnsLoaded] = useState(false);
  const [columns, setColumnState] = useState(initialColumns);

  useEffect(() => {
    if (data === undefined) {
      return;
    }

    if (columnsLoaded === false) {
      data?.contentTypes?.forEach((v: TwinDtoContentType) => {
        const column: GridColDef = {
          field: v.name!,
          headerName: v.name,
          flex: 1,
          //minWidth: 160,
          type: v.isBool ? 'boolean' : (v.isNumber ? 'number' : 'string'),
          //hide: i > 2, //deprecated -> why do we exclude
          renderCell: (params: any) => {
            const value = Reflect.get(params.row!.contents, v.name!);
            if (value !== undefined) {
              if (v.isBool) {
                //doesn't seem like the grid like raw true/false values?
                return value.toString();
              }
              return value;
            }
            return '';
          }
        };

        if (v.isNumber) {
          column.filterOperators = numberOperators();
        }
        else if (v.isString) {
          column.filterOperators = stringOperators();
        }

        if (!initialColumns.some(v => v.field == column.field)) {
          initialColumns.push(column);
        }
      });

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

export default TwinsByModelGrid;
