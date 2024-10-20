import { useState } from 'react'
import { ModalsContext } from './ModalsContext'

export default function ModalsProvider({ children }) {
  const [modalIds, setModalIds] = useState([])

  const context = {
    isTop(modalId) {
      return modalIds.slice(-1)[0] === modalId
    },

    registerModalId(modalId) {
      setModalIds((prevIds) => [...prevIds, modalId])
    },

    unregisterModalId(modalId) {
      setModalIds((prevModalIds) =>
        prevModalIds.filter((prevModalId) => prevModalId !== modalId)
      )
    },
  }

  return (
    <ModalsContext.Provider value={context}>{children}</ModalsContext.Provider>
  )
}
