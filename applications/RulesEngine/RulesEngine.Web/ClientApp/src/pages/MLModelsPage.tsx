import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogContentText, Grid, Snackbar, Stack } from '@mui/material';
import { DataGridPro, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { useEffect, useState } from 'react';
import { useQuery, useQueryClient } from 'react-query';
import { VisibleIf } from '../components/auth/Can';
import { ExportToCsv } from '../components/ExportToCsv';
import FlexTitle from '../components/FlexPageTitle';
import { buildCacheKey, mapFilterSpecifications, gridPageSizes, mapSortSpecifications, stringOperators } from '../components/grids/GridFunctions';
import RuleUpload from '../components/RuleUpload';
import StyledLink from '../components/styled/StyledLink';
import useApi from '../hooks/useApi';
import useLocalStorage from '../hooks/useLocalStorage';
import { BatchRequestDto, RuleParameterDto } from '../Rules';
import env from '../services/EnvService';

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const MLModelsPage = () => {

  const baseApi = env.baseapi();
  const queryClient = useQueryClient();

  const query = {
    key: "all",
    pageId: 'MLModels'
  };

  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_SortModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_PaginationModel`);
  const colsKey = buildCacheKey(`${query.pageId}_ColumnModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'Name', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { });
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
  } = useQuery(['mlModels', query.key, query.pageId, paginationModel, sortModel, filters], async () => await apiclient.mLModels(createRequest()), { keepPreviousData: true })

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportMLModels(request);
    },
    createBatchRequest: createRequest
  };

  // Define columns
  const columns =
    [
      {
        field: 'Id', headerName: 'Id', maxWidth: 200,
        valueGetter: (params: any) => params.row.id,
        filterOperators: stringOperators()
      },
      {
        field: 'FullName', headerName: 'FullName', flex: 1, minWidth: 200,
        valueGetter: (params: any) => params.row.fullName,
        renderCell: (params: any) => <StyledLink to={'/mlmodel/' + encodeURIComponent(params.row.id)}>{params.row.fullName}</StyledLink>,
        filterOperators: stringOperators()
      },
      {
        field: 'Description', headerName: 'Description', flex: 1, minWidth: 300, filterable: false
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
  const downloadTokenQuery = useQuery('mlmodeldownload', async (_c) => {
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
            ML Models
          </FlexTitle>
        </Grid>
        <Grid item xs={12} md={8}>
          <VisibleIf canExportRules>
            {downloadTokenQuery.isFetched &&
              <div style={{ float: 'right', paddingLeft: 5 }}>
                <a href={baseApi + "api/File/downloadMLModels?token=" + encodeURIComponent(downloadTokenQuery.data!.token!)} target="_blank">
                  <Button variant="outlined" color="secondary">Download</Button>
                </a>
              </div>
            }
          </VisibleIf>

          <VisibleIf canViewAdminPage>
            <RuleUpload saveRules={false} saveGlobals={false} saveMLModels={true} uploadFinished={(r) => {
              if (r.failureCount > 0) {
                handleOpenUploadDialog();
              }
              else {
                setUploadSuccess(true);
              }
              queryClient.invalidateQueries('mlModels');
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
                {uploadResult.uniqueCount} models(s) uploaded successfully<br />
                {(uploadResult.duplicateCount! > 0) && <>{uploadResult.duplicateCount} duplicate rule(s):<br />
                  {uploadResult.duplicates}</>}
              </Alert>
            </Snackbar>
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

export default MLModelsPage;
