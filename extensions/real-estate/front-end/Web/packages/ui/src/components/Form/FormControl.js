import { useState } from 'react'
import _ from 'lodash'
import { caseInsensitiveEquals, passToFunction } from '@willow/ui'
import Label from 'components/Label/Label'
import { useForm } from './FormContext'
import { FormControlContext } from './FormControlContext'

export default function FormControl({
  id = undefined,
  name = undefined,
  errorName = undefined,
  label = undefined,
  value = undefined,
  defaultValue = null,
  error = undefined,
  readOnly = undefined,
  disabled = undefined,
  required = undefined,
  children = undefined,
  onChange = undefined,
  clearError = undefined,
  setData = undefined,
  hiddenLabel = undefined,
  ...rest
}) {
  const form = useForm()

  const [hasFocus, setHasFocus] = useState(false)

  const nextValue = value ?? _.get(form?.data, name) ?? defaultValue

  let nextError =
    error ??
    form?.errors.find((formError) =>
      caseInsensitiveEquals(formError.name, errorName ?? name)
    )?.message
  if (nextError === false) {
    nextError = null
  }

  const nextReadOnly = readOnly ?? form?.readOnly ?? false

  function handleChange(newValue, ...args) {
    if (onChange != null) {
      onChange(newValue, ...args)
      form?.clearError(errorName ?? name, clearError)
      return
    }

    if (name != null) {
      form?.setData((prevData) =>
        setData
          ? passToFunction(setData, (mainDataKey, dataIndexToUpdate) => {
              const mainData = prevData[mainDataKey]
              mainData[dataIndexToUpdate][name] = newValue
              return _.set(prevData, mainDataKey, mainData)
            })
          : _.set(prevData, name, newValue)
      )
      form?.clearError(errorName ?? name, clearError)
    }
  }

  const context = {
    setHasFocus,
  }

  return (
    <FormControlContext.Provider value={context}>
      <Label
        id={id}
        label={label}
        error={nextError}
        readOnly={nextReadOnly}
        disabled={disabled}
        value={nextValue}
        required={required}
        hasFocus={hasFocus}
        hiddenLabel={hiddenLabel}
      >
        {(labelId) =>
          children(
            {
              id: labelId,
              ...rest,
              value: nextValue,
              error: nextError,
              readOnly: nextReadOnly,
              disabled,
              onChange: handleChange,
            },
            form
          )
        }
      </Label>
    </FormControlContext.Provider>
  )
}
