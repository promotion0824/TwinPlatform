import {InteractionRequiredAuthError, InteractionStatus} from '@azure/msal-browser';
import {useMsal} from '@azure/msal-react';
import {Autocomplete, Stack, TextField} from '@mui/material';
import {useEffect, useState} from 'react';
import {Controller} from 'react-hook-form';
import {loginRequest} from '../../config';
import {callGetModuleTypesSearch} from '../../services/dashboardService';
import {FormInputApplicationTypeProps} from '../../types/FormInputProps';

export const FormApplicationTypeSearchTextBox = ({
                                                   name,
                                                   control,
                                                   label,
                                                   sx,
                                                   isAnyAllow = true,
                                                   moduleVersionChanged,
                                                   setOpenError
                                                 }: FormInputApplicationTypeProps) => {
  const {instance, inProgress, accounts} = useMsal();
  const [moduleTypes, setModuleTypes] = useState<string[]>([]);
  const [moduleTypeSearch, setModuleTypeSearch] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [moduleTypeVersions, setModuleTypeVersions] = useState<{ moduleType: string; latestVersion: string; }[]>([]);

  const pageSize = 20;
  const page = 1;

  useEffect(() => {
    const getData = setTimeout(async () => {
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
        const response = await callGetModuleTypesSearch(accessToken, pageSize, page, moduleTypeSearch === "Any" ? undefined : moduleTypeSearch);

        const responseObject = response.data;

        setModuleTypeVersions(responseObject?.items);

        const map = (responseObject.items as []).map((value: any) => value.moduleType);
        if (isAnyAllow) {
          setModuleTypes(["Any"].concat(map));
        } else {
          setModuleTypes(map);
        }

        setLoading(false);
      } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
          instance.acquireTokenRedirect(accessTokenRequest);
        }

        console.log(error);
        setOpenError(true);     // Show the erorr Notification
        setLoading(false);
      }
    }, 250);

    return () => clearTimeout(getData);
  }, [instance, accounts, inProgress, moduleTypeSearch, isAnyAllow, setOpenError]);

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
                const version = moduleTypeVersions?.find(item => item.moduleType === newValue)?.latestVersion;
                moduleVersionChanged(version);
              }

              onChange(newValue)
            }}
            value={value}
            onInputChange={(_, newInputValue) => setModuleTypeSearch(newInputValue)}
            options={moduleTypes ?? []}
            loading={loading}
            getOptionLabel={(option: string) => option}
            isOptionEqualToValue={(option, value) => value ? option === value : true}
            renderInput={(params) => <TextField {...params} label={label} helperText={error ? error.message : null}
                                                error={!!error}/>}
          />
        </Stack>
      )}
      name={name}
      control={control}
    />);
}
