import { ExpandMore } from '@mui/icons-material';
import Delete from '@mui/icons-material/Delete';
import { Alert, AlertColor, Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, Divider, FormHelperText, Grid, Snackbar, Stack } from '@mui/material';
import { Suspense, useState } from 'react';
import { useMutation, UseMutationResult, useQuery, useQueryClient } from 'react-query';
import { useNavigate } from 'react-router-dom';
import useApi from '../hooks/useApi';
import { FunctionParameterDto, GlobalVariableDto, RuleDto, RuleParameterDto, ValidationReponseDto } from '../Rules';
import { VisibleIf } from './auth/Can';
import ExpressionParameters from './formparts/ExpressionParameters';
import FunctionParameters from './formparts/FunctionParameters';
import GlobalVariableDetails from './formparts/GlobalVariableDetails';
import { GetGlobalVariableTypeText } from './GlobalVariableTypeFormatter';
import RuleSimulation from './RuleSimulation';

interface SaveGlobalProps {
  global: GlobalVariableDto;
  formContext: any;
  onActioned: () => void;
  onSuccess: () => void;
  onError: (err: string) => void;
}

export function GetSaveGlobalMutation(props: SaveGlobalProps): UseMutationResult<void, unknown, any, any> {
  const { global, onActioned, onError, onSuccess, formContext } = props;
  const { setError } = formContext;
  const apiclient = useApi();
  const queryClient = useQueryClient();

  function firstLower(val: string) {
    return val.replace(/(?:^|\s)\S/g, function (a) {
      return a.toLowerCase();
    });
  };

  return useMutation(async (data: GlobalVariableDto) => {
    try {
      //Set the JSON to empty for security reasons
      data!.json = '';
      await apiclient.upsertGlobalVariable(global.id, data);

      queryClient.invalidateQueries(['globalvarable', global.id], {
        exact: true
      });

      //refresh intellisense for editor
      queryClient.invalidateQueries(['lookupdata'], {
        exact: true
      });

      queryClient.invalidateQueries(['globalreferences', global.id], {
        exact: true
      });

      onSuccess();
    }
    catch (err) {
      onError(err);

      const validationResponse = err as ValidationReponseDto;
      if (validationResponse) {
        validationResponse.results?.forEach((x, _i) => {
          setError(firstLower(x.field!), { type: 'manual', message: x.message! });
        });
      }
    }

    onActioned();
  });
}

const GlobalVariableForm = (params: { global: GlobalVariableDto, validate: (global: GlobalVariableDto) => void, formContext: any }) => {

  const [global, setGlobal] = useState(params.global);
  const [showSimulation, setShowSimulation] = useState<boolean>(false);
  const [rule, setRule] = useState(new RuleDto());
  const [allParams, setAllParams] = useState([...global.parameters!.map((item: FunctionParameterDto ): RuleParameterDto => {
    return {
      fieldId: item.name,
      init: item.init,
      toJSON: item.toJSON
    };
  }
  ), ...global.expression!]);
  const apiclient = useApi();
  const isNew = global.id === undefined;
  const { register, handleSubmit, clearErrors, setValue, formState: { errors, isSubmitting } } = params.formContext;

  const saveProps = {
    global: global,
    formContext: params.formContext,
    onActioned: () => {
      setSubmitActioned(true);
    },
    onSuccess: () => {
      setSubmitSuccess(true);
      setSeverity("success");
    },
    onError: (_: any) => onError()
  };

  const mutation = GetSaveGlobalMutation(saveProps);

  const onSubmit = async () => {
    await mutation.mutateAsync(global);
  }

  useQuery(["validateGlobalParams", global], async (_x: any) => {
    clearErrors();
    params.validate(global);
  });

  const onError = () => {
    setSubmitSuccess(false);
    setSeverity("error");
    setSubmitActioned(true);
  }

  const navigate = useNavigate();

  const [isDeleting, setDeleting] = useState(false);
  const [deleteDialog, setDeleteDialog] = useState(false);
  const handleCloseDeleteDialog = () => {
    navigate('../globals');
  };
  const onDelete = async (_: any) => {
    setDeleting(true);
    await apiclient.deleteGlobalVariable(global.id!).then(() => setDeleteDialog(true));
  };

  const [submitActioned, setSubmitActioned] = useState(false);
  const [submitSuccess, setSubmitSuccess] = useState(false);
  const [severity, setSeverity] = useState<AlertColor>("error");
  const handleCloseAlert = () => { setSubmitActioned(false); }

  const updateGlobal = (input: any) => {
    const newGlobal: GlobalVariableDto = new GlobalVariableDto();
    newGlobal.init({ ...global, ...input });
    setGlobal(newGlobal);

  };

  const updateSimulationParameters = (parameters: RuleParameterDto[]) => {
    const newRule: RuleDto = new RuleDto();
    newRule.init({ ...rule, id: 'global_simulation', parameters: parameters, templateId: "calculated-point" });
    setRule(newRule);
  };

  const updateAllParams = (parameters: RuleParameterDto[]) => {
    setAllParams(parameters);
  }

  const initSimulationRule = () => {
    setShowSimulation(!showSimulation);

    let expression = `${global.name}(${global.parameters!.map(_ => 'this').join(", ")})`;

    const ruleParameter = new RuleParameterDto(
      {
        pointExpression: expression,
        fieldId: "result",
        name: "result"
      });

    updateSimulationParameters([ruleParameter]);
  }

  const getFormErrors = () => errors;

  const getFormRegister = () => register;

  return (

    /* "handleSubmit" will validate your inputs before invoking "onSubmit" */
    <Box component="form" autoComplete="off" onSubmit={handleSubmit(onSubmit, onError)}>
      {!isNew &&
        <VisibleIf canEditRules policies={global.policies}>
          <Grid container spacing={2} >
            <Grid item>
              <Button variant="contained" onClick={onDelete} disabled={isDeleting} color="error">
                <Delete sx={{ mr: 1, fontSize: "medium" }} />
                Delete {GetGlobalVariableTypeText(global)}
              </Button>
            </Grid>
          </Grid>
        </VisibleIf>
      }

      <GlobalVariableDetails global={global} register={register} setValue={setValue} errors={errors} validate={() => params.validate(global)} />

      <FunctionParameters
        parameters={global.parameters!}
        allParams={allParams}
        label={"Input Parameters"}
        isOpen={true}
        updateParameters={(p) => updateGlobal({ parameters: p })}
        updateAllParams={updateAllParams}
        getFormErrors={getFormErrors}
        getFormRegister={getFormRegister}
      />

      <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()["parameters"]?.message}</></FormHelperText>

      <ExpressionParameters
        parameters={global.expression!}
        allParams={allParams}
        label={"Expressions"}
        showUnits={true}
        showField={false}
        showSettings={false}
        isOpen={true}
        updateParameters={(p) => updateGlobal({ expression: p })}
        updateAllParams={updateAllParams}
        getFormErrors={getFormErrors}
        getFormRegister={getFormRegister}
      />

      <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()["expression"]?.message}</></FormHelperText>

      <Grid container spacing={1} alignContent="center" sx={{ mt: 1 }}>
        <Grid item>
          <Button onClick={() => initSimulationRule()}
            variant="outlined" color="secondary">
            Simulation Test <ExpandMore sx={{ fontSize: 20 }} />
          </Button>
        </Grid>
        <VisibleIf canEditRules policies={global.policies}>
          <Grid item>
            <Button variant="contained" type="submit" disabled={isSubmitting || isDeleting} color="primary" className="float-right">Submit</Button>
          </Grid>
        </VisibleIf>
        <Grid item>
          {(mutation.isLoading || isDeleting) && <CircularProgress size={20} className="float-right" />}
        </Grid>
      </Grid>

      <Grid item xs={12} sx={{ mt: 2 }}>
        {showSimulation &&
          <Stack spacing={2}>
            <Divider textAlign="left">Execution Simulation</Divider>
            <ExpressionParameters
              parameters={rule?.parameters!}
              allParams={rule?.parameters!}
              label={"Simulation expressions"}
              showUnits={true}
              showField={true}
              showSettings={true}
              updateParameters={updateSimulationParameters}
              updateAllParams={updateSimulationParameters}
              getFormErrors={getFormErrors}
              getFormRegister={getFormRegister}
            />
            <RuleSimulation
              ruleId={""}
              equipmentId={""}
              showEquipmentInput={true}
              rule={rule}
              showOutputBindings={true}
              globalVariable={global}
              showModelInput={true}
              showInsights={false}
              canAddSimulations={true}
              canAddRelatedRulesSimulations={false}
            />
          </Stack>
        }
      </Grid>

      <Suspense fallback={<div>Loading...</div>}>
        <Snackbar open={submitActioned} onClose={(isNew && submitSuccess) ? handleCloseDeleteDialog : handleCloseAlert} autoHideDuration={1000} >
          <Alert onClose={(isNew && submitSuccess) ? handleCloseDeleteDialog : handleCloseAlert} sx={{ width: '100%' }} variant="filled" severity={severity}>
            {submitSuccess && <>Submit successful.</>}
            {!submitSuccess && <>Submit failed</>}
          </Alert>
        </Snackbar>
      </Suspense>

      {/*Dialog to inform user on deletion process*/}
      <Dialog open={deleteDialog} onClose={handleCloseDeleteDialog}>
        <DialogContent>
          <DialogContentText>
            Global deleted successfully.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDeleteDialog} variant="contained" color="primary">
            Back to globals
          </Button>
        </DialogActions>
      </Dialog>
    </Box >

  );
}

export default GlobalVariableForm;
