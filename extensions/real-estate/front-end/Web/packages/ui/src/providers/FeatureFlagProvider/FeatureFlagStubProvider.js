import { FeatureFlagContext } from './FeatureFlagContext'

export default function FeatureFlagStubProvider({
  children,
  hasFeatureToggle = (_feature) => true,
  featureFlagsLoaded = true,
  errorOnLoad = false,
}) {
  const context = {
    hasFeatureToggle,
    isLoaded: featureFlagsLoaded,
    isError: errorOnLoad,
  }

  return (
    <FeatureFlagContext.Provider value={context}>
      {children}
    </FeatureFlagContext.Provider>
  )
}
