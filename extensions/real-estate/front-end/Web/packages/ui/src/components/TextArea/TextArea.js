import { forwardRef } from 'react'
import { useForm, FormControl } from 'components/Form/Form'
import TextArea from './TextArea/TextArea'

export default forwardRef(function TextAreaComponent(
  { debounce, ...rest },
  forwardedRef
) {
  const form = useForm()

  const nextDebounce = debounce ?? form?.debounce

  return (
    <FormControl {...rest} defaultValue="">
      {(control) => (
        <TextArea {...control} ref={forwardedRef} debounce={nextDebounce} />
      )}
    </FormControl>
  )
})
