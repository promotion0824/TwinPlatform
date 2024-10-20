import { Stack } from '@mui/material';
import Box from '@mui/material/Box';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, gridExpandedSortedRowIdsSelector, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton, useGridApiContext } from '@mui/x-data-grid-pro';
import { useQuery } from 'react-query';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, RuleReferenceDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { GlobalLinkFormatter, RuleLinkFormatter } from '../LinkFormatters';
import { buildCacheKey, gridPageSizes, createCsvFileResponse, stringOperators } from './GridFunctions';

interface IRuleReferencesTableProps {
  key: string,
  invokeQuery: () => Promise<RuleReferenceDto[]>
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const RuleReferencesGrid = ({ query }: { query: IRuleReferencesTableProps }) => {
  const id = query.key;
  const sortKey = buildCacheKey(`${id}_RuleReferences_SortModel`);
  const paginationKey = buildCacheKey(`RuleReferences_PaginationModel`);
  const colsKey = buildCacheKey(`${id}_RuleReferences_ColumnModel`);
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'referenceType', sort: 'desc' }]);
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { });

  const fetchReferences = async () => {
    return query.invokeQuery();
  };

  const {
    isLoading,
    data,
    isFetching
  } = useQuery(['rulereferences', id], async () => await fetchReferences())

  const getExport = () => {
    const apiRef = useGridApiContext();
    return {
      downloadCsv: (_: BatchRequestDto) => {
        const ids = gridExpandedSortedRowIdsSelector(apiRef);

        return createCsvFileResponse(data?.filter(v => ids.indexOf(v.id!) >= 0).map((v) => {
          return {
            referenceType: v.referenceType,
            id: v.id,
            name: v.name
          };
        }), "SkillReferences.csv");
      },
      createBatchRequest: () => new BatchRequestDto()
    };
  };

  const columnsParameters: GridColDef[] = [
    {
      field: 'referenceType', headerName: 'Type', width: 100
    },
    {
      field: 'name', headerName: 'Name', flex: 1, minWidth: 200, filterOperators: stringOperators(),
      renderCell: (params: any) => {
        if (params.row!.referenceType == "Rule") {
          return RuleLinkFormatter(params.row!.id, params.row!.name);
        }

        //force a reload here so it starts on the correct tab
        return GlobalLinkFormatter(params.row!.id, params.row!.name, true);
      }
    },
    {
      field: 'id', headerName: 'Id', width: 300
    }
  ];

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        rows={(data ?? []).map((x: RuleReferenceDto) => ({ ...x, id: x.id }))}
        loading={isLoading || isFetching}
        pageSizeOptions={gridPageSizes()}
        columns={columnsParameters}
        pagination
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        onSortModelChange={setSortModel}
        sortingOrder={sortingOrder}
        columnVisibilityModel={columnVisibilityModel}
        onColumnVisibilityModelChange={(newModel: any) => setColumnVisibilityModel(newModel)}
        hideFooterSelectedRowCount
        initialState={{
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
                <ExportToCsv source={getExport()} />
              </Box>
            </GridToolbarContainer>
          ),
          noRowsOverlay: () => (
            <Stack margin={2}>
              {'No references found'}
            </Stack>
          ),
        }}
      />
    </Box>);
};

export default RuleReferencesGrid;
