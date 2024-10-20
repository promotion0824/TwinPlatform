import { createContext, useContext } from 'react'

import {
  ProviderRequiredError,
  usePagedSites,
  PagedSiteResult,
} from '@willow/common'

export type PagedPortfolioContextType = {
  enabled?: boolean /* TODO: can be removed after removing feature flag pagedPortfolioList */
  pagedSites: PagedSiteResult[]
  /** The total number of all sites that is available with the filters applied if any */
  numOfAllSitesCanBeLoaded?: number
  queryResult: ReturnType<typeof usePagedSites>
}

export const PagedPortfolioContext = createContext<
  PagedPortfolioContextType | undefined
>(undefined)

export default function usePagedPortfolio() {
  const context = useContext(PagedPortfolioContext)
  if (context == null) {
    throw new ProviderRequiredError('PagedPortfolio')
  }
  return context
}
