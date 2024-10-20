import { RuleUIElementDto, RuleUIElementType } from '../Rules';
import { FormControl, Input, InputLabel } from '@mui/material';

interface IFieldDescriptionProps {
  element: RuleUIElementDto
}

/**
 * Displays a UI element input field description
 * @param element
 */
const FieldDescription = ({ element }: IFieldDescriptionProps) => {

  const { name, elementType, units } = element;

  switch (elementType) {

    case RuleUIElementType._1: // double
      return (
        <FormControl component="fieldset" fullWidth={true}>
          <InputLabel htmlFor="numeric-value">{name} ({units}): </InputLabel>
          <Input id="numeric-value" disabled={true} value="A numeric value" />
        </FormControl>
      );
    case RuleUIElementType._2:  // percentage
      return (
        <FormControl component="fieldset" fullWidth={true}>
          <InputLabel htmlFor="percentage-value">{name} ({units}): </InputLabel>
          <Input id="percentage-value" disabled={true} value="A percentage value" />
          {/*    <FormHelperText id="my-helper-text">....</FormHelperText>*/}
        </FormControl>
      );
    case RuleUIElementType._3: // int
      return (
        <FormControl component="fieldset" fullWidth={true}>
          <InputLabel htmlFor="numeric-value">{name} ({units}): </InputLabel>
          <Input id="integer-value" disabled={false} value="An integer value" />
        </FormControl>
      );
    case RuleUIElementType._4: // string
      return (
        <FormControl component="fieldset" fullWidth={true}>
          <InputLabel htmlFor="string-value">{name}: </InputLabel>
          <Input id="string-value" disabled={true} value="A string value" />
        </FormControl>
      );
    case RuleUIElementType._5: // expression
      return (
        <FormControl component="fieldset" fullWidth={true}>
          <InputLabel htmlFor="expression-value">{name}: </InputLabel>
          <Input id="expression-value" disabled={true} value={"A Willow Expression " + units} />
        </FormControl>
      );
    default:
      return (
        <FormControl component="fieldset">Undefined field type {elementType}</FormControl>
      );
  }
}

export default FieldDescription;
