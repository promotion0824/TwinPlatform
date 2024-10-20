import TextField from '@mui/material/TextField';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { RoleType, RoleModel, RoleFieldNames } from '../../types/RoleModel';
import { SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { Stack } from '@mui/material';
import { useState } from 'react';
import { useLoading } from '../../Hooks/useLoading';
import { RoleClient } from '../../Services/AuthClient';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Button, Group, Modal } from '@willowinc/ui';

export default function RoleEdit({ refreshData, getEditModel }: { refreshData: () => void, getEditModel: () => RoleModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors } } = useForm<RoleType>({ resolver: yupResolver(RoleModel.validationSchema), defaultValues: getEditModel() });

  const EditRoleRecordAsync: SubmitHandler<RoleType> = async (data: RoleType) => {
    try {
      loader(true, 'Updating role');
      // Hide Edit role Dialog and resetData
      setOpen(false);
      await RoleClient.UpdateRole(data);

      //refreshTableData
      refreshData();
      enqueueSnackbar('Role updated successfully', { variant: 'success' });
    } catch (e: any) {
      enqueueSnackbar('Error while updating role.', { variant: 'error' }, e);
    }
    finally {
      loader(false);
    }
  }

  return (
    <div>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Edit Role
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Edit Role">
        <DialogContent>
          <DialogContentText>
            Please edit the role name and click submit to update the role or cancel to close the dialog
          </DialogContentText>

          <form onSubmit={handleSubmit(EditRoleRecordAsync)}>
            <Stack direction="column" spacing={2}>
              <TextField
                autoFocus
                margin="dense"
                id={RoleFieldNames.name.field}
                label={RoleFieldNames.name.label}
                type="text"
                fullWidth
                variant="outlined"
                error={errors.name ? true : false}
                helperText={errors.name?.message as string}
                {...register('name')}
              />
              <TextField
                margin="dense"
                id={RoleFieldNames.description.field}
                label={RoleFieldNames.description.label}
                type="text"
                fullWidth
                variant="outlined"
                error={!!errors.description}
                helperText={errors.description?.message as string}
                {...register('description')}
              />
            </Stack>
            <Group justify='flex-end' mt="s24">
              <Button type='reset' kind='secondary' onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary'>Submit</Button>
            </Group>
          </form>
        </DialogContent>
      </Modal>
    </div >
  );
}
