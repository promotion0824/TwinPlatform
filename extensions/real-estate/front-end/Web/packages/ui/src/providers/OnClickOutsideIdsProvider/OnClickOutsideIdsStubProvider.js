import { OnClickOutsideIdsContext } from './OnClickOutsideIdsContext'

export default function OnClickOutsideIdsStubProvider({ children }) {
  const context = {
    isTop() {},

    registerOnClickOutsideId() {},

    unregisterOnClickOutsideId() {},
  }

  return (
    <OnClickOutsideIdsContext.Provider value={context}>
      {children}
    </OnClickOutsideIdsContext.Provider>
  )
}
