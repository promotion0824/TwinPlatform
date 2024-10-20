import { useState } from 'react'
import _ from 'lodash'
import Snackbars from './Snackbars'
import { SnackbarContext } from './SnackbarContext'

const DEFAULT_TIMEOUT = 5000

export default function SnackbarProvider(props) {
  const { children } = props

  const [snackbars, setSnackbars] = useState([])

  function getMessages(header, description) {
    let arr = header
    if (!_.isArray(header)) {
      arr = _.isObject(header)
        ? [
            {
              icon: 'error',
              ...header,
            },
          ]
        : [
            {
              icon: 'error',
              header,
              description,
            },
          ]
    }

    return arr.map((item) =>
      _.isObject(item)
        ? item
        : {
            icon: 'error',
            header: item,
            description: undefined,
          }
    )
  }

  const context = {
    snackbars,

    show(header, description) {
      const nextSnackbar = {
        snackbarId: _.uniqueId(),
        messages: getMessages(header, description),
        timeout: header?.timeout ?? DEFAULT_TIMEOUT,
      }

      setSnackbars((prevSnackbars) => [nextSnackbar, ...prevSnackbars])
    },

    hide(snackbarId) {
      setSnackbars((prevSnackbars) =>
        prevSnackbars.filter((snackbar) => snackbar.snackbarId !== snackbarId)
      )
    },

    clear() {
      setSnackbars([])
    },
  }

  return (
    <SnackbarContext.Provider {...props} value={context}>
      {children}
      <Snackbars />
    </SnackbarContext.Provider>
  )
}
