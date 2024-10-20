import { useLayoutEffect } from 'react'
import _ from 'lodash'
import Label from 'components/LabelNew/Label'
import { useForm } from './FormContext'

export default function FormControl({
  labelId,
  label,
  name,
  error,
  errorName,
  value,
  initialValue = null,
  showError,
  readOnly,
  required,
  children,
  onChange,
  ...rest
}) {
  const form = useForm()

  const derivedValue = value ?? _.get(form?.data, name) ?? initialValue
  const nextReadOnly = readOnly ?? form?.readOnly
  const nextError =
    error ??
    form?.errors.find(
      (formError) =>
        formError.name?.toLowerCase() ===
        (errorName != null ? errorName.toLowerCase() : name?.toLowerCase())
    )?.message

  function handleChange(nextValue, options) {
    form?.setData(name, nextValue)
    form?.clearError(errorName ?? name)

    try {
      onChange?.(nextValue, options)
    } catch (err) {
      // Validation errors
      if (err?.status === 422 && Array.isArray(err.errors)) {
        form?.setValidationErrors(err.errors)
      }
    }
  }

  useLayoutEffect(() => {
    form?.setData(name, derivedValue)
  }, [])

  return (
    <Label
      labelId={labelId}
      label={label}
      readOnly={nextReadOnly}
      error={nextError}
      showError={showError}
      value={derivedValue}
      required={required}
    >
      {(labelContext) =>
        children({
          id: labelContext.id,
          ...rest,
          value: derivedValue,
          readOnly: nextReadOnly,
          error,
          onChange: handleChange,
        })
      }
    </Label>
  )
}
