import { Link } from 'react-router-dom';
import { GridColDef, GridToolbar, useGridApiRef } from '@mui/x-data-grid-pro';
import { Button, styled, Typography, TextField, Paper, CircularProgress } from '@mui/material';
import { StyledHeader } from '../../../components/Common/StyledComponents';
import { useMemo, useState } from 'react';
import { ApiException } from '../../../services/Clients';
import { PopUpExceptionTemplate } from '../../../components/PopUps/PopUpExceptionTemplate';
import useGetDQRules from '../hooks/useGetDQRules';
import useOntology from '../../../hooks/useOntology/useOntology';
import { AuthHandler } from '../../../components/AuthHandler';
import { AppPermissions } from '../../../AppPermissions';
import DeleteIcon from '@mui/icons-material/Delete';
import AlertDialog from '../../../components/Common/AlertDialog';
import useDeleteAllDQRules from '../hooks/useDeleteAllDQRules';
import { useQueryClient } from 'react-query';
import { StyledDialogCaution } from '../../../components/Common/StyledComponents';
import { usePersistentGridState } from '../../../hooks/usePersistentGridState';
import { DataGrid } from '@willowinc/ui';

// #TechDebt: When doing cleanup, make this common and use elsewhere when getting string from ADT
const DefaultLanguage = 'en';
// Return a plain string as is, or if given an object of the form: {"en": "Hello"}
//  return the string in language "lang",  if present, otherwise any string found in any language.
// If "text" is not in the correct format, return "unk" (defaults to "???")
function getLocalizedText(text: any, unk: string | null = '???', lang = DefaultLanguage) {
  if (typeof text === 'string') return text;
  if (typeof text !== 'object' || Object.keys(text).length < 1) return unk;
  let str = text[lang] ?? text[Object.keys(text)[0]];
  return typeof str === 'string' ? str : unk;
}

const DQRuleManagmentPage = () => {
  const [openPopUp, setOpenPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ApiException>();
  const { data: ontology, isLoading, isSuccess } = useOntology();
  const isDependenciesLoaded = isSuccess;

  const apiRef = useGridApiRef();
  const { savedState } = usePersistentGridState(apiRef, 'dq-rules-management', isDependenciesLoaded);

  const [paginationModel, setPaginationModel] = useState({
    pageSize: 250,
    page: 0,
  });

  const {
    data: allRules,
    isLoading: isGetDQRulesLoading,
    isFetching,
  } = useGetDQRules({
    onError: (error: ApiException) => {
      setErrorMessage(error);
      setOpenPopUp(true);
    },
  });

  const isGridLoading = isGetDQRulesLoading || isLoading || isFetching;

  const columns: GridColDef[] = useMemo(
    () => [
      { field: 'id', headerName: 'Id', flex: 1 },
      { field: 'name', headerName: 'Name', flex: 1, valueGetter: getLocalizedText },
      {
        field: 'primaryModelId',
        headerName: 'Primary Model',
        flex: 1.0,
        valueGetter: (id) => ontology.getModelById(id.value)?.name,
      },
      { field: 'description', headerName: 'Description', flex: 1, valueGetter: getLocalizedText },
    ],
    [ontology]
  );

  return (
    <div>
      <AuthHandler requiredPermissions={[AppPermissions.CanReadDQRules]} noAccessAlert>
        <StyledHeader variant="h1">Data Quality Rules</StyledHeader>

        <ActionButtons />

        <div style={{ height: '81vh', width: '100%', backgroundColor: '#242424' }}>
          {isDependenciesLoaded ? (
            <DataGrid
              apiRef={apiRef}
              initialState={savedState}
              rows={allRules?.rules ?? []}
              columns={columns}
              components={{ Toolbar: GridToolbar }}
              data-cy="Columns"
              paginationModel={paginationModel}
              onPaginationModelChange={setPaginationModel}
              pageSizeOptions={[250, 500, 1000]}
              loading={isGridLoading}
            />
          ) : (
            <Paper
              variant="outlined"
              sx={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
            >
              <CircularProgress />
            </Paper>
          )}
        </div>
      </AuthHandler>

      <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
    </div>
  );
};

function ActionButtons() {
  return (
    <Container>
      <LeftContainer>
        <AuthHandler requiredPermissions={[AppPermissions.CanUploadDQRules]}>
          <Link to="/data-quality/upload-rules">
            <Button variant="contained">Upload</Button>
          </Link>
        </AuthHandler>
      </LeftContainer>

      <RightContainer>
        <AuthHandler requiredPermissions={[AppPermissions.CanUploadDQRules]}>
          <DeleteAllDQRuleButton />
        </AuthHandler>
      </RightContainer>
    </Container>
  );
}

const Container = styled('div')({
  display: 'flex',
  justifyContent: 'space-between',
  margin: '10px 0 8px 0',
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

/**
 * Button for deleting all DQ rules
 * When clicked, opens a dialog box to confirm deletion
 */
const DeleteAllDQRuleButton = () => {
  const queryClient = useQueryClient();
  const { mutate: deleteAllDQRules } = useDeleteAllDQRules({
    onSuccess: () => {
      // refresh dq rules table
      queryClient.invalidateQueries(['getDQRules']);
    },
  });

  const [open, setOpen] = useState(false);
  const [confirmation, setConfirmation] = useState('');

  const handleClose = () => {
    setOpen(false);
    setConfirmation('');
  };
  const handleSubmit = async () => {
    await deleteAllDQRules({});
    // todo: add snackbar show to request?
    setOpen(false);
    setConfirmation('');
  };
  const enableDeletion = () => {
    if (confirmation === 'DELETE') return false;
    else return true;
  };

  return (
    <>
      <Button variant="contained" color="error" size="small" onClick={() => setOpen(true)} startIcon={<DeleteIcon />}>
        Delete All
      </Button>

      {/* confirmation deletion popup */}
      <AlertDialog
        onClose={handleClose}
        open={open}
        title="Are you sure you want to delete all Data Quality Rules?"
        content={
          <>
            <StyledDialogCaution>
              <Typography variant="subtitle1">Caution:</Typography>
              <Typography variant="subtitle2">This action will delete all Data Quality Rules!</Typography>
            </StyledDialogCaution>
            <Typography
              sx={{
                letterSpacing: '0.15px',
                marginTop: '16px !important',

                color: '#FFFFFF',
              }}
              variant="subtitle2"
            >
              Please type "DELETE" to confirm deletion.
            </Typography>
            <TextField
              sx={{ paddingTop: '10px !important' }}
              fullWidth
              variant="filled"
              value={confirmation}
              autoComplete="off"
              onChange={(event) => setConfirmation(event.target.value)}
            />
          </>
        }
        onSubmit={handleSubmit}
        actionButtons={
          <>
            <Button variant="contained" color="secondary" onClick={handleClose}>
              Cancel
            </Button>
            <Button variant="contained" color="error" onClick={handleSubmit} autoFocus disabled={enableDeletion()}>
              Delete
            </Button>
          </>
        }
      />
    </>
  );
};

export default DQRuleManagmentPage;
