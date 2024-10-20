import {InteractionRequiredAuthError, InteractionStatus} from '@azure/msal-browser';
import {useMsal} from '@azure/msal-react';
import {yupResolver} from '@hookform/resolvers/yup';
import { Icon } from '@willowinc/ui';
import {Box, Button, DialogActions} from '@mui/material';
import Dialog from '@mui/material/Dialog';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import IconButton from '@mui/material/IconButton';
import {styled} from '@mui/material/styles';
import * as React from 'react';
import {useCallback, useEffect, useState} from 'react';
import {useForm} from 'react-hook-form';
import * as yup from 'yup';
import {loginRequest} from '../config';
import {callPostDeploymentByModuleType} from '../services/dashboardService';
import {CreateDialogProps} from '../types/CreateDialogProps';
import {FormApplicationTypeSearchTextBox} from './form-components/FormApplicationTypeSearchTextBox';
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
          sx={{
            position: 'absolute',
            right: 8,
            top: 8,
            color: (theme) => theme.palette.grey[500],
          }}
        >
          <Icon icon="close"/>
        </IconButton>
      ) : null}
    </DialogTitle>
  );
};

interface IFormDeploymentByConnectorType {
  connectorType: string;
  version: string;
}

const schema = yup.object({
  connectorType: yup.string().required("Connector Type is required"),
  version: yup.string()
    .matches(/^(\d+\.)?(\d+\.)?(\d+)$/, {
      excludeEmptyString: true,
      message: 'Version must be in X.Y or X.Y.Z format. Example: 1.0 or 1.0.0'
    })
    .required('Version is required')
}).required();

export default function DeployerDialogCreateByConnectorType(props: CreateDialogProps) {
  const {open, closeHandler, onConfirm, setOpenError} = props;

  const defaultValues = {
    connectorType: '',
    version: '',
  }

  const methods = useForm<IFormDeploymentByConnectorType>({
    defaultValues: defaultValues,
    resolver: yupResolver(schema)
  });
  const {handleSubmit, control, reset, watch} = methods;
  const {instance, inProgress, accounts} = useMsal();
  const [accessToken, setAccessToken] = useState<string>('');

  const onSubmitModule = async (form: IFormDeploymentByConnectorType) => {
    const createDeploymentByModuleType = {
      moduleType: form.connectorType,
      version: form.version,
    };

    try {
      await callPostDeploymentByModuleType(accessToken, createDeploymentByModuleType);
      closeHandler();
      onConfirm(true);
    } catch (error) {
      console.log(error)
      setOpenError(true);     // Show the erorr Notification
    }
  };

  const [version, setVersion] = useState<string>('');       // The latest version of the selected connector type

  // The connector type selection changed so the latest version may also change
  const handleConnectorVersionChanged = useCallback((newValue: string) => {
    setVersion(newValue);
  }, []);

  // If the latest connector type version changed, update the default version on the form - keep the connector type
  useEffect(() => {
    reset({version, connectorType: watch('connectorType')});
  }, [version, reset, watch]);

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
  }, [instance, accounts, inProgress, setOpenError]);

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
          Create Deployment By Connector Type
        </BootstrapDialogTitle>
        <DialogContent>
          <Box sx={{width: 450}}>
            <FormApplicationTypeSearchTextBox
              name="connectorType"
              control={control}
              label="Connector Type"
              isAnyAllow={false}
              moduleVersionChanged={handleConnectorVersionChanged}
              sx={{marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%'}}
              setOpenError={setOpenError}
            />
            <FormInputText name="version" control={control} label="Version"
                           sx={{marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%'}}/>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button sx={{marginBottom: 1, marginRight: 1}} variant="contained" onClick={handleSubmit(onSubmitModule)}>
            OK
          </Button>
        </DialogActions>
      </BootstrapDialog>
    </div>
  );
}
