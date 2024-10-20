import { OnClickOutsideIdsContext } from './OnClickOutsideIdsContext'

/**
 * Stub version of OnClickOutsideIdsProvider
 */
export default function OnClickOutsideIdsProvider({ children }) {
  const context = {
    isTop: (onClickOutsideId) => false,
    registerOnClickOutsideId: (onClickOutsideId) => {},
    unregisterOnClickOutsideId: (onClickOutsideId) => {},
  }

  return (
    <OnClickOutsideIdsContext.Provider value={context}>
      {children}
    </OnClickOutsideIdsContext.Provider>
  )
}
