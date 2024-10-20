import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { GridRowSelectionModel } from '@willowinc/ui';
import { DialogActions } from '@mui/material';
import { useLoading } from '../../Hooks/useLoading';
import { UserClient } from '../../Services/AuthClient';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Button, Group, Modal } from '@willowinc/ui';
import { MouseEventHandler, useState } from 'react';

export default function UserDelete({ refreshData, selectionModel }: { refreshData: () => void, selectionModel: GridRowSelectionModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();


  const handleDelete: MouseEventHandler<HTMLButtonElement> = async (e) => {
    e.preventDefault();
    const DeleteUserRecordAsync = async () => {
      try {
        loader(true, 'Deleting user');
        // Hide Delete User Dialog and resetData
        setOpen(false);
        await UserClient.DeleteUser(selectionModel.at(0)?.valueOf() as string);
        //refreshTableData
        refreshData();

        enqueueSnackbar('User deleted successfully', { variant: 'success' });

      } catch (e:any) {
        enqueueSnackbar('Error while deleting user', { variant: 'error' },e);
      }
      finally {
        loader(false);
      }
    }
    await DeleteUserRecordAsync();
  }

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Delete User
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Delete User">
        <DialogContent>
          <DialogContentText>
            Are you sure want to delete the selected user?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button type='button' kind='secondary' onClick={() => setOpen(false)}>No</Button>
          <Button type='button' kind='primary'  onClick={handleDelete}>Yes</Button>
        </DialogActions>
      </Modal>
    </>
  );
}
