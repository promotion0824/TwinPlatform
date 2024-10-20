import IconForModel from '../components/icons/IconForModel';
import { DataGridPro, GridFilterModel, GridSortDirection, GridSortModel, GridColumnVisibilityModel, GridPaginationModel, GridToolbarContainer, GridToolbarColumnsButton, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { useQuery } from 'react-query';
import useApi from '../hooks/useApi';
import StyledLink from '../components/styled/StyledLink';
import { Box, Stack, Tooltip, Typography } from '@mui/material';
import { buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, numberOperators, stringOperators } from '../components/grids/GridFunctions';
import { BatchRequestDto } from '../Rules';
import { ExportToCsv } from '../components/ExportToCsv';
import useLocalStorage from '../hooks/useLocalStorage';
import { useState, useEffect } from 'react';
import { TrueFormatter } from '../components/LinkFormatters';
import FlexTitle from '../components/FlexPageTitle';

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const ModelsPage = () => {

  const gridProps = {
    key: "all",
    pageId: 'Models'
  };

  const filterKey = buildCacheKey(`${gridProps.pageId}_${gridProps.key}_FilterModel`);
  const sortKey = buildCacheKey(`${gridProps.pageId}_SortModel`);
  const colsKey = buildCacheKey(`${gridProps.pageId}_ColumnModel`);
  const paginationKey = buildCacheKey(`${gridProps.pageId}_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'ModelId', sort: 'asc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, {});
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

  const fetchModels = async () => {
    const request = createRequest();
    return await apiclient.ontology("", request);
  }

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportOntology("", request);
    },
    createBatchRequest: createRequest
  };

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['models', paginationModel, sortModel, filters], async () => await fetchModels(), { keepPreviousData: true })

  const ModelFormatter = (params: { modelId: string }) => (<StyledLink to={"/model/" + params.modelId}><IconForModel modelId={params.modelId} size={14} /> {(params.modelId || '')}</StyledLink >);

  const columns =
    [
      {
        field: 'ModelId', headerName: 'Model', flex: 2, minWidth: 300,
        renderCell: (params: any) => { return ModelFormatter(params.row); },
        filterOperators: stringOperators()
      },
      {
        field: 'Label', headerName: 'Name', flex: 2, minWidth: 300,
        valueGetter: (params: any) => params.row.label,
        filterOperators: stringOperators()
      },
      {
        field: 'Units', headerName: 'Unit(s)', flex: 1, minWidth: 160,
        renderCell: (params: any) => {
          return (<Tooltip title={params.row.units.join()}><Typography variant="body2">{params.row.units.join()}</Typography></Tooltip>)
        },
        valueGetter: (params: any) => params.row.units,
        filterOperators: stringOperators()
      },
      {
        field: 'Count', headerName: 'Exact', flex: 1, minWidth: 160,
        valueGetter: (params: any) => params.row.count,
        filterOperators: numberOperators()
      },
      {
        field: 'Total', headerName: 'Total', flex: 1, minWidth: 160,
        renderCell: (params: any) => { return params.row.total; },
        filterOperators: numberOperators()
      },
      {
        field: 'IsCapability', headerName: 'Capability', width: 120, type: 'boolean',
        renderCell: (params: any) => { return TrueFormatter(params.row!.isCapability); }
      },
    ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
  };

  const handleFilterChange = (newModel: GridFilterModel) => {
    setFilters(newModel)
  };

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(data?.total || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => data?.total !== undefined ? data?.total : prevRowCountState);
  }, [data?.total, setRowCountState]);

  return (
    <Stack spacing={2}>
      <FlexTitle>
        Models
      </FlexTitle>
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
    </Stack>);
}

export default ModelsPage;
