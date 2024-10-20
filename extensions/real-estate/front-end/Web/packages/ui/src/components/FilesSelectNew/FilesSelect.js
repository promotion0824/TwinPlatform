import { FormControl } from 'components/Form/Form'
import FilesSelect from './FilesSelect/FilesSelect'

export default function FilesSelectComponent({ children, ...rest }) {
  return (
    <FormControl {...rest} defaultValue={[]}>
      {(props) => <FilesSelect {...props}>{children}</FilesSelect>}
    </FormControl>
  )
}
