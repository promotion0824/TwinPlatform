import {TextField} from '@mui/material';
import {Controller} from 'react-hook-form';
import {FormInputProps} from '../../types/FormInputProps';

export const FormInputText = ({name, control, label, sx}: FormInputProps) => {
  return (
    <Controller
      render={({
                 field: {onChange, onBlur, value, ref},
                 fieldState: {error},
               }) => (
        <TextField
          value={value}
          onChange={onChange} // send value to hook form
          onBlur={onBlur} // notify when input is touched
          inputRef={ref} // wire up the input ref
          helperText={error ? error.message : null}
          error={!!error}
          sx={sx}
          variant="outlined"
          label={label}
        />
      )}
      name={name}
      control={control}
    />);
};
