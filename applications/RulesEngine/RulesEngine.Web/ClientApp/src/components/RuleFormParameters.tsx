import { Box, FormHelperText, Stack } from '@mui/material';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import Field from '../components/Field';
import { useStateContext } from '../providers/PageStateProvider';
import { RuleDto, RuleParameterDto, RuleUIElementType } from '../Rules';
import ExpressionParameters from './formparts/ExpressionParameters';

const RuleFormParameters = (params: { rule: RuleDto, formContext: any, validateRule: (rule: RuleDto) => void, setHasParameterErrors: (result: boolean) => void }) => {
  const [rule, setRule] = useState(params.rule);
  const [allParams, setAllParams] = useState([...rule.parameters!, ...rule.impactScores!]);

  const { register, clearErrors, setValue, formState: { errors } } = params.formContext;

  const { setPageState } = useStateContext();

  useQuery(["validateRuleParameters", rule], async (_x: any) => {
    clearErrors();
    params.validateRule(rule);
  });

  // Whenever params.rule is invalidated, we need to refresh our copy.
  useEffect(() => {
    setRule(params.rule);
    console.log('useEffect RuleFormParameters');
  }, [params.rule]);

  const getFormErrors = () => errors;

  const getFormRegister = () => register;

  const updateParameters = (parameters: RuleParameterDto[]) => {
    rule.init({ ...rule, parameters: parameters });
    setRule(rule);

    setPageState(true);
  };

  const updateScores = (parameters: RuleParameterDto[]) => {
    rule.init({ ...rule, impactScores: parameters });
    setRule(rule);

    setPageState(true);
  };

  const updateFilters = (parameters: RuleParameterDto[]) => {
    rule.init({ ...rule, filters: parameters });
    setRule(rule);

    setPageState(true);
  };

  const updateAllParams = (parameters: RuleParameterDto[]) => {
    setAllParams(parameters);
  }

  const updateDouble = (id: string, value: number) => {
    let element = rule.elements!.find((v) => v.id === id);

    if (element && value) {
      if (element.elementType == RuleUIElementType._2) {
        element.valueDouble = value / 100;
      }
      else {
        element.valueDouble = value;
      }

      rule.init({ ...rule, elements: rule.elements });
      setRule(rule);

      setPageState(true);
    }

    setValue(id, value, {
      shouldValidate: true
    });
  };

  const updateInt = (id: string, value: any) => {
    let element = rule.elements!.find((v) => v.id === id);
    if (element && value) {
      element.valueInt = value;

      setPageState(true);
    }

    setValue(id, value, {
      shouldValidate: true
    });
  };

  const updateString = (id: string, value: any) => {
    let element = rule.elements!.find((v) => v.id === id);
    if (element) {
      element.valueString = value;

      setPageState(true);
    }
  };

  return (
    <Box sx={{ flexGrow: 1 }}>
      <Stack spacing={1}>
        <Stack spacing={2} direction={'row'}>
          {rule.elements?.map((ui, i) =>
          (
            <Field key={i} element={ui} register={register} errors={errors} onUpdateDouble={updateDouble} onUpdateInt={updateInt} onUpdateString={updateString} />
          ))
          }
        </Stack>
        <ExpressionParameters
          parameters={rule.parameters!}
          allParams={allParams}
          label={"Capabilities"}
          showUnits={true}
          showField={true}
          showSettings={true}
          isOpen={true}
          updateParameters={updateParameters}
          updateAllParams={updateAllParams}
          getFormErrors={getFormErrors}
          getFormRegister={getFormRegister}
        />

        <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()["Parameters"]?.message}</></FormHelperText>

        {!rule.isCalculatedPoint &&
          <>
            <ExpressionParameters
              parameters={rule.impactScores!}
              allParams={allParams}
              label={"Impact scores"}
              showUnits={true}
              showField={true}
              showSettings={true}
              isOpen={false}
              updateParameters={updateScores}
              updateAllParams={updateAllParams}
              getFormErrors={getFormErrors}
              getFormRegister={getFormRegister}
            />

            <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()["ImpactScores"]?.message}</></FormHelperText>
          </>
        }

        <ExpressionParameters
          parameters={rule.filters!}
          allParams={allParams}
          label={"Filters"}
          showUnits={false}
          showField={false}
          showSettings={false}
          isOpen={false}
          updateParameters={updateFilters}
          updateAllParams={updateAllParams}
          getFormErrors={getFormErrors}
          getFormRegister={getFormRegister}
        />

        <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()["Filters"]?.message}</></FormHelperText>
      </Stack>
    </Box >
  );
}

export default RuleFormParameters
