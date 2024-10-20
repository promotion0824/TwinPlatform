import { DialogActions } from '@mui/material';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { GridRowSelectionModel } from '@willowinc/ui';
import { MouseEventHandler, useState } from 'react';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { GroupClient } from '../../Services/AuthClient';
import { GroupModel } from '../../types/GroupModel';
import { Button, Group, Modal } from '@willowinc/ui';

export default function RemoveUserFromGroup({ groupModel, refreshData, selectionModel }: { groupModel: GroupModel, refreshData: () => void, selectionModel: GridRowSelectionModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();

  const handleDelete: MouseEventHandler<HTMLButtonElement> = async (e) => {
    e.preventDefault();
    const DeleteGroupUserRecordAsync = async () => {
      try {
        loader(true, 'Deleting user from group');
        // Hide Add GroupUser Delete Dialog
        setOpen(false);
        await GroupClient.RemoveUserFromGroup(groupModel.id, selectionModel.at(0)?.valueOf() as string);
        //refreshTableData
        refreshData();

        enqueueSnackbar('User removed successfully', { variant: 'success' });

      } catch (e:any) {
        enqueueSnackbar('Error while deleting User from group', { variant: 'error' },e);
      }
      finally {
        loader(false);
      }
    }
    await DeleteGroupUserRecordAsync();
  }

  return (
    <>
      <Button kind='primary' onClick={() => { setOpen(true) }}> Remove
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Remove User">
        <DialogContent>
          <DialogContentText>
            Are you sure want to remove the selected user?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button type='button' kind='secondary' onClick={() => setOpen(false)}>No</Button>
          <Button type='button' kind='primary' onClick={handleDelete}>Yes</Button>
        </DialogActions>
      </Modal>
    </>
  );
}
