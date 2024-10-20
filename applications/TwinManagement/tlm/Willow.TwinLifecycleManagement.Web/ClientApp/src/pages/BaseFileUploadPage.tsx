import * as React from 'react';
import { useContext, useState } from 'react';
import { Button, Box, FormControl, TextField, Typography, Stack, FormControlLabel, Checkbox } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ApiException, ErrorResponse, FileParameter, JobsEntry, NestedTwin } from '../services/Clients';
import FilePresentIcon from '@mui/icons-material/FilePresent';
import IconButton from '@mui/material/IconButton';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import { FileLoadInformation } from '../types/FileLoadInformation';
import { BasePageInformation } from '../types/BasePageInformation';
import { StyledHeader } from '../components/Common/StyledComponents';
import { useNavigate } from 'react-router-dom';
import { AppContext } from '../components/Layout';
import { PopUpExceptionTemplate } from '../components/PopUps/PopUpExceptionTemplate';
import LocationSelector from '../components/Selectors/LocationSelector';
import { useQueryClient } from 'react-query';

const Input = styled('input')({
  display: 'none',
});

interface IFileProps {
  onFileSelect: React.Dispatch<React.SetStateAction<File[] | undefined>>;
  setFileNames: React.Dispatch<React.SetStateAction<string[] | undefined>>;
}

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
      <Input accept=".csv, .xlsx" id="contained-button-file" multiple type="file" onChange={handleFileInput} />
      <Button variant="contained" data-cy="upload-button" component="span">
        Select Files
      </Button>
    </label>
  );
};

const BaseFileUpload = (pageInformation: BasePageInformation): JSX.Element | null => {
  const queryClient = useQueryClient();
  const [deleteOnlyRelationships, setDeleteOnlyRelationships] = useState(false);
  const [selectedFiles, setSelectedFile] = useState<File[]>();
  const [selectedFileNames, setSelectedFileNames] = useState<string[]>();
  const [selectedLocation, setSelectedLocation] = useState<NestedTwin | null>(null);
  const [comments, setComments] = useState<string>('');
  const [disable, setDisable] = useState(true);
  const [includeRelationships, setIncludeRelationships] = useState(true);
  const [includeTwinProperties, setIncludeTwinProperties] = useState(true);
  const navigate = useNavigate();
  const [appContext, setAppContext] = useContext(AppContext);
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

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

      const fileRequest: FileLoadInformation = {
        // SiteId is optional as empty string. Generated sdk doesn't allow null/undefined value.
        SiteId: selectedLocation?.twin?.siteID || '',
        IncludeRelationships: includeRelationships,
        IncludeTwinProperties: includeTwinProperties,
        Type: pageInformation.Type,
        UserData: `[${pageInformation.Type} By File] ${comments}`,
        FormFiles: formFiles,
        DeleteOnlyRelationships: deleteOnlyRelationships,
      };

      setDisable(true);
      setAppContext({ inProgress: true });
      // Send files to backend
      pageInformation
        .Action(fileRequest)
        .then((_res: JobsEntry) => {
          navigate(`../jobs/${_res.jobId}/details`, { replace: false });
        })
        .catch((error: ErrorResponse | ApiException) => {
          setErrorMessage(error);
          setShowPopUp(true);
          setOpenPopUp(true);
        })
        .finally(() => {
          setDisable(false);
          setAppContext({ inProgress: false });
          queryClient.invalidateQueries('models');
          queryClient.invalidateQueries('locations');
        });
    }
  };

  React.useEffect(
    () => {
      setDisable(
        selectedFiles === null ||
          selectedFiles === undefined ||
          selectedFiles.length === 0 ||
          appContext.inProgress ||
          (!includeRelationships && !includeTwinProperties) ||
          (pageInformation.Type === 'Delete' && comments === '')
      );
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [selectedFiles, appContext, includeRelationships, includeTwinProperties, comments]
  );

  const deleteFile = (fileName: string) => {
    setSelectedFileNames(
      selectedFileNames?.filter((selectedFileName) => {
        return selectedFileName !== fileName;
      })
    );

    setSelectedFile(
      selectedFiles?.filter((file) => {
        return file.name !== fileName;
      })
    );
  };

  return (
    <>
      <form>
        <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
          <StyledHeader variant="h1">
            {pageInformation.Type === 'Delete'
              ? `Delete Twins from a File`
              : `${pageInformation.Type} ${pageInformation.Entity}`}
          </StyledHeader>
          <Box sx={{ m: 5 }}> </Box>
          {/* First step */}
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
              <Typography variant="h5">Twins files:</Typography>

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
          {/* Second step */}
          {pageInformation.Type === 'Delete' ? (
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
              <Typography variant="h5">Deletion reason:</Typography>
            </>
          ) : (
            <>
              <Typography variant="h5">Location (optional):</Typography>
              <LocationSelector
                selectedLocation={selectedLocation}
                setSelectedLocation={setSelectedLocation}
                sx={{ direction: 'row', width: '30%', minWidth: 400, maxWidth: '100%' }}
              />
              <FormControl>
                <FormControlLabel
                  control={
                    <Checkbox
                      data-cy="include-relationships-checkBox"
                      defaultChecked={true}
                      onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                        setIncludeRelationships(event.target.checked);
                      }}
                    />
                  }
                  label="Include Relationships"
                />
              </FormControl>
              <FormControl>
                <FormControlLabel
                  control={
                    <Checkbox
                      data-cy="include-twin-properties-checkBox"
                      defaultChecked={true}
                      onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                        setIncludeTwinProperties(event.target.checked);
                      }}
                    />
                  }
                  label="Include Twin Properties"
                />
              </FormControl>
              <Typography variant="h5">Import reason:</Typography>
            </>
          )}
          <FormControl
            fullWidth
            required
            sx={{
              minWidth: 650,
              maxWidth: '1000%',
            }}
          >
            <TextField
              fullWidth
              label="Comments"
              data-cy="comment"
              variant="filled"
              value={comments}
              onChange={(event) => setComments(event.target.value)}
              disabled={appContext.inProgress}
            />
          </FormControl>
          {/* Submit form */}
          <Box sx={{ m: 5 }}> </Box>
          <Button
            variant="contained"
            data-cy="delete-twins-file"
            size="large"
            onClick={submitRequest}
            disabled={disable}
          >
            {pageInformation.Type} Twins
          </Button>
        </Stack>
      </form>
      {showPopUp ? (
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      ) : (
        <></>
      )}
    </>
  );
};

const BaseFileUploadPage = (pageInformation: BasePageInformation) => {
  return (
    <div style={{ width: '100%' }}>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <BaseFileUpload {...pageInformation} />
      </Stack>
    </div>
  );
};

export { BaseFileUploadPage };
