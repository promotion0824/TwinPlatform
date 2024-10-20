import { Box, Modal, Typography } from '@mui/material';
import { DataGridPro, GridColDef, GridToolbar, useGridApiRef } from '@mui/x-data-grid-pro';
import { useMemo, useState } from 'react';
import useSelectFilterGrid from '../../../hooks/useSelectFilterGrid';
import './JobErrorsTable.css';
import { usePersistentGridState } from '../../../hooks/usePersistentGridState';

const JobErrorsTable = ({ entitiesError }: any) => {
  const apiRef = useGridApiRef();
  const { savedState } = usePersistentGridState(apiRef, 'jobs-errors');

  const [open, setOpen] = useState(false);
  const [modalContent, setModalContent] = useState({ id: '', message: '' });
  const handleOpen = (id: string, message: string) => {
    setModalContent({ id, message });
    setOpen(true);
  };
  const handleClose = () => setOpen(false);
  const errorCodeFilter = useSelectFilterGrid();

  const [paginationModel, setPaginationModel] = useState({
    pageSize: 250,
    page: 0,
  });

  const columns: GridColDef[] = useMemo(
    () => [
      { field: 'entityId', headerName: 'Entity id', width: 300 },
      {
        field: 'errorCode',
        headerName: 'Error code',
        width: 200,
        type: 'singleSelect',
        valueOptions: errorCodeFilter.filterOptions,
      },
      { field: 'error', headerName: 'Error message', width: 700, groupable: true, cellClassName: 'errorMessage' },
    ],
    []
  );

  const rows = useMemo(
    () =>
      Object.keys(entitiesError).map((key, index) => {
        return {
          id: `${key}${index}`,
          entityId: key,
          error: entitiesError[key],
          errorCode: errorCodeFilter.enableDropDownFilter(getErrorCodeFromMessage(entitiesError[key])),
        };
      }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [entitiesError]
  );

  return (
    <>
      <p>
        <i>Details table:</i>
      </p>
      <DataGridPro
        apiRef={apiRef}
        initialState={savedState}
        style={{ flexShrink: 1 }}
        rows={rows}
        columns={columns}
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        pageSizeOptions={[250, 500, 1000]}
        components={{ Toolbar: GridToolbar }}
        checkboxSelection
        onCellClick={(row) => {
          if (row.field === 'error') handleOpen(row.id as string, row.value as string);
        }}
      />
      <Modal
        open={open}
        onClose={handleClose}
        aria-labelledby="modal-modal-title"
        aria-describedby="modal-modal-description"
      >
        <Box sx={style}>
          <Typography id="modal-modal-title" variant="subtitle1" component="h2">
            {modalContent.id}
          </Typography>
          <br></br>
          <pre>{modalContent.message}</pre>
        </Box>
      </Modal>
    </>
  );
};

export default JobErrorsTable;

const getErrorCodeFromMessage = (message: string) =>
  message?.includes('ErrorCode:') ? message.split('ErrorCode:')[1].split('\n')[0] : 'No code';

const style = {
  position: 'absolute' as 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 800,
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
};
