import { Alert, Box, Button, Stack, Tab, Tabs, Typography } from '@mui/material';
import * as React from 'react';
import { ErrorBoundary, withErrorBoundary } from 'react-error-boundary';
import { useForm } from 'react-hook-form';
import { QueryErrorResetBoundary, useQuery } from 'react-query';
import { useParams } from 'react-router-dom';
import { ErrorFallback } from '../components/error/errorBoundary';
import FlexTitle from '../components/FlexPageTitle';
import RuleReferencesGrid from '../components/grids/RuleReferencesGrid';
import MLModelForm from '../components/MLModelForm';
import StyledLink from '../components/styled/StyledLink';
import TabPanel from '../components/tabs/TabPanel';
import useApi from '../hooks/useApi';
import { MLModelDto } from '../Rules';

const MLModelSingle = withErrorBoundary(() => {
  
  const params = useParams<{ id: string }>();
  const sharedForm = useForm();
  //the data here will be an object since an object was
  const apiclient = useApi();

  const data = useQuery(["mlmodel", params.id], async (_x: any) => {
    var data = await apiclient.getMLModel(params.id);
    return data;
  }, {
    useErrorBoundary: true //with the error boundary this is required for catching react-query related errors. To be optimised
  }).data;

  const model = data as MLModelDto;

  const [value, setValue] = React.useState(0);
  const handleChange = (_event: React.ChangeEvent<{}>, newValue: number) => {
    setValue(newValue);
  };

  if (model) {
    const ruleReferencesQuery = {
      invokeQuery: () => {
        return apiclient.getMLModelReferences(model.id);
      },
      key: model.id!
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
              {((model.error?.length ?? 0) > 0) && <Alert severity="warning">
                <Typography variant="caption">The model contains errors</Typography> <Typography variant="subtitle1">{model.error}</Typography></Alert>}

              <FlexTitle>
                <StyledLink to={"/mlmodels"}>ML Models</StyledLink>
                {model.fullName}
              </FlexTitle>
              <Box flexGrow={1} mt={0}>
                <Tabs value={value} onChange={handleChange} selectionFollowsFocus indicatorColor="primary">
                  <Tab label="Details" />
                  <Tab label="References" />
                </Tabs>

                { /* EDIT GLOBAL */}

                <TabPanel value={value} index={0}>
                  <MLModelForm model={model} formContext={sharedForm} />
                </TabPanel>

                <TabPanel value={value} index={1}>
                  <RuleReferencesGrid query={ruleReferencesQuery} />
                </TabPanel>
              </Box>
            </Stack>
          </ErrorBoundary>
        )}

      </QueryErrorResetBoundary>
    )
  } else {
    return <div>Loading...</div>
  }
}, {
  FallbackComponent: ErrorFallback, //using general error view
  onError(error, info) {
    console.log('from error boundary in Global Variable single: ', error, info)
  },
})

export default MLModelSingle;
