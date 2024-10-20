import { ConfigContext } from './ConfigContext'

export default function ConfigStubProvider({
  children,
  hasFeatureToggle = () => true,
}) {
  const context = {
    hasFeatureToggle,
  }

  return (
    <ConfigContext.Provider value={context}>{children}</ConfigContext.Provider>
  )
}
