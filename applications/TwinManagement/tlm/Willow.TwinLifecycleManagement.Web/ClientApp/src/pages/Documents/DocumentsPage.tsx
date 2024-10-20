import { useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react';
import {
  GridColDef,
  GridRenderCellParams,
  GridRowId,
  GridRowModel,
  GridToolbar,
  useGridApiContext,
  useGridApiRef,
} from '@mui/x-data-grid-pro';
import { Alert, AlertProps, Autocomplete, Button, Snackbar, Stack, TextField, styled } from '@mui/material';
import { Link } from 'react-router-dom';
import { StyledHeader } from '../../components/Common/StyledComponents';
import { AppContext } from '../../components/Layout';
import useApi from '../../hooks/useApi';
import { ApiException, Document, ErrorResponse, IInterfaceTwinsInfo } from '../../services/Clients';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import { AuthHandler } from '../../components/AuthHandler';
import { AppPermissions } from '../../AppPermissions';
import useOntology from '../../hooks/useOntology/useOntology';
import { useQuery } from 'react-query';
import { usePersistentGridState } from '../../hooks/usePersistentGridState';
import { DataGrid } from '@willowinc/ui';

function DocTypeEditInputCell(props: GridRenderCellParams) {
  const { id, field, value } = props;
  const { data: ontology, isLoading } = useOntology({}, 'dtmi:com:willowinc:Document;1');
  const twinInfoValue = ontology.getModelById(`dtmi:com:willowinc:${value};1`);
  const apiRef = useGridApiContext();

  const handleChange = (_: React.SyntheticEvent, newValue: IInterfaceTwinsInfo | null) => {
    apiRef.current.setEditCellValue({
      id,
      field,
      value: newValue?.id?.replace('dtmi:com:willowinc:', '').replace(';1', ''),
    });
  };

  return (
    <Autocomplete
      options={ontology.getModels()}
      getOptionLabel={(option: IInterfaceTwinsInfo) => `${option.name}`}
      noOptionsText={isLoading ? 'Loading...' : 'No document type found'}
      autoComplete={false}
      filterSelectedOptions={false}
      id="document_type"
      value={twinInfoValue}
      onChange={handleChange}
      isOptionEqualToValue={(option, value) => option?.id === value?.id}
      sx={{ width: '100%' }}
      renderInput={(params: any) => (
        <TextField {...params} autoComplete="off" fullWidth variant="filled" label="Type"></TextField>
      )}
    />
  );
}

const DocumentsPage = () => {
  const apiGridRef = useGridApiRef();
  const { savedState } = usePersistentGridState(apiGridRef, 'documents');

  const api = useApi();
  const [disable, setDisable] = useState(false);
  const [appContext, setAppContext] = useContext(AppContext);
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  const renderDocTypeEditInputCell: GridColDef['renderCell'] = (params) => {
    return <DocTypeEditInputCell {...params} />;
  };

  const docColumns: GridColDef[] = useMemo(
    () => [
      {
        field: 'uniqueId',
        headerName: 'Id',
        flex: 4,
      },
      {
        field: 'fileName',
        headerName: 'File Name',
        flex: 4,
      },
      {
        field: 'siteName',
        headerName: 'Site Name',
        flex: 4,
      },
      {
        field: 'siteId',
        headerName: 'Site Id',
        flex: 4,
      },
      {
        field: 'createdDate',
        headerName: 'Created Date',
        flex: 3,
        type: 'date',
        sortComparator: (x, y) => new Date(x).getTime() - new Date(y).getTime(),
        valueFormatter: (params: any) => {
          return params.value
            ? new Date(params.value).toLocaleDateString('en-US', {
                weekday: 'short',
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: 'numeric',
                minute: 'numeric',
                timeZoneName: 'short',
              })
            : '';
        },
      },
      {
        field: 'createdBy',
        headerName: 'Created By',
        flex: 3,
      },
      {
        field: 'documentType',
        headerName: 'Document Type',
        flex: 2,
        editable: true,
        renderEditCell: renderDocTypeEditInputCell,
      },
    ],
    []
  );

  function useApiRef() {
    const apiRef: any = useRef(null);
    const _columns = useMemo(
      () =>
        docColumns.concat({
          field: '__HIDDEN__',
          filterable: false,
          headerName: '',
          width: 0,
          renderCell: (params) => {
            apiRef.current = params.api;
            return null;
          },
        }),
      []
    );

    return { apiRef, columns: _columns };
  }

  const useDocumentMutation = () => {
    return useCallback(
      (doc: Partial<Document>) =>
        new Promise<Partial<Document>>((resolve, reject) => {
          api
            .putDocument(doc.fileName, doc.documentType, doc.id)
            .then((res) => {
              if (res.isSuccessful) {
                resolve({ ...doc, documentType: doc.documentType });
              } else {
                reject(new Error(res.errorMessage));
              }
            })
            .catch((error: ErrorResponse | ApiException) => {
              reject(new Error(error.message));
            })
            .finally(() => {
              setDisable(false);
              setAppContext({ inProgress: false });
            });
        }),
      []
    );
  };

  useEffect(() => {
    setDisable(appContext.inProgress);
  }, [appContext]);

  const { data = [], isLoading } = useQuery('documents', () => api.documentsGET(), {
    onError: (error: ErrorResponse | ApiException) => {
      setDisable(false);
      setErrorMessage(error);
      setShowPopUp(true);
      setOpenPopUp(true);
    },
  });

  //Call useApiRef to store the DataGrid API reference
  const { apiRef, columns } = useApiRef();

  // const quickFilterCreatedTodayHandler = () => {
  //   const filterItem: GridFilterItem = {
  //     field: 'createdDate',
  //     operator: 'is',
  //     value: new Date().toISOString(),
  //   };
  //   apiRef.current.upsertFilterItem(filterItem);
  // };

  // TODO: Uncomment when modified date is available to use
  // const quickFilterLastModifiedTodayMeHandler = () => {
  //   const filterItem: GridFilterItem = { columnField: 'lastModified', operatorValue: 'is', value: new Date().toISOString() };
  //   apiRef.current.upsertFilterItem(filterItem);
  // }

  const handleClickExport = () => {
    setDisable(true);
    setAppContext({ inProgress: true });

    //Get the visible rows and extract their id's
    const filteredRows: Map<GridRowId, GridRowModel> = apiGridRef.current.getRowModels();
    const documentsIdsForExporting = [...filteredRows.values()].map((x) => x.id!);

    api
      .twinIds(documentsIdsForExporting)
      .then((res) => {
        const href = window.URL.createObjectURL(res.data);
        const a = document.createElement('a');
        a.download = res.fileName ?? '';
        a.href = href;
        a.click();
        a.href = '';
      })
      .catch((error: ErrorResponse | ApiException) => {
        setDisable(false);
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      })
      .finally(() => {
        setDisable(false);
        setAppContext({ inProgress: false });
      });
  };

  const mutateRow = useDocumentMutation();

  const [snackbar, setSnackbar] = useState<Pick<AlertProps, 'children' | 'severity'> | null>(null);

  const handleCloseSnackbar = () => setSnackbar(null);

  const processRowUpdate = useCallback(
    async (newRow: GridRowModel) => {
      // Make the HTTP request to save in the backend
      const response = await mutateRow(newRow);
      setSnackbar({ children: 'Document successfully saved', severity: 'success' });
      return response;
    },
    [mutateRow]
  );

  const handleProcessRowUpdateError = useCallback((error: Error) => {
    setSnackbar({ children: error.message, severity: 'error' });
  }, []);

  const [paginationModel, setPaginationModel] = useState({
    pageSize: 250,
    page: 0,
  });

  return (
    <>
      <AuthHandler requiredPermissions={[AppPermissions.CanReadDocuments]} noAccessAlert>
        <StyledHeader variant="h1">Documents</StyledHeader>

        <ButtonsContainer>
          <LeftContainer>
            <AuthHandler requiredPermissions={[AppPermissions.CanImportDocuments]}>
              <Link to="/upload-documents">
                <Button variant="contained">Upload</Button>
              </Link>
            </AuthHandler>

            <AuthHandler requiredPermissions={[AppPermissions.CanExportTwins]}>
              <Button
                data-cy="export-button"
                variant="contained"
                size="small"
                onClick={handleClickExport}
                disabled={disable}
              >
                Export
              </Button>
            </AuthHandler>
          </LeftContainer>

          <RightContainer></RightContainer>
        </ButtonsContainer>

        {/*}<Typography variant="h1">Quick filters:</Typography>*/}
        {/* <Grid container columnSpacing={1}> */}
        {/*TODO: to be addressed later when lastUpdateDate is ready*/}
        {/*<Grid item>*/}
        {/*  <Button*/}
        {/*    data-cy="quick-filter-modified-today-button"*/}
        {/*    variant="contained"*/}
        {/*    size="small"*/}
        {/*    onClick={quickFilterLastModifiedTodayMeHandler}*/}
        {/*    disabled={disable}>*/}
        {/*    Modified today*/}
        {/*  </Button>*/}
        {/*</Grid>*/}

        {/*TODO: Remove until this is made to work - if we decide it has value */}
        {/*<Grid item>*/}
        {/*
            <Grid item>
              <Button
                data-cy="quick-filter-created-today-button"
                variant="contained"
                size="small"
                onClick={quickFilterCreatedTodayHandler}
                disabled={disable}>
                Created today
              </Button>
            </Grid>
            */}
        {/* </Grid> */}
        <div style={{ height: '81vh', width: '100%', backgroundColor: '#242424' }}>
          <DataGrid
            apiRef={apiGridRef}
            initialState={savedState}
            rows={data}
            data-cy="Columns"
            columns={columns}
            paginationModel={paginationModel}
            onPaginationModelChange={setPaginationModel}
            pageSizeOptions={[250, 500, 1000]}
            slots={{ toolbar: GridToolbar }}
            processRowUpdate={processRowUpdate}
            onProcessRowUpdateError={handleProcessRowUpdateError}
            loading={isLoading}
          />
        </div>
      </AuthHandler>
      {showPopUp ? (
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      ) : (
        <></>
      )}
      {!!snackbar && (
        <Snackbar
          open
          sx={{ top: '90px !important' }}
          anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
          onClose={handleCloseSnackbar}
          autoHideDuration={6000}
        >
          <Alert {...snackbar} onClose={handleCloseSnackbar} variant="filled" />
        </Snackbar>
      )}
    </>
  );
};

export default DocumentsPage;

const ButtonsContainer = styled('div')({
  display: 'flex',
  justifyContent: 'space-between',
  margin: '10px 0 8px 0',
  width: '100%',
});

const gap = 8;

const LeftContainer = styled('div')({
  display: 'flex',
  gap,
});

const RightContainer = styled('div')({
  display: 'flex',
  gap,
});
