import { Autocomplete, TextField, Chip, Checkbox } from '@mui/material';
import { useState } from 'react';
import { useQuery } from 'react-query';
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank';
import CheckBoxIcon from '@mui/icons-material/CheckBox';

const icon = <CheckBoxOutlineBlankIcon fontSize="small" />;
const checkedIcon = <CheckBoxIcon fontSize="small" />;

const TagsEditor = (props: { 
  id: string, 
  defaultValue: string[] | undefined, 
  valueChanged: (value: string[]) => any,
  queryKey: string, // Prop for query key
  allowFreeText?: boolean
  queryFn: (queryParam: any) => Promise<string[]> // Prop for query function
}) => {
  const { id, defaultValue = [], valueChanged, queryKey, queryFn, allowFreeText = true } = props;
  const [value, setValue] = useState<string[]>(defaultValue);

  const lookupQuery = useQuery([queryKey], async (queryParam: any) => {
    return queryFn(queryParam); // Use the passed query function
  }, {
    initialData: []
  });

  return (
    <Autocomplete
      id={id}
      freeSolo={allowFreeText}
      multiple
      disableCloseOnSelect
      options={lookupQuery.data?.sort((a, b) => a!.localeCompare(b!, 'en', { sensitivity: 'base' })) ?? []}
      getOptionLabel={(option) => option}
      value={value}
      onChange={(_, newValue) => { setValue(newValue); valueChanged(newValue); }}
      renderOption={(props, option, { selected }) => (
        <li {...props}>
          <Checkbox
            icon={icon}
            checkedIcon={checkedIcon}
            sx={{ fontSize: '12px' }}
            checked={selected}
          />
          {option}
        </li>
      )} 
      onBlur={() => { setValue(value); valueChanged(value); }}
      renderTags={(value, getTagProps) =>
        value.map((option, index) => (
          <Chip
            variant="outlined"
            label={option}
            size="small" sx={{ fontSize: '12px' }}
            {...getTagProps({ index })}
          />
        ))
      }
      renderInput={(params) => ( <TextField {...params} label="Tag(s)" /> )}
    />
  );
}

export default TagsEditor;
