import { ProviderRequiredError } from '@willow/common'
import { createContext, useContext } from 'react'

export const FeatureFlagContext = createContext<
  | {
      isLoaded: boolean
      hasFeatureToggle: (flagName: string) => boolean
    }
  | undefined
>(undefined)

export function useFeatureFlag() {
  const context = useContext(FeatureFlagContext)
  if (context == null) {
    throw new ProviderRequiredError('FeatureFlag')
  }
  return context
}
