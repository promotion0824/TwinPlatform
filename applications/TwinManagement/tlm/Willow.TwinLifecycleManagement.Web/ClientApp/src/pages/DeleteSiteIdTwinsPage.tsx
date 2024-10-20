import { useContext, useEffect, useState } from 'react';
import {
  Button,
  FormControl,
  Typography,
  Stack,
  TextField,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  useTheme,
  useMediaQuery,
  alpha,
  styled,
  FormControlLabel,
  Checkbox,
} from '@mui/material';
import useApi from './../hooks/useApi';
import { StyledHeader } from './../components/Common/StyledComponents';
import { AppContext } from './../components/Layout';
import { PopUpExceptionTemplate } from './../components/PopUps/PopUpExceptionTemplate';
import React from 'react';
import { ApiException, ErrorResponse, NestedTwin } from './../services/Clients';
import { useNavigate } from 'react-router-dom';
import { AuthHandler } from './../components/AuthHandler';
import { AppPermissions } from './../AppPermissions';
import LocationSelector from './../components/Selectors/LocationSelector';
import useUserInfo from './../hooks/useUserInfo';

const StyledDialogCaution = styled(DialogContentText)(({ theme }) => ({
  borderRadius: theme.shape.borderRadius,
  color: alpha(theme.palette.text.primary, 0.95),
  backgroundColor: alpha('#f4433630', 0.2),
  textDecoration: 'none',
  padding: '6px 10px',
}));

const DeleteAllTwins = () => {
  const api = useApi();
  const userInfo = useUserInfo();
  const [open, setOpen] = useState(false);
  const [disable, setDisable] = useState(true);
  const [deleteReason, setDeleteReason] = useState('');
  const [confirmation, setConfirmation] = useState('');
  const theme = useTheme();
  const navigate = useNavigate();
  const fullScreen = useMediaQuery(theme.breakpoints.down('md'));
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();
  const [appContext, setAppContext] = useContext(AppContext);
  const [deleteOnlyRelationships, setDeleteOnlyRelationships] = useState(false);
  const [selectedLocation, setSelectedLocation] = useState<NestedTwin | null>(null);

  const enableDeletion = () => {
    return confirmation !== 'DELETE';
  };

  const handleClickOpen = () => {
    setOpen(true);
    setAppContext({ inProgress: false });
    setDisable(false);
  };

  const handleClose = () => {
    setOpen(false);
    setDisable(false);
    setConfirmation('');
    setAppContext({ inProgress: false });
  };

  const submitDeleteRequest = () => {
    if (!selectedLocation?.twin?.siteID) {
      alert('Location is required');
    }
    handleClose();
    setDisable(true);
    setAppContext({ inProgress: true });

    api
      .twinsDELETE2(
        selectedLocation?.twin?.siteID,
        userInfo.userEmail,
        `[Delete By Location] ${deleteReason}`,
        deleteOnlyRelationships
      )
      .then((_res) => {
        setDisable(false);
        navigate(`../jobs/${_res.jobId}/details`, { replace: false });
      })
      .catch((error) => {
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      })
      .finally(() => {
        setDisable(false);
        setAppContext({ inProgress: false });
      });
  };

  // disable submit button if following user inputs are empty: selected location and deletion reason
  useEffect(() => {
    setDisable(!selectedLocation || deleteReason === '' || appContext.inProgress);
  }, [appContext, selectedLocation, deleteReason]);

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanDeleteTwins]} noAccessAlert>
      <Typography variant="h5">Location:</Typography>
      <LocationSelector
        sx={{ width: '30%' }}
        selectedLocation={selectedLocation}
        setSelectedLocation={setSelectedLocation}
      />
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

      <Typography variant="h5">2. Deletion reason:</Typography>
      <FormControl
        fullWidth
        required
        sx={{
          width: '30%',
        }}
      >
        <TextField
          fullWidth
          label="Comment"
          data-cy="comment-siteID"
          variant="filled"
          value={deleteReason}
          onChange={(event) => setDeleteReason(event.target.value)}
          disabled={appContext.inProgress}
        />
      </FormControl>

      <Button
        data-cy="delete-twins-siteid"
        variant="contained"
        size="large"
        onClick={handleClickOpen}
        disabled={disable}
      >
        Delete Twins
      </Button>
      <Dialog fullScreen={fullScreen} open={open} onClose={handleClose} aria-labelledby="responsive-dialog-title">
        <DialogTitle id="responsive-dialog-title">{'Do you really want to delete all Twins?'}</DialogTitle>
        <DialogContent>
          <StyledDialogCaution>
            <Typography variant="subtitle1">Caution:</Typography>
            <Typography variant="subtitle2">
              This action will delete all Twins and relationships related with provided site it!
            </Typography>
          </StyledDialogCaution>
          <DialogTitle
            sx={{ paddingLeft: 0 }}
          >{`Please type "DELETE" if you really want to execute this action`}</DialogTitle>
          <FormControl className="centerMyForm" required fullWidth>
            <TextField
              fullWidth
              label="Confirmation"
              variant="filled"
              data-cy="confirmationID"
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
            data-cy="cancel-siteID"
          >
            Cancel
          </Button>
          <Button
            autoFocus
            onClick={submitDeleteRequest}
            sx={{ maxWidth: '90%' }}
            variant="contained"
            size="large"
            disabled={enableDeletion()}
            color="error"
            data-cy="proceed-button-siteid"
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
    </AuthHandler>
  );
};

const DeleteSiteIdTwinsPage = () => {
  return (
    <div style={{ width: '100%' }}>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <StyledHeader variant="h1">Delete All Twins in Location</StyledHeader>
        <DeleteAllTwins />
      </Stack>
    </div>
  );
};

export default DeleteSiteIdTwinsPage;
