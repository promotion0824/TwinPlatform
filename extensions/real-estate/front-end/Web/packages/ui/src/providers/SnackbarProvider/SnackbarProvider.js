import { useState } from 'react'
import _ from 'lodash'
import { SnackbarContext } from './SnackbarContext'
import Snackbars from './Snackbars'

export { useSnackbar } from './SnackbarContext'

/**
 * TODO:
 * - Update new snackbar component
 * - Add missing behaviour from https://willow.atlassian.net/wiki/spaces/PE/pages/2016477416/Snackbars
 * - Deprecate old snackbar
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/52231
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/52234
 *
 */
export function SnackbarProvider({ children }) {
  const [snackbars, setSnackbars] = useState([])
  const [toasts, setToasts] = useState([])

  const context = {
    show(message, options = {}) {
      const snackbarId = _.uniqueId()
      const { isToast = false } = options
      function close() {
        context.close({ snackbarId, isToast })
      }

      const content = _.isFunction(message) ? message(close) : message

      if (isToast) {
        setToasts((prevToasts) => [
          ...prevToasts,
          {
            snackbarId,
            message: content,
            isClosing: false,
            onClose: options?.onClose,
            isError: options?.isError,
            closeButtonLabel: options?.closeButtonLabel,
            height: options?.height ?? undefined,
            color: options?.color,
          },
        ])
      } else {
        setSnackbars((prevSnackbars) => [
          ...prevSnackbars,
          {
            snackbarId,
            message: content,
            description: options?.description,
            icon: options?.icon ?? 'error',
            isClosing: false,
            onClose: options?.onClose,
          },
        ])
      }
    },

    hide({ snackbarId, isToast = false }) {
      if (isToast) {
        setToasts((prevToasts) =>
          prevToasts.map((prevToast) =>
            prevToast.snackbarId === snackbarId
              ? {
                  ...prevToast,
                  isClosing: true,
                }
              : prevToast
          )
        )
      } else {
        setSnackbars((prevSnackbars) =>
          prevSnackbars.map((prevSnackbar) =>
            prevSnackbar.snackbarId === snackbarId
              ? {
                  ...prevSnackbar,
                  isClosing: true,
                }
              : prevSnackbar
          )
        )
      }
    },

    clear() {
      setSnackbars((prevSnackbars) =>
        prevSnackbars.map((prevSnackbar) => ({
          ...prevSnackbar,
          isClosing: true,
        }))
      )
    },

    close({ snackbarId, isToast = false }) {
      if (isToast) {
        const foundToast = toasts.find(
          (toast) => toast.snackbarId === snackbarId
        )
        foundToast?.onClose?.()
        setToasts((prevToasts) =>
          prevToasts.filter((prevToast) => prevToast.snackbarId !== snackbarId)
        )
      } else {
        const foundSnackBar = snackbars.find(
          (snackbar) => snackbar.snackbarId === snackbarId
        )
        foundSnackBar?.onClose?.()
        setSnackbars((prevSnackbars) =>
          prevSnackbars.filter(
            (prevSnackbar) => prevSnackbar.snackbarId !== snackbarId
          )
        )
      }
    },
  }

  return (
    <SnackbarContext.Provider value={context}>
      {children}
      <Snackbars snackbars={snackbars} toasts={toasts} />
    </SnackbarContext.Provider>
  )
}
