import { useEffect, useState } from "react";
import { FormControl, FormHelperText, TextField } from "@mui/material";
import { FieldValues, UseFormRegister } from "react-hook-form";

const TitleField = (params: {
  rule: { name: string | undefined },
  register: UseFormRegister<FieldValues>,
  errors: { [x: string]: any; },
  label?: string,
  placeholder?: string,
  valueChangedEvent?: (newValue: string) => void
}) => {

  const { rule, register, errors, label, placeholder, valueChangedEvent } = params;
  const [ruleName, setRuleName] = useState(rule.name);

  const onBlur = (e: any) => {
    const hasChanged = rule.name != e.target.value
    rule.name = e.target.value;
    setRuleName(e.target.value);
    if (hasChanged && valueChangedEvent) {
      valueChangedEvent(rule.name!);
    }
  }

  useEffect(() => {
    setRuleName(rule.name);
  }, [rule])

  return (
    <FormControl key={"name"} fullWidth>
      <TextField
        id={"name"}
        {...register("name", { required: true, value: ruleName ?? '' })}
        fullWidth
        autoComplete='off'
        error={!!errors.name}
        label={label ?? "Name*"}
        placeholder={placeholder ?? "Enter a name for the rule"}
        //required - This way not red border is showing??
        //helperText={errors.name && <span>A rule name is required</span>}
        onBlur={onBlur}
      />
      <FormHelperText sx={{ color: 'red' }}><>{errors.name?.message}</></FormHelperText>
    </FormControl>
  );
};

export default TitleField;
