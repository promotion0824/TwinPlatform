import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { GridRowSelectionModel } from '@willowinc/ui';
import { useState, MouseEventHandler } from 'react';
import { DialogActions } from '@mui/material';
import { useLoading } from '../../Hooks/useLoading';
import { RoleClient } from '../../Services/AuthClient';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Button, Group, Modal } from '@willowinc/ui';

export default function RoleDelete({ refreshData, selectionModel }: { refreshData: () => void, selectionModel: GridRowSelectionModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();

  const handleDelete: MouseEventHandler<HTMLButtonElement> = async (e) => {
    e.preventDefault();
    const DeleteRoleRecordAsync = async () => {
      try {
        loader(true, 'Deleting role');
        // Hide Add Role Dialog and resetData
        setOpen(false);

        await RoleClient.DeleteRole(selectionModel.at(0)?.valueOf() as string);
        //refreshTableData
        refreshData();
        enqueueSnackbar('Role deleted successfully', { variant: 'success' });

      } catch (e:any) {
        enqueueSnackbar('Error while deleting role', { variant: 'error' },e);
      }
      finally {
        loader(false);
      }
    }
    await DeleteRoleRecordAsync();
  }

  return (
    <>
      <Button kind='primary' onClick={() => { setOpen(true) }}> Delete role
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Delete Role">
        <DialogContent>
          <DialogContentText>
            Are you sure want to delete the selected role(s)?
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
