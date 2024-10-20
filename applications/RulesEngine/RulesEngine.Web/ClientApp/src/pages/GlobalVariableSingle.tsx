import { Alert, AlertColor, Box, Button, Snackbar, Stack, Tab, Tabs, Typography } from '@mui/material';
import * as React from 'react';
import { Suspense, useEffect, useState } from 'react';
import { ErrorBoundary, withErrorBoundary } from 'react-error-boundary';
import { useForm } from 'react-hook-form';
import { QueryErrorResetBoundary, useQuery, useQueryClient } from 'react-query';
import { useLocation, useParams } from 'react-router-dom';
import { ErrorFallback } from '../components/error/errorBoundary';
import FlexTitle from '../components/FlexPageTitle';
import { GlobalErrorFallback } from '../components/error/errorBoundary';
import GlobalVariableForm, { GetSaveGlobalMutation } from '../components/GlobalVariableForm';
import { GetGlobalVariableTypeText } from '../components/GlobalVariableTypeFormatter';
import RuleReferencesGrid from '../components/grids/RuleReferencesGrid';
import JsonEditor from '../components/JsonEditor';
import StyledLink from '../components/styled/StyledLink';
import TabPanel from '../components/tabs/TabPanel';
import useApi from '../hooks/useApi';
import { GlobalVariableDto, ValidationReponseDto } from '../Rules';

const GlobalVariableSingle = withErrorBoundary(() => {
  
  const params = useParams<{ id: string }>();
  const location = useLocation();
  //the data here will be an object since an object was
  const queryClient = useQueryClient();
  const apiclient = useApi();
  const sharedForm = useForm();
  const { clearErrors } = sharedForm;
  const isNew = location.state !== null;

  //Need to keep the rule json seperate since we set the JSON to empty on requests for security reasons
  const [macroJSON, setMacroJSON] = useState<string>();

  const data = location.state ?? useQuery(["globalvarable", params.id], async (_x: any) => {
    var data = await apiclient.getGlobalVariable(params.id);
    setMacroJSON(data?.json!);
    return data;
  }, {
    useErrorBoundary: true //with the error boundary this is required for catching react-query related errors. To be optimised
  }).data;

  const global = data as GlobalVariableDto;

  const [saveSuccess, setSaveSuccess] = useState(false);
  const [saveCompleted, setSaveCompleted] = useState(false);

  const handleCloseAlert = () => {
    setSaveCompleted(false);
  };

  const [severity, setSeverity] = useState<AlertColor>("error");

  const saveProps = {
    global: global,
    formContext: sharedForm,
    onActioned: () => {
      setSaveCompleted(true);
    },
    onSuccess: () => {
      setSaveSuccess(true);
      setSeverity("success");
    },
    onError: () => {
      setSaveSuccess(false);
      setSeverity("error");
    }
  };

  const mutation = GetSaveGlobalMutation(saveProps);

  //Validation - For now focused on Parameters
  const validateGlobalVariable = async (global: GlobalVariableDto) => {

    function firstLower(val: string) {
      return val.replace(/(?:^|\s)\S/g, function (a) {
        return a.toLowerCase();
      });
    };

    try {
      //Set the JSON to empty for security reasons
      global!.json = '';
      clearErrors();
      await apiclient.validateGlobalVariable(global);
    }
    catch (err) {
      const validationResponse = err as ValidationReponseDto;
      if (validationResponse && validationResponse.results) {
        validationResponse.results?.forEach((x) => {
          sharedForm.setError(firstLower(x.field!), { type: 'manual', message: x.message! });
        });
      }
    }
  }

  //Validate once at this level to indicate validation issues
  useEffect(() => {
    if (global) {
      clearErrors();
      validateGlobalVariable(global);
    }
  }, [global?.id]);

  const [value, setValue] = React.useState(0);
  const handleChange = (_event: React.ChangeEvent<{}>, newValue: number) => {
    setValue(newValue);
    // In case the name has changed, invalidate the all globals page grid
    queryClient.invalidateQueries(["globalswithfilter"]);
  };

  if (global) {
    const jsonEditorProps = {
      input: macroJSON,
      saveJsonObject: (jsonObject: any) => {
        return mutation.mutateAsync(jsonObject);
      }
    };

    const ruleReferencesQuery = {
      invokeQuery: () => {
        return apiclient.getGlobalReferences(global.id);
      },
      key: global.id!
    };


    return (
      <QueryErrorResetBoundary>
        {({ reset }) => (
          <ErrorBoundary
            onReset={reset}
            fallbackRender={({ resetErrorBoundary }) => (
              <div>
                There was an error! <Button onClick={() => resetErrorBoundary()}>Try again</Button>
              </div>
            )}>
            <Stack spacing={2}>
              <FlexTitle>
                <StyledLink to={"/globals"}>Globals</StyledLink>
                {!isNew ? `${global.name}` : `New ${GetGlobalVariableTypeText(global)}`}
              </FlexTitle>
              <Box flexGrow={1} mt={0}>
                <Tabs value={value} onChange={handleChange} selectionFollowsFocus indicatorColor="primary">
                  <Tab label={GetGlobalVariableTypeText(global)} />
                  <Tab label="References" />
                  <Tab label="Export" />
                </Tabs>

                { /* EDIT GLOBAL */}

                <TabPanel value={value} index={0}>
                  <GlobalVariableForm global={global} validate={validateGlobalVariable} formContext={sharedForm} />
                </TabPanel>

                <TabPanel value={value} index={1}>
                  <RuleReferencesGrid query={ruleReferencesQuery} />
                </TabPanel>

                <TabPanel value={value} index={2}>
                  <JsonEditor props={jsonEditorProps} />
                </TabPanel>
              </Box>
            </Stack>

            <Suspense fallback={<div>Loading...</div>}>
              <Snackbar open={saveCompleted} onClose={handleCloseAlert} autoHideDuration={10000} >
                <Alert onClose={handleCloseAlert} variant="filled" severity={severity}>
                  {saveSuccess && <Typography variant="body1">Save successful.</Typography>}
                  {!saveSuccess && <Typography variant="body1">Save failed</Typography>}
                </Alert>
              </Snackbar>
            </Suspense>
          </ErrorBoundary>
        )}

      </QueryErrorResetBoundary>
    )
  } else {
    return <div>Loading...</div>
  }
}, {
  FallbackComponent: GlobalErrorFallback, //using general error view
  onError(error, info) {
    console.log('from error boundary in Global Variable single: ', error, info)
  },
})

export default GlobalVariableSingle;
