import { Autocomplete, TextField } from '@mui/material';
import { useState } from 'react';
import { useQuery } from 'react-query';
import useApi from '../hooks/useApi';

const UnitsLookupComboBox = (props: { showLabel?: boolean, id: string, defaultValue: string | undefined, valueChanged: (value: string) => any }) => {
  const id = props.id;
  const valueChanged = props.valueChanged;
  const showLabel = props.showLabel ?? true;
  const [defaultValue, setDefaultValue] = useState(props.defaultValue ?? "");
  const apiclient = useApi();

  const unitsLookupQuery = useQuery(["unitsLookup"], async (_x: any) => {
    
    const units = await apiclient.getUnits();
    const result: string[] = [];

    units.forEach(v => {
      result.push(v.name!);
      v.aliases!.forEach(a => {
        result.push(a);
      });
    });

    return result;
  });

  const [value, setValue] = useState("");

  return <Autocomplete
    id={id}
    freeSolo
    options={unitsLookupQuery.data?.sort((a, b) => a!.localeCompare(b!, 'en', { sensitivity: 'base' })) ?? []}
    getOptionLabel={(option) => { return option; }}
    value={defaultValue}
    sx={{ border: "none" }}
    renderOption={(props, option) => <li {...props}>{option}</li>}
    onBlur={() => { valueChanged(value); }}
    onInputChange={(_, newInputValue) => { setValue(newInputValue); }}
    renderInput={(params) => <TextField {...params} label={showLabel ? "Unit" : ""} />}
  />
}

export default UnitsLookupComboBox;
