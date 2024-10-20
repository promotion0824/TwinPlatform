import React, { Dispatch } from 'react';
import { useState, useEffect, useContext } from 'react';
import useApi from '../../hooks/useApi';
import { AppContext } from '../../components/Layout';
import { Box, Button as MUIButton, Stack, styled, Typography } from '@mui/material';
import FilePresentIcon from '@mui/icons-material/FilePresent';
import IconButton from '@mui/material/IconButton';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import { ApiException, Body, CreateDocumentResponse, ErrorResponse, NestedTwin } from '../../services/Clients';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import { StyledHeader } from '../../components/Common/StyledComponents';
import { DataGridPro, GridColDef, GridToolbar } from '@mui/x-data-grid-pro';
import '../../components/css/JobsTable.css';
import LocationSelector from '../../components/Selectors/LocationSelector';
import { useMutation } from 'react-query';
import { Modal, Button } from '@willowinc/ui';
import { BlobServiceClient, AnonymousCredential, ContainerClient } from '@azure/storage-blob';
import useGetBlobUploadInfo from './hooks/useGetBlobUploadInfo';

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

const DocumentsUploadPage = () => {
  return (
    <div style={{ width: '100%' }}>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <DocumentsUpload />
      </Stack>
    </div>
  );
};

const DocumentsUpload = (): JSX.Element | null => {
  const api = useApi();
  const [pageState, setPageState] = useState<'uploadCompleted' | 'inProgress' | 'form'>('form');

  const [createDocumentResultResponse, setCreateDocumentResultResponse] = useState<CreateDocumentResponse[]>([]);

  const selectedFilesState = useState<File[]>();

  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  const submitExportTwinsRequest = () => {
    if (createDocumentResultResponse.length > 0) {
      api
        .twinIds(createDocumentResultResponse.filter((x) => x.isSuccessful).map((x) => x.twinId!))
        .then((res) => {
          const href = window.URL.createObjectURL(res.data);
          const a = document.createElement('a');
          a.download = res.fileName ?? '';
          a.href = href;
          a.click();
          a.href = '';
        })
        .catch((error: ErrorResponse | ApiException) => {
          setErrorMessage(error);
          setShowPopUp(true);
          setOpenPopUp(true);
        });
    }
  };

  const resetToDefault = () => {
    setPageState('form');
    setCreateDocumentResultResponse([]);

    selectedFilesState[1](undefined);
  };

  const { mutateAsync: postDocument } = useMutation(
    ({ fileName, blobPath, siteId }: { fileName: string; blobPath: string; siteId: string }) => {
      var body = new Body();
      body.fileName = fileName;
      body.blobPath = blobPath;
      body.siteId = siteId;
      return api.clientUploadDocument(body);
    },
    {
      onSuccess: (data: CreateDocumentResponse) => {
        setCreateDocumentResultResponse((prev) => {
          return [...prev!, data];
        });
        setPageState('uploadCompleted');
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
      {pageState === 'uploadCompleted' ? (
        <UploadCompleted
          resetToDefault={resetToDefault}
          createDocumentResultResponse={createDocumentResultResponse}
          submitExportTwinsRequest={submitExportTwinsRequest}
        />
      ) : pageState === 'inProgress' ? (
        <InProgress />
      ) : (
        <FilesUploadForm
          setPageState={setPageState}
          selectedFilesState={selectedFilesState}
          postDocument={postDocument}
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
          The documents are being uploaded. You will be notified when the upload operation is complete.
        </StyledHeader>
      </Stack>
    </>
  );
}

function UploadCompleted({
  resetToDefault,
  createDocumentResultResponse,
  submitExportTwinsRequest,
}: {
  resetToDefault: () => void;
  createDocumentResultResponse: CreateDocumentResponse[];
  submitExportTwinsRequest: () => void;
}) {
  const columns: GridColDef[] = [
    {
      field: 'result',
      headerName: 'Result',
      width: 150,
      valueGetter: (params: any) => (params.row.isSuccessful ? 'Success' : 'Fail'),
    },
    {
      field: 'fileName',
      headerName: 'File Name',
      width: 450,
    },
    {
      field: 'twinFieldOrErrorMessage',
      headerName: 'Twin ID or error message',
      width: 690,
      valueGetter: (params: any) => (params.row.isSuccessful ? params.row.twinId : params.row.errorMessage),
    },
  ];

  const [paginationModel, setPaginationModel] = React.useState({
    pageSize: 250,
    page: 0,
  });

  return (
    <>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <StyledHeader variant="h2">Upload Complete {}</StyledHeader>
      </Stack>
      <ButtonContainer>
        <Button kind="primary" onClick={resetToDefault}>
          New Upload
        </Button>
        <Button kind="primary" onClick={submitExportTwinsRequest}>
          Export
        </Button>
      </ButtonContainer>

      <div style={{ height: '80vh', width: '100%' }}>
        <DataGridPro
          rows={createDocumentResultResponse}
          getRowId={() => Math.random()}
          columns={columns}
          pageSizeOptions={[250, 500, 1000]}
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          slots={{ toolbar: GridToolbar }}
          initialState={{
            sorting: {
              sortModel: [{ field: 'result', sort: 'asc' }],
            },
          }}
        />
      </div>
    </>
  );
}

const ButtonContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  justifyContent: 'space-between',
  width: '100%',
});

function FilesUploadForm({
  setPageState,
  selectedFilesState,
  postDocument,
}: {
  setPageState: React.Dispatch<React.SetStateAction<'uploadCompleted' | 'inProgress' | 'form'>>;
  selectedFilesState: [File[] | undefined, React.Dispatch<React.SetStateAction<File[] | undefined>>];
  postDocument: any;
}) {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [_, setAppContext] = useContext(AppContext);

  const [disable, setDisable] = useState(true);
  const [open, setOpen] = useState(false);

  const [selectedLocation, setSelectedLocation] = useState<NestedTwin | null>(null);

  const [selectedFiles, setSelectedFiles] = selectedFilesState;

  const deleteFile = (fileName: string) => {
    setSelectedFiles(
      selectedFiles?.filter((file) => {
        return file.name !== fileName;
      })
    );
  };

  const handleClickOpen = () => {
    setOpen(true);
  };

  const handleClose = () => {
    setOpen(false);
    setDisable(false);
  };

  const locationIsInvalid = () => !!selectedLocation && !!selectedLocation.twin && !selectedLocation.twin.siteID;

  useEffect(() => {
    setDisable(
      selectedFiles === null || selectedFiles === undefined || selectedFiles.length === 0 || locationIsInvalid()
    );
  }, [selectedFiles, selectedLocation]);

  const getBlobUploadInfo = useGetBlobUploadInfo({
    fileNames: selectedFiles?.map((file) => file.name) || [],
  });

  const handleFileUpload = async () => {
    if (!selectedFiles) return;

    const { data: documentBlobUploadInfo } = await getBlobUploadInfo.query.refetch();

    const { sasToken, containerName, blobPaths } = documentBlobUploadInfo || {};
    if (!sasToken || !containerName || !blobPaths) return;

    const blobServiceClient = new BlobServiceClient(sasToken, new AnonymousCredential());
    var containerClient = blobServiceClient.getContainerClient(containerName);

    await Promise.all(
      selectedFiles.map((file) => uploadBlobAndCreateDocument(file, blobPaths[file.name], containerClient))
    );
  };

  const uploadBlobAndCreateDocument = async (file: File, blobName: string, containerClient: ContainerClient) => {
    var blockBlobClient = containerClient.getBlockBlobClient(blobName);

    await convertFileToArrayBuffer(file).then(async (fileArrayBuffer) => {
      if (fileArrayBuffer === null || fileArrayBuffer.byteLength < 1) return;

      await blockBlobClient.uploadData(fileArrayBuffer, {
        blobHTTPHeaders: {
          blobContentType: file.type,
        },
      });

      await postDocument({ fileName: file.name, blobPath: blobName, siteId: selectedLocation?.twin?.siteID || '' });
    });
  };

  const submitDocumentUploadRequest = async () => {
    handleClose();
    setDisable(true);
    setAppContext({ inProgress: true });
    setPageState('inProgress');

    await handleFileUpload();

    setAppContext({ inProgress: false });
  };

  return (
    <>
      <form>
        <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
          <StyledHeader variant="h1">Document Upload</StyledHeader>
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
              <Typography variant="h5">Document files:</Typography>

              <FileUploader setSelectedFiles={setSelectedFiles} />
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
            <Typography variant="h5">Location:</Typography>
            <LocationSelector
              selectedLocation={selectedLocation}
              setSelectedLocation={setSelectedLocation}
              error={locationIsInvalid()}
              helperText={locationIsInvalid() ? 'Selected location twin has no Site Id.' : null}
              sx={{ direction: 'row', width: '30%', minWidth: 400, maxWidth: '100%' }}
            />
          </Box>
          {/* Submit form */}

          <Button variant="contained" size="large" onClick={handleClickOpen} disabled={disable}>
            Upload
          </Button>

          <ConfirmationModal
            isOpen={open}
            setOpen={setOpen}
            selectedFiles={selectedFiles}
            submitDocumentUploadRequest={submitDocumentUploadRequest}
          />
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

function ConfirmationModal({
  isOpen,
  setOpen,
  selectedFiles,
  submitDocumentUploadRequest,
}: {
  isOpen: boolean;
  setOpen: Dispatch<React.SetStateAction<boolean>>;
  selectedFiles: File[] | undefined;
  submitDocumentUploadRequest: () => void;
}) {
  const close = () => {
    setOpen(false);
  };

  return (
    <Modal closeOnClickOutside={false} centered opened={isOpen} onClose={close} header="Upload Documents">
      <ModalContent>
        <div>
          <span>Do you really want to upload {selectedFiles?.length.toString()} new documents?</span>
          <ModalButtonContainer>
            <Button kind="secondary" onClick={close}>
              Cancel
            </Button>
            <Button
              kind="primary"
              onClick={() => {
                close();
                submitDocumentUploadRequest();
              }}
            >
              Proceed
            </Button>
          </ModalButtonContainer>
        </div>
      </ModalContent>
    </Modal>
  );
}

const ModalContent = styled('div')({ padding: '1rem' });

const ModalButtonContainer = styled('div')({
  display: 'flex',
  gap: 10,
  flexDirection: 'row',
  justifyContent: 'flex-end',
  marginTop: 24,
});

export default DocumentsUploadPage;
