import {
  UseInfiniteQueryOptions,
  UseInfiniteQueryResult,
  useInfiniteQuery,
  QueryFunctionContext,
} from 'react-query'

import { getLocation } from '@willow/common/hooks/useGetSites'
import { api } from '@willow/ui'
import { PagedSiteResult, PagedSites } from './types'

type PagedSitesPayload = {
  sortSpecifications?: [
    {
      field: string
      sort: string
    }
  ]
  filterSpecifications?: [
    {
      field: string
      operator: string
      value: string
    }
  ]
  page?: number
  pageSize: number
}

const pagedSitesUrl = '/v2/me/sites'
const pagedSitesQueryKey = 'pagedSites'
const fetchPagedSites = async (payloadData: PagedSitesPayload) => {
  const response = await api.post<PagedSites>(pagedSitesUrl, payloadData)

  return response.data
}

type QueryResult = Omit<PagedSites, 'items'> & { items: PagedSiteResult[] }
/**
 * To fetch sites and pagination data from /v2/me/sites with POST method.
 * page number starts with 1.
 */
export default function usePagedSites({
  payload,
  options,
}: {
  payload: PagedSitesPayload
  options?: UseInfiniteQueryOptions<PagedSites, unknown, QueryResult>
}): UseInfiniteQueryResult<QueryResult> {
  return useInfiniteQuery(
    [pagedSitesQueryKey],

    ({
      pageParam = payload.page ?? 1 /* page number starts with 1 */,
    }: QueryFunctionContext<string, number>) =>
      fetchPagedSites({ ...payload, page: pageParam }),

    {
      select: ({ pages, ...restResponse }) => ({
        ...restResponse,
        pages: pages.map(({ items, ...rest }) => ({
          ...rest,
          items: items.map((site) => ({
            ...site,
            location: getLocation(site.longitude, site.latitude),
          })),
        })),
      }),
      getNextPageParam: (lastPage): number | undefined =>
        lastPage.after === 0
          ? undefined
          : calcNextPageNumber(lastPage.before, payload.pageSize),

      ...options,
    }
  )
}

function calcNextPageNumber(before: number, pageSize: number) {
  // if ${before / pageSize} is not an integer, Math.min will make it to
  // include number of ${before % pageSize} sites from the previous page.
  // Which means it will result a small duplication. But this is better than missing data.
  // It is not a problem at the moment, can remove duplicated site in the select function
  // if this becomes a problem in the future.
  return Math.min(before / pageSize) + 2
}
