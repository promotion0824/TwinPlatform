import { useContext, useEffect, useRef, useState } from 'react'
import { useTransition } from 'hooks'
import useGlobalModal from './useGlobalModal'
import SingleModalContext from './SingleModalContext'

export function useSingleModal() {
  return useContext(SingleModalContext)
}

export function SingleModalProvider(props) {
  const { modalId, resolve } = props

  const globalModal = useGlobalModal()

  const [isOpen, setIsOpen] = useState(false)
  const responseRef = useRef()

  useEffect(() => {
    setIsOpen(true)
  }, [])

  const transition = useTransition({
    isOpen,

    onClose() {
      globalModal.close(modalId)

      resolve(responseRef.current)
    },
  })

  function close(response) {
    responseRef.current = response
    setIsOpen(false)
  }

  function closeIfTopModal() {
    if (transition.status === 'entered') {
      const isTopModal = globalModal.modals.slice(-1)[0]?.modalId === modalId
      if (isTopModal) {
        close()
      }
    }
  }

  const context = {
    transition,

    open: globalModal.open,
    close,
    closeIfTopModal,
  }

  return <SingleModalContext.Provider {...props} value={context} />
}
