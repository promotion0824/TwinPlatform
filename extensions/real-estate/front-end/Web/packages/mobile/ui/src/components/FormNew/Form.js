import { forwardRef, useEffect, useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useTimer } from 'hooks'
import { useSnackbar } from 'providers'
import { useEffectOnceMounted } from '@willow/common'
import Blocker from 'components/Blocker/Blocker'
import { FormContext } from './FormContext'
import styles from './Form.css'

export { useForm } from './FormContext'
export { default as FormControl } from './FormControl'
export { default as ValidationError } from './ValidationError'

export default forwardRef(function Form(
  {
    defaultValue,
    readOnly,
    success,
    showSubmitted = true,
    debounce = true,
    className,
    children,
    onSubmit = () => {},
    onSubmitted = () => {},
    preventBlockOnSubmitted = false,
    ...rest
  },
  forwardedRef
) {
  const snackbar = useSnackbar()
  const timer = useTimer()

  const ref = useRef()
  const formRef = ref ?? forwardedRef

  const [state, setState] = useState({
    data: _.cloneDeep(defaultValue) ?? {},
    isSubmitting: false,
    isSubmitted: false,
    isSuccessful: false,
    isError: false,
    errors: [],
  })

  const cxClassName = cx(styles.form, className)
  const showSuccess = state.isSuccessful && success != null

  useEffect(() => {
    formRef.current.querySelector('[data-error]')?.focus?.()
  }, [state.isError])

  function reset() {
    snackbar.clear()

    setState((prevState) => ({
      ...prevState,
      data: _.cloneDeep(defaultValue) ?? {},
      isSubmitting: false,
      isSubmitted: false,
      isError: false,
    }))
  }

  function handleSubmitted(response) {
    setState((prevState) => ({
      ...prevState,
      isSubmitted: true,
      isSuccessful: true,
    }))

    onSubmitted({ ...state, reset, response })
  }

  async function submit() {
    try {
      document.activeElement?.blur?.()
      snackbar.clear()

      setState((prevState) => ({
        ...prevState,
        isSubmitting: true,
        isSubmitted: false,
        isSuccessful: false,
        isError: false,
        errors: [],
      }))

      const response = await onSubmit(state)

      setState((prevState) => ({
        ...prevState,
        isSubmitting: false,
        isSubmitted: true,
        isSuccessful: false,
      }))

      if (showSubmitted) {
        timer.setTimeout(() => {
          handleSubmitted(response)
        }, 1000)
      } else {
        handleSubmitted(response)
      }
    } catch (err) {
      if (err?.name === 'AbortError') {
        return
      }

      let errors = [{ message: 'An error has occurred' }]
      if (err?.response?.status === 422 && Array.isArray(err.response.data)) {
        errors = err.response.data
      } else if (
        err?.response?.status === 422 &&
        Array.isArray(err.response.data.items)
      ) {
        errors = err.response.data.items
        if (err.response.data.message != null) {
          errors = [{ message: err.response.data.message }, ...errors]
        }
      } else if (err?.status === 422 && Array.isArray(err.errors)) {
        errors = err.errors
      } else {
        console.error(err) // eslint-disable-line
      }

      const snackbarErrors = errors
        .filter((error) => error.name == null)
        .map((error) => ({
          icon: 'error',
          header: error.message,
          description: error.description,
        }))

      if (snackbarErrors.length > 0) {
        snackbar.show(snackbarErrors)
      }

      setState((prevState) => ({
        ...prevState,
        isSubmitting: false,
        isSuccessful: false,
        isError: true,
        errors: errors.filter((error) => error.name != null),
      }))

      timer.setTimeout(() => {
        setState((prevState) => ({
          ...prevState,
          isError: false,
        }))
      }, 1000)
    }
  }

  const context = {
    data: state.data,
    isSubmitting: state.isSubmitting,
    isSubmitted: state.isSubmitted,
    isError: state.isError,
    errors: state.errors,
    readOnly,
    debounce,

    submit,

    setData(key, value) {
      if (key != null) {
        if (_.isFunction(key)) {
          setState((prevState) => {
            const nextData = key(prevState.data) ?? {}
            const keys = Object.keys(nextData).map((nextKey) =>
              nextKey.toLowerCase()
            )

            return {
              ...prevState,
              data: {
                ...prevState.data,
                ...nextData,
              },
              errors: prevState.errors.filter((prevError) =>
                keys.includes(prevError.name?.toLowerCase())
              ),
            }
          })
        } else if (_.isObject(key)) {
          setState((prevState) => {
            const keys = Object.keys(key).map((nextKey) =>
              nextKey.toLowerCase()
            )

            return {
              ...prevState,
              data: {
                ...prevState.data,
                ...key,
              },
              errors: prevState.errors.filter(
                (prevError) => !keys.includes(prevError.name?.toLowerCase())
              ),
            }
          })
        } else {
          setState((prevState) => ({
            ...prevState,
            data: _.set(prevState.data, key, value),
            errors: prevState.errors.filter(
              (prevError) =>
                prevError.name?.toLowerCase() !== key?.toLowerCase()
            ),
          }))
        }
      }
    },

    clearError(name) {
      setState((prevState) => ({
        ...prevState,
        errors: prevState.errors.filter(
          (prevError) => prevError.name?.toLowerCase() !== name?.toLowerCase()
        ),
      }))
    },

    setValidationErrors(errors) {
      setState((prevState) => ({
        ...prevState,
        errors: errors.filter((error) => error.name != null),
      }))
    },

    reset,
  }

  useEffectOnceMounted(() => {
    if (readOnly === true) {
      context.reset()
    }
  }, [readOnly])

  return (
    <FormContext.Provider value={context}>
      {!showSuccess && (
        <>
          <form
            {...rest}
            ref={formRef}
            className={cxClassName}
            onSubmit={(e) => {
              e.preventDefault()

              context.submit()
            }}
          >
            {_.isFunction(children) ? children(context) : children}
          </form>
          {(state.isSubmitting ||
            (state.isSubmitted && !preventBlockOnSubmitted)) && <Blocker />}
        </>
      )}
      {showSuccess && (_.isFunction(success) ? success(context) : success)}
    </FormContext.Provider>
  )
})
