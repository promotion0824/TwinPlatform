import { useState } from 'react';
import useApi from '../hooks/useApi';
import { withErrorBoundary, ErrorBoundary } from 'react-error-boundary';
import { QueryErrorResetBoundary, useMutation, useQuery } from 'react-query';
import { Alert, Box, Button, Grid, Snackbar, Stack } from '@mui/material';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from "react-router-dom";
import { RuleDto, ValidationReponseDto } from '../Rules';
import { ErrorFallback } from '../components/error/errorBoundary';
import TemplatePickerField from '../components/fields/TemplatePickerField';
import RuleDetails from '../components/formparts/RuleDetails';
import FlexTitle from '../components/FlexPageTitle';
import StyledLink from '../components/styled/StyledLink';

const RuleCreatePage = withErrorBoundary(() => {
  const params = useParams<{ id: string }>();
  const apiclient = useApi();
  const navigate = useNavigate();
  const { register, handleSubmit, formState: { errors, isSubmitting }, setError } = useForm();
  const [error, setErrorOnPage] = useState<string>();

  const [ruleDto, setRuleDto] = useState<RuleDto>();
  const [ruleTemplateName, setRuleTemplateName] = useState<string>();

  useQuery(["ruleCreate", params.id], async () => {
    if (params.id) {
      var data = await apiclient.createRule(params.id);

      console.log('ruleCreate with param templateId', params.id, data);

      setRuleDto(data);
      setRuleTemplateName(data.templateName ?? params.id);
    }
  }, {
    useErrorBoundary: true //with the error boundary this is required for catching react-query related errors. To be optimised
  });

  const mutation = useMutation(async (_: RuleDto) => {
    //Set the JSON to empty for security reasons
    ruleDto!.json = '';
    await apiclient.upsertRule(undefined, ruleDto)
      .then(function (res) { navigate(`/rule/` + encodeURIComponent(res.id!)); })
      .catch(function (err) { setErrorOnPage(err.message); });
  })

  const onSubmit = async (data: any, _e: any) => {
    try {
      await mutation.mutateAsync(data);
    }
    catch (e) {
      console.log('onSubmit', e);
      const validationResponse = e as ValidationReponseDto;
      if (validationResponse) {
        validationResponse.results?.forEach((x) => {
          setError(x.field!, { type: 'manual', message: x.message! });
        });
      }
      setErrorOnPage("Request error:" + e);
      return;
    }
  }

  const onError = (data: any, e: any) => {
    console.log('Submit error: ', ruleDto);
    // Set errors?
    console.log('onError', data, e);
  }

  return (
    <Box component="form" autoComplete="off" onSubmit={handleSubmit(onSubmit, onError)}>
      <QueryErrorResetBoundary>
        {({ reset }: { reset: any }) => (
          <ErrorBoundary onReset={reset} fallbackRender={({ resetErrorBoundary }) => (
            <div>
              There was an error!
              <Button onClick={() => resetErrorBoundary()}>Try again</Button>
            </div>
          )}>
            {!ruleDto?.templateId && <TemplatePickerField selectionChanged={
              async (id: any, name: any) => {
                await apiclient.createRule(id).then(function (res) { setRuleDto(res); }); setRuleTemplateName(name);
              }} />}
            {
              ruleDto?.templateId &&
              <Stack spacing={2}>
                <FlexTitle>
                  <StyledLink to={"/rules"}>Skills</StyledLink>
                  <>New {ruleTemplateName}</>
                </FlexTitle>
                <Box flexGrow={1} sx={{ maxWidth: '80ch' }}>
                  <RuleDetails rule={ruleDto} register={register} errors={errors} />
                  <br />
                  <Grid container direction="row" alignItems="flex-end" justifyContent="right" sx={{ flexGrow: 1 }} pt={1}>
                    <Grid item>
                      <Button variant="contained" type="submit" disabled={isSubmitting} color="primary" sx={{ minWidth: '80px' }}>Submit</Button>
                    </Grid>
                  </Grid>
                  {<Snackbar open={error != null} autoHideDuration={6000}>
                    <Alert severity="error">
                      {error}
                    </Alert>
                  </Snackbar>}
                </Box>
              </Stack>
            }
          </ErrorBoundary>
        )}
      </QueryErrorResetBoundary>
    </Box>
  );
},
  {
    FallbackComponent: ErrorFallback, //using general error view
    onError(error, info) {
      console.log('from error boundary in Rulesingle: ', error, info)
    },
  })
export default RuleCreatePage;
