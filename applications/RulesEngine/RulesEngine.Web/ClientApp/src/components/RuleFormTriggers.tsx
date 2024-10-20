import { Box, FormHelperText } from '@mui/material';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import { RuleDto, RuleTriggerDto, RuleTriggerType } from '../Rules';
import RuleTriggers from './formparts/RuleTriggers';

const RuleFormTriggers = (params: { rule: RuleDto, formContext: any, validateRule: (rule: RuleDto) => void }) => {
  const [rule, setRule] = useState(params.rule);

  const { register, clearErrors, formState: { errors } } = params.formContext;

  useQuery(["validateRuleTriggers", rule], async (_x: any) => {
    clearErrors();
    params.validateRule(rule);
  });

  // Whenever params.rule is invalidated, we need to refresh our copy.
  useEffect(() => {
    setRule(params.rule);
  }, [params.rule]);

  const getFormErrors = () => errors;

  const getFormRegister = () => register;

  const updateTriggers = (ruleTriggers: RuleTriggerDto[], triggerType: RuleTriggerType) => {
    const otherTriggers = rule.ruleTriggers!.filter(v => v.triggerType != triggerType);
    rule.init({ ...rule, ruleTriggers: [...ruleTriggers, ...otherTriggers] });
    setRule(rule);
  };

  const commanTriggerType = RuleTriggerType._1;

  return (
    <Box sx={{ flexGrow: 1 }}>
      <RuleTriggers
        triggers={rule.ruleTriggers!.filter(v => v.triggerType == commanTriggerType)}
        triggerType={commanTriggerType}
        label={"Commands"}
        isOpen={true}
        allowMultiple={true}
        updateTriggers={(t) => updateTriggers(t, commanTriggerType)}
        getFormErrors={getFormErrors}
        getFormRegister={getFormRegister}
      />

      <FormHelperText sx={{ color: 'red' }}><>{getFormErrors()["RuleTriggers"]?.message}</></FormHelperText>
    </Box >
  );
}

export default RuleFormTriggers
