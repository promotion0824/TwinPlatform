import { BatchRequestDto, EquipmentDto, RelatedEntityDto } from '../../Rules';
import { DataGridPro, GridColumnVisibilityModel, GridComparatorFn, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import compareStringsLexNumeric from '../AlphaNumericSorter';
import { ModelIdFormatter, RelatedEntityFormatter } from '../LinkFormatters';
import { ExportToCsv } from '../ExportToCsv';
import { buildCacheKey, gridPageSizes, createCsvFileResponse, stringOperators } from './GridFunctions';
import useLocalStorage from '../../hooks/useLocalStorage';
import { useEffect, useState } from 'react';
import { Box, Grid, Stack } from '@mui/material';

interface RelatedEntitiesGridProps {
  equipment: EquipmentDto,
  related: RelatedEntityDto[] | undefined,
  inverseRelated: RelatedEntityDto[] | undefined,
  key: string,
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const RelatedEntitiesGrid = ({ query }: { query: RelatedEntitiesGridProps }) => {

  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_RelatedEntitiesGrid_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_${query.key}_RelatedEntitiesGrid_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_${query.key}_RelatedEntitiesGrid_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_RelatedEntitiesGrid_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'Name', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, {});
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });
  const data = query?.related?.concat(query.inverseRelated ?? []) ?? [];

  const csvExport = {
    downloadCsv: (_: BatchRequestDto) => {
      return createCsvFileResponse(query.related?.concat(query.inverseRelated ?? []).map((v) => {
        return {
          Relationship: v.relationship,
          RelatedItem: v.name,
          Model: v.modelId,
          Substance: v.substance,
          Unit: v.unit,
        };
      }), `EquipmentRelatedEntities_${query.equipment.equipmentId}.csv`);
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  const nameSortComparator: GridComparatorFn<string> = (v1, v2) =>
    compareStringsLexNumeric(v1?.toString() ?? "", v2?.toString() ?? "");

  const columnsRelatedEntity = [
    {
      field: 'relationship', headerName: 'Relationship', flex: 1, minWidth: 160,
      filterOperators: stringOperators()
    },
    {
      field: 'name', headerName: 'Related item', flex: 2, minWidth: 300,
      renderCell: (params: any) => { return RelatedEntityFormatter(params.row); },
      sortComparator: nameSortComparator,
      filterOperators: stringOperators()
    },
    {
      field: 'modelId', headerName: 'Model', flex: 2, minWidth: 300,
      renderCell: (params: any) => { return ModelIdFormatter(params.row!.modelId); },
      filterOperators: stringOperators(),
    },
    {
      field: 'substance', headerName: 'Substance', flex: 1, minWidth: 160,
      filterOperators: stringOperators()
    },
    {
      field: 'unit', headerName: 'Unit', flex: 1, minWidth: 160,
      filterOperators: stringOperators()
    },
    {
      field: 'TimeZone', headerName: 'Time Zone', minWidth: 150, filterOperators: stringOperators(), valueGetter: (params: any) => params.row.timezone
    }
  ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 1 });
  };

  const handleFilterChange = (newModel: GridFilterModel) => {
    setFilters(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 1 });
  };

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(data.length || 0);
  useEffect(() => {
    setRowCountState(() => data.length);
  }, [data.length, setRowCountState]);

  if (query.related) {
    return (
      <Box sx={{ flex: 1 }}>
        <DataGridPro
          autoHeight
          getRowId={(row) => `${row?.id} + ${row?.relationship}`}
          rows={data}
          rowCount={rowCountState}
          pageSizeOptions={gridPageSizes()}
          columns={columnsRelatedEntity}
          pagination
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
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
      </Box>
    );
  }
  else {
    return <div>Loading...</div>
  }
}

export default RelatedEntitiesGrid;
