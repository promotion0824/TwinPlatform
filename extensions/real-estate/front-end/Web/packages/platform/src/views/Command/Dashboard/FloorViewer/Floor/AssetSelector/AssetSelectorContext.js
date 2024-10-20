import { createContext, useContext } from 'react'

export const AssetSelectorContext = createContext()

export function useAssetSelector() {
  return useContext(AssetSelectorContext)
}
