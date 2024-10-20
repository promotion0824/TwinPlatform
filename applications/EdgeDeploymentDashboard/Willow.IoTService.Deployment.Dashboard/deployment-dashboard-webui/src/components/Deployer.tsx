import { InteractionRequiredAuthError, InteractionStatus } from '@azure/msal-browser';
import { useMsal } from '@azure/msal-react';
import { yupResolver } from '@hookform/resolvers/yup';
import { Icon, Panel, PanelGroup, PanelHeader } from '@willowinc/ui';
import { Box, Button, Grid, Typography } from '@mui/material';
import IconButton from '@mui/material/IconButton';
import { useCallback, useEffect, useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { array, object, string } from 'yup';
import { loginRequest } from '../config';
import { callPostDeploymentBatch } from '../services/dashboardService';
import DeployerDialogCreateByConnectorType from './DeployerCreateByConnectorType';
import { FormApplicationTypeSearchTextBox } from './form-components/FormApplicationTypeSearchTextBox';
import { FormConnectorSearchTextBox } from './form-components/FormConnectorSearchTextBox';
import { FormInputText } from './form-components/FormInputText';
import { PanelHeaderContentWithActions } from './PanelHeaderContentWithActions';

interface IFormCreateDeployment {
  moduleId: string;
  version: string;
  moduleType: string;
}

interface IFormCreateGroupDeployment {
  createDeploymentCommands: IFormCreateDeployment[];
}

const createDeploymentCommandSchema = object({
  moduleId: string().required("Connector is required"),
  version: string().matches(/^(\d+\.)?(\d+\.)?(\d+)$/, {
    excludeEmptyString: false,
    message: "Version must be in X.Y or X.Y.Z format. Example: 1.0 or 1.0.0"
  }),
  moduleType: string().optional()
}).required();

const schema = object({
  createDeploymentCommands: array().of(createDeploymentCommandSchema)
}).required();

export default function Deployer(props: { setOpenError: (open: boolean) => void; }) {
  const { setOpenError } = props;

  const defaultValues = {
    createDeploymentCommands: [{
      version: '1.0.0',
      moduleType: 'Any'
    }]
  };

  const methods = useForm<IFormCreateGroupDeployment>({ defaultValues: defaultValues, resolver: yupResolver(schema) });
  const { handleSubmit, control } = methods;
  const { fields, remove, insert } = useFieldArray({
    control,
    name: 'createDeploymentCommands',
    keyName: 'id'
  });

  const { instance, inProgress, accounts } = useMsal();
  const [accessToken, setAccessToken] = useState<string>('');
  const [openDialogCreateByConnectorType, setOpenDialogCreateByModuleType] = useState<boolean>(false);
  const navigate = useNavigate();

  const addItem = (currentIndex: number, currentModuleType: string) => {
    insert(++currentIndex, {
      moduleId: '',
      version: '1.0.0',
      moduleType: currentModuleType
    });
  };

  const onSubmitDeployment = async (form: IFormCreateGroupDeployment) => {
    const deployment = {
      createDeploymentCommands: form.createDeploymentCommands
    };

    try {
      await callPostDeploymentBatch(accessToken, deployment);
      navigate("/deployments");
    } catch (error) {
      console.log(error);
      setOpenError(true);     // Show the erorr Notification
    }
  };

  const handleCloseDialogCreateByConnectorType = useCallback(() => {
    setOpenDialogCreateByModuleType(false);
  }, []);

  const handleCreateByModuleType = () => {
    setOpenDialogCreateByModuleType(true);
  };

  const handleSaveCreateByConnectorType = useCallback((change: boolean) => {
    if (change) {
      navigate("/deployments");
    }
  }, [navigate]);

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
    <PanelGroup>
      <Panel>
        <PanelHeader>
          <PanelHeaderContentWithActions>
            <span>Deployer</span>
            <Button variant="contained" size="small" onClick={handleCreateByModuleType}>
              Create Deployment By Connector Type
            </Button>

          </PanelHeaderContentWithActions>
        </PanelHeader>
        <Box sx={{padding: "16px"}}>
          <Grid container>
            {fields.map((item, index) => (
              <Grid item key={item.id}>
                <Box sx={{ width: '100%', display: "flex", justifyContent: "space-between" }}>
                  <FormApplicationTypeSearchTextBox
                    name={`createDeploymentCommands.${index}.moduleType`}
                    control={control}
                    label="Application Type"
                    sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: 300 }}
                    setOpenError={setOpenError}
                  />
                  <FormInputText
                    name={`createDeploymentCommands.${index}.version`}
                    control={control}
                    label="Version"
                    sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: 100 }}
                  />
                  <FormConnectorSearchTextBox
                    name={`createDeploymentCommands.${index}.moduleId`}
                    control={control}
                    label="Connector"
                    index={index}
                    sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: 500 }}
                    setOpenError={setOpenError}
                  />
                  <IconButton onClick={() => addItem(index, item.moduleType)}
                    sx={{ marginTop: 1, marginBottom: 1, marginRight: 1 }}>
                    <Icon icon="add" color="primary" />
                  </IconButton>
                  <IconButton onClick={() => fields.length > 1 && remove(index)}
                    sx={{ marginTop: 1, marginBottom: 1, marginRight: 1 }}>
                    <Icon icon="remove" color="primary" />
                  </IconButton>
                </Box>
              </Grid>
            ))}
          </Grid>
          <Button variant="contained" size="small" onClick={handleSubmit(onSubmitDeployment)}
            sx={{ marginTop: 1, marginBottom: 1, marginRight: 1, width: 917 }}
          >
            SUBMIT
          </Button>
        </Box>
        <DeployerDialogCreateByConnectorType
          open={openDialogCreateByConnectorType}
          closeHandler={handleCloseDialogCreateByConnectorType}
          onConfirm={handleSaveCreateByConnectorType}
          setOpenError={setOpenError}
        />
      </Panel>
    </PanelGroup>
  );
}
