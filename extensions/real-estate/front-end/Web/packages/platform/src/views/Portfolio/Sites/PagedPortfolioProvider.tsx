import { ReactNode } from 'react'
import { last } from 'lodash'

import { usePagedSites } from '@willow/common'
import { useFeatureFlag } from '@willow/ui'
import {
  PagedPortfolioContext,
  PagedPortfolioContextType,
} from './PagedPortfolioContext'

export default function PagedPortfolioProvider({
  children,
  enabled = false,
}: {
  children: ReactNode
  enabled?: boolean /* TODO: can be removed after removing feature flag pagedPortfolioList */
}) {
  // no need to config the options at the moment,
  // can make it a function if it's required in the future.
  const pagedPortfolioQuery = usePagedSites({
    payload: {
      page: 1,
      pageSize: 10,
    },
    options: {
      // query will be disabled when create the provider,
      // this is a temp control as it will be always true after we finish
      // the work for paginated portfolio
      enabled,
    },
  })

  // No need to memoize as it either won't be changed if the user doesn't scroll,
  // or will need to be recalculated if new sites are loaded.
  const pagedSites =
    pagedPortfolioQuery.data?.pages.flatMap(({ items }) => items) ?? []
  const numOfAllSitesCanBeLoaded =
    pagedPortfolioQuery.data?.pages[
      (last<number | undefined>(
        // no way to type pageParams in useInfiniteQuery;
        // and the first pageParams will always be undefined:
        // https://github.com/TanStack/query/discussions/1606;
        // pageParams starts as 1.
        pagedPortfolioQuery.data?.pageParams as (number | undefined)[]
      ) ?? 1) - 1
    ].total

  const context: PagedPortfolioContextType = {
    enabled,
    queryResult: pagedPortfolioQuery,
    pagedSites,
    numOfAllSitesCanBeLoaded,
  }

  return (
    <PagedPortfolioContext.Provider value={context}>
      {children}
    </PagedPortfolioContext.Provider>
  )
}

export const usePagedPortfolioListFeatureFlag = () => {
  const featureFlags = useFeatureFlag()

  return featureFlags.hasFeatureToggle('pagedPortfolioList')
}
