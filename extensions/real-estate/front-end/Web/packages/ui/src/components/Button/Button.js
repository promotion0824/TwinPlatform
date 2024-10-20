import { forwardRef } from 'react'
import { useForm } from 'components/Form/FormContext'
import Button from './Button/Button'

export default forwardRef(function ButtonComponent(
  { type, onClick, ...rest },
  forwardedRef
) {
  const form = useForm()

  if (form != null && type === 'submit') {
    return (
      <Button
        loading={form.isSubmitting}
        successful={form.isSuccessful}
        error={form.isError}
        onClick={
          onClick == null && form != null ? () => form.submit() : onClick
        }
        {...rest}
        ref={forwardedRef}
        type={type}
      />
    )
  }

  return <Button {...rest} ref={forwardedRef} type={type} onClick={onClick} />
})
