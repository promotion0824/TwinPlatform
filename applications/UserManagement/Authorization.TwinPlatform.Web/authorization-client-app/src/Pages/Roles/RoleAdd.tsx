import TextField from '@mui/material/TextField';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { RoleType, RoleModel, RoleFieldNames } from '../../types/RoleModel';
import { SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useState } from 'react';
import Stack from '@mui/material/Stack';
import { useLoading } from '../../Hooks/useLoading';
import { RoleClient } from '../../Services/AuthClient';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Button, Group, Modal } from '@willowinc/ui';

export default function RoleAdd({ refreshData }: { refreshData: () => void }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, reset } = useForm<RoleType>({ resolver: yupResolver(RoleModel.validationSchema), defaultValues: new RoleModel() });

  const AddRoleRecordAsync: SubmitHandler<RoleType> = async (data: RoleType) => {
    try {
      loader(true, 'Adding Role');
      // Hide Add Role Dialog and resetData
      setOpen(false);
      await RoleClient.AddRole(data);

      //Reset Form Values
      reset(new RoleModel());
      //refreshTableData
      refreshData();
      enqueueSnackbar('Role added successfully', { variant: 'success' });
    } catch (e:any) {
      enqueueSnackbar('Error while creating role.', { variant: 'error' },e);
    }
    finally {
      loader(false);
    }
  }

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Add Role
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Add Role">
        <DialogContent>
          <DialogContentText>
            Please provide a role name and click Add to create a role or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddRoleRecordAsync)}>
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
              <Button type='reset' kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary' variant="contained">Add</Button>
            </Group>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
