import { InteractionRequiredAuthError, InteractionStatus } from '@azure/msal-browser';
import { useMsal } from '@azure/msal-react';
import { yupResolver } from '@hookform/resolvers/yup';
import { Button, Icon, IconButton } from '@willowinc/ui';
import { DialogActions, Grid } from '@mui/material';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { useEffect, useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import * as yup from 'yup';
import { loginRequest } from '../config';
import { callPutModules } from '../services/dashboardService';
import { EditConnectorDialogProps } from '../types/EditConnectorDialogProps';
import { FormInputCheckbox } from './form-components/FormInputCheckbox';
import { FormInputText } from './form-components/FormInputText';
import { FormPlatformSearchTextBox } from './form-components/FormPlatformTextBox';

interface IFormEnvironment {
  EnvKey: string;
  EnvValue: string;
}

interface IFormInputUpdateConfiguration {
  ioTHubName: string;
  deviceName: string;
  isAutoDeploy: boolean;
  env: IFormEnvironment[]
  platform: string;
}

const createEnvSchema = yup.object({
  EnvKey: yup.string().required("Variable key is required"),
  EnvValue: yup.string().required("Variable value is required")
}).required();

const schema = yup.object({
  ioTHubName: yup.string().required("IoT Hub is required"),
  deviceName: yup.string().required("Device Name is required"),
  env: yup.array().of(createEnvSchema)
}).required();

export default function ConnectorDialogTabs(props: EditConnectorDialogProps) {
  const { connector, closeHandler, onConfirm, setOpenError } = props;
  const { instance, inProgress, accounts } = useMsal();
  const [accessToken, setAccessToken] = useState('');

  let env: { EnvKey: string; EnvValue: unknown; }[] = [];
  if (connector.environment && connector.environment !== "{}") {
    const objectArray = Object.entries(JSON.parse(connector.environment));
    objectArray.forEach(([key, value]) => env.push({ "EnvKey": key, "EnvValue": value }));
  } else {
    env.push({ "EnvKey": "", "EnvValue": "" });
  }

  const defaultValues = {
    ioTHubName: connector.ioTHubName ?? '',
    deviceName: connector.deviceName ?? '',
    isAutoDeploy: connector.isAutoDeployment ?? false,
    env: env as IFormEnvironment[],
    platform: connector.platform ?? 'arm64v8',
  }
  const methods = useForm<IFormInputUpdateConfiguration>({ defaultValues: defaultValues, resolver: yupResolver(schema) });
  const { handleSubmit, control } = methods;
  const { fields, remove, insert } = useFieldArray({
    control,
    name: 'env',
    keyName: 'id'
  });

  const addItem = (currentIndex: number) => {
    insert(++currentIndex, { EnvKey: '', EnvValue: '' });
  };

  useEffect(() => {
    const getAccessToken = async () => {
      // Already loaded or loading
      if (inProgress !== InteractionStatus.None) {
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

    getAccessToken();
  }, [instance, accounts, inProgress, accessToken, setOpenError]);

  const onSubmitSaveConfiguration = async (form: IFormInputUpdateConfiguration) => {
    const updatedConnector = {
      id: connector.id,
      ioTHubName: form.ioTHubName,
      deviceName: form.deviceName,
      isAutoDeploy: form.isAutoDeploy,
      environment: form.env ? JSON.stringify(Object.fromEntries(form.env.map(x => [x.EnvKey, x.EnvValue]))) : "",
      platform: form.platform,
    };

    try {
      await callPutModules(accessToken, updatedConnector);

      closeHandler();
      onConfirm(true);
    } catch (error) {
      console.log(error);
      setOpenError(true);     // Show the erorr Notification
    }
  };

  return (
    <Box>
      <Typography sx={{ marginBottom: 1, marginRight: 1, width: '100%' }}>Configuration</Typography>
      <FormInputText name="ioTHubName" control={control} label="IoT Hub"
        sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%' }} />
      <FormInputText name="deviceName" control={control} label="Device Name"
        sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%' }} />
      <FormPlatformSearchTextBox name="platform" control={control} label="platform"
        sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%' }} />
      <FormInputCheckbox name="isAutoDeploy" control={control} label="Auto Deploy" />
      <Grid container>
        {fields.map((item, index) => (
          <Grid item key={item.id}>
            <Box sx={{ width: '100%', display: "flex", justifyContent: "space-between" }}>
              <FormInputText
                name={`env.${index}.EnvKey`}
                control={control}
                label="Env Variable Key"
                sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: 210 }}
              />
              <FormInputText
                name={`env.${index}.EnvValue`}
                control={control}
                label="Env Variable Value"
                sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: 210 }}
              />
              <IconButton icon="add" onClick={() => addItem(index)} style={{ marginTop: 1, marginBottom: 1, marginRight: 1 }}>
                {/*<Icon icon="add" color="primary" />*/}
              </IconButton>
              <IconButton icon="remove" onClick={() => remove(index)} style={{ marginTop: 1, marginBottom: 1, marginRight: 1 }}>
                {/*<Icon icon="remove" color="primary" />*/}
              </IconButton>
            </Box>
          </Grid>
        ))}
      </Grid>
      <DialogActions>
        <Button variant="contained" onClick={handleSubmit(onSubmitSaveConfiguration)}>
          APPLY CHANGES
        </Button>
      </DialogActions>
    </Box>
  );
}
