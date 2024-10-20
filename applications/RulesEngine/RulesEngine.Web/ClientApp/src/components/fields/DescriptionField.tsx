import { FormControl, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { FieldValues, UseFormRegister } from "react-hook-form";

const DescriptionField = (params: {
  rule: { description: string | undefined },
  register: UseFormRegister<FieldValues>,
  errors: { [x: string]: any; },
  label?: string,
  placeholder?: string,
  required?: boolean
}) => {

  const { rule, register, errors, label, placeholder, required } = params;
  const [ruleDescription, setRuleDescription] = useState(rule.description ?? "");

  const onChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    rule.description = e.target.value;
    setRuleDescription(e.target.value);
  }

  useEffect(() => {
    setRuleDescription(rule.description ?? "");
  }, [rule])

  return (
    <FormControl key={"description"} fullWidth>
      <TextField
        id={"description"}
        {...register("description", { required: required ?? false, value: ruleDescription || '' })}
        fullWidth
        autoComplete='off'
        error={!!errors.description}
        label={label ?? "Description"}
        placeholder={placeholder ?? "Enter a description for the rule"}
        multiline
        onChange={onChange}
      />
    </FormControl>
  );
};

export default DescriptionField;
