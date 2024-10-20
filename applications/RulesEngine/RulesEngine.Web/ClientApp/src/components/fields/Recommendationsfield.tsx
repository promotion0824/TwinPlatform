import { FormControl } from "@mui/material";
import { useEffect, useState } from "react";
import { FieldValues, UseFormRegister } from "react-hook-form";
import { WillowTextEditor } from "../WillowExpressionEditor";

const Recommendationsfield = (params: {
  rule: { recommendations: string | undefined },
  register: UseFormRegister<FieldValues>,
  errors: { [x: string]: any; },
  label?: string,
  placeholder?: string,
  required?: boolean
}) => {

  const { rule, errors, label } = params;
  const [recommendations, setRecommendations] = useState(rule.recommendations ?? "");

  useEffect(() => {
    setRecommendations(rule.recommendations ?? "");
  }, [rule])

  return (
    <FormControl key={"recommendations"} fullWidth>
      <WillowTextEditor
        id={"recommendations"}
        p={recommendations || ''}
        label={label ?? "Recommendations"}
        onTextChanged={(newValue: string) => { rule.recommendations = newValue; setRecommendations(newValue); }}
        getFormErrors={() => errors}
      />
    </FormControl>
  );
};

export default Recommendationsfield;
