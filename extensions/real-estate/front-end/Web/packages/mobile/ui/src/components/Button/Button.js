import { forwardRef } from 'react'
import { useForm } from 'components/Form/Form'
import ButtonComponent from './Button/Button'

export default forwardRef(function Button(
  { type, onClick, ...rest },
  forwardedRef
) {
  const form = useForm()

  return type === 'submit' ? (
    <ButtonComponent
      ref={forwardedRef}
      loading={form?.isSubmitting}
      success={form?.isSubmitted}
      error={form?.isError}
      onClick={onClick == null && form != null ? () => form.submit() : onClick}
      {...rest}
      type={type}
    />
  ) : (
    <ButtonComponent
      {...rest}
      ref={forwardedRef}
      type={type}
      onClick={onClick}
    />
  )
})
