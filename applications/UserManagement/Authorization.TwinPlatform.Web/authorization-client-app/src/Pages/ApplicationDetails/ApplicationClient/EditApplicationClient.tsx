import { yupResolver } from '@hookform/resolvers/yup';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import { AxiosError, AxiosResponse } from 'axios';
import { useEffect, useState } from 'react';
import { SubmitHandler, useForm } from 'react-hook-form';
import { Button, Group as LayoutGroup, Loader, Modal } from '@willowinc/ui';
import { useLoading } from '../../../Hooks/useLoading';
import { useCustomSnackbar } from '../../../Hooks/useCustomSnackbar';
import { ApplicationClientModel } from '../../../types/ApplicationClientModel';
import { ApplicationApiClient } from '../../../Services/AuthClient';
import { ClientAppPasswordModel, SecretCredentials } from '../../../types/ClientAppPasswordModel';
import { ContentCopy } from '@mui/icons-material';
import { IconButton, InputAdornment } from '@mui/material';

export default function EditApplicationClient({ editModel, refreshData, credentialList }: { editModel: ApplicationClientModel, refreshData: () => void, credentialList: { secrets: SecretCredentials, loaded: boolean } }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const { register, handleSubmit, formState: { errors }, reset } = useForm<ApplicationClientModel>({ resolver: yupResolver(ApplicationClientModel.validationSchema), defaultValues: editModel });
  const [secret, setSecret] = useState<ClientAppPasswordModel>();

  useEffect(() => {
    reset(editModel);
  }, [editModel, reset]);

  useEffect(() => {
    if (!!editModel && credentialList.loaded) {
      setSecret(credentialList.secrets[editModel.clientId]);
    }
  }, [credentialList, editModel]);

  const OnModalClose = () => {
    setOpen(false);
    if (!!secret && !!secret.secretText) // if secret text is visible - we might have generated or rotated the secret so refresh parent grid
    {
      refreshData();
    }
  };

  const UpdateApplicationClient: SubmitHandler<ApplicationClientModel> = async (data: ApplicationClientModel) => {
    try {
      loader(true, 'Updating client details.');
      setOpen(false);

      ApplicationApiClient.UpdateApplicationClient(data).then((d: AxiosResponse<any, any>) => {
        //Reset Form Values
        reset(new ApplicationClientModel());
        //refreshTableData
        refreshData();

        // Hide Add Group Dialog and resetData
        enqueueSnackbar('Client details updated successfully.', { variant: 'success' });
      }).catch((error: AxiosError) => {
        enqueueSnackbar('Error while updating application client.', { variant: 'error' }, error);
      }).finally(() => {
        loader(false);
      });
    } catch (e) {
      console.error(e);
    }
  }

  const generateSecret = async () => {

    try {
      loader(true, 'Generating secret.');
      var response = await ApplicationApiClient.GenerateClientSecret(editModel.application!.name, editModel.name);
      setSecret(response);
      enqueueSnackbar('Secret generated successfully.', { variant: 'success' });

    } catch (e:any) {
      enqueueSnackbar('Error while generating the secret.', { variant: 'error' }, e);
    }
    finally {
      loader(false);
    }
  };

  const handleCopyClick = async (textToCopy: string) => {
    try {
      await navigator.clipboard.writeText(textToCopy);
      enqueueSnackbar('Copied to Clipboard.', { variant: 'success' });
    } catch (err) {
      console.error('Failed to copy: ', err);
    }
  };

  return (
    <>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Edit Client
      </Button>
      <Modal opened={open} onClose={OnModalClose} withCloseButton={false} size="lg" header="Edit Client">
        <DialogContent>
          <DialogContentText>
            Please edit the client name and click submit to update or cancel to close the dialog
          </DialogContentText>
          <form onSubmit={handleSubmit(UpdateApplicationClient)}>
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

              <TextField
                autoFocus
                margin="none"
                id="clientId"
                label="Client Id"
                type="text"
                fullWidth
                variant="outlined"
                InputProps={{
                  readOnly: true,
                  endAdornment:
                    <InputAdornment position="end">
                      <IconButton
                        onClick={(e) => handleCopyClick(editModel.clientId)}
                        aria-label="Copy Client Id">
                        <ContentCopy fontSize="small"></ContentCopy>
                      </IconButton>
                    </InputAdornment>,
                }}
                sx={{ paddingRight: "0px" }}
                {...register('clientId')}
              />

              {credentialList.loaded ?
                <>
                  {!!secret ?
                    <>
                      <TextField
                        autoFocus
                        margin="dense"
                        id="secret"
                        label="Client Secret"
                        type={!secret?.secretText ? "password" : "text"}
                        fullWidth
                        variant="outlined"
                        InputProps={{
                          readOnly: true,
                          endAdornment:
                            <InputAdornment position="end">
                              {!!secret?.secretText ?
                                <IconButton
                                  onClick={(e) => handleCopyClick(secret.secretText)}
                                  aria-label="Copy Client Id">
                                  <ContentCopy fontSize="small"></ContentCopy>
                                </IconButton>
                                :
                                <></>
                              }
                            </InputAdornment>,
                        }}
                        value={!secret?.secretText ? ''.padEnd(40, 'x') : secret.secretText}
                        helperText={`Expires on ${new Date(secret.endTime).toDateString()}` as string}
                      />
                      <Button title="Rotate Client Secret" type='button' kind='negative' variant="contained" onClick={generateSecret}>Rotate</Button>
                    </>
                    :
                    <Button title="Generate New Client Secret" type='button' kind='primary' variant="contained" onClick={generateSecret}>Generate Secret</Button>
                  }
                </>
                :
                <Loader size="sm" variant="dots" />
              }
            </Stack>
            <LayoutGroup justify='flex-end' mt="s24">
              <Button type='reset' kind="secondary" onClick={OnModalClose}>Cancel</Button>
              <Button type='submit' kind='primary' variant="contained">Submit</Button>
            </LayoutGroup>
          </form>
        </DialogContent>
      </Modal>
    </>
  );
}
