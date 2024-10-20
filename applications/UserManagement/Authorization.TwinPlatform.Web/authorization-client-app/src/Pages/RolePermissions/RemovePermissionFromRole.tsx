import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { GridRowSelectionModel } from '@willowinc/ui';
import { useState, MouseEventHandler } from 'react';
import { DialogActions } from '@mui/material';
import { RoleModel } from '../../types/RoleModel';
import { useLoading } from '../../Hooks/useLoading';
import { RoleClient } from '../../Services/AuthClient';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Button, Group, Modal } from '@willowinc/ui';

export default function RemovePermissionFromRole({ roleModel, refreshData, selectionModel }: { roleModel: RoleModel, refreshData: () => void, selectionModel: GridRowSelectionModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();

  const handleDelete: MouseEventHandler<HTMLButtonElement> = async (e) => {
    e.preventDefault();
    const DeleteRolePermissionRecordAsync = async () => {
      try {
        loader(true, 'Deleting permission from the role');
        // Hide Add RolePermission Delete Dialog
        setOpen(false);
        await RoleClient.RemovePermissionFromRole(roleModel.id, selectionModel.at(0)?.valueOf() as string);
        //refreshTableData
        refreshData();

        enqueueSnackbar('Permission removed from the role', { variant: 'success' });
      } catch (e: any) {
        enqueueSnackbar('Error while deleting permission from role', { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    await DeleteRolePermissionRecordAsync();
  }

  return (
    <div>
      <Button kind='primary' onClick={() => { setOpen(true) }}> Remove Permission
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Remove Permission">
        <DialogContent>
          <DialogContentText>
            Are you sure want to remove the selected permission?
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
