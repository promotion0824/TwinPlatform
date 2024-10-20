import React from 'react';
import { useState, useEffect, useContext } from 'react';
import useApi from '../../hooks/useApi';
import { AppContext } from '../../components/Layout';
import { Box, Button as MUIButton, Stack, styled, TextField, Typography } from '@mui/material';
import FilePresentIcon from '@mui/icons-material/FilePresent';
import IconButton from '@mui/material/IconButton';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import { ApiException, ErrorResponse, ImportTimeSeriesHistoricalRequest } from '../../services/Clients';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import { StyledHeader } from '../../components/Common/StyledComponents';
import '../../components/css/JobsTable.css';
import { useMutation } from 'react-query';
import { Button } from '@willowinc/ui';
import { BlobServiceClient, AnonymousCredential, ContainerClient } from '@azure/storage-blob';
import useGetBlobUploadInfo from './hooks/useGetBlobUploadInfo';
import { useNavigate } from 'react-router-dom';

const Input = styled('input')({
  display: 'none',
});

const FileUploader = ({
  setSelectedFiles,
}: {
  setSelectedFiles: React.Dispatch<React.SetStateAction<File[] | undefined>>;
}) => {
  const handleFileInput = (event: React.ChangeEvent<HTMLInputElement>) => {
    // Handle validations
    if (event.target.files === null) {
      return;
    } else {
      // Gather all the selected files and do checks if needed
      const formFiles: File[] = [];

      for (let currentFile of Array.from(event.target.files)) {
        formFiles.push(currentFile);
      }

      setSelectedFiles(formFiles);
    }
  };

  return (
    <label htmlFor="contained-button-file">
      <Input id="contained-button-file" multiple type="file" onChange={handleFileInput} />
      <MUIButton variant="contained" component="span">
        Select Files...
      </MUIButton>
    </label>
  );
};

const ImportTimeSeriesPage = () => {
  return (
    <div style={{ width: '100%' }}>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <TimeSeriesUpload />
      </Stack>
    </div>
  );
};

const TimeSeriesUpload = (): JSX.Element | null => {
  const navigate = useNavigate();
  const api = useApi();
  const [pageState, setPageState] = useState<'uploadCompleted' | 'inProgress' | 'form'>('form');

  const selectedFilesState = useState<File[]>();

  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  const { mutateAsync: postTimeSeries } = useMutation(
    ({ request }: { request: ImportTimeSeriesHistoricalRequest }) => {
      return api.clientUploadTimeSeries(request);
    },
    {
      onSuccess: (jobId: string) => {
        setPageState('uploadCompleted');
        navigate(`../jobs/${jobId}/details`, { replace: false });
      },
      onError: (error: ErrorResponse | ApiException) => {
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      },
    }
  );

  const { mutateAsync: postTimeSeriesWithSasUrl } = useMutation(
    ({ sasUrl }: { sasUrl: string }) => {
      return api.importTimeSeriesWithSasUrl(sasUrl, '');
    },
    {
      onSuccess: (jobId: string) => {
        setPageState('uploadCompleted');
        navigate(`../jobs/${jobId}/details`, { replace: false });
      },
      onError: (error: ErrorResponse | ApiException) => {
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      },
    }
  );

  return (
    <>
      {pageState === 'inProgress' ? (
        <InProgress />
      ) : (
        <FilesUploadForm
          setPageState={setPageState}
          selectedFilesState={selectedFilesState}
          postTimeSeries={postTimeSeries}
          postTimeSeriesWithSasUrl={postTimeSeriesWithSasUrl}
        />
      )}

      {showPopUp ? (
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      ) : (
        <></>
      )}
    </>
  );
};

function InProgress() {
  return (
    <>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <StyledHeader variant="h2">Please wait</StyledHeader>
        <StyledHeader variant="h6">
          The files are being uploaded. You will be notified when the upload operation is complete.
        </StyledHeader>
      </Stack>
    </>
  );
}

function FilesUploadForm({
  setPageState,
  selectedFilesState,
  postTimeSeries,
  postTimeSeriesWithSasUrl,
}: {
  setPageState: React.Dispatch<React.SetStateAction<'uploadCompleted' | 'inProgress' | 'form'>>;
  selectedFilesState: [File[] | undefined, React.Dispatch<React.SetStateAction<File[] | undefined>>];
  postTimeSeries: any;
  postTimeSeriesWithSasUrl: any;
}) {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [appContext, setAppContext] = useContext(AppContext);

  const [disable, setDisable] = useState(true);
  // const [open, setOpen] = useState(false);

  const [selectedFiles, setSelectedFiles] = selectedFilesState;
  const [sasUrl, setSasUrl] = useState<string>('');

  const deleteFile = (fileName: string) => {
    setSelectedFiles(
      selectedFiles?.filter((file) => {
        return file.name !== fileName;
      })
    );
  };

  useEffect(() => {
    setDisable(
      (!selectedFiles || selectedFiles.length === 0 || appContext.inProgress) && (!sasUrl || sasUrl.length === 0)
    );
  }, [selectedFiles, appContext, sasUrl]);

  const getBlobUploadInfo = useGetBlobUploadInfo({
    fileNames: selectedFiles?.map((file) => file.name) || [],
  });

  const handleFileUpload = async () => {
    if (!selectedFiles) return;

    const { data: timeSeriesBlobUploadInfo } = await getBlobUploadInfo.refetch();

    const { sasToken, containerName, blobPaths } = timeSeriesBlobUploadInfo || {};
    if (!sasToken || !containerName || !blobPaths) return;

    const blobServiceClient = new BlobServiceClient(sasToken, new AnonymousCredential());
    var containerClient = blobServiceClient.getContainerClient(containerName);

    await Promise.all(
      selectedFiles.map((file) => uploadBlobAndImportTimeSeries(file, blobPaths[file.name], containerClient))
    );

    var fileNames = selectedFiles.map((file) => file.name);
    var request = new ImportTimeSeriesHistoricalRequest();
    request.fileNames = fileNames;
    await postTimeSeries({ request: request });
  };

  const uploadBlobAndImportTimeSeries = async (file: File, blobName: string, containerClient: ContainerClient) => {
    var blockBlobClient = containerClient.getBlockBlobClient(blobName);

    await convertFileToArrayBuffer(file).then(async (fileArrayBuffer) => {
      if (fileArrayBuffer === null || fileArrayBuffer.byteLength < 1) return;

      await blockBlobClient.uploadData(fileArrayBuffer, {
        blobHTTPHeaders: {
          blobContentType: file.type,
        },
      });
    });
  };

  const submitTimeSeriesUploadRequest = async () => {
    setDisable(true);
    setAppContext({ inProgress: true });
    setPageState('inProgress');

    if (!sasUrl || sasUrl.length === 0) {
      await handleFileUpload();
    } else {
      await postTimeSeriesWithSasUrl({ sasUrl: sasUrl });
    }

    setAppContext({ inProgress: false });
  };

  return (
    <>
      <form>
        <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
          <StyledHeader variant="h1">Import Time Series</StyledHeader>
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
              <Typography variant="h5">Time Series files:</Typography>
              <FileUploader setSelectedFiles={setSelectedFiles} />
              <Typography>OR</Typography>
              <TextField
                id="sas-url-input"
                label="SAS URL"
                variant="outlined"
                autoComplete="off"
                onChange={(e) => {
                  setSasUrl(e.target.value);
                }}
              />
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
              {selectedFiles && selectedFiles.length > 0 ? (
                selectedFiles?.map((file) => (
                  <Typography variant="body2" key={Math.random()}>
                    <IconButton onClick={() => deleteFile(file.name)}>
                      <HighlightOffIcon />
                    </IconButton>
                    <FilePresentIcon /> {file.name}
                  </Typography>
                ))
              ) : (
                <Typography variant="body2">No files selected...</Typography>
              )}
            </Box>
          </Box>
          {/* Submit form */}

          <Button variant="contained" size="large" onClick={submitTimeSeriesUploadRequest} disabled={disable}>
            Import
          </Button>
        </Stack>
      </form>
    </>
  );
}

const convertStringToArrayBuffer = (str: string) => {
  const textEncoder = new TextEncoder();
  return textEncoder.encode(str).buffer;
};

function convertFileToArrayBuffer(file: File): Promise<ArrayBuffer | null> {
  return new Promise((resolve, reject) => {
    if (!file || !file.name) {
      reject(new Error('Invalid or missing file.'));
    }

    const reader = new FileReader();

    reader.onload = () => {
      const arrayBuffer: ArrayBuffer | null | string = reader.result;

      if (arrayBuffer === null) {
        resolve(null);
        return;
      }
      if (typeof arrayBuffer === 'string') {
        resolve(convertStringToArrayBuffer(arrayBuffer));
        return;
      }
      if (!arrayBuffer) {
        reject(new Error('Failed to read file into ArrayBuffer.'));
        return;
      }

      resolve(arrayBuffer);
    };

    reader.onerror = () => {
      reject(new Error('Error reading file.'));
    };

    reader.readAsArrayBuffer(file);
  });
}

export default ImportTimeSeriesPage;
