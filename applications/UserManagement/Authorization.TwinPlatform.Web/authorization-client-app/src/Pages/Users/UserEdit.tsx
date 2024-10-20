import TextField from '@mui/material/TextField';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { Controller, SubmitHandler, useForm, useFormState } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useState } from 'react';
import Stack from '@mui/material/Stack';
import { UserFieldNames, UserModel, UserType } from '../../types/UserModel';
import { Checkbox, FormControl, FormControlLabel, FormLabel } from '@mui/material';
import { useLoading } from '../../Hooks/useLoading';
import { UserClient } from '../../Services/AuthClient';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Button, Group, Modal } from '@willowinc/ui';

export default function UserEdit({ refreshData, getEditModel }: { refreshData: () => void, getEditModel: () => UserModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, control, handleSubmit, formState: { errors } } = useForm<UserType>({ resolver: yupResolver(UserModel.validationSchema), defaultValues: getEditModel() });
  const EditUserRecordAsync: SubmitHandler<UserType> = async (data: UserType) => {
    try {
      loader(true, 'Updating User');
      // Hide Edit User Dialog and resetData
      setOpen(false);
      await UserClient.UpdateUser(data);
      //refreshTableData
      refreshData();
      enqueueSnackbar('User record updated successfully', { variant: 'success' });

    } catch (e: any) {
      enqueueSnackbar('Error while updating user', { variant: 'error' }, e);
    }
    finally {
      loader(false);
    }
  }

  return (
    <div>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Edit User
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Edit User">
        <DialogContent>
          <DialogContentText>
            Please provide user details and click submit to update user or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(EditUserRecordAsync)}>
            <Stack spacing={2}>

              <TextField
                autoFocus
                margin="dense"
                id={UserFieldNames.firstName.field}
                label={UserFieldNames.firstName.label}
                type="text"
                fullWidth
                variant="outlined"
                error={errors.firstName ? true : false}
                helperText={errors.firstName?.message as string}
                {...register('firstName')}
              />
              <TextField
                autoFocus
                margin="dense"
                id={UserFieldNames.lastName.field}
                label={UserFieldNames.lastName.label}
                type="text"
                fullWidth
                variant="outlined"
                error={errors.lastName ? true : false}
                helperText={errors.lastName?.message as string}
                {...register('lastName')}
              />
              <TextField
                autoFocus
                margin="dense"
                id={UserFieldNames.email.field}
                label={UserFieldNames.email.label}
                type="text"
                fullWidth
                variant="outlined"
                error={errors.email ? true : false}
                helperText={errors.email?.message as string}
                {...register('email')}
              />
              <FormControl size={"small"} variant={"outlined"}>
                <FormLabel >{UserFieldNames.status.label}</FormLabel>
                <FormControlLabel
                  control={
                    <Controller
                      name={'status'}
                      render={({ field: { onChange, value } }) => (
                        <Checkbox defaultChecked={value === 0} defaultValue={value}
                          onChange={e => onChange(e.target.checked ? 0 : 1)}
                        />
                      )}
                      control={control}
                    />
                  }
                  label={'Active'}
                />
              </FormControl>
            </Stack>

            <Group justify='flex-end'>
              <Button type='reset' kind='secondary' variant="contained" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary' variant="contained">Submit</Button>
            </Group>
          </form>
        </DialogContent>
      </Modal>
    </div>
  );
}
