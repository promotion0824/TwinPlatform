import { forwardRef } from 'react'
import { useForm, FormControl } from '../FormNew/Form'
import TextArea from './TextArea/TextArea'

export default forwardRef(function TextAreaComponent(
  { debounce, children, ...rest },
  forwardedRef
) {
  const form = useForm()

  return (
    <FormControl {...rest} initialValue="">
      {(props) => (
        <TextArea
          {...props}
          debounce={debounce ?? form?.debounce}
          ref={forwardedRef}
        >
          {children}
        </TextArea>
      )}
    </FormControl>
  )
})
