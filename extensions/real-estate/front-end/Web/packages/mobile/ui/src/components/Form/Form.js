import { useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useSnackbar } from 'providers/snackbar/SnackbarContext'
import { useApi, useLayoutEffectOnUpdate, useTimer } from 'hooks'
import Blocker from 'components/Blocker/Blocker'
import { useEffectOnceMounted, useLatest } from '@willow/common'
import { FormContext } from './FormContext'
import styles from './Form.css'

export { useForm } from './FormContext'
export { useFormControl } from './FormControlContext'
export { default as withFormControl } from './withFormControl'

export default function Form(props) {
  // eslint-disable-line
  const {
    value,
    defaultValue,
    method = 'post',
    url,
    block,
    readOnly,
    disabled,
    success,
    showSubmitted = true,
    showSuccessful = true,
    showErrors = true,
    className,
    children,
    onSubmit = () => {},
    onSubmitted = () => {},
    onError = () => {},
    onChange = () => {},
    ...rest
  } = props

  const snackbar = useSnackbar()
  const api = useApi()
  const timer = useTimer()

  const latestOnSubmit = useLatest(onSubmit)
  const latestOnSubmitted = useLatest(onSubmitted)
  const latestOnError = useLatest(onError)

  const [state, setState] = useState({
    value: defaultValue ?? {},
    defaultValue: defaultValue ?? value ?? {},
    latestValue: defaultValue ?? value ?? {},
    errors: [],
    response: undefined,
    status: undefined,
  })

  const derivedValue = value ?? state.value

  const isSubmittedRef = useRef(false)
  const controlsRef = useRef([])

  useEffectOnceMounted(() => {
    onChange(state.value)
  }, [state.value])

  const context = {
    readOnly,
    disabled,

    value: derivedValue,
    errors: state.errors,
    response: state.response,
    isSubmitting: state.status === 'submitting',
    isSubmitted: state.status === 'submitted',
    isSuccessful: state.status === 'successful',
    hasError: state.status === 'error',
    hasChanged: !_.isEqual(derivedValue, state.latestValue),

    async submit() {
      // eslint-disable-line
      try {
        snackbar.clear()
        timer.clearTimeout()
        isSubmittedRef.current = false

        setState((prevState) => ({
          ...prevState,
          errors: [],
          response: undefined,
          status: 'submitting',
        }))
        let response
        if (url != null) {
          response = await api.ajax(
            method,
            url,
            !_.isEmpty(derivedValue) ? derivedValue : undefined
          )
        }

        const submitValue = {
          ...context,
          api,
          response,
        }

        const submitResponse = await latestOnSubmit(submitValue)
        if (submitResponse !== undefined) {
          submitValue.response = submitResponse
        }

        setState((prevState) => {
          let status
          if (showSuccessful) status = 'successful'
          if (showSubmitted) status = 'submitted'

          return {
            ...prevState,
            latestValue: value ?? prevState.value,
            errors: [],
            response: submitValue.response,
            status,
          }
        })

        if (showSuccessful) {
          isSubmittedRef.current = true
        }

        if (showSubmitted) {
          timer.setTimeout(() => {
            setState((prevState) => ({
              ...prevState,
              status: showSuccessful ? 'successful' : undefined,
            }))

            latestOnSubmitted(submitValue)
          }, 1000)
        } else {
          latestOnSubmitted(submitValue)
        }
      } catch (err) {
        if (api.isCancel(err)) {
          return
        }

        let nextErrors = [{ message: 'An error has occurred' }]
        if (
          err?.response?.status === 422 &&
          _.isArray(err.response.data.items)
        ) {
          nextErrors = err.response.data.items
          if (err.response.data.message != null) {
            nextErrors = [{ message: err.response.data.message }, ...nextErrors]
          }
        } else if (err?.status === 422 && _.isArray(err.errors)) {
          nextErrors = err.errors
        } else {
          console.error(err) // eslint-disable-line
        }

        const snackbarErrors = nextErrors
          .filter(
            (error) =>
              !controlsRef.current.some((control) =>
                context.errorMatchesKey(error, control.key)
              )
          )
          .map((error) => error.message)

        if (showErrors && snackbarErrors.length > 0) {
          snackbar.show(snackbarErrors)
        }

        const submitValue = {
          ...context,
          nextErrors,
          snackbarErrors,
        }

        await latestOnError(err, submitValue)

        setState((prevState) => ({
          ...prevState,
          errors: nextErrors,
          response: undefined,
          status: 'error',
        }))

        timer.setTimeout(() => {
          setState((prevState) => ({
            ...prevState,
            status: undefined,
          }))
        }, 1000)
      }
    },

    setValue(nextValue, isInitializing = false) {
      if (isSubmittedRef.current) {
        return
      }

      if (value === undefined) {
        setState((prevState) => {
          const nextStateValue = _.isFunction(nextValue)
            ? nextValue(prevState.value)
            : nextValue

          return {
            ...prevState,
            value: nextStateValue,
            defaultValue: isInitializing
              ? nextStateValue
              : prevState.defaultValue,
            latestValue: isInitializing
              ? nextStateValue
              : prevState.latestValue,
          }
        })
      } else {
        onChange(_.isFunction(nextValue) ? nextValue(derivedValue) : nextValue)
      }
    },

    reset(nextValue) {
      snackbar.clear()
      isSubmittedRef.current = false

      setState((prevState) => {
        const resetValue = nextValue ?? { ...prevState.defaultValue }

        return {
          ...prevState,
          value: resetValue,
          defaultValue: resetValue,
          latestValue: resetValue,
          errors: [],
          response: undefined,
          status: undefined,
        }
      })
    },

    registerControl(controlId, key) {
      controlsRef.current = [...controlsRef.current, { controlId, key }]
    },

    unregisterControl(controlId) {
      controlsRef.current = controlsRef.current.filter(
        (control) => control.controlId !== controlId
      )
    },

    updateControl(key) {
      setState((prevState) => ({
        ...prevState,
        errors: prevState.errors.filter(
          (error) => !context.errorMatchesKey(error, key)
        ),
      }))
    },

    errorMatchesKey(error, key) {
      return key != null && error.name?.toLowerCase() === key?.toLowerCase()
    },
  }

  useLayoutEffectOnUpdate(() => {
    if (readOnly) {
      context.reset()
    }
  }, [readOnly])

  function handleSubmit(e) {
    e.preventDefault()
    e.stopPropagation()
  }

  const { isSubmitting, isSubmitted, isSuccessful } = context

  const cxClassName = cx(styles.form, className)
  const showSuccess = isSuccessful && success != null

  return (
    <FormContext.Provider value={context}>
      {!showSuccess && (
        <form {...rest} className={cxClassName} onSubmit={handleSubmit}>
          {_.isFunction(children) ? children(context) : children}
          {(isSubmitting || isSubmitted || isSuccessful) && (
            <Blocker key={isSubmitting} position={block} />
          )}
        </form>
      )}
      {showSuccess && (_.isFunction(success) ? success(context) : success)}
    </FormContext.Provider>
  )
}
