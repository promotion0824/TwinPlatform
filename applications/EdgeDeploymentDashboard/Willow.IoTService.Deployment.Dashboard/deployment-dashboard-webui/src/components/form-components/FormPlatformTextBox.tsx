import {Autocomplete, Stack, TextField} from '@mui/material';
import {Controller} from 'react-hook-form';
import {FormInputProps} from '../../types/FormInputProps';

// Combobox of supported platforms
export const FormPlatformSearchTextBox = ({name, control, label, sx}: FormInputProps) => {
  const platforms = ['arm64v8', 'arm32v7', 'amd64',];   // Supported platforms

  return (
    <Controller
      render={({
                 field: {onChange, value,},
               }) => (
        <Stack sx={sx}>
          <Autocomplete
            onChange={(_, newValue) => onChange(newValue)}
            value={value}
            options={platforms}
            renderInput={(params) => <TextField {...params} label={label}/>}
          />
        </Stack>
      )}
      name={name}
      control={control}
    />);
}
