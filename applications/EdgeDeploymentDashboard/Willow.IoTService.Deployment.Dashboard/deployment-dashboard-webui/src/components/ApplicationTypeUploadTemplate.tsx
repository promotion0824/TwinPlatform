import { InteractionRequiredAuthError, InteractionStatus } from '@azure/msal-browser';
import { useMsal } from '@azure/msal-react';
import { yupResolver } from '@hookform/resolvers/yup';
import { Button, Icon, IconButton } from '@willowinc/ui';
import { Box, DialogActions } from '@mui/material';
import Dialog from '@mui/material/Dialog';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import { styled } from '@mui/material/styles';
import FormData from 'form-data';
import * as React from 'react';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import * as yup from 'yup';
import { loginRequest } from '../config';
import { callPostModuleTypes } from '../services/dashboardService';
import { UploadTemplateDialogProps } from '../types/UploadTemplateDialogProps';
import { FormInputFile } from './form-components/FormInputFile';
import { FormInputText } from './form-components/FormInputText';

const BootstrapDialog = styled(Dialog)(({ theme }) => ({
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
  const { children, onClose, ...other } = props;

  return (
    <DialogTitle sx={{ m: 0, p: 2 }} {...other}>
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
            //color: (theme) => theme.palette.grey[500],
          }}
        >
          <Icon icon='close' />
        </IconButton>
      ) : null}
    </DialogTitle>
  );
};

interface IFormUploadTemplate {
  version: string;
  content: FileList;
}

const schema = yup.object({
  version: yup.string().required('Version is required')
    .matches(/^(\d+\.)?(\d+\.)?(\d+)$/, {
      excludeEmptyString: true,
      message: "Version must be in X.Y or X.Y.Z format. Example: 1.0 or 1.0.0"
    }),
  content: yup.mixed().test('required', 'Please select a file', value => {
    return value && value.length;
  })
}).required();

export default function ApplicationTypeUploadTemplate(props: UploadTemplateDialogProps) {
  const { open, closeHandler, onConfirm, moduleType, setOpenError } = props;

  const defaultValues = {
    version: ''
  }
  const methods = useForm<IFormUploadTemplate>({ defaultValues: defaultValues, resolver: yupResolver(schema) });
  const { handleSubmit, control, reset } = methods;
  const { instance, inProgress, accounts } = useMsal();
  const [accessToken, setAccessToken] = useState<string>('');

  const onSubmitUploadTemplate = async (form: IFormUploadTemplate) => {
    const formData = new FormData();
    formData.append('moduleType', moduleType.name);
    formData.append('version', form.version);
    formData.append('content', form.content[0]);
    try {
      await callPostModuleTypes(accessToken, formData);
      closeHandler();
      onConfirm(true);
    } catch (error) {
      console.log(error);
      setOpenError(true);     // Show the erorr Notification
    }
  }

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
    }
    acquireToken();
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
          {moduleType.name}: Upload New Template
        </BootstrapDialogTitle>
        <DialogContent>
          <Box sx={{ width: 500 }}>
            <FormInputText name="version" control={control} label="Version"
              sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%' }} />
            <FormInputFile name="content" control={control} label="Template File"
              sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: '100%' }} />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button style={{ marginBottom: 1, marginRight: 1 }} variant="contained"
            onClick={handleSubmit(onSubmitUploadTemplate)}>
            OK
          </Button>
        </DialogActions>
      </BootstrapDialog>
    </div>
  );
}
