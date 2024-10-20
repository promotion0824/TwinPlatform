import { Stack, Tooltip, Typography } from '@mui/material';
import Box from '@mui/material/Box';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, GridPaginationModel, GridToolbarContainer } from '@mui/x-data-grid-pro';
import { useEffect, useState } from 'react';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, IRuleParameterBoundDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { ValidFormatterBoundParamater } from '../LinkFormatters';
import { CumulativeTypeLookup, RuleInstanceStatusLookup } from '../Lookups';
import { buildCacheKey, createCsvFileResponse, gridPageSizes, stringOperators } from './GridFunctions';
import { Switch } from '@willowinc/ui';
import CopyToClipboardButton from '../CopyToClipboard';

interface IBindingsTableProps {
  parameters: IRuleParameterBoundDto[] | undefined,
  showUnits: boolean,
  showCumulativeSetting: boolean,
  key: string,
  pageId: string,
  additionalColumns?: GridColDef[]
}
const BindingsTable = ({ props }: { props: IBindingsTableProps }) => {

  if (!props.parameters) return <>Waiting...</>

  const additionalColumns = props.additionalColumns ?? [];

  const colsKey = buildCacheKey(`${props.pageId}_${props.key}_BindingsTable_ColumnModel`);
  const paginationKey = buildCacheKey(`${props.pageId}_BindingsTable_PaginationModel`);
  const [columnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, {
    units: props.showUnits,
    cumulativeSetting: props.showCumulativeSetting
  });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const csvExport = {
    downloadCsv: (_: BatchRequestDto) => {
      return createCsvFileResponse(props.parameters?.map((v) => {
        return {
          Name: v.name,
          Expression: showReadableExpression ? v.pointExpressionExplained : v.pointExpression
        };
      }), "Bindings.csv");
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  const columnsParameters: GridColDef[] = [
    {
      field: 'Status', headerName: 'Status', width: 80, type: "singleSelect", cellClassName: "MuiDataGrid-cell--textCenter", sortable: false,
      valueOptions: () => { return RuleInstanceStatusLookup.GetStatusFilter(); },
      renderCell: (p: any) => { return ValidFormatterBoundParamater(p.row); }
    },
    {
      field: 'name', headerName: 'Name', flex: 1.5, minWidth: 250, sortable: false,
      renderCell: (params: any) => (<>{params.row.name}&nbsp;<span style={{ fontSize: 8 }}>({params.row.fieldId})</span></>),
      filterOperators: stringOperators()
    },
    {
      field: 'pointExpression', headerName: 'Expression', flex: 3, minWidth: 250, sortable: false,
      renderCell: (params: any) => (
        <Tooltip sx={{ maxWidth: '100%' }} title={
          <Stack spacing={1}>
            <Stack direction='row' alignItems='center'>
              <Typography sx={{ wordBreak: 'break-all' }}>{!showReadableExpression ? params.row.pointExpressionExplained : params.row.pointExpression}<CopyToClipboardButton content={!showReadableExpression ? params.row.pointExpressionExplained : params.row.pointExpression} /></Typography>
            </Stack>
          </Stack>}>
          <Stack direction='row' alignItems='center'>
            <Typography>{showReadableExpression ? params.row.pointExpressionExplained : params.row.pointExpression}</Typography>
          </Stack>
        </Tooltip>),
      filterOperators: stringOperators()
    },
    {
      field: 'units', headerName: 'Units', width: 100, sortable: false,
      filterOperators: stringOperators()
    },
    {
      field: 'cumulativeSetting', headerName: 'Settings', flex: 1, type: 'singleSelect', sortable: false,
      valueOptions: () => { return CumulativeTypeLookup.GetCumulativeTypeFilter(); },
      renderCell: (p: any) => {
        var displayString = CumulativeTypeLookup.GetTypeName(p.row.cumulativeSetting);
        return (<Tooltip title={displayString}><Typography>{displayString}</Typography></Tooltip>)
      }
    },
    ...additionalColumns
  ];

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(props.parameters?.length || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => props.parameters?.length !== undefined ? props.parameters?.length : prevRowCountState);
  }, [props.parameters?.length, setRowCountState]);

  const [showReadableExpression, setShowReadableExpression] = useState(true);

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        rows={props.parameters.map((x: IRuleParameterBoundDto) => ({ ...x, id: x.fieldId }))}
        rowCount={rowCountState}
        pageSizeOptions={gridPageSizes()}
        columns={columnsParameters}
        pagination
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        columnVisibilityModel={columnVisibilityModel}
        disableColumnSelector
        disableColumnFilter
        disableRowSelectionOnClick
        hideFooterSelectedRowCount
        getRowHeight={() => 'auto'}
        slots={{
          toolbar: () => (
            <GridToolbarContainer>
              <Stack direction="row" alignItems="center" spacing={2}>
                <ExportToCsv source={csvExport} />
                <Switch checked={showReadableExpression} label="Show twins" labelPosition="left"
                  onChange={(event) => { setShowReadableExpression(event.currentTarget.checked); }} />
              </Stack>
            </GridToolbarContainer>
          ),
          noRowsOverlay: () => (
            <Stack margin={2}>
              {'No rows to display'}
            </Stack>
          ),
        }}
        sx={{
          '& .MuiDataGrid-row:hover': { backgroundColor: 'transparent' },
          '& .MuiDataGrid-cell:focus': { outline: 'none' },
          '& .MuiDataGrid-cell': { wordBreak: 'break-word', textWrap: 'wrap!important', py: '10px' }
        }}
      />
    </Box>);
};

export default BindingsTable;
