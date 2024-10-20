import { useContext, useEffect, useState } from 'react';
import {
  Button,
  Box,
  Typography,
  Stack,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  TextField,
  useTheme,
  useMediaQuery,
  FormControlLabel,
  Checkbox,
} from '@mui/material';
import { BasePageInformation } from '../types/BasePageInformation';
import { StyledDialogCaution, StyledHeader } from '../components/Common/StyledComponents';
import { PopUpExceptionTemplate } from '../components/PopUps/PopUpExceptionTemplate';
import { AppContext } from '../components/Layout';
import { useNavigate } from 'react-router-dom';
import { ApiException, ErrorResponse } from '../services/Clients';

const BaseDeleteAll = (pageInformation: BasePageInformation) => {
  const [open, setOpen] = useState(false);
  const [disable, setDisable] = useState(false);
  const [deletionReason, setDeletionReason] = useState('');
  const [confirmation, setConfirmation] = useState('');
  const navigate = useNavigate();
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('md'));
  const [appContext, setAppContext] = useContext(AppContext);
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();
  const [deleteOnlyRelationships, setDeleteOnlyRelationships] = useState(false);

  // disable submit button if no delete reason is provided or app is in progress.
  useEffect(() => {
    setDisable(appContext.inProgress || deletionReason === '');
  }, [appContext, deletionReason]);

  const enableDeletion = () => {
    if (confirmation === 'DELETE') return false;
    else return true;
  };

  const handleClickOpen = () => {
    setOpen(true);
    setDisable(true);
    setAppContext({ inProgress: true });
  };

  const handleClose = () => {
    setOpen(false);
    setDisable(false);
    setAppContext({ inProgress: false });
    setConfirmation('');
  };

  const submitDeleteRequest = () => {
    handleClose();
    setDisable(true);
    setAppContext({ inProgress: true });
    pageInformation
      .Action(`[Delete All] ${deletionReason}`, deleteOnlyRelationships)
      .then((_res: any) => {
        navigate(`../jobs/${_res.jobId}/details`, { replace: false });
      })
      .catch((error: ErrorResponse | ApiException) => {
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      })
      .finally(() => {
        setAppContext({ inProgress: false });
        setDisable(false);
      });
  };

  return (
    <>
      {pageInformation.Entity === 'Twins' ? (
        <>
          <FormControl>
            <FormControlLabel
              data-cy="checkBox"
              control={
                <Checkbox
                  defaultChecked={deleteOnlyRelationships}
                  onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                    setDeleteOnlyRelationships(event.target.checked);
                  }}
                />
              }
              label="Delete only Relationships"
            />
          </FormControl>
        </>
      ) : (
        <></>
      )}
      <Typography variant="h5">Deletion reason:</Typography>
      <FormControl
        fullWidth
        required
        sx={{
          minWidth: 120,
          maxWidth: '50%',
        }}
      >
        <TextField
          fullWidth
          data-cy="comment"
          label="Comment"
          variant="filled"
          value={deletionReason}
          onChange={(event) => setDeletionReason(event.target.value)}
          disabled={appContext.inProgress}
        />
      </FormControl>
      <Box sx={{ m: 5 }}> </Box>

      <Button variant="contained" data-cy="delete-twins" size="large" onClick={handleClickOpen} disabled={disable}>
        {pageInformation.Type + ' ' + pageInformation.Entity}
      </Button>
      <Dialog fullScreen={fullScreen} open={open} onClose={handleClose} aria-labelledby="responsive-dialog-title">
        <DialogTitle id="responsive-dialog-title">{`Do you really want to ${pageInformation.Type} all ${pageInformation.Entity}?`}</DialogTitle>
        <DialogContent>
          <StyledDialogCaution>
            <Typography variant="subtitle1">Caution:</Typography>
            <Typography variant="subtitle2">This action will delete all {pageInformation.Entity}!</Typography>
          </StyledDialogCaution>
          <DialogTitle>{`Please type "DELETE" if you agree with this action`}</DialogTitle>
          <FormControl className="centerMyForm" required fullWidth>
            <TextField
              fullWidth
              label="Confirmation"
              data-cy="confirmation"
              variant="filled"
              value={confirmation}
              onChange={(event) => setConfirmation(event.target.value)}
            />
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button
            autoFocus
            onClick={handleClose}
            sx={{ maxWidth: '90%', backgroundColor: theme.palette.secondary.light }}
            variant="contained"
            size="large"
            data-cy="cancel-button"
          >
            Cancel
          </Button>
          <Button
            autoFocus
            data-cy="proceed-button"
            onClick={submitDeleteRequest}
            sx={{ maxWidth: '90%' }}
            variant="contained"
            size="large"
            disabled={enableDeletion()}
            color="error"
          >
            Proceed
          </Button>
        </DialogActions>
      </Dialog>
      {showPopUp ? (
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      ) : (
        <></>
      )}
    </>
  );
};

const BaseDeleteAllPage = (pageInformation: BasePageInformation) => {
  return (
    <div style={{ width: '100%' }}>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <StyledHeader variant="h1">Delete All {pageInformation.Entity}</StyledHeader>
        <Box sx={{ m: 5 }}> </Box>
        <BaseDeleteAll {...pageInformation} />
      </Stack>
    </div>
  );
};

export { BaseDeleteAllPage };
