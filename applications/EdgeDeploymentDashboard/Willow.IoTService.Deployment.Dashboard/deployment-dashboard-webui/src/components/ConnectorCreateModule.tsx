import {InteractionRequiredAuthError, InteractionStatus} from '@azure/msal-browser';
import {useMsal} from '@azure/msal-react';
import {yupResolver} from '@hookform/resolvers/yup';
import { Button, Icon, IconButton, } from '@willowinc/ui';
import {Box, DialogActions} from '@mui/material';
import Dialog from '@mui/material/Dialog';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import {styled} from '@mui/material/styles';
import * as React from 'react';
import {useEffect, useState} from 'react';
import {useForm} from 'react-hook-form';
import * as yup from 'yup';
import {loginRequest} from '../config';
import {callPostModules} from '../services/dashboardService';
import {CreateDialogProps} from '../types/CreateDialogProps';
import {FormApplicationTypeSearchTextBox} from './form-components/FormApplicationTypeSearchTextBox';
import {FormInputCheckbox} from './form-components/FormInputCheckbox';
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
          icon="close"
          style={{
            position: 'absolute',
            right: 8,
            top: 8,
            //color: (theme) => theme.palette.grey[500],
          }}
        >
        </IconButton>
      ) : null}
    </DialogTitle>
  );
};

interface IFormCreateModule {
  name: string;
  applicationType: string;
  siteId: string;
  isBaseModule: boolean;
}

// Regular expression to check if string is a valid UUID
const regexExp = /^[0-9a-fA-F]{8}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{4}\b-[0-9a-fA-F]{12}$/gi;

const schema = yup.object().shape({
  name: yup.string().required("Name is required"),
  applicationType: yup.string().when('isBaseModule', {
    is: false,
    then: yup.string().required("Application Type is required"),
    otherwise: yup.string().notRequired().nullable()
  }),
  siteId: yup.string().required('SiteId is required')
    .matches(regexExp, {excludeEmptyString: false, message: "Please enter valid UUID string"}),
  isBaseModule: yup.bool().required()
}).required();

export default function DeploymentDialogCreateDeployment(props: CreateDialogProps) {
  const {open, closeHandler, onConfirm, setOpenError} = props;

  const defaultValues = {
    name: '',
    applicationType: '',
    siteId: '',
    isBaseModule: false
  }
  const methods = useForm<IFormCreateModule>({defaultValues: defaultValues, resolver: yupResolver(schema)});
  const {handleSubmit, control, reset} = methods;
  const {instance, inProgress, accounts} = useMsal();
  const [accessToken, setAccessToken] = useState<string>('');

  const onSubmitModule = async (form: IFormCreateModule) => {
    const createModule = {
      name: form.name,
      applicationType: form.applicationType,
      siteId: form.siteId,
      isBaseModule: form.isBaseModule
    };

    try {
      await callPostModules(accessToken, createModule);
      closeHandler();
      onConfirm(true);
    } catch (error) {
      console.log(error)
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
          Create Module
        </BootstrapDialogTitle>
        <DialogContent>
          <Box sx={{width: 450}}>
            <FormInputText name="name" control={control} label="Name"
                           sx={{marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%'}}/>
            <FormApplicationTypeSearchTextBox
              name="applicationType"
              control={control}
              label="Application Type"
              isAnyAllow={false}
              sx={{marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%'}}
              setOpenError={setOpenError}
            />
            <FormInputText name="siteId" control={control} label="Site Id"
                           sx={{marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%'}}/>
            <FormInputCheckbox name="isBaseModule" control={control} label="Base Module"/>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button style={{marginBottom: 1, marginRight: 1}} variant="contained" onClick={handleSubmit(onSubmitModule)}>
            OK
          </Button>
        </DialogActions>
      </BootstrapDialog>
    </div>
  );
}
