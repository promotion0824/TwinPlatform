import {InteractionRequiredAuthError, InteractionStatus} from '@azure/msal-browser';
import {useMsal} from '@azure/msal-react';
import {Autocomplete, Stack, TextField} from '@mui/material';
import {useEffect, useState} from 'react';
import {Controller} from 'react-hook-form';
import {loginRequest} from '../../config';
import {callGetDeploymentsSearch} from '../../services/dashboardService';
import {DeploymentRecord} from '../../types/DeploymentRecord';
import {FormInputProps} from '../../types/FormInputProps';

export const FormDeploymentSearchTextBox = ({name, control, label, filterValue, sx, setOpenError}: FormInputProps) => {
  const {instance, inProgress, accounts} = useMsal();
  const [deployments, setDeployments] = useState<DeploymentRecord[]>();
  const [loading, setLoading] = useState<boolean>(false);
  const pageSize = 10;
  const page = 1;

  useEffect(() => {
    const getDeployments = async () => {
      // Already loaded or loading
      if (deployments || inProgress !== InteractionStatus.None) {
        return;
      }

      const accessTokenRequest = {
        scopes: loginRequest.scopes,
        account: accounts[0],
      };

      setLoading(true);

      try {
        const accessTokenResponse = await instance.acquireTokenSilent(accessTokenRequest);
        // Acquire token silent success
        const accessToken = accessTokenResponse.accessToken;

        const response = await callGetDeploymentsSearch(accessToken, pageSize, page, filterValue ?? null, null);
        const responseObject = response.data;
        setDeployments(responseObject.items);
        setLoading(false);
      } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
          instance.acquireTokenRedirect(accessTokenRequest);
        }

        console.log(error);
        if (setOpenError) {
          setOpenError(true);     // Show the erorr Notification
        }
      }
    };

    getDeployments();
  }, [instance, accounts, inProgress, deployments, filterValue, setOpenError]);

  return (
    <Controller
      render={({
                 field: {onChange, onBlur, value, ref},
                 fieldState: {error},
               }) => (
        <Stack sx={sx}>
          <Autocomplete
            onChange={(_, newValue) => onChange(newValue?.id)}
            value={value?.id}
            options={deployments ?? []}
            loading={loading}
            getOptionLabel={(option: DeploymentRecord) =>
              `ID: ${option.id}
                     Version: ${option.version}
                     Date: ${new Date(option.dateTimeApplied).toISOString().slice(0, -5)}`
            }
            isOptionEqualToValue={(option, value) => option.id === value.id}
            renderInput={(params) => <TextField {...params} label={label} helperText={error ? error.message : null}
                                                error={!!error}/>}
          />
        </Stack>
      )}
      name={name}
      control={control}
    />);
}
