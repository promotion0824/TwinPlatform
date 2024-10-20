import { createContext, useContext } from 'react'

export const AssetModalContext = createContext()

export function useAssetModal() {
  return useContext(AssetModalContext)
}
