import { Grid, Stack } from '@mui/material';
import Box from '@mui/material/Box';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarContainer } from '@mui/x-data-grid-pro';
import { useEffect, useState } from 'react';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, RuleDependencyBoundDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { RuleInstanceLinkFormatterById, RuleLinkFormatter, TwinLinkFormatterById } from '../LinkFormatters';
import { buildCacheKey, gridPageSizes, createCsvFileResponse } from './GridFunctions';

interface IRuleInstanceDependenciesTableProps {
  dependencies: RuleDependencyBoundDto[] | undefined,
  key: string,
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const RuleInstanceDependenciesTable = ({ props }: { props: IRuleInstanceDependenciesTableProps }) => {
  const sortKey = buildCacheKey(`${props.pageId}_${props.key}_RuleInstanceDependenciesTable_SortModel`);
  const colsKey = buildCacheKey(`${props.pageId}_${props.key}_RuleInstanceDependenciesTable_ColumnModel`);
  const paginationKey = buildCacheKey(`${props.pageId}_RuleInstanceDependenciesTable_PaginationModel`);
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, []);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { ruleId: false, twinId: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return createCsvFileResponse(props.dependencies?.map((v) => {
        return {
          ruleName: v.ruleName,
          ruleId: v.ruleId,
          twinName: v.twinName,
          relationship: v.relationship,
          twinId: v.twinId,
          ruleInstanceId: v.ruleInstanceId
        };
      }), "SkillInstanceDependencies.csv");
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  const columnsParameters: GridColDef[] = [
    {
      field: 'ruleName', flex: 1, headerName: 'Skill',
      renderCell: (p: any) => { return RuleLinkFormatter(p.row!.ruleId, p.row!.ruleName); },
    },
    {
      field: 'twinName', flex: 1, headerName: 'Twin',
      renderCell: (p: any) => { return TwinLinkFormatterById(p.row!.twinId, p.row!.twinName); },
    },
    {
      field: 'relationship', flex: 2, headerName: 'Relationship'
    },
    {
      field: 'ruleInstanceId', flex: 2, headerName: 'Skill Instance Id',
      renderCell: (p: any) => { return RuleInstanceLinkFormatterById(p.row!.ruleInstanceId, p.row!.ruleInstanceId); },
    },
    {
      field: 'ruleId', headerName: 'Skill Id'
    },
    {
      field: 'twinId', headerName: 'Twin Id'
    }
  ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
  };

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(props.dependencies?.length || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => props.dependencies?.length !== undefined ? props.dependencies?.length : prevRowCountState);
  }, [props.dependencies?.length, setRowCountState]);

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        rows={props.dependencies?.map((x: RuleDependencyBoundDto) => ({ ...x, id: x.ruleInstanceId })) ?? []}
        rowCount={rowCountState}
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

export default RuleInstanceDependenciesTable;
