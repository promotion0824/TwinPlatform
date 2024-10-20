import { createContext, useContext } from 'react'

export const FeatureFlagContext = createContext()

export function useFeatureFlag() {
  return useContext(FeatureFlagContext)
}
