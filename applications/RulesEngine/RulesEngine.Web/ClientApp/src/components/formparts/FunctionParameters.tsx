import { Add, ArrowForwardIosSharp, Delete, KeyboardArrowDown, KeyboardArrowUp } from '@mui/icons-material';
import { Accordion, AccordionDetails, Box, Button, FormControl, FormHelperText, Grid, Slide, Stack, styled, TextField, useTheme } from '@mui/material';
import MuiAccordionSummary, { AccordionSummaryProps } from '@mui/material/AccordionSummary';
import Typography from '@mui/material/Typography';
import { useEffect, useState } from 'react';
import { FieldErrors, FieldValues, UseFormRegister } from 'react-hook-form';
import { FunctionParameterDto, RuleParameterDto } from '../../Rules';
import UnitsLookupComboBox from '../UnitsLookupComboBox';

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

interface EditParametersProps {
  parameters: FunctionParameterDto[],
  allParams: RuleParameterDto[],
  label: string,
  isOpen?: boolean,
  updateParameters: (parameters: FunctionParameterDto[]) => void,
  updateAllParams: (parameters: RuleParameterDto[]) => void,
  getFormErrors: () => FieldErrors,
  getFormRegister: () => UseFormRegister<FieldValues>
}

export const FunctionParameters = (params: EditParametersProps) => {
  const [parameterChecked, setParameterChecked] = useState(false);
  const [expression, setExpression] = useState<any>({ name: "", description: "", units: "" });
  const [expanded, setExpanded] = useState(params.isOpen ?? true);
  const label = params.label;
  const [parameters, setParamaters] = useState(params.parameters);
  const [allParams, setAllParams] = useState(params.allParams);
  const errors = params.getFormErrors;

  const updateParam = (existingParameter: FunctionParameterDto, name: string | undefined, description: string | undefined, units: string | undefined) => {
    if (name !== existingParameter.name) {
      setAllParams(oldState => {
        let newState = [...oldState];
        const index = newState.findIndex(v => v.fieldId === existingParameter.name);
        newState[index].fieldId = name;
        return newState;
      });
    }
    existingParameter.name = name ?? "";
    existingParameter.description = description ?? "";
    existingParameter.units = units ?? "";
    setParamaters(oldState => {
      let newState = [...oldState];
      const index = newState.findIndex(v => v.name == name);
      newState[index] = existingParameter;
      return newState;
    });
  };

  function handleParameterOrderChanged(position: any, direction: string) {
    const items = Array.from(parameters!);
    const [reorderedItem] = items.splice(position, 1);
    if (direction === 'up')
      items.splice(position > 0 ? position - 1 : position, 0, reorderedItem)
    else if (direction === 'down')
      items.splice(position + 1, 0, reorderedItem);
    setParamaters(items);
    params.updateParameters(items);
  }

  const addParameter = () => {
    setParameterChecked((prev) => !prev);
    const newParam: FunctionParameterDto = new FunctionParameterDto();
    newParam.init({ name: "", description: "" });
    setExpression(newParam);
  };

  const saveParameter = () => {
    if (!(expression.name?.length > 0)) {
      return;
    }

    setParameterChecked((prev) => !prev);
    const newParameters = [...parameters!, expression];
    setParamaters(newParameters);
    params.updateParameters(newParameters);

    const newAllParam: RuleParameterDto = new RuleParameterDto();
    newAllParam.init({fieldId: expression.name});
    const newAllParams = [...allParams!, newAllParam];
    setAllParams(newAllParams);
    params.updateAllParams(newAllParams);
  }

  const deleteParameter = (eId: any) => {
    let newParameters = Array.from(parameters!);
    let newAllParams = Array.from(allParams!);
    newParameters.splice(eId, 1);

    const newIndex = allParams.findIndex(param => param.fieldId === parameters[eId].name);
    newAllParams.splice(newIndex, 1);

    setParamaters(newParameters);
    setAllParams(newAllParams);
    params.updateParameters(newParameters);
    params.updateAllParams(newAllParams);
  }

  const cancelParameter = () => {
    setParameterChecked(false);
  }

  // Whenever the rule is invalidated, we need to refresh our paramter list.
  useEffect(() => {
    setParamaters(params.parameters);
    setAllParams(params.allParams);
  }, [params.parameters, params.allParams]);

  return (
    <>
      <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }} expanded={expanded} onChange={() => setExpanded(!expanded)}>
        <AccordionSummary>
          <Typography variant="h4">{label}</Typography>
        </AccordionSummary>
        <AccordionDetails>

          <Stack direction="column" spacing={2}>
            {
              parameters &&
              parameters.map((p, index) => {
                return (
                  <Box flexGrow={1} key={index}>
                    <Grid container alignItems="top" spacing={2} >
                      <Grid item xs={3}>
                        <FormControl
                          sx={{ '& .MuiFormHelperText-root': { color: 'red' }, width: '100%' }}>
                          <TextField
                            value={p.name}
                            label="Name"
                            size="small"
                            onChange={(e) => {
                              updateParam(p, e.target.value, p.description, p.units);
                            }}
                            onBlur={() => {
                              params.updateParameters(parameters);
                            }}
                            fullWidth />
                        </FormControl>
                      </Grid>
                      <Grid item xs={4}>
                        <FormControl
                          sx={{ '& .MuiFormHelperText-root': { color: 'red' }, width: '100%' }}>
                          <TextField
                            value={p.description}
                            label="Description"
                            size="small"
                            onChange={(e) => {
                              updateParam(p, p.name!, e.target.value, p.units)
                            }}
                            onBlur={() => {
                              params.updateParameters(parameters);
                            }}
                            fullWidth />
                        </FormControl>
                      </Grid>
                      <Grid item xs={2}>
                        <FormControl key={`${p.name!}_unit`} sx={{ width: '100%' }}>
                          <UnitsLookupComboBox
                            id={`${p.name}_units`}
                            defaultValue={p.units}
                            valueChanged={(v) => {
                              updateParam(p, p.name!, p.description, v)
                            }}
                          />
                        </FormControl>
                      </Grid>
                      <Grid item xs={3}>
                        <Stack direction="row" alignItems="center" spacing={1} mt={2}>
                          <Stack spacing={0}>
                            <KeyboardArrowUp sx={{ cursor: "pointer", fontSize: '18px' }} onClick={() => handleParameterOrderChanged(index, 'up')} />
                            <KeyboardArrowDown sx={{ cursor: "pointer", fontSize: '18px' }} onClick={() => handleParameterOrderChanged(index, 'down')} />
                          </Stack>

                          <Delete sx={{ cursor: "pointer", fontSize: '16px' }} onClick={() => deleteParameter(index)} />
                        </Stack>
                      </Grid>
                    </Grid>
                    <FormHelperText sx={{ color: 'red' }}><>{errors()[p.name!]?.message}</></FormHelperText>
                  </Box>
                );
              })
            }
          </Stack>

          <Button variant="outlined" color="secondary" sx={{ mb: 2 }} onClick={addParameter}>
            Add Parameter <Add sx={{ fontSize: 20 }} />
          </Button>

          {
            parameterChecked &&
            <Slide direction="left" in={parameterChecked} mountOnEnter unmountOnExit>
              <Grid container mb={2}>
                <Grid item xs={10}>
                  <Grid container alignItems="top" spacing={2} >
                    <Grid item xs={5}>
                      <TextField
                        id="new-expression-name"
                        label="Name"
                        value={expression.name}
                        onChange={(e) => {
                          setExpression({ ...expression, name: e.target.value });
                        }}
                        size="small"
                        fullWidth />
                    </Grid>
                    <Grid item xs={2}>
                      <FormControl key={`${expression.name}_units`} sx={{ width: '100%' }}>
                        <UnitsLookupComboBox
                          id={`new-units`}
                          defaultValue={expression.units}
                          valueChanged={(v) => {
                            setExpression({ ...expression, units: v });
                          }}
                        />
                      </FormControl>
                    </Grid>
                  </Grid>
                  <Grid container spacing={1} sx={{ mt: 1 }}>
                    <Grid item>
                      <Button variant="outlined" color="secondary" onClick={cancelParameter}>Cancel</Button>
                    </Grid>
                    <Grid item>
                      <Button variant="contained" color="primary" onClick={saveParameter}>Save</Button>
                    </Grid>
                  </Grid>
                </Grid>
              </Grid>
            </Slide>
          }

        </AccordionDetails>
      </Accordion>
    </>
  );
};

export default FunctionParameters
