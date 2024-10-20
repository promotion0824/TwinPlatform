import { FormControl } from 'components/Form/Form'
import Select from './Select/Select'

export { default as Option } from './Select/Option'

export default function SelectComponent({ children, ...rest }) {
  return (
    <FormControl {...rest}>
      {(control) => <Select {...control}>{children}</Select>}
    </FormControl>
  )
}
