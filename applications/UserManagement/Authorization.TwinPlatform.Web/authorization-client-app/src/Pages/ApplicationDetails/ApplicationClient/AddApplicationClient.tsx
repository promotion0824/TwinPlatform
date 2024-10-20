import { yupResolver } from '@hookform/resolvers/yup';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { AxiosError, AxiosResponse } from 'axios';
import { useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Button, Group as LayoutGroup, Modal } from '@willowinc/ui';
import { useLoading } from '../../../Hooks/useLoading';
import { useCustomSnackbar } from '../../../Hooks/useCustomSnackbar';
import { ApplicationClientModel } from '../../../types/ApplicationClientModel';
import { ApplicationApiClient } from '../../../Services/AuthClient';
import { ApplicationModel } from '../../../types/ApplicationModel';

export default function AddApplicationClient({ application, refreshData }: { application: ApplicationModel, refreshData: () => void }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, reset } = useForm<ApplicationClientModel>({ resolver: yupResolver(ApplicationClientModel.validationSchema), defaultValues: new ApplicationClientModel() });

  const AddApplicationClient: SubmitHandler<ApplicationClientModel> = async (data: ApplicationClientModel) => {
    try {
      loader(true, 'Adding application client.');
      setOpen(false);

      // set the Application
      data.application = application;

      ApplicationApiClient.AddApplicationClient(data).then((d: AxiosResponse<any, any>) => {
        //Reset Form Values
        reset(new ApplicationClientModel());
        //refreshTableData
        refreshData();

        // Hide Add Group Dialog and resetData
        enqueueSnackbar('Application Client created successfully.', { variant: 'success' });
      }).catch((error: AxiosError) => {
        enqueueSnackbar('Error while creating application client.', { variant: 'error' }, error);
      }).finally(() => {
        loader(false);
      });
    } catch (e) {
      console.error(e);
    }
  }

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Add Client
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Add Client">
        <DialogContent>
          <DialogContentText>
            Please provide the client name and click Add to create the client or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(AddApplicationClient)}>
            <Stack spacing={2}>

              <TextField
                autoFocus
                margin="dense"
                id="name"
                label="Client name"
                type="text"
                fullWidth
                variant="outlined"
                error={errors.name ? true : false}
                helperText={errors.name?.message as string}
                {...register('name')}
              />

              <TextField
                autoFocus
                margin="dense"
                id="description"
                label="Client description"
                type="text"
                fullWidth
                variant="outlined"
                error={errors.description ? true : false}
                helperText={errors.description?.message as string}
                {...register('description')}
              />

            </Stack>
            <LayoutGroup justify='flex-end' mt="s24">
              <Button type='reset' kind="secondary" onClick={() => setOpen(false)}>Cancel</Button>
              <Button type='submit' kind='primary' variant="contained">Add</Button>
            </LayoutGroup>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
