import { ProviderRequiredError } from '@willow/common'
import { Site } from '@willow/common/site/site/types'
import { createContext, useContext } from 'react'

export const SiteContext = createContext<Site | undefined>(undefined)

export function useSite() {
  const context = useContext(SiteContext)
  if (context == null) {
    throw new ProviderRequiredError('Site')
  }
  return context
}

/**
 * the following AllSites type is used on SiteSelect.tsx dropdown component
 * to indicate user is viewing the "collection" of every sites of the portfolio
 */
export type AllSites = { id: null; name?: string }
