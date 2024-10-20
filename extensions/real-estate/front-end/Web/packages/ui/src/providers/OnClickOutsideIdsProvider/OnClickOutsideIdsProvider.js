import { useState } from 'react'
import { OnClickOutsideIdsContext } from './OnClickOutsideIdsContext'

export { useOnClickOutsideIds } from './OnClickOutsideIdsContext'

export function OnClickOutsideIdsProvider({ children }) {
  const [onClickOutsideIds, setOnClickOutsideIds] = useState([])

  const context = {
    isTop(onClickOutsideId) {
      return onClickOutsideIds.slice(-1)[0] === onClickOutsideId
    },

    registerOnClickOutsideId(onClickOutsideId) {
      setOnClickOutsideIds((prevOnClickOutsideIds) => [
        ...prevOnClickOutsideIds,
        onClickOutsideId,
      ])
    },

    unregisterOnClickOutsideId(onClickOutsideId) {
      setOnClickOutsideIds((prevOnClickOutsideIds) =>
        prevOnClickOutsideIds.filter(
          (prevOnClickOutsideId) => prevOnClickOutsideId !== onClickOutsideId
        )
      )
    },
  }

  return (
    <OnClickOutsideIdsContext.Provider value={context}>
      {children}
    </OnClickOutsideIdsContext.Provider>
  )
}
