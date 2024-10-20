import { forwardRef } from 'react'
import { FormControl } from 'components/Form/Form'
import Checkbox from './Checkbox/Checkbox'

export default forwardRef(function CheckboxComponent(
  { children, ...rest },
  forwardedRef
) {
  return (
    <FormControl {...rest} defaultValue={false}>
      {(props) => (
        <Checkbox
          ref={forwardedRef}
          {...props}
          role="checkbox"
          aria-checked={!!rest?.value}
        >
          {children}
        </Checkbox>
      )}
    </FormControl>
  )
})
