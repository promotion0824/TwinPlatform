import { yupResolver } from '@hookform/resolvers/yup';
import { Stack } from '@mui/material';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import TextField from '@mui/material/TextField';
import { useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { GroupClient } from '../../Services/AuthClient';
import { GroupModel, Group } from '../../types/GroupModel';
import { Button, Group as LayoutGroup, Modal } from '@willowinc/ui';

export default function GroupEdit({ refreshData, getEditModel }: { refreshData: () => void, getEditModel: () => GroupModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors } } = useForm<Group>({ resolver: yupResolver(GroupModel.validationSchema), defaultValues: getEditModel() });

  const EditGroupRecordAsync: SubmitHandler<Group> = async (data: Group) => {
    try {
      loader(true, 'Updating Group');
      // Hide Edit Group Dialog and resetData
      setOpen(false);

      await GroupClient.UpdateGroup(data);

      //refreshTableData
      refreshData();
      enqueueSnackbar('Group updated successfully', { variant: 'success' });
    } catch (e: any) {
      enqueueSnackbar('Error while updating the group', { variant: 'error' }, e);
    }
    finally {
      loader(false);
    }
  }

  return (
    <div>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Edit Group
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Edit Group">
        <DialogContent>
          <DialogContentText>
            Please edit group name and click submit to update the group or cancel to close the dialog
          </DialogContentText>

          <form onSubmit={handleSubmit(EditGroupRecordAsync)}>
            <Stack direction="column" spacing={2}>
              <TextField
                autoFocus
                margin="dense"
                id="name"
                label="Group Name"
                type="text"
                fullWidth
                variant="outlined"
                error={errors.name ? true : false}
                helperText={errors.name?.message as string}
                {...register('name')}
              />

              <TextField
                margin="dense"
                id="type"
                label="Group Type"
                type="text"
                fullWidth
                variant="standard"
                value={getEditModel().groupType?.name}
                color="info"
                sx={{fill:'white',disabled:"disabled"} }
              />

            </Stack>
            <LayoutGroup justify="flex-end" mt="s24">
              <Button type='reset' kind='secondary' onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary'>Submit</Button>
            </LayoutGroup>
          </form>
        </DialogContent>
      </Modal>
    </div >
  );
}
