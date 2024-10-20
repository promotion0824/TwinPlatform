import { DialogActions } from '@mui/material';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { GridRowSelectionModel } from '@willowinc/ui';
import { MouseEventHandler, useState } from 'react';
import { Button, Modal } from '@willowinc/ui';
import { useLoading } from '../../../Hooks/useLoading';
import { useCustomSnackbar } from '../../../Hooks/useCustomSnackbar';
import { AssignmentClient } from '../../../Services/AuthClient';

export default function ClientAssignmentDelete({ refreshData, selectionModel }: { refreshData: () => void, selectionModel: GridRowSelectionModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();

  const handleDelete: MouseEventHandler<HTMLButtonElement> = async (e) => {
    e.preventDefault();
    const DeleteClientAssignment = async () => {
      try {
        loader(true, 'Deleting assignment');
        // Hide Add assignment Dialog and resetData
        setOpen(false);
        await AssignmentClient.DeleteClientAssignment(selectionModel.at(0)?.valueOf() as string);
        //refreshTableData
        refreshData();

        enqueueSnackbar('Client assignment deleted successfully', { variant: 'success' });

      } catch (e: any) {

        enqueueSnackbar('Error while deleting the client assignment', { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    await DeleteClientAssignment();
  }

  return (
    <div>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Delete Assignment
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Delete Assignment">
        <DialogContent>
          <DialogContentText>
            Are you sure want to delete the selected assignment(s)?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button type='button' kind='secondary' onClick={() => setOpen(false)}>No</Button>
          <Button type='button' kind='primary' onClick={handleDelete}>Yes</Button>
        </DialogActions>
      </Modal>
    </div>
  );
}
