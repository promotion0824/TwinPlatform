import {Checkbox, FormControlLabel} from '@mui/material';
import {Controller} from 'react-hook-form';
import {FormInputProps} from '../../types/FormInputProps';

export const FormInputCheckbox = ({name, control, label, setValue, sx}: FormInputProps) => {
  return <FormControlLabel
    control={
      <Controller
        name={name}
        render={({field: {onChange, value}}) => {
          return (
            <Checkbox
              checked={value}
              onChange={onChange}
              sx={sx}
            />
          );
        }}
        control={control}
      />
    }
    label={label}
    key={setValue}
  />
};
