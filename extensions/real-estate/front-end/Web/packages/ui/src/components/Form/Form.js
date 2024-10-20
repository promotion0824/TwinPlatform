import { useEffect, useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useEffectOnceMounted } from '@willow/common'
import {
  passToFunction,
  useApi,
  useFetchRefresh,
  useHasUnmountedRef,
  useLanguage,
  useSnackbar,
  useTimer,
} from '@willow/ui'
import { useModal } from 'components/Modal/Modal'
import Blocker from 'components/Blocker/Blocker'
import getErrors from './getErrors'
import { FormContext } from './FormContext'
import styles from './Form.css'

export { useForm } from './FormContext'
export { useFormControl } from './FormControlContext'
export { default as FormControl } from './FormControl'
export { default as ValidationError } from './ValidationError'

export default function Form({
  defaultValue,
  readOnly,
  debounce = 300,
  success,
  className,
  children,
  onSubmit,
  onSubmitted,
  preventBlockOnSubmitted = false,
  skipErrorSnackbar = false,
  ...rest
}) {
  const api = useApi()
  const fetchRefresh = useFetchRefresh()
  const hasUnmountedRef = useHasUnmountedRef()
  const modal = useModal()
  const snackbar = useSnackbar()
  const timer = useTimer()
  const { language } = useLanguage()

  const formRef = useRef()

  const [state, setState] = useState(() => ({
    data: _.cloneDeep(defaultValue) ?? {},
    isSubmitting: false,
    isSuccessful: false,
    isSubmitted: false,
    isError: false,
    errors: [],
  }))

  useEffect(() => {
    if (state.isError) {
      formRef.current.querySelector('[data-error]')?.focus?.()
      formRef.current.querySelector('[data-error]')?.select?.()
    }
  }, [state.isError])

  function clearNestedErrors(
    prevErrors,
    mainErrorKey,
    errorNameToClear,
    errorIndexToClear
  ) {
    if (!mainErrorKey || !prevErrors.length) return prevErrors

    let currentErrors = prevErrors.find((err) => err.name === mainErrorKey)
    if (currentErrors) {
      currentErrors = {
        ...currentErrors,
        nestedErrors: currentErrors.nestedErrors
          .map((nestedError) => {
            if (nestedError.index !== errorIndexToClear) return nestedError
            return {
              ...nestedError,
              errors: nestedError.errors.filter(
                (err) => err.name !== errorNameToClear
              ),
            }
          })
          .filter((x) => x?.errors?.length),
      }
      if (!currentErrors.nestedErrors || !currentErrors.nestedErrors.length)
        currentErrors = null
    }
    return prevErrors
      .map((err) => (err.name !== mainErrorKey ? err : currentErrors))
      .filter((x) => !!x)
  }

  const context = {
    readOnly,
    debounce,
    data: state.data,
    initialData: defaultValue,
    isSubmitting: state.isSubmitting,
    isSuccessful: state.isSuccessful,
    isSubmitted: state.isSubmitted,
    isError: state.isError,
    errors: state.errors,

    async submit() {
      try {
        document.activeElement?.blur?.()
        snackbar.clear()

        setState((prevState) => ({
          ...prevState,
          isSubmitting: true,
          isSuccessful: false,
          isSubmitted: false,
          isError: false,
          errors: [],
        }))

        const response = await onSubmit?.({
          ...context,
          api,
          fetchRefresh,
          modal,
        })

        if (hasUnmountedRef.current) {
          return
        }

        setState((prevState) => ({
          ...prevState,
          isSubmitting: false,
          isSuccessful: true,
          isSubmitted: false,
          isError: false,
        }))

        await timer.sleep(1000)

        if (hasUnmountedRef.current) {
          return
        }

        setState((prevState) => ({
          ...prevState,
          isSubmitted: true,
        }))

        onSubmitted?.({
          ...context,
          response,
          api,
          fetchRefresh,
          modal,
        })
      } catch (err) {
        if (err?.response == null && err?.name !== 'ValidationError') {
          console.error(err) // eslint-disable-line
        }

        const errors = getErrors(err, language)

        if (!skipErrorSnackbar) {
          errors.snackbarErrors.forEach((error) => {
            snackbar.show(error.message, {
              description: error.description,
            })
          })
        }

        if (hasUnmountedRef.current) {
          return
        }

        setState((prevState) => ({
          ...prevState,
          isSubmitting: false,
          isSuccessful: false,
          isSubmitted: false,
          isError: true,
          errors: errors.allErrors,
        }))

        await timer.sleep(1000)

        if (hasUnmountedRef.current) {
          return
        }

        setState((prevState) => ({
          ...prevState,
          isError: false,
        }))
      }
    },

    setData(fn) {
      setState((prevState) => ({
        ...prevState,
        data: passToFunction(fn, prevState.data),
      }))
    },

    clearError(name, fn) {
      setState((prevState) => ({
        ...prevState,
        errors: fn
          ? passToFunction(
              fn,
              (mainErrorKey, errorIndexToClear, propNameOverride) =>
                clearNestedErrors(
                  prevState.errors,
                  mainErrorKey,
                  propNameOverride ?? name,
                  errorIndexToClear
                )
            )
          : prevState.errors.filter(
              (prevError) =>
                prevError.name?.toLowerCase() !== name?.toLowerCase()
            ),
      }))
    },

    setErrors(fn) {
      setState((prevState) => ({
        ...prevState,
        errors: passToFunction(fn, prevState.errors),
      }))
    },

    reset() {
      snackbar.clear()

      setState((prevState) => ({
        ...prevState,
        data: _.cloneDeep(defaultValue) ?? {},
        isSubmitting: false,
        isSuccessful: false,
        isSubmitted: false,
        isError: false,
        errors: [],
      }))
    },
  }

  useEffectOnceMounted(() => {
    if (readOnly === true) {
      context.reset()
    }
  }, [readOnly])

  const cxClassName = cx(styles.form, className)
  const showSuccess = state.isSuccessful && success != null

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
            {passToFunction(children, context)}
          </form>
          {(state.isSubmitting ||
            (state.isSuccessful && !preventBlockOnSubmitted)) && <Blocker />}
        </>
      )}
      {showSuccess && passToFunction(success, context)}
    </FormContext.Provider>
  )
}
