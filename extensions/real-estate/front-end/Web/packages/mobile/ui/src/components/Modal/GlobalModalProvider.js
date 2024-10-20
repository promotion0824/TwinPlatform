import { useState } from 'react'
import _ from 'lodash'
import GlobalModalContext from './GlobalModalContext'
import { SingleModalProvider } from './SingleModalProvider'

export { default as useGlobalModal } from './useGlobalModal'

export function GlobalModalProvider(props) {
  const { children } = props

  const [modals, setModals] = useState([])

  const context = {
    modals,

    open(component) {
      return new Promise((resolve) => {
        setModals((prevModals) => [
          ...prevModals,
          {
            modalId: _.uniqueId(),
            component,
            resolve,
          },
        ])
      })
    },

    close(modalId) {
      setModals((prevModals) =>
        prevModals.filter((prevModal) => prevModal.modalId !== modalId)
      )
    },
  }

  return (
    <GlobalModalContext.Provider {...props} value={context}>
      {children}
      {modals.map((modal) => (
        <SingleModalProvider
          key={modal.modalId}
          modalId={modal.modalId}
          resolve={modal.resolve}
        >
          {modal.component}
        </SingleModalProvider>
      ))}
    </GlobalModalContext.Provider>
  )
}
