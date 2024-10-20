import { Autocomplete, FormControl, TextField } from "@mui/material";
import { FieldValues, UseFormRegister } from "react-hook-form";
import { useQuery } from "react-query";
import useApi from "../../hooks/useApi";
import { RuleDto } from "../../Rules"

const CategoryField = (params: {
  rule: RuleDto,
  register: UseFormRegister<FieldValues>,
  errors: { [x: string]: any; }
}) => {

  const { rule, register, errors } = params;

  const apiclient = useApi();

  const categories = useQuery(['categories'], async (x: any) => {
    const cats = await apiclient.ruleCategories();
    return cats;
  });

  return (
    <FormControl key={"category"} fullWidth>
      <Autocomplete
        freeSolo
        options={categories.data ?? []}
        getOptionLabel={(option) => { return option; }}
        value={rule.category!}
        sx={{ border: "none" }}
        renderOption={(props, option) => <li {...props}>{option}</li>}
        renderInput={(params) =>
          <TextField
            {...params}
            id={"category"}
            {...register("category", { required: true, value: rule.category! })}
            error={!!errors.category}
            label="Category*"
            placeholder="Enter or select a category"
          />}
        onInputChange={(_, newInputValue) => { rule.category = newInputValue!; }}
        onChange={(_, newInputValue) => { rule.category = newInputValue!; }}
      />
    </FormControl>
  );
};

export default CategoryField;
