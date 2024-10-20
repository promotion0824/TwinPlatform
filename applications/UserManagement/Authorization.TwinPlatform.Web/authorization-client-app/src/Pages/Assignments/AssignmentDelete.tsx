import * as React from 'react';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { GridRowSelectionModel } from '@mui/x-data-grid';
import { useState, MouseEventHandler } from 'react';
import { DialogActions } from '@mui/material';
import { useLoading } from '../../Hooks/useLoading';
import { AssignmentModel } from '../../types/AssignmentModel';
import { AssignmentClient } from '../../Services/AuthClient';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Button, Group, Modal } from '@willowinc/ui';

export default function AssignmentDelete({ refreshData, selectionModel, rows }: { refreshData: () => void, selectionModel: GridRowSelectionModel, rows: AssignmentModel[] }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();


  const handleDelete: MouseEventHandler<HTMLButtonElement> = async (e) => {
    e.preventDefault();
    const DeleteUserAssignmentRecordAsync = async () => {
      try {
        loader(true, 'Deleting Assignment');
        // Hide Add Group Dialog and resetData
        setOpen(false);
        const selectedId: string = selectionModel.at(0)?.valueOf() as string;
        const selectedRowModel = rows.find((item) => { return item.id === selectedId });

        if (selectedRowModel?.type == 'U') {
          await AssignmentClient.DeleteUserAssignment(selectedId);
        } else {
          await AssignmentClient.DeleteGroupAssignment(selectedId);
        }

        //refreshTableData
        refreshData();
        enqueueSnackbar("Assignment removed successfully", { variant: 'success' });

      } catch (e:any) {
        enqueueSnackbar('Error while deleting user assignment', { variant: 'error' },e);
      }
      finally {
        loader(false);
      }
    }
    await DeleteUserAssignmentRecordAsync();
  }

  return (
    <div>
      <Button kind='primary' onClick={() => { setOpen(true) }}> Delete Assignment
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
