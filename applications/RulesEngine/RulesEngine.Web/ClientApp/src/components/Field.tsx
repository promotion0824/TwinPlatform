import { FormControl, FormHelperText, FormLabel, InputAdornment, InputLabel, OutlinedInput, TextField } from '@mui/material';
import { RuleUIElementDto, RuleUIElementType } from '../Rules';
import { FieldValues, UseFormRegister } from 'react-hook-form';

interface IFieldProps {
  fullWidth?: boolean,
  element: RuleUIElementDto,
  register: UseFormRegister<FieldValues>,
  errors: { [x: string]: any },
  onUpdateDouble?: (id: string, value: number) => void,
  onUpdateString?: (id: string, value: any) => void,
  onUpdateInt?: (id: string, value: number) => void,
}

/**
 * Displays a UI element input field
 * @param element
 */
const Field = ({ element, register, errors, fullWidth, onUpdateDouble, onUpdateString, onUpdateInt }: IFieldProps) => {

  const { id, name, elementType, units, valueDouble, valueString, valueInt } = element;

  const validationId = id!;

  switch (elementType) {

    case RuleUIElementType._1: // double
      {
        return (
          <FormControl fullWidth={fullWidth} >
            <TextField
              id={id}
              {...register(validationId, { required: true })}
              defaultValue={valueDouble}
              placeholder={valueDouble!.toString()}
              autoComplete='off'
              label={name}
              type="number"
              error={errors[validationId] ? true : false}
              fullWidth
              inputProps={{ step: "any" }}
              InputProps={{
                inputMode: "numeric",
                endAdornment: <InputAdornment position="end" > {units}</InputAdornment>
              }}
              onBlur={(e) => onUpdateDouble?.(validationId, Number(e.target.value))} />
            <FormHelperText>{errors[validationId]?.message}</FormHelperText>
          </FormControl>
        );
      }
    case RuleUIElementType._2:  // percentage
      return (
        <FormControl fullWidth={fullWidth} >
          {/* include validation with required or other standard HTML validation rules */}
          <TextField
            id={id}
            {...register(validationId, {
              required: { value: true, message: "Percentage 1-100% is required" },
              max: { value: 100, message: "Percentage cannot be over 100%" },
              min: { value: 1, message: "Percentage must be at least 1%" }
            })}
            defaultValue={(valueDouble! * 100.0)}
            placeholder={(valueDouble! * 100.0).toFixed(2).toString()}
            label={name}
            type="number"
            inputProps={{ step: "any" }}
            autoComplete='off'
            error={errors[validationId] ? true : false}
            fullWidth
            InputProps={{
              endAdornment: <InputAdornment position="end" > {units}</InputAdornment>
            }}
            onBlur={(e) => onUpdateDouble?.(validationId, Number(e.target.value))}
          />
          {/* errors will return when field validation fails  */}
          <FormHelperText>{errors[validationId]?.message}</FormHelperText>
        </FormControl>
      );
    case RuleUIElementType._3: // int
      return (
        <FormControl fullWidth={fullWidth} >
          {/* include validation with required or other standard HTML validation rules */}
          <TextField
            id={id}
            {...register(validationId, {
              required: "Must enter a value",
              min: { value: 1, message: "Must be 1 or more" }
            })}
            defaultValue={valueInt}
            placeholder={(valueInt!.toString())}
            label={name}
            autoComplete='off'
            type="number"
            error={errors[validationId] ? true : false}
            fullWidth
            InputProps={{
              endAdornment: <InputAdornment position="end" > {units}</InputAdornment>
            }}
            onBlur={(e) => onUpdateInt?.(validationId, Number(e.target.value))}
          />
          {/* errors will return when field validation fails  */}
          <FormHelperText>{errors[validationId]?.message}</FormHelperText>
        </FormControl>
      );
    case RuleUIElementType._4: // string
      return (
        <FormControl fullWidth={fullWidth} >
          <FormLabel className="inputLabel">{name} </FormLabel>
          <TextField
            id={id}
            {...register(validationId, { required: true, max: 100, min: 1 })}
            defaultValue={valueString}
            label={name}
            placeholder={valueString!.toString()}
            autoComplete='off'
            error={errors[validationId] ? true : false}
            InputProps={{
              endAdornment: <InputAdornment position="end" > {units}</InputAdornment>
            }}
            onBlur={(e) => onUpdateString?.(validationId, e.target.value)} />
          <FormLabel className="inputUnits">{units}</FormLabel>
          {/* errors will return when field validation fails  */}
          <FormHelperText>{errors[validationId]?.message}</FormHelperText>
        </FormControl>
      );
    case RuleUIElementType._5: // expression
      return (
        <FormControl fullWidth={fullWidth} >
          <TextField
            id={id}
            label={name}
            onBlur={(e) => onUpdateString?.(validationId, e.target.value)}
            value={valueString} sx={{ fontSize: '6px' }} />
          <FormLabel className="inputUnits"  >{units}</FormLabel>
        </FormControl>
      );
    default:
      return (
        <FormControl>Undefined field type {elementType}</FormControl>
      );
  }
}

export default Field;
