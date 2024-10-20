import { DndContext, closestCenter, PointerSensor, useSensor, useSensors, DragEndEvent } from "@dnd-kit/core";
import { useSortable, SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { Add, ArrowForwardIosSharp, Delete, DriveFileRenameOutline, Settings, DragIndicator } from '@mui/icons-material';
import { Accordion, AccordionDetails, Box, Button, FormControl, FormHelperText, Grid, Slide, Stack, styled, TextField, Tooltip } from '@mui/material';
import MuiAccordionSummary, { AccordionSummaryProps } from '@mui/material/AccordionSummary';
import Badge from '@mui/material/Badge';
import Typography from '@mui/material/Typography';
import { useEffect, useState } from 'react';
import { FieldErrors, FieldValues, UseFormRegister } from 'react-hook-form';
import { RuleParameterDto } from '../../Rules';
import CopyToClipboardButton from '../CopyToClipboard';
import { CumulativeTypeLookup } from '../Lookups';
import SelectionMenu from '../menu/SelectionMenu';
import UnitsLookupComboBox from '../UnitsLookupComboBox';
import { WillowExpressionEditor } from '../WillowExpressionEditor';

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
  parameters: RuleParameterDto[],
  allParams: RuleParameterDto[],
  label: string,
  showUnits: boolean,
  showField: boolean,
  showSettings: boolean,
  isOpen?: boolean,
  canAdd?: boolean,
  canDelete?: boolean,
  canChangeOrder?: boolean,
  canRename?: boolean,
  validationFieldPrefix?: string,
  updateParameters: (parameters: RuleParameterDto[]) => void,
  updateAllParams: (parameters: RuleParameterDto[]) => void,
  getFormErrors: () => FieldErrors,
  getFormRegister: () => UseFormRegister<FieldValues>
}

const ExpressionParameter = (params: { id: string, parameters: RuleParameterDto[], parameter: RuleParameterDto, canChangeOrder: boolean, validationFieldPrefix?: string, canDelete: boolean, canRename: boolean, showSettings: boolean, showField: boolean, deleteParameter: (p: RuleParameterDto) => void, saveParameter: (p: RuleParameterDto) => void, getFormErrors: () => FieldErrors }) => {
  const parameter = params.parameter;
  const canRename = params.canDelete;
  const canDelete = params.canRename;
  const canChangeOrder = params.canChangeOrder;
  const showField = params.showField;
  const validationFieldPrefix = params.validationFieldPrefix;
  const showSettings = params.showSettings;
  const saveParameter = params.saveParameter;
  const deleteParameter = params.deleteParameter;
  const dragId = parameter.fieldId!;
  const getFormErrors = params.getFormErrors;
  const label = `${parameter.name} ${parameter.cumulativeSetting! > 0 ? CumulativeTypeLookup.GetDisplayString(parameter.cumulativeSetting!) : ""}`;
  const menuItems = CumulativeTypeLookup.GetCumulativeTypeFilter();

  const [selectedItemIndex, setSelectedItemIndex] = useState(0);
  const [anchorEl, setAnchorEl] = useState(null);
  const [rename, setRename] = useState(false);

  function handleMenuClick(event: any) {
    setSelectedItemIndex(parameter.cumulativeSetting ?? 0);
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
    setSelectedItemIndex(0);
  };

  function handleMenuItemSelect(itemIndex: any) {
    parameter.cumulativeSetting = itemIndex;
    saveParameter(parameter);
    setAnchorEl(null);
    setSelectedItemIndex(0);
  };

  function getFieldId(fieldName: string) {
    if (validationFieldPrefix) {
      return `${validationFieldPrefix}_${fieldName}`;
    }
    return fieldName;
  }

  const {
    attributes,
    isDragging,
    listeners,
    setNodeRef,
    transform,
    transition,
  } = useSortable({
    id: dragId,
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition
  };
  const buttonDragStyle = {
    cursor: isDragging ? "grabbing" : "grab"
  };

  return (
    <Box
      key={dragId}
      style={style}
      flexGrow={1}>
      <Grid container alignItems="top" spacing={0.5} >
        {(canRename && rename) &&
          <Grid item xs={12}>
            <ParamDetails
              id={parameter.fieldId!}
              name={parameter.name!}
              unit={parameter.units!}
              showUnits={false}
              showField={showField}
              saveParameter={(id, name, _) => {
                parameter.fieldId = id;
                parameter.name = name;
                saveParameter(parameter);
                setRename(false);
              }}
              cancelParameter={() => {
                setRename(false);
              }} />
          </Grid>
        }
        <Grid item xs={5}>
          <Stack direction="row" alignItems="center">
            {label} {showField && <>({parameter.fieldId})</>}
          </Stack>
        </Grid>

        <Grid item xs={5}>
          <Stack direction="row" alignItems="center" spacing={0.5} justifyContent="flex-end">
            <CopyToClipboardButton description="Copy field id" content={parameter.fieldId!} />
            {showSettings &&
              <Tooltip title="Settings">
                {parameter.cumulativeSetting! > 0 ?
                  <Badge variant="dot" color="info" overlap="rectangular">
                    <Settings sx={{ cursor: "pointer", fontSize: '16px' }} onClick={(event) => handleMenuClick(event)} />
                  </Badge> :
                  <Settings sx={{ cursor: "pointer", fontSize: '16px' }} onClick={(event) => handleMenuClick(event)} />
                }
              </Tooltip>
            }

            {canDelete && <Tooltip title="Delete"><Delete sx={{ cursor: "pointer", fontSize: '16px' }} onClick={() => deleteParameter(parameter)} /></Tooltip>}

            {canRename && <Tooltip title="Rename"><DriveFileRenameOutline sx={{ cursor: "pointer", fontSize: '16px' }} onClick={() => setRename(true)} /></Tooltip>}

          </Stack>
        </Grid>

        <Grid item xs={2}>
          <Typography variant="body1">Unit</Typography>
        </Grid>

        <Grid item xs={10}>


          <FormControl key={getFieldId(parameter.fieldId!)}
            sx={{ '& .MuiFormHelperText-root': { color: 'red' }, width: '100%' }}>

            <WillowExpressionEditor
              p={parameter}
              label={""}
              parameters={params.parameters}
              onParameterChanged={() => saveParameter(parameter)}
              getFormErrors={getFormErrors}
            />
          </FormControl>
          <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()[getFieldId(parameter.fieldId!)]?.message}</></FormHelperText>
        </Grid>

        <Grid item xs={2}>
          <Stack direction="row" alignItems="center"> <FormControl fullWidth key={`${getFieldId(parameter.fieldId!)}_units`} sx={{ '& .MuiFormHelperText-root': { color: 'red' } }}>
            <UnitsLookupComboBox
              id={`${getFieldId(parameter.fieldId!)}_units`}
              showLabel={false}
              defaultValue={parameter.units}
              valueChanged={(v) => {
                parameter.units = v;
                saveParameter(parameter);
              }}
            />
          </FormControl>
            {canChangeOrder && <Box
              component="div"
              {...attributes}
              {...listeners}
              data-dnd-id={dragId}
              data-dnd-type="item"
              ref={setNodeRef}
              style={buttonDragStyle}
            >
              <DragIndicator sx={{ fontSize: '18px', mt: 1 }} />
            </Box>
            }</Stack>

          <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()[`${getFieldId(parameter.fieldId!)}_units`]?.message}</></FormHelperText>
        </Grid>
      </Grid>

      <SelectionMenu
        items={menuItems}
        selectedIndex={selectedItemIndex}
        open={Boolean(anchorEl)}
        anchorEl={anchorEl}
        onSelect={handleMenuItemSelect}
        onClose={handleMenuClose}
      />
    </Box>
  );
}

const ParamDetails = (params: { id: string, name: string, unit: string, showField: boolean, showUnits: boolean, saveParameter: (id: string, name: string, unit: string) => void, cancelParameter: () => void }) => {
  const [id, setId] = useState(params.id);
  const [name, setName] = useState(params.name);
  const [unit, setUnit] = useState(params.unit);

  const showField = params.showField;
  const showUnits = params.showUnits;
  const saveParameter = params.saveParameter;
  const cancelParameter = params.cancelParameter;

  return (<Slide direction="left" in={true} mountOnEnter unmountOnExit>
    <Grid container mb={2}>
      <Grid item xs={10}>
        <Grid container alignItems="top" spacing={1} >
          <Grid item xs={5}>
            <TextField
              id="new-expression-name"
              key="newExpression"
              label="Name"
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                if (!showField) {
                  setId(e.target.value);
                }
              }}
              size="small"
              fullWidth />
          </Grid>
          {showField &&
            <Grid item xs={showUnits ? 5 : 7}>
              <TextField
                id="new-fieldId"
                key="newFieldId"
                label="FieldId"
                value={id}
                onChange={(e) => {
                  setId(e.target.value);
                }}
                size="small"
                fullWidth />
            </Grid>}
          {showUnits &&
            <Grid item xs={2}>
              <UnitsLookupComboBox
                id="new-Units"
                defaultValue={unit}
                valueChanged={(value) => {
                  setUnit(value);
                }}
              />
            </Grid>}
        </Grid>
        <Grid container spacing={1} sx={{ mt: 1 }}>
          <Grid item>
            <Button variant="outlined" color="secondary" onClick={() => {
              cancelParameter();
            }}>Cancel</Button>
          </Grid>
          <Grid item>
            <Button variant="contained" color="primary" onClick={() => {
              saveParameter(id, name, unit);
            }}>OK</Button>
          </Grid>
        </Grid>
      </Grid>
    </Grid>
  </Slide>);
}

const ExpressionParameters = (params: EditParametersProps) => {
  const [parameterChecked, setParameterChecked] = useState(false);
  const [expanded, setExpanded] = useState(params.isOpen ?? true);
  const label = params.label;
  const [parameters, setParameters] = useState(params.parameters);
  const [allParams, setAllParams] = useState(params.allParams);
  const showUnits = params.showUnits;
  const showField = params.showField;
  const showSettings = params.showSettings;
  const canAdd = params.canAdd ?? true;
  const canDelete = params.canDelete ?? true;
  const canChangeOrder = params.canChangeOrder ?? true;
  const canRename = params.canRename ?? true;

  const addParameter = (id: string, name: string, units: string) => {
    if (!(id?.length > 0) || !(name?.length > 0)) {
      return;
    }

    let index = parameters.findIndex(v => v.fieldId == "result");
    let index2 = allParams.findIndex(v => v.fieldId == "result");

    if (index < 0) {
      if (parameters.length > 0) {
        index = parameters.length;
      }
      else {
        index = 0;
      }
    }

    if (index2 < 0) {
      if (allParams.length > 0) {
        index2 = allParams.length;
      }
      else {
        index2 = 0;
      }
    }

    const newParameters = [...parameters!];
    const newAllParams = [...allParams!];

    const newParam = new RuleParameterDto({ name: name, fieldId: id, pointExpression: "0.0", units: units });

    newParameters.splice(index, 0, newParam);
    newAllParams.splice(index2, 0, newParam);

    setParameters(newParameters);
    setAllParams(newAllParams);

    params.updateParameters(newParameters);
    params.updateAllParams(newAllParams);
  }

  const deleteParameter = (eId: any) => {
    let newParameters = Array.from(parameters!);
    let newAllParams = Array.from(allParams!);
    newParameters.splice(eId, 1);

    const deletedParam = parameters[eId];
    const newIndex = allParams.findIndex(param => param.fieldId === deletedParam.fieldId);
    newAllParams.splice(newIndex, 1);
    setParameters(newParameters);
    setAllParams(newAllParams);
    params.updateParameters(newParameters);
    params.updateAllParams(newAllParams);
  }

  // Whenever the rule is invalidated, we need to refresh our paramter list.
  useEffect(() => {
    setParameters(params.parameters);
    setAllParams(params.allParams);
  }, [params.parameters, params.allParams]);

  const sensors = useSensors(useSensor(PointerSensor));

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (active.id !== over!.id) {
      const oldIndex = parameters.findIndex((item) => item.fieldId === active.id);
      const newIndex = parameters.findIndex((item) => item.fieldId === over!.id);

      const newItems = Array.from(parameters);
      const [movedItem] = newItems.splice(oldIndex, 1);
      newItems.splice(newIndex, 0, movedItem);
      setParameters(newItems);
      params.updateParameters(newItems);
    }
  };


  return (
    <>
      <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }} expanded={expanded} onChange={() => setExpanded(!expanded)}>
        {(label.length > 0) && <AccordionSummary>
          <Typography variant="h4">{label}</Typography>
        </AccordionSummary>}
        <AccordionDetails sx={{ padding: (label.length == 0) ? "0px" : undefined }}>
          {canAdd && <Button variant="outlined" color="secondary" sx={{ mb: 2 }} onClick={() => setParameterChecked(true)}>
            Add Expression <Add sx={{ fontSize: 20 }} />
          </Button>}

          <Stack direction="column" spacing={2}>
            {parameters &&
              <DndContext
                sensors={sensors}
                collisionDetection={closestCenter}
                onDragEnd={handleDragEnd}
              >
                <SortableContext items={parameters.map((v) => ({ id: v.fieldId! }))} strategy={verticalListSortingStrategy}>
                  {
                    parameters.map((p, index) => {
                      return (
                        <ExpressionParameter
                          id={index.toString()}
                          key={index}
                          parameters={allParams}
                          parameter={p}
                          canRename={canRename}
                          validationFieldPrefix={params.validationFieldPrefix}
                          canDelete={canDelete}
                          showSettings={showSettings}
                          showField={showField}
                          canChangeOrder={canChangeOrder}
                          getFormErrors={params.getFormErrors}
                          saveParameter={() => {
                            params.updateParameters(parameters);
                            parameters.forEach(param => {
                              let existingParam = allParams.find(p => p.name === param.name);
                              if (existingParam) {
                                  if (existingParam.fieldId !== param.fieldId) {
                                      existingParam.fieldId = param.fieldId;
                                  }
                              } else {
                                  allParams.push(param);
                              }
                          });
                            params.updateAllParams(allParams);
                            setParameters([...parameters]);
                            setAllParams([...allParams]);
                          }}
                          deleteParameter={() => {
                            deleteParameter(index);
                          }}
                        />
                      );
                    })
                  }
                </SortableContext>
              </DndContext>
            }
            {
              parameterChecked &&
              <ParamDetails
                id={""}
                name={""}
                unit={""}
                showUnits={showUnits}
                showField={showField}
                saveParameter={(id, name, unit) => {
                  addParameter(id, name, unit);
                  setParameterChecked(false);
                }}
                cancelParameter={() => {
                  setParameterChecked(false);
                }} />
            }
          </Stack>
        </AccordionDetails>
      </Accordion>
    </>
  );
};

export default ExpressionParameters
