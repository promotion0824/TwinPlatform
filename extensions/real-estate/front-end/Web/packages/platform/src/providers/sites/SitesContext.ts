import { ProviderRequiredError } from '@willow/common'
import { Site } from '@willow/common/site/site/types'
import { createContext, useContext } from 'react'

export const SitesContext = createContext<Site[] | undefined>(undefined)

export function useSites() {
  const context = useContext(SitesContext)
  if (context == null) {
    throw new ProviderRequiredError('Sites')
  }
  return context
}
