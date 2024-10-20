import { forwardRef } from 'react'
import { useForm, FormControl } from 'components/Form/Form'
import Input from './Input/Input'

export default forwardRef(function InputComponent(
  { debounce, children, ...rest },
  forwardedRef
) {
  const form = useForm()

  return (
    <FormControl {...rest} initialValue="">
      {(props) => (
        <Input
          {...props}
          ref={forwardedRef}
          debounce={debounce ?? form?.debounce}
        >
          {children}
        </Input>
      )}
    </FormControl>
  )
})
