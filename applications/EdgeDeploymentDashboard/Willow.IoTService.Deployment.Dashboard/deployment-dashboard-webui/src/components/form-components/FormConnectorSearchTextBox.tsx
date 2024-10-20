import {InteractionRequiredAuthError, InteractionStatus} from '@azure/msal-browser';
import {useMsal} from '@azure/msal-react';
import {Autocomplete, Stack, TextField} from '@mui/material';
import {useEffect, useState} from 'react';
import {Controller, useWatch} from 'react-hook-form';
import {loginRequest} from '../../config';
import {callGetModulesSearch, callGetModuleTypesSearch} from '../../services/dashboardService';
import {Connector} from '../../types/Connector';
import {FormInputModuleProps} from '../../types/FormInputProps';

export const FormConnectorSearchTextBox = ({
                                             name,
                                             control,
                                             label,
                                             sx,
                                             index,
                                             filterValue,
                                             moduleVersionChanged,
                                             setOpenError
                                           }: FormInputModuleProps) => {
  const {instance, inProgress, accounts} = useMsal();
  const [connectors, setConnectors] = useState<Connector[]>();
  const [moduleTypeVersions, setModuleTypeVersions] = useState<{ moduleType: string; latestVersion: string; }[]>();
  const [connectorSearch, setConnectorSearch] = useState<string | undefined>(filterValue);
  const [loading, setLoading] = useState<boolean>(false);

  const watchModuleType = useWatch({
    control,
    name: `createDeploymentCommands.${index}.moduleType`,
  });
  const pageSize = 10;
  const page = 1;

  useEffect(() => {
    const getData = setTimeout(async () => {
      // Already loaded or loading
      if (inProgress !== InteractionStatus.None) {
        return;
      }

      const accessTokenRequest = {
        scopes: loginRequest.scopes,
        account: accounts[0],
      };
      try {
        setLoading(true);

        const accessTokenResponse = await instance.acquireTokenSilent(accessTokenRequest);

        // Acquire token silent success
        const accessToken = accessTokenResponse.accessToken;
        const moduleType = watchModuleType === "Any" ? undefined : watchModuleType;
        let response = await callGetModulesSearch(accessToken, pageSize, page, connectorSearch, undefined, moduleType);
        const responseObject = response.data;

        setConnectors(responseObject.items);

        // Read the current version for each module type
        response = await callGetModuleTypesSearch(accessToken, 50, 1);
        setModuleTypeVersions(response?.data?.items);

        setLoading(false);
      } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
          instance.acquireTokenRedirect(accessTokenRequest);
        }

        console.error(error);
        setOpenError(true);     // Show the erorr Notification
        setLoading(false);
      }
    }, 250);

    return () => clearTimeout(getData);
  }, [instance, accounts, inProgress, watchModuleType, connectorSearch, setOpenError]);

  return (
    <Controller
      render={({
                 field: {onChange, onBlur, value, ref},
                 fieldState: {error},
               }) => (
        <Stack sx={sx}>
          <Autocomplete
            onChange={(_, newValue) => {
              if (moduleVersionChanged) {
                // A new connector has been selected, so update the version
                const version = moduleTypeVersions?.find(item => item.moduleType === newValue?.moduleType)?.latestVersion;
                moduleVersionChanged(version);
              }

              onChange(newValue?.id);
            }}
            value={value?.id}
            onInputChange={(_, newInputValue) => setConnectorSearch(newInputValue)}
            options={connectors ?? []}
            loading={loading}
            getOptionLabel={(option: Connector) =>
              `Name: ${option.name} | Device: ${option.deviceName} | Type: ${option.moduleType} | Hub: ${option.ioTHubName}`
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
