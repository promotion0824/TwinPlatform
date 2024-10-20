import { useContext, useEffect, useState } from 'react';
import { Button, Box, FormControl, Typography, Stack, TextField } from '@mui/material';
import useApi from '../../hooks/useApi';
import { modelsImport } from '../../config';
import { ApiException, ErrorResponse, GitRepoRequest, JobsEntry } from '../../services/Clients';
import { StyledHeader } from '../../components/Common/StyledComponents';
import { useNavigate } from 'react-router-dom';
import { AppContext } from '../../components/Layout';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import { AuthHandler } from '../../components/AuthHandler';
import { AppPermissions } from '../../AppPermissions';
import useUserInfo from '../../hooks/useUserInfo';

const ModelsUploader = () => {
  const api = useApi();
  const userInfo = useUserInfo();
  const [branchReference, setBranchReference] = useState(modelsImport.branchRefs[0]);
  const [userData, setUserData] = useState<string>('');
  const [disable, setDisable] = useState(false);
  const navigate = useNavigate();
  const [appContext, setAppContext] = useContext(AppContext);

  useEffect(() => {
    setDisable(appContext.inProgress);
  }, [appContext]);
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  const submitForm = () => {
    const gitRepoRequest = new GitRepoRequest();
    gitRepoRequest.folderPath = 'Building';
    gitRepoRequest.branchRef = branchReference;
    gitRepoRequest.userInfo = `[Import From Git] ${userData}`;
    gitRepoRequest.userId = userInfo.userEmail;

    setDisable(true);
    setAppContext({ inProgress: true });

    api
      .modelsPOST(gitRepoRequest)
      .then((_res: JobsEntry) => {
        if (_res) {
          navigate(`../jobs/${_res.jobId}/details`, { replace: false });
        }
      })
      .catch((error: ErrorResponse | ApiException) => {
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      })
      .finally(() => {
        setDisable(false);
        setAppContext({ inProgress: false });
      });
  };

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanImportModelsFromGit]} noAccessAlert>
      <Typography variant="h1">Commit SHA (optional):</Typography>
      <FormControl fullWidth required sx={{ minWidth: 120, maxWidth: '30%' }}>
        <TextField
          fullWidth
          id="filled-basic"
          label="From"
          variant="filled"
          value={branchReference}
          onChange={(event: any) => setBranchReference(event.target.value)}
          disabled={appContext.inProgress}
        />
      </FormControl>
      <Box sx={{ m: 5 }}> </Box>

      <Typography variant="h1">Import reason (optional):</Typography>
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
          data-cy="comment_field"
          label="Comment"
          variant="filled"
          value={userData}
          onChange={(event: any) => setUserData(event.target.value)}
          disabled={appContext.inProgress}
        />
      </FormControl>

      {/* Submit the form */}

      <Box sx={{ m: 5 }}> </Box>
      <Button
        sx={{ maxWidth: '90%' }}
        onClick={submitForm}
        variant="contained"
        size="large"
        data-cy="import-button"
        disabled={disable}
      >
        Import Models
      </Button>
      {showPopUp ? (
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      ) : (
        <></>
      )}
    </AuthHandler>
  );
};

const ImportModelsPage = () => {
  return (
    //The use of the container is not necessary. The current implementation is used to shocase the ability
    //of the component to adapt to the size of the parent element
    <div style={{ width: '100%' }}>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <StyledHeader variant="h2">Import Models from GitHub</StyledHeader>
        <Box sx={{ m: 5 }}> </Box>
        <ModelsUploader />
      </Stack>
    </div>
  );
};

export default ImportModelsPage;
