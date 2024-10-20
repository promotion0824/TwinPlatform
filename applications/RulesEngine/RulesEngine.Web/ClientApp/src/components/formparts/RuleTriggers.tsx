import { Add, ArrowForwardIosSharp, Delete } from '@mui/icons-material';
import { Accordion, AccordionDetails, Box, Button, Card, CardContent, FormControl, FormHelperText, Grid, InputLabel, MenuItem, Select, Slide, Stack, styled, TextField, useTheme } from '@mui/material';
import MuiAccordionSummary, { AccordionSummaryProps } from '@mui/material/AccordionSummary';
import Typography from '@mui/material/Typography';
import * as React from 'react';
import { useEffect, useState } from 'react';
import { FieldErrors, FieldValues, UseFormRegister } from 'react-hook-form';
import { CommandType, RuleParameterDto, RuleTriggerDto, RuleTriggerType } from '../../Rules';
import { GetCommandTypeFilter } from '../commands/CommandTypeFormatter';
import { GetTriggerTypeText } from '../commands/TriggerTypeFormatter';
import ExpressionParameters from './ExpressionParameters';

const AccordionSummary = styled((props: AccordionSummaryProps) => (
  <MuiAccordionSummary
    expandIcon={<ArrowForwardIosSharp sx={{ color: "white", fontSize: "0.9rem" }} />}
    {...props}
  />
))(({ theme }) => ({
  flexDirection: 'row-reverse',
  '& .MuiAccordionSummary-expandIconWrapper.Mui-expanded': {
    transform: 'rotate(90deg)',
  },
  '& .MuiAccordionSummary-content': {
    marginLeft: theme.spacing(1),
  },
}));

interface EditTriggersProps {
  triggers: RuleTriggerDto[],
  label: string,
  triggerType: number,
  isOpen?: boolean,
  allowMultiple?: boolean,
  updateTriggers: (triggers: RuleTriggerDto[]) => void,
  getFormErrors: () => FieldErrors,
  getFormRegister: () => UseFormRegister<FieldValues>
}

const RuleTrigger = (params: { trigger: RuleTriggerDto, getFormErrors: () => FieldErrors, getFormRegister: () => UseFormRegister<FieldValues>, expand: boolean, onChange: (value: RuleTriggerDto) => void, onDelete: (value: RuleTriggerDto) => void }) => {

  const trigger = params.trigger;
  const getFormErrors = params.getFormErrors;
  const getFormRegister = params.getFormRegister;
  const expand = params.expand;
  const onChange = params.onChange;
  const onDelete = params.onDelete;
  const isCommandTrigger = trigger.triggerType == RuleTriggerType._1;
  const [expanded, setExpanded] = useState(expand);
  const [name, setName] = useState(trigger.name);

  const parameters: RuleParameterDto[] = [];
  parameters.push(trigger.condition!);
  if (isCommandTrigger) {
    parameters.push(trigger.point!);
    parameters.push(trigger.value!);
  }

  const updateParameters = (item: RuleTriggerDto, parameters: RuleParameterDto[]) => {
    trigger.condition = parameters.find(v => v.fieldId == item.condition?.fieldId);
    trigger.point = parameters.find(v => v.fieldId == item.point?.fieldId);
    trigger.value = parameters.find(v => v.fieldId == item.value?.fieldId);
    onChange(trigger);
  };

  return (<Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }} expanded={expanded} onChange={() => setExpanded(!expanded)}>
    <AccordionSummary>
      <Typography variant="h4">{trigger.name}</Typography>
    </AccordionSummary>
    <AccordionDetails>
      <Box flexGrow={1} mb={1}>
        <Grid container alignItems="top" spacing={2} mb={3} >
          <Grid item xs={5}>
            <TextField
              id="new-expression-name"
              key="newTrigger"
              label="Name"
              value={name}
              onBlur={(e) => {
                if (name != trigger.name) {
                  trigger.name = name;
                  onChange(trigger);
                }
              }}
              onChange={(e) => {
                setName(e.target.value)
              }}
              size="small"
              fullWidth />
          </Grid>
          {isCommandTrigger && <Grid item xs={5}>
            <FormControl fullWidth>
              <InputLabel id="demo-simple-select-label">Command Type</InputLabel>
              <Select
                labelId="demo-simple-select-label"
                id="demo-simple-select"
                value={trigger.commandType}
                label="Command Type"
                onChange={(e) => {
                  trigger.commandType = e.target.value as CommandType;
                  onChange(trigger);
                }}
              >
                {GetCommandTypeFilter().map((v) => (<MenuItem key={v.label} value={v.value}>{v.label}</MenuItem>))}
              </Select>
            </FormControl>
          </Grid>}
          <Grid item xs={12}>
            <ExpressionParameters
              parameters={parameters}
              allParams={[]}
              validationFieldPrefix={trigger.name}
              label={""}
              showUnits={true}
              showField={false}
              showSettings={false}
              isOpen={true}
              canAdd={false}
              canDelete={false}
              canRename={false}
              canChangeOrder={false}
              updateParameters={(p) => updateParameters(trigger, p)}
              updateAllParams={()=>{}}
              getFormErrors={getFormErrors}
              getFormRegister={getFormRegister}
            />
          </Grid>
          <Grid item xs={12}>
            <Button variant="contained" onClick={() => onDelete(trigger)} color="error">
              Delete
            </Button>
          </Grid>
        </Grid>
        <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()[trigger.name!]?.message}</></FormHelperText>
      </Box>
    </AccordionDetails>
  </Accordion>);
};


const RuleTriggers = (params: EditTriggersProps) => {
  const [triggerChecked, setTriggerChecked] = useState(false);
  const [trigger, setTrigger] = useState<any>({ name: "" });
  const [expanded, setExpanded] = useState(params.isOpen ?? true);
  const label = params.label;
  const allowMultiple = params.allowMultiple ?? true;
  const triggerType = params.triggerType;
  const triggerTypeText = GetTriggerTypeText(triggerType);
  const [triggers, setTriggers] = useState(params.triggers);
  const isCommandTrigger = triggerType == RuleTriggerType._1;

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setTrigger({ ...trigger, name: event.target.value });
  };

  const handleIdChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setTrigger({ ...trigger, id: event.target.value });
  };

  const addTrigger = () => {
    setTriggerChecked((prev) => !prev);
    const newParam: RuleTriggerDto = new RuleTriggerDto();

    let conditionParam = new RuleParameterDto();
    conditionParam.init({ name: "Condition", fieldId: "condition", pointExpression: "IS_FAULTY", units: "bool" });
    const id = `${triggerTypeText?.toLocaleLowerCase()}_${(triggers.length + 1)}`;
    const name = `${triggerTypeText} ${(triggers.length + 1)}`;

    if (isCommandTrigger) {
      let pointParam = new RuleParameterDto();
      pointParam.init({ name: "Point", fieldId: "point", pointExpression: "", units: "" });
      let valueParam = new RuleParameterDto();
      valueParam.init({ name: "Value", fieldId: "value", pointExpression: "", units: "" });

      newParam.init({
        id: id,
        name: name,
        triggerType: triggerType,
        condition: conditionParam,
        commandType: CommandType._1,
        point: pointParam,
        value: valueParam
      });
    }
    else {
      newParam.init({
        id: id,
        name: name,
        triggerType: triggerType,
        condition: conditionParam
      });
    }

    setTrigger(newParam);
  };

  const saveTrigger = () => {
    if (!(trigger.name?.length > 0)) {
      return;
    }

    setTriggerChecked((prev) => !prev);
    const newTriggers = [...triggers!];
    newTriggers.push(trigger);
    setTriggers(newTriggers);
    params.updateTriggers(newTriggers);
  }

  const deleteTrigger = (eId: any) => {
    let newTriggers = Array.from(triggers!)
    newTriggers.splice(eId, 1);
    setTriggers(newTriggers);
    params.updateTriggers(newTriggers);
  }

  const cancelTrigger = () => {
    setTriggerChecked(false);
  }

  // Whenever the rule is invalidated, we need to refresh our paramter list.
  useEffect(() => {
    setTriggers(params.triggers);
  }, [params.triggers]);


  const updateParameters = (item: RuleTriggerDto) => {
    setTriggers(oldState => {
      let newState = [...oldState];
      const index = newState.findIndex(v => v.name == item!.name);
      newState[index] = item!;
      return newState;
    });

    console.log('updateParameters on RuleTriggers');
    params.updateTriggers(triggers);
  };

  return (
    <>
      <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }} expanded={expanded} onChange={() => setExpanded(!expanded)}>
        <AccordionSummary>
          <Typography variant="h4">{label}</Typography>
        </AccordionSummary>
        <AccordionDetails>
          <Stack direction="column" spacing={2}>
            {
              triggers &&
              triggers.map((t, index) => {
                return (
                  <RuleTrigger
                    key={index}
                    trigger={t}
                    getFormErrors={params.getFormErrors}
                    getFormRegister={params.getFormRegister}
                    expand={t.name == trigger.name || triggers.length <= 1}
                    onChange={(e) => {
                      updateParameters(e);
                    }}
                    onDelete={(e) => {
                      deleteTrigger(index);
                    }} />
                );
              })
            }
          </Stack>

          {
            triggerChecked &&
            <Slide direction="left" in={triggerChecked} mountOnEnter unmountOnExit>
              <Grid container mb={3}>
                <Grid item xs={10}>
                  <Grid container alignItems="top" spacing={2} >
                    <Grid item xs={8}>
                      <TextField
                        id="new-expression-name"
                        key="newTrigger"
                        label="Name"
                        value={trigger.name}
                        onChange={handleChange}
                        size="small"
                        fullWidth />
                    </Grid>
                  </Grid>
                  <Grid container spacing={1} sx={{ mt: 1 }}>
                    <Grid item>
                      <Button variant="outlined" color="secondary" onClick={cancelTrigger}>Cancel</Button>
                    </Grid>
                    <Grid item>
                      <Button variant="contained" color="primary" onClick={saveTrigger}>Add</Button>
                    </Grid>
                  </Grid>
                </Grid>
              </Grid>
            </Slide>
          }
          {(allowMultiple ? true : (triggers.length == 0)) && <Button variant="outlined" color="secondary" sx={{ mb: 2 }} onClick={addTrigger}>
            Add Command <Add sx={{ fontSize: 20 }} />
          </Button>}
        </AccordionDetails>
      </Accordion>
    </>
  );
};

export default RuleTriggers
