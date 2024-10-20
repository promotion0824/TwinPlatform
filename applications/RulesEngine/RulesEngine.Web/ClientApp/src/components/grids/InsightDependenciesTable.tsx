import { Grid, Stack } from '@mui/material';
import Box from '@mui/material/Box';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarContainer } from '@mui/x-data-grid-pro';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, InsightDependencyDto, InsightDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { InsightFaultyFormatter, InsightLinkFormatterById, RuleLinkFormatter } from '../LinkFormatters';
import { buildCacheKey, gridPageSizes, createCsvFileResponse, stringOperators } from './GridFunctions';

interface IInsightDependenciesTableProps {
  insight: InsightDto,
  key: string,
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const InsightDependenciesTable = ({ props }: { props: IInsightDependenciesTableProps }) => {
  const sortKey = buildCacheKey(`${props.pageId}_${props.key}_InsightDependenciesTable_SortModel`);
  const colsKey = buildCacheKey(`${props.pageId}_${props.key}_InsightDependenciesTable_ColumnModel`);
  const paginationKey = buildCacheKey(`${props.pageId}_InsightDependenciesTable_PaginationModel`);
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, []);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { ruleId: false, twinId: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });
  const apiclient = useApi();

  const fetchDependantInsights = async () => {
    return apiclient.getDependantInsights(props.insight.id!);
  }

  const {
    isFetching,
    isLoading,
    data,
  } = useQuery(['dependantinsights', props.insight.id], async () => await fetchDependantInsights(), { keepPreviousData: true })


  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return createCsvFileResponse(props.insight.dependencies?.map((v) => {
        return {
          insightId: v.insightId,
          relationship: v.relationship
        };
      }), "InsightDependencies.csv");
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  const columnsParameters: GridColDef[] = [
    {
      field: 'IsFaulty', headerName: 'Faulty', width: 70, type: 'boolean', renderCell: (params: any) => { return InsightFaultyFormatter(params.row.insight!); }
    },
    {
      field: 'insightId', flex: 1, headerName: 'Insight', filterOperators: stringOperators(),
      renderCell: (p: any) => { return InsightLinkFormatterById(p.row!.insightId, p.row!.insightId); },
    },
    {
      field: 'relationship', maxWidth: 300, headerName: 'Relationship', filterOperators: stringOperators()
    },
    {
      field: 'ruleName', headerName: 'Skill', flex: 1, minWidth: 200, filterOperators: stringOperators(),
      renderCell: (params: any) => {
        return RuleLinkFormatter(params.row!.insight!.ruleId, params.row!.insight!.ruleName);
      }
    },
    {
      field: 'ruleId', headerName: 'Id', filterOperators: stringOperators()
    }
  ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
  };

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        loading={isLoading || isFetching}
        rows={(data ?? [])?.map((x: InsightDependencyDto) => ({ ...x, id: x.insightId })) ?? []}
        rowCount={data?.length ?? 0}
        pageSizeOptions={gridPageSizes()}
        columns={columnsParameters}
        pagination
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        onSortModelChange={handleSortModelChange}
        sortingOrder={sortingOrder}
        columnVisibilityModel={columnVisibilityModel}
        onColumnVisibilityModelChange={(newModel: any) => setColumnVisibilityModel(newModel)}
        disableColumnSelector
        disableColumnFilter
        hideFooterSelectedRowCount
        initialState={{
          sorting: {
            sortModel: [...sortModel]
          }
        }}
        slots={{
          toolbar: () => (
            <GridToolbarContainer sx={{ mt: 1 }}>
              <Grid container item>
                <ExportToCsv source={csvExport} />
              </Grid>
            </GridToolbarContainer>
          ),
          noRowsOverlay: () => (
            <Stack margin={2}>
              {'No rows to display'}
            </Stack>
          ),
        }}
      />
    </Box>);
};

export default InsightDependenciesTable;
