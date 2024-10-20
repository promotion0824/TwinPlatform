import { DialogActions } from '@mui/material';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { GridRowSelectionModel } from '@willowinc/ui';
import { MouseEventHandler, useState } from 'react';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { GroupClient } from '../../Services/AuthClient';
import { Button, Modal } from '@willowinc/ui';

export default function GroupDelete({ refreshData, selectionModel }: { refreshData: () => void, selectionModel: GridRowSelectionModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();

  const handleDelete: MouseEventHandler<HTMLButtonElement> = async (e) => {
    e.preventDefault();
    const DeleteGroupRecordAsync = async () => {
      try {
        loader(true, 'Deleting Group');
        // Hide Add Group Dialog and resetData
        setOpen(false);
        await GroupClient.DeleteGroup(selectionModel.at(0)?.valueOf() as string);
        //refreshTableData
        refreshData();

        enqueueSnackbar('Group deleted successfully', { variant: 'success' });

      } catch (e: any) {
        
        enqueueSnackbar('Error while deleting group', { variant: 'error' },e);
      }
      finally {
        loader(false);
      }
    }
    await DeleteGroupRecordAsync();
  }

  return (
    <div>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Delete Group
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Delete Group">
        <DialogContent>
          <DialogContentText>
            Are you sure want to delete the selected group(s)?
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
