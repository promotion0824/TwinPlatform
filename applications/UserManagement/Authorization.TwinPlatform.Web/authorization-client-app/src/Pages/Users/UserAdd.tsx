import TextField from '@mui/material/TextField';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import { Button, Group, Modal } from '@willowinc/ui';
import { Controller, SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { useState, useCallback } from 'react';
import Stack from '@mui/material/Stack';
import { UserFieldNames, UserModel, UserType } from '../../types/UserModel';
import { Checkbox, FormControl, FormControlLabel, FormLabel } from '@mui/material';
import { useLoading } from '../../Hooks/useLoading';
import { UserClient } from '../../Services/AuthClient';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';

export default function UserAdd({ refreshData }: { refreshData: () => void }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, control, handleSubmit, formState: { errors }, reset } = useForm<UserType>({ resolver: yupResolver(UserModel.validationSchema), defaultValues: new UserModel() });

  const AddUserRecordAsync: SubmitHandler<UserType> = async (data: UserType) => {
    try {
      loader(true, 'Adding new user');
      // Hide Add User Dialog and resetData
      setOpen(false);
      await UserClient.AddUser(data);
      //Reset Form Values
      reset(new UserModel());
      //refreshTableData
      refreshData();

      enqueueSnackbar('User added successfully', { variant: 'success' });

    } catch (e:any) {      
      enqueueSnackbar('Error while adding new user', { variant: 'error' },e);
    }
    finally {
      loader(false);
    }
  }

  return (
    <div>
      <Button color='primary' variant="contained" onClick={useCallback(() => { setOpen(true) },[])}> Add User
      </Button>
      <Modal opened={open} onClose={useCallback(() => { setOpen(false) },[])} withCloseButton={false} header="Add User" size="lg">
        <DialogContent>
          <DialogContentText>
            Please provide user details and click Add to create a user or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddUserRecordAsync)}>
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
                      render={({ field: { onChange } }) => (
                        <Checkbox defaultChecked defaultValue={0}
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
              <Button type='reset' kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary' variant="contained">Add</Button>
            </Group>
          </form>
        </DialogContent>
      </Modal>
    </div>
  );
}
