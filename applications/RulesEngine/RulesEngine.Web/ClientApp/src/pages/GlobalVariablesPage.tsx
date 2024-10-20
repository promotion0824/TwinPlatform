import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogContentText, Grid, Snackbar, Stack } from '@mui/material';
import { DataGridPro, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import env from '../services/EnvService';
import { useEffect, useState } from 'react';
import { useQuery, useQueryClient } from 'react-query';
import { Link } from 'react-router-dom';
import { VisibleIf } from '../components/auth/Can';
import { buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, singleSelectCollectionOperators, stringOperators } from '../components/grids/GridFunctions';
import RuleUpload from '../components/RuleUpload';
import useApi from '../hooks/useApi';
import useLocalStorage from '../hooks/useLocalStorage';
import { BatchRequestDto, GlobalVariableDto, GlobalVariableType, RuleParameterDto } from '../Rules';
import StyledLink from '../components/styled/StyledLink';
import { GetGlobalVariableTypeText } from '../components/GlobalVariableTypeFormatter';
import { ExportToCsv } from '../components/ExportToCsv';
import FlexTitle from '../components/FlexPageTitle';

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const GlobalVariablesPage = () => {

  const baseApi = env.baseapi();
  const queryClient = useQueryClient();

  const query = {
    key: "all",
    pageId: 'GlobalVariables'
  };

  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_SortModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_PaginationModel`);
  const colsKey = buildCacheKey(`${query.pageId}_ColumnModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'Name', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { Id: false });
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

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['globalVariables', query.key, query.pageId, paginationModel, sortModel, filters], async (_) => await apiclient.globals(createRequest()), { keepPreviousData: true })

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportGlobals(request);
    },
    createBatchRequest: createRequest
  };

  const fetchTags = useQuery(["globalVariableTags"], async () => {
    try {
      const tags = await apiclient.globalVariableTags();
      return tags;
    } catch (error) {
      return []; // Return empty array in case of error
    }
  });

  // Define columns for global variables
  const columns =
    [
      {
        field: 'variableType', headerName: 'Type', maxWidth: 200,
        renderCell: (params: any) => { return GetGlobalVariableTypeText(params.row); }, filterable: false
      },
      {
        field: 'Name', headerName: 'Name', flex: 2, minWidth: 200,
        valueGetter: (params: any) => params.row.name,
        renderCell: (params: any) => <StyledLink to={'/global/' + encodeURIComponent(params.row.id)}>{params.row.name}</StyledLink>,
        filterOperators: stringOperators()
      },
      {
        field: 'expression', headerName: 'Expression', flex: 1, minWidth: 200, filterable: false,
        valueGetter: (params: any) => params.row.expression.length > 0 ? params.row.expression[params.row.expression.length - 1].pointExpression : "REDACTED",
      },
      {
        field: 'description', headerName: 'Description', flex: 1, minWidth: 300, filterable: false
      },
      {
        field: 'Id', headerName: 'Id', flex: 1, minWidth: 200, filterable: false
      },
      {
        field: 'Tags', headerName: 'Tags', flex: 1, minWidth: 200, type: "singleSelect",
        valueOptions: () => {
          if (fetchTags.isLoading || fetchTags.isError) {
            return []; // return empty array or loading indicator if data is not yet available
          }
          return fetchTags.data || []; // return the tags data if available
        },
        valueGetter: (params: any) => params.row.tags,
        filterOperators: singleSelectCollectionOperators(),
        valueFormatter: (params: any) => {
          const tagsArray = params.value as string[];
          return tagsArray?.join(", ");
        }
      }
    ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
  };

  const handleFilterChange = (newModel: GridFilterModel) => {
    setFilters(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  const [uploadResult, setUploadResult] = useState<any>({});
  const [uploadDialog, setUploadDialog] = useState(false);
  const [uploadSuccess, setUploadSuccess] = useState(false);
  const handleOpenUploadDialog = () => { setUploadDialog(true); };
  const handleCloseUploadDialog = () => { setUploadDialog(false); };
  const handleCloseUploadAlert = () => { setUploadSuccess(false); };

  // A DIFFERENT token is used for download,
  // and not all readers have download rights
  // so don't error boundary on this
  const downloadTokenQuery = useQuery('globaldownload', async (_c) => {
    try {
      return await apiclient.getTokenForInsightsDownload();
    }
    catch (e: any) {
      return "";
    }
  }, {
    useErrorBoundary: false
  });

  const expression = new RuleParameterDto();

  expression.init({ name: "result", fieldId: "result", pointExpression: "0.0" });

  const globalMacro = new GlobalVariableDto({ name: "", description: "", expression: [expression], parameters: [], variableType: GlobalVariableType._0 });

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(data?.total || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => data?.total !== undefined ? data?.total : prevRowCountState);
  }, [data?.total, setRowCountState]);

  return (
    <Stack spacing={2}>
      <Grid container>
        <Grid item xs={12} md={4}>
          <FlexTitle>
            Globals
          </FlexTitle>
        </Grid>
        <Grid item xs={12} md={8}>
          <VisibleIf canExportRules>
            {downloadTokenQuery.isFetched &&
              <div style={{ float: 'right', paddingLeft: 5 }}>
                <a href={baseApi + "api/File/downloadGlobals?token=" + encodeURIComponent(downloadTokenQuery.data!.token!)} target="_blank">
                  <Button variant="outlined" color="secondary">Download</Button>
                </a>
              </div>
            }
          </VisibleIf>

          <VisibleIf canViewAdminPage>
            <RuleUpload saveRules={false} saveGlobals={true} uploadFinished={(r) => {
              if (r.failureCount > 0) {
                handleOpenUploadDialog();
              }
              else {
                setUploadSuccess(true);
              }
              queryClient.invalidateQueries('globalVariables');
              setUploadResult(r);
            }} />
            {/*Dialog to inform user on upload process*/}
            <Dialog open={uploadDialog} onClose={handleCloseUploadDialog} fullWidth maxWidth="sm">
              <DialogContent>
                <DialogContentText>
                  {uploadResult.processedCount} file(s) processed<br />
                  {uploadResult.uniqueCount} rule(s) uploaded<br />
                  {(uploadResult.duplicateCount! > 0) &&
                    <><br />{uploadResult.duplicateCount} duplicate rule(s):<br />
                      {uploadResult.duplicates}<br /></>}
                  {(uploadResult.failureCount! > 0) &&
                    <><br /><Alert severity="error">{uploadResult.failureCount} file(s) failed:<br />
                      {uploadResult.failures}</Alert></>}
                </DialogContentText>
              </DialogContent>
              <DialogActions>
                <Button onClick={handleCloseUploadDialog} variant="contained" color="primary">
                  Close
                </Button>
              </DialogActions>
            </Dialog>
            <Snackbar open={uploadSuccess} onClose={handleCloseUploadAlert} autoHideDuration={15000} >
              <Alert onClose={handleCloseUploadAlert} sx={{ width: '100%' }} variant="filled">
                {uploadResult.processedCount} file(s) processed<br />
                {uploadResult.uniqueCount} global(s) uploaded successfully<br />
                {(uploadResult.duplicateCount! > 0) && <>{uploadResult.duplicateCount} duplicate rule(s):<br />
                  {uploadResult.duplicates}</>}
              </Alert>
            </Snackbar>
          </VisibleIf>

          <VisibleIf canEditRules>
            <div style={{ float: 'right', paddingLeft: 5 }}>
              <Link to="/global/new" state={globalMacro}>
                <Button variant="contained" color="primary">Create Macro</Button>
              </Link>
            </div>
          </VisibleIf>
        </Grid>
      </Grid>

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
            )
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
    </Stack>
  );
}

export default GlobalVariablesPage;
