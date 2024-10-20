import numberUtils from 'utils/numberUtils'
import { FormControl } from 'components/Form/Form'
import Input from 'components/Input/Input/Input'

export default function NumberInput({ format, min, max, children, ...rest }) {
  return (
    <FormControl {...rest}>
      {(props) => (
        <Input
          inputMode="numeric"
          {...props}
          onChange={(nextStr) => {
            const nextValue = numberUtils.parse(nextStr, format, { min, max })

            if (props.value !== nextValue) {
              props.onChange(nextValue)
            }
          }}
        >
          {children}
        </Input>
      )}
    </FormControl>
  )
}
