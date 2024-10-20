import { forwardRef } from 'react'
import { useForm, FormControl } from 'components/Form/Form'
import Input from './Input/Input'

export default forwardRef(function InputComponent(
  { debounce, ...rest },
  forwardedRef
) {
  const form = useForm()

  const nextDebounce = debounce ?? form?.debounce

  return (
    <FormControl {...rest} defaultValue="">
      {(control) => (
        <Input {...control} ref={forwardedRef} debounce={nextDebounce} />
      )}
    </FormControl>
  )
})
