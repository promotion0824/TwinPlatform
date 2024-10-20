import { FormControl } from "@mui/material";
import { useEffect, useState } from "react";
import { FieldValues, UseFormRegister } from "react-hook-form";
import { WillowTextEditor } from "../WillowExpressionEditor";

const DescriptionEditorField = (params: {
  rule: { description: string | undefined },
  register: UseFormRegister<FieldValues>,
  errors: { [x: string]: any; },
  label?: string,
  placeholder?: string,
  required?: boolean
}) => {

  const { rule, errors, label } = params;
  const [ruleDescription, setRuleDescription] = useState(rule.description ?? "");

  useEffect(() => {
    setRuleDescription(rule.description ?? "");
  }, [rule])

  return (
    <FormControl key={"description"} fullWidth>
      <WillowTextEditor
        id={"description"}
        p={ruleDescription || ''}
        label={label ?? "Description"}
        onTextChanged={(newValue: string) => { rule.description = newValue; setRuleDescription(newValue); }}
        getFormErrors={() => errors}
      />
    </FormControl>
  );
};

export default DescriptionEditorField;
