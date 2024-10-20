import { BatchRequestDto, CapabilityDto, EquipmentDto } from '../../Rules';
import { DataGridPro, GridColumnVisibilityModel, GridComparatorFn, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import compareStringsLexNumeric from '../AlphaNumericSorter';
import { LinkFormatter2, ModelFormatter2 } from '../LinkFormatters';
import { ExportToCsv } from '../ExportToCsv';
import { buildCacheKey, createCsvFileResponse, gridPageSizes, stringOperators } from './GridFunctions';
import useLocalStorage from '../../hooks/useLocalStorage';
import { useEffect, useState } from 'react';
import { Box, Grid, Stack } from '@mui/material';

interface CapabilitiesGridProps {
  equipment: EquipmentDto,
  capabilities: CapabilityDto[] | undefined,
  key: string,
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const CapabilitiesGrid = ({ query }: { query: CapabilitiesGridProps }) => {

  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_CapabilitiesGrid_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_${query.key}_CapabilitiesGrid_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_${query.key}_CapabilitiesGrid_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_CapabilitiesGrid_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'Name', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { id: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return createCsvFileResponse(query.capabilities?.map((v) => {
        return {
          Name: v.name,
          Model: v.modelId,
          Unit: v.unit,
          Id: v.id
        };
      }), `EquipmentCapabilities_${query.equipment.equipmentId}.csv`);
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  const nameSortComparator: GridComparatorFn<string> = (v1, v2) =>
    compareStringsLexNumeric(v1?.toString() ?? "", v2?.toString() ?? "");

  const columns =
    [
      {
        field: 'name', headerName: 'Name', flex: 2, minWidth: 300,
        formatter: LinkFormatter2,
        renderCell: (params: any) => { return LinkFormatter2(params.row); },
        sortComparator: nameSortComparator,
        filterOperators: stringOperators()
      },
      {
        field: 'modelId', headerName: 'Model', flex: 2, minWidth: 300,
        renderCell: (params: any) => { return ModelFormatter2(params.row); },
        filterOperators: stringOperators()
      },
      {
        field: 'unit', headerName: 'Unit', flex: 1, minWidth: 160,
        filterOperators: stringOperators()
      },
      {
        field: 'id', headerName: 'Id', minWidth: 160,
        filterOperators: stringOperators()
      },
    ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  const handleFilterChange = (newModel: GridFilterModel) => {
    setFilters(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        rows={query?.capabilities ?? []}
        pageSizeOptions={gridPageSizes()}
        columns={columns}
        pagination
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        onSortModelChange={handleSortModelChange}
        onFilterModelChange={handleFilterChange}
        sortingOrder={sortingOrder}
        columnVisibilityModel={columnVisibilityModel}
        onColumnVisibilityModelChange={(newModel: any) => setColumnVisibilityModel(newModel)}
        disableMultipleColumnsSorting
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
              {'No rows to display'}
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

export default CapabilitiesGrid;
