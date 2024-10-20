import { useMemo } from 'react'
import { FeatureFlagContext } from './FeatureFlagContext'

export default function FeatureFlagStubProvider({
  children,
  hasFeatureToggle = (_featureFlag) => false,
}) {
  const context = useMemo(
    () => ({
      hasFeatureToggle,
      isLoaded: true,
    }),
    []
  )

  return (
    <FeatureFlagContext.Provider value={context}>
      {children}
    </FeatureFlagContext.Provider>
  )
}
