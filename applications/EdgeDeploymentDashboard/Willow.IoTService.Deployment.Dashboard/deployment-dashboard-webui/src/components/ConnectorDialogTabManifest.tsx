import {InteractionRequiredAuthError, InteractionStatus} from '@azure/msal-browser';
import {useMsal} from '@azure/msal-react';
import {yupResolver} from '@hookform/resolvers/yup';
import {Button, DialogActions} from '@mui/material';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import {useEffect, useState} from 'react';
import {useForm} from 'react-hook-form';
import * as yup from 'yup';
import {loginRequest} from '../config';
import {callGetDeploymentManifests} from '../services/dashboardService';
import {EditConnectorDialogProps} from '../types/EditConnectorDialogProps';
import {FormDeploymentSearchTextBox} from './form-components/FormDeploymentSearchTextBox';

interface IFormInputDownloadManifest {
  deploymentId: string;
}

const schema = yup.object({
  deploymentId: yup.string().required("Deployment selection is required"),
}).required();

export default function ConnectorDialogTabManifest(props: EditConnectorDialogProps) {
  const {connector, closeHandler, setOpenError} = props;
  const {instance, inProgress, accounts} = useMsal();
  const [accessToken, setAccessToken] = useState('');

  const methods = useForm<IFormInputDownloadManifest>({resolver: yupResolver(schema)});
  const {handleSubmit, control} = methods;

  useEffect(() => {
    const acquireToken = async () => {
      // Already loaded or loading
      if (accessToken || inProgress !== InteractionStatus.None) {
        return;
      }

      const accessTokenRequest = {
        scopes: loginRequest.scopes,
        account: accounts[0],
      };
      try {
        const accessTokenResponse = await instance.acquireTokenSilent(accessTokenRequest);
        // Acquire token silent success
        const newAccessToken = accessTokenResponse.accessToken;
        setAccessToken(newAccessToken);
      } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
          instance.acquireTokenRedirect(accessTokenRequest);
        }

        console.log(error);
        setOpenError(true);     // Show the erorr Notification
      }
    }

    acquireToken();
  }, [instance, accounts, inProgress, accessToken, setOpenError]);

  const onSubmitDownloadManifest = async (form: IFormInputDownloadManifest) => {
    try {
      const response = await callGetDeploymentManifests(accessToken, form.deploymentId);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', form.deploymentId + `.zip`);
      document.body.appendChild(link);
      link.click();
      link.parentNode?.removeChild(link);
      closeHandler();
    } catch (error: any) {
      console.error(error.message);
      setOpenError(true);     // Show the erorr Notification
    }
  };

  return (
    <Box>
      <Typography sx={{marginBottom: 1, width: '100%'}}>Manifest</Typography>
      <FormDeploymentSearchTextBox
        name="deploymentId"
        control={control}
        label="Deployment"
        filterValue={connector.id}
        sx={{marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%'}}
        setOpenError={setOpenError}
      />
      <DialogActions>
        <Button autoFocus variant="contained" onClick={handleSubmit(onSubmitDownloadManifest)}>
          DOWNLOAD MANIFEST
        </Button>
      </DialogActions>
    </Box>
  );
}
