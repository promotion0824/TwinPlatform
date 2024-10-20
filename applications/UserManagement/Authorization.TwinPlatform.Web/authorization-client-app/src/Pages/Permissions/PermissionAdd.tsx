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

export default function PermissionAdd({ refreshData }: { refreshData: () => void }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, reset, control } = useForm<PermissionType>({ resolver: yupResolver(PermissionModel.validationSchema), defaultValues: new PermissionModel() });
  const [apps, setApps] = useState<{ data: ApplicationModel[] }>({ data: [] });

  const OnSearchApp = async (searchText: string) => {
    if (apps.data.length > 0) {
      return;
    }
    let allApps = await ApplicationApiClient.GetAllApplications();
    setApps({ data: allApps });
  };

  const AddPermissionRecordAsync: SubmitHandler<PermissionModel> = async (data) => {
    try {
      loader(true, 'Adding Permission');
      // Hide Add Group Dialog and resetData
      setOpen(false);
      await PermissionClient.AddPermission(data);
      //refreshTableData
      refreshData();
      //reset Fomr
      reset(new PermissionModel());
    } catch (e:any) {
      enqueueSnackbar('Error while creating the permission.', { variant: 'error' },e);
    }
    finally {
      loader(false);
    }
  }

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Add Permission
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} header="Add Permission" size="lg">
        <DialogContent>
          <DialogContentText>
            Please provide permission details and click Add to create a permission or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddPermissionRecordAsync)}>
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
              <Group justify='flex-end'>
                <Button type='reset'  kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
                <Button type='submit' kind='primary' variant="contained">Add</Button>
              </Group>
            </Stack>

          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
