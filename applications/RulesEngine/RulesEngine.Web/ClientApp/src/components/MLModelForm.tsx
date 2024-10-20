import { ExpandMore } from '@mui/icons-material';
import Delete from '@mui/icons-material/Delete';
import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, Divider, Grid, Stack } from '@mui/material';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import useApi from '../hooks/useApi';
import { MLModelDto, RuleDto, RuleParameterDto } from '../Rules';
import ExpressionParameters from './formparts/ExpressionParameters';
import MLModelDetails from './formparts/MLModelDetails';
import RuleSimulation from './RuleSimulation';

const MLModelForm = (params: { model: MLModelDto, formContext: any }) => {

  const [model, _] = useState(params.model);
  const [showSimulation, setShowSimulation] = useState<boolean>(false);
  const [rule, setRule] = useState(new RuleDto());
  const apiclient = useApi();
  const isNew = model.id === undefined;
  const { register, formState: { errors } } = params.formContext;

  const navigate = useNavigate();

  const [isDeleting, setDeleting] = useState(false);
  const [deleteDialog, setDeleteDialog] = useState(false);
  const handleCloseDeleteDialog = () => {
    navigate('../mlmodels');
  };
  const onDelete = async () => {
    setDeleting(true);
    await apiclient.deleteMLModel(model.id!).then(() => setDeleteDialog(true));
  };

  const updateSimulationParameters = (parameters: RuleParameterDto[]) => {
    const newRule: RuleDto = new RuleDto();
    newRule.init({ ...rule, id: 'model_simulation', parameters: parameters, templateId: "calculated-point" });
    setRule(newRule);
  };

  const initSimulationRule = () => {
    setShowSimulation(!showSimulation);

    let expression = `${model.fullName}(${model.inputParams!.map(v => `${v.name}`).join(", ")})`;

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
    <Box component="form" autoComplete="off">
      {!isNew &&
        <Grid container spacing={2} >
          <Grid item>
            <Button variant="contained" onClick={onDelete} disabled={isDeleting} color="error">
              <Delete sx={{ mr: 1, fontSize: "medium" }} />
              Delete Model
            </Button>
          </Grid>
        </Grid>
      }

      <MLModelDetails model={model} />

      <Grid container spacing={1} alignContent="center" sx={{ mt: 1 }}>
        <Grid item>
          <Button onClick={() => initSimulationRule()}
            variant="outlined" color="secondary">
            Simulation Test <ExpandMore sx={{ fontSize: 20 }} />
          </Button>
        </Grid>
        <Grid item>
          {(isDeleting) && <CircularProgress size={20} className="float-right" />}
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
            <RuleSimulation ruleId={""} equipmentId={""} showEquipmentInput={true} rule={rule} showOutputBindings={true} showModelInput={true} showInsights={false} />
          </Stack>
        }
      </Grid>

      {/*Dialog to inform user on deletion process*/}
      <Dialog open={deleteDialog} onClose={handleCloseDeleteDialog}>
        <DialogContent>
          <DialogContentText>
            Model deleted successfully.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDeleteDialog} variant="contained" color="primary">
            Back to models
          </Button>
        </DialogActions>
      </Dialog>
    </Box >

  );
}

export default MLModelForm;
