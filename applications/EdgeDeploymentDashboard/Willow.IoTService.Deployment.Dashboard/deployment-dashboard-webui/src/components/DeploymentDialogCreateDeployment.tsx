import {InteractionRequiredAuthError, InteractionStatus} from '@azure/msal-browser';
import {useMsal} from '@azure/msal-react';
import {yupResolver} from '@hookform/resolvers/yup';
import { IconButton, Button, Icon } from '@willowinc/ui';
import {Box, DialogActions} from '@mui/material';
import Dialog from '@mui/material/Dialog';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import {styled} from '@mui/material/styles';
import * as React from 'react';
import {useCallback, useEffect, useState} from 'react';
import {useForm} from 'react-hook-form';
import * as yup from 'yup';
import {loginRequest} from '../config';
import {callPostDeployment} from '../services/dashboardService';
import {CreateDeploymentDialogProps} from '../types/CreateDeploymentDialogProps';
import {FormConnectorSearchTextBox} from './form-components/FormConnectorSearchTextBox';
import {FormInputText} from './form-components/FormInputText';

const BootstrapDialog = styled(Dialog)(({theme}) => ({
  '& .MuiDialogContent-root': {
    padding: theme.spacing(2),
  },
  '& .MuiDialogActions-root': {
    padding: theme.spacing(1),
  },
}));

interface DialogTitleProps {
  id: string;
  children?: React.ReactNode;
  onClose: () => void;
}

const BootstrapDialogTitle = (props: DialogTitleProps) => {
  const {children, onClose, ...other} = props;

  return (
    <DialogTitle sx={{m: 0, p: 2}} {...other}>
      {children}
      {onClose ? (
        <IconButton
          aria-label="close"
          onClick={onClose}
          style={{
            position: 'absolute',
            right: 8,
            top: 8,
            //TODO:
            //color: (theme: any): any => theme.palette.grey[500],
          }}
        >
          <Icon icon="close"/>
        </IconButton>
      ) : null}
    </DialogTitle>
  );
};

interface IFormCreateDeployment {
  moduleId: string;
  version: string;
}

const schema = yup.object({
  moduleId: yup.string()
    .required('Connector is required'),
  version: yup.string()
    .matches(/^(\d+\.)?(\d+\.)?(\d+)$/, {
      excludeEmptyString: true,
      message: 'Version must be in X.Y or X.Y.Z format. Example: 1.0 or 1.0.0'
    })
    .required('Version is required')
}).required();

export default function DeploymentDialogCreateDeployment(props: CreateDeploymentDialogProps) {
  const {open, closeHandler, onConfirm, connector, setOpenError} = props;

  const defaultValues = {
    moduleId: connector?.id,
    version: connector?.version ?? '',
  }
  const methods = useForm<IFormCreateDeployment>({defaultValues: defaultValues, resolver: yupResolver(schema)});
  const {handleSubmit, control, reset, watch} = methods;
  const {instance, inProgress, accounts} = useMsal();
  const [accessToken, setAccessToken] = useState<string>('');

  const onSubmitDeployment = async (form: IFormCreateDeployment) => {
    const deployment = {
      moduleId: form.moduleId,
      version: form.version
    };

    try {
      await callPostDeployment(accessToken, deployment);
      closeHandler();
      onConfirm(true);
    } catch (error) {
      console.log(error);
      setOpenError(true);     // Show the erorr Notification
    }
  };

  useEffect(() => {
    const waitReset = async () => {
      await new Promise(resolve => setTimeout(resolve, 1000));
      reset();
    };

    if (!open) {
      waitReset();
    }
  }, [open, reset]);

  useEffect(() => {
    const acquireToken = async () => {
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
    };

    acquireToken();
  }, [instance, accounts, inProgress, setOpenError]);

  const [version, setVersion] = useState<string>(connector?.version ?? '');       // The latest version of the selected module type

  // The module type selection changed so the latest version may also change
  const handleModuleVersionChanged = useCallback((newValue: string) => {
    setVersion(newValue);
  }, []);

  // If the latest module type version changed, update the default version on the form - keep the module id
  useEffect(() => {
    reset({version, moduleId: watch('moduleId')});
  }, [version, reset, watch]);

  return (
    <div>
      <BootstrapDialog
        open={open}
        onClose={() => {
          closeHandler();
        }}
        aria-labelledby="dialog-title"
      >
        <BootstrapDialogTitle id="dialog-title" onClose={() => {
          closeHandler();
        }}>
          Create Deployment
        </BootstrapDialogTitle>
        <DialogContent>
          <Box sx={{width: 450}}>
            <FormConnectorSearchTextBox
              name="moduleId"
              control={control}
              label="Connector"
              filterValue={connector?.name}
              setValue={connector}
              moduleVersionChanged={handleModuleVersionChanged}
              sx={{margin: "auto", marginTop: 1, marginBottom: 1}}
              setOpenError={setOpenError}
            />
            <FormInputText name="version" control={control} label="Version"
                           sx={{marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%'}}/>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button style={{marginBottom: 1, marginRight: 1}} variant="contained" onClick={handleSubmit(onSubmitDeployment)}>
            OK
          </Button>
        </DialogActions>
      </BootstrapDialog>
    </div>
  );
}
