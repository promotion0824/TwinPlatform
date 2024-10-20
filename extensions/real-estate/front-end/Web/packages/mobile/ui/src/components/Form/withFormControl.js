import { forwardRef, useLayoutEffect } from 'react'
import _ from 'lodash'
import { useUniqueId } from 'hooks'
import { useForm } from './FormContext'
import { FormControlContext } from './FormControlContext'

export default function withFormControl(defaultValue = null) {
  return (WrappedComponent) =>
    forwardRef((props, forwardedRef) => {
      const {
        label,
        name,
        nameKey,
        value,
        error,
        title,
        readOnly,
        disabled,
        onChange = () => {},
        ...rest
      } = props

      const form = useForm()

      const controlId = useUniqueId()
      const key = nameKey ?? name

      function updateFormValue(nextValue, isInitializing = false) {
        if (value === undefined && form != null && name != null) {
          form.setValue((prevFormValue) => {
            const nextFormValue = _.cloneDeep(prevFormValue)
            const derivedNextValue = _.isFunction(nextValue)
              ? nextValue(_.get(prevFormValue, name))
              : nextValue

            _.set(nextFormValue, name, derivedNextValue)

            return nextFormValue
          }, isInitializing)
        }
      }

      const derivedDefaultValue = _.isFunction(defaultValue)
        ? defaultValue(props)
        : defaultValue

      let derivedValue = value
      if (derivedValue === undefined && form != null)
        derivedValue = _.get(form.value, name)
      if (derivedValue === undefined) derivedValue = derivedDefaultValue

      let derivedError = form?.errors.find((formError) =>
        form.errorMatchesKey(formError, key)
      )?.message

      if (error) {
        derivedError = _.isString(error) ? error : ''
      }

      useLayoutEffect(() => {
        form?.registerControl(controlId, key)
        updateFormValue(derivedValue, true)

        return () => {
          form?.unregisterControl(controlId)
        }
      }, [name]) // eslint-disable-line

      const context = {
        name,
        nameKey,
        label,
        error: derivedError,
        title: derivedError || title,
        readOnly: readOnly ?? form?.readOnly,
        disabled: disabled ?? form?.disabled,
      }

      return (
        <FormControlContext.Provider value={context}>
          <WrappedComponent
            {...rest}
            ref={forwardedRef}
            value={derivedValue}
            onChange={(nextValue) => {
              updateFormValue(nextValue)

              onChange(nextValue)

              form?.updateControl(key)
            }}
          />
        </FormControlContext.Provider>
      )
    })
}
