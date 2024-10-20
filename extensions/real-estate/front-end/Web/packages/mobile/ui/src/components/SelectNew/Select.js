import { FormControl } from 'components/FormNew/Form'
import Select from './Select/Select'

export { default as Option } from './Select/Option'
export { default as OptGroup } from './Select/OptGroup'

export default function SelectComponent({ children, ...rest }) {
  return (
    <FormControl {...rest}>
      {(props) => <Select {...props}>{children}</Select>}
    </FormControl>
  )
}
