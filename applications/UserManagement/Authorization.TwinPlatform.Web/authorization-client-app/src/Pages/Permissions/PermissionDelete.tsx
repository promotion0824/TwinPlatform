import { DialogActions } from '@mui/material';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { GridRowSelectionModel } from '@willowinc/ui';
import { MouseEventHandler, useState } from 'react';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { PermissionClient } from '../../Services/AuthClient';
import { Button, Group, Modal } from '@willowinc/ui';

export default function PermissionDelete({ refreshData, selectionModel }: { refreshData: () => void, selectionModel: GridRowSelectionModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();


  const handleDelete: MouseEventHandler<HTMLButtonElement> = async (e) => {
    e.preventDefault();
    const DeletePermissionRecordAsync = async () => {
      try {
        loader(true, 'Deleting Permission');
        setOpen(false);
        await PermissionClient.DeletePermission(selectionModel.at(0)?.valueOf() as string);
        //refreshTableData
        refreshData();
        enqueueSnackbar('Permission deleted successfully', { variant: 'success' });
      } catch (e: any) {
        enqueueSnackbar('Error while deleting permission', { variant: 'error'},e);
      }
      finally {
        loader(false);
      }
    }
    await DeletePermissionRecordAsync();
  }

  return (
    <div>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Delete Permission
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Delete Permission">
        <DialogContent>
          <DialogContentText>
            Are you sure want to delete the selected permission(s)?
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
