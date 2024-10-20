import { Alert, Box, Button, IconButton, Stack, styled, Typography } from '@mui/material';
import { useContext, useEffect, useState } from 'react';
import useApi from '../../../hooks/useApi';
import { RuleFilesLoadInformation } from '../../../types/RuleFilesLoadInformation';
import FilePresentIcon from '@mui/icons-material/FilePresent';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import { ApiException, ErrorResponse, FileParameter, RuleFileUploadResponse } from '../../../services/Clients';
import { AppContext } from '../../../components/Layout';
import { PopUpExceptionTemplate } from '../../../components/PopUps/PopUpExceptionTemplate';
import { StyledHeader } from '../../../components/Common/StyledComponents';
import { BasePageInformation } from '../../../types/BasePageInformation';
import { useNavigate } from 'react-router-dom';
import { AuthHandler } from '../../../components/AuthHandler';
import { AppPermissions } from '../../../AppPermissions';

const Input = styled('input')({
  display: 'none',
});

interface IFileProps {
  onFileSelect: React.Dispatch<React.SetStateAction<File[] | undefined>>;
  setFileNames: React.Dispatch<React.SetStateAction<string[] | undefined>>;
}

// TODO: Note that we have to select all files in one go from the file-chooser -
//  this means we can't add files from more that one directory without submitting first.
// If we don't like this flow, we can make it so that files are accumulated between
//  file-chooser dialogs rather than clearing the file least each time
const FileUploader = (props: IFileProps) => {
  const handleFileInput = (event: React.ChangeEvent<HTMLInputElement>) => {
    // Handle validations
    if (event.target.files === null || event.target.files.length === 0) {
      return;
    } else {
      // Gather all the selected files and do checks if needed
      const formFiles: File[] = [];
      const formFileName: string[] = [];
      for (let currentFile of Array.from(event.target.files)) {
        formFiles.push(currentFile);
        formFileName.push(currentFile.name);
      }
      event.target.files = null;
      event.target.value = '';
      props.onFileSelect(formFiles);
      props.setFileNames(formFileName);
    }
  };

  return (
    <label htmlFor="contained-button-file">
      <Input accept="application/JSON" id="contained-button-file" multiple type="file" onChange={handleFileInput} />
      <Button variant="contained" data-cy="upload-button" component="span">
        Select Files
      </Button>
    </label>
  );
};

const DQUploadRulePage = () => {
  const [selectedFiles, setSelectedFile] = useState<File[]>();
  const [selectedFileNames, setSelectedFileNames] = useState<string[]>();
  const [disable, setDisable] = useState<boolean>(true);
  const [appContext, setAppContext] = useContext(AppContext);
  const [openPopUp, setOpenPopUp] = useState<boolean>(true);
  const [showPopUp, setShowPopUp] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();
  const [openAlert, setOpenAlert] = useState<boolean>(false);
  const [fileUploadErrors, setFileUploadErrors] = useState<string[]>();
  const [fileUploadSuccessMsg, setFileUploadSuccessMsg] = useState<string>();

  const navigate = useNavigate();

  const api = useApi('multipart/form-data');
  const pageAction = (info: RuleFilesLoadInformation) => {
    return api.upload(info.FormFiles);
  };

  const pageInformation: BasePageInformation = {
    Action: pageAction,
    Type: '',
    Entity: '',
  };

  const deleteFile = (fileName: string) => {
    setSelectedFileNames(selectedFileNames?.filter((selectedFileName) => selectedFileName !== fileName));
    setSelectedFile(selectedFiles?.filter((file) => file.name !== fileName));
  };

  const handleClose = () => {
    setDisable(false);
  };

  const submitRequest = () => {
    handleClose();
    setDisable(true);
    if (selectedFiles && selectedFiles.length > 0) {
      const formFiles = selectedFiles.map((file) => {
        let fileParam: FileParameter = {
          data: file,
          fileName: file.name,
        };
        return fileParam;
      });

      const fileRequest: RuleFilesLoadInformation = {
        FormFiles: formFiles,
      };

      setDisable(true);
      setAppContext({ inProgress: true });
      // Send files to backend
      pageInformation
        .Action(fileRequest)
        .then((_res: RuleFileUploadResponse) => {
          let succeedFileCount = 0;
          let uploadErrors = [];
          for (const [key, value] of Object.entries(_res.fileUploaded!)) {
            if (value) {
              uploadErrors.push(`${key}: ${value}`);
            } else {
              succeedFileCount++;
            }
          }

          if (uploadErrors.length > 0) {
            setFileUploadErrors(uploadErrors);
            succeedFileCount > 0 && setFileUploadSuccessMsg(`Successfully uploaded ${succeedFileCount} files`);
            setOpenAlert(true);
          } else {
            navigate(`/data-quality/rules`);
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
    }
  };

  useEffect(() => {
    setDisable(!selectedFiles || selectedFiles.length === 0 || appContext.inProgress);
  }, [selectedFiles, appContext]);

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanUploadDQRules]} noAccessAlert>
      <form>
        <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
          <StyledHeader variant="h1">Upload Rules</StyledHeader>
          <Box sx={{ m: 5 }} />
          <Box>
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'row',
                flexWrap: 'wrap',
                alignContent: 'flex-start',
                gap: 2,
                alignItems: 'center',
              }}
            >
              <Typography variant="h5">Rule files:</Typography>

              <FileUploader setFileNames={setSelectedFileNames} onFileSelect={setSelectedFile} />
            </Box>
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'column',
                flexWrap: 'wrap',
                alignContent: 'flex-start',
                pl: 1,
                m: 1,
              }}
            >
              {selectedFileNames && selectedFileNames.length > 0 ? (
                selectedFileNames?.map((fileName) => (
                  <Typography variant="body2" key={Math.random()}>
                    <IconButton onClick={() => deleteFile(fileName)}>
                      <HighlightOffIcon />
                    </IconButton>
                    <FilePresentIcon key={Math.random()} /> {fileName}
                  </Typography>
                ))
              ) : (
                <Typography variant="body2">No files selected...</Typography>
              )}
            </Box>
          </Box>
          <Box sx={{ m: 5 }} />
          <Button
            variant="contained"
            data-cy="submit-rule-files"
            size="large"
            onClick={submitRequest}
            disabled={disable}
          >
            Upload
          </Button>
        </Stack>
      </form>
      {showPopUp && (
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      )}
      {openAlert && (
        <>
          {fileUploadErrors?.map((fileUploadError: string) => (
            <Alert severity="error">{fileUploadError}</Alert>
          ))}
        </>
      )}
      {openAlert && <Alert severity="success">{fileUploadSuccessMsg}</Alert>}
    </AuthHandler>
  );
};

export default DQUploadRulePage;
