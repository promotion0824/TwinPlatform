import { yupResolver } from '@hookform/resolvers/yup';
import { Stack } from '@mui/material';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import TextField from '@mui/material/TextField';
import { useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { ApplicationApiClient, PermissionClient } from '../../Services/AuthClient';
import { PermissionFieldNames, PermissionModel, PermissionType } from '../../types/PermissionModel';
import { Button, Group, Modal } from '@willowinc/ui';
import { ApplicationModel } from '../../types/ApplicationModel';
import FormAutoComplete from '../../Components/FormComponents/FormAutoComplete';

export default function PermissionEdit({ refreshData, getEditModel }: { refreshData: () => void, getEditModel: () => PermissionModel }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors },control } = useForm<PermissionType>({ resolver: yupResolver(PermissionModel.validationSchema), defaultValues: getEditModel() });

  const [apps, setApps] = useState<{ data: ApplicationModel[] }>({ data: [] });

  const OnSearchApp = async (searchText: string) => {
    if (apps.data.length > 0) {
      return;
    }
    let allApps = await ApplicationApiClient.GetAllApplications();
    setApps({ data: allApps });
  };

  const EditPermissionAsync: SubmitHandler<PermissionModel> = async (data) => {
    try {
      loader(true, 'Updating Permission');
      // Hide Add Permission Dialog and resetData
      setOpen(false);
      await PermissionClient.UpdatePermission(data);

      //refreshTableData
      refreshData();
      enqueueSnackbar('Permission updated successfully', { variant: 'success' });
    } catch (e: any) {
      enqueueSnackbar('Error while updating the permission.', { variant: 'error' }, e);
    }
    finally {
      loader(false);
    }
  }

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Edit Permission
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Edit Permission">
        <DialogContent>
          <DialogContentText>
            Please edit permission details and click submit to update the permission or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(EditPermissionAsync)}>
            <Stack spacing={2}>
              <TextField
                autoFocus
                margin="dense"
                id={PermissionFieldNames.name.field}
                label={PermissionFieldNames.name.label}
                type="text"
                fullWidth
                variant="outlined"
                error={errors.name ? true : false}
                helperText={errors.name?.message as string}
                {...register('name')}
              />

              <FormAutoComplete<PermissionModel, ApplicationModel>
                control={control}
                label={PermissionFieldNames.applicationName.label}
                errors={errors}
                fieldName={PermissionFieldNames.applicationName.field}
                options={apps.data}
                OnUpdateInput={OnSearchApp}
                getOptionLabel={(option) => {
                  return option.name;
                }}
                isOptionEqToValue={(option, value) => { return option.id === value.id }}
              />

              <TextField
                margin="dense"
                id={PermissionFieldNames.description.field}
                label={PermissionFieldNames.description.label}
                type="text"
                fullWidth
                variant="outlined"
                error={errors.description ? true : false}
                helperText={errors.description?.message as string}
                {...register('description')}
              />

              <Group justify='flex-end' mt="s24">
                <Button type='reset' kind='secondary' onClick={() => setOpen(false)}>Cancel</Button>
                <Button type='submit' kind='primary'>Submit</Button>
              </Group>
            </Stack>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
