import React from 'react'
import { useInfiniteQuery, useQuery } from 'react-query'
import tw from 'twin.macro'
import Tail from 'components/Tail/Tail'
import Text from 'components/Text/Text'
import Icon from 'components/Icon/Icon'
import Flex from 'components/Flex/Flex'
import { useIntersectionObserverRef } from '@willow/ui'
import cookie from 'utils/cookie'
import { useTranslation } from 'react-i18next'
import fetchAssets from '../../utils/fetchAssets.ts'

function getApiPrefix() {
  // If we are on platform, the prefix is stored in
  // `packages/ui/src/hooks/useApi/getUrl.js`, if we are on mobile, the prefix
  // is stored in `packages/mobile/ui/src/hooks/useApi/useApi.js`. However in
  // both cases it is retrieved from this cookie, so we use that as a reliable
  // way to get it.
  return cookie.get('api')
}

/**
 * Render a list of assets retrieved from the api.
 *
 * Assets are retrieved in pages. When the last asset in the list is visible,
 * more items will be retrieved.
 *
 * The `AssetComponent` prop is required and must be a component that accepts a
 * prop called `asset` and a ref. The ref is used to detect when the final item
 * is visible.
 *
 * `params` is also required and represents the query parameters that are sent
 * to the assets API endpoint. At the minimum, it must include a `siteId`
 * attribute, but may additionally include any arguments the assets route
 * accepts, for example `categoryId`, `searchKeyword`, `floorCode`.
 *
 * `getObservableElements` is passed through to a `useIntersectionObserverRef`.
 * Use it if your `AssetComponent` returns an element with `display: contents`
 * (see the documentation for `useIntersectionObserverRef`);
 *
 * Note: almost none of the logic in this component is specific to assets. In
 * the very near future we could generalise this to something like
 * `FetchedItemList`.
 *
 * This component assumes that the endpoint returns a JSON payload which is
 * an array of items. We stop fetching when we receive an empty array.
 */
export default function AssetsList({
  AssetComponent,
  params,
  getObservableElements,
}) {
  const { t } = useTranslation()

  // To reduce load on the server, we require that either a category or
  // floor code be specified, or the search keyword length is at least
  // three characters.
  const avoidingQuery =
    params.categoryId == null &&
    params.floorCode == null &&
    (params.searchKeyword == null || params.searchKeyword.length < 3)

  // We control the page size so we can check the number of returned results
  // against it in order to determine whether to fetch another page (since the
  // backend does not currently tell us whether or not there are more results).
  // We keep retrieving pages as long as there is a complete page of results
  // returned. In the case where a complete page is returned *and* there are no
  // more items this means we will do one redundant request but there's nothing
  // we can do about that from here.
  const pageSize = 100

  // The backend ignores pageNumber / pageSize params for non-ADT sites, so
  // the only way to know if we should try to retrieve additional pages is to
  // separately query the site to find out if it's an ADT site.
  const siteRequest = useQuery(
    ['site', params.siteId],
    async () => {
      // We use a raw `fetch` here instead of useApi because useApi is inconsistent
      // between platform & mobile as to whether the prefix should be prepended
      // to the path passed into it.
      return (
        await fetch(`/${getApiPrefix()}/api/sites/${params.siteId}`)
      ).json()
    },
    [params.siteId]
  )

  // React-Query uses `pageParam`, the API uses `pageNumber`.
  // Note that the assets list endpoint considers the first page to be page 0,
  // which is inconsistent with the tickets list API, where the first page is
  // page 1. Be careful around this if generalising.
  const doFetch = async ({ pageParam = 0, signal }) => {
    if (avoidingQuery) {
      return Promise.resolve([])
    }
    return fetchAssets(
      {
        ...params,
        pageNumber: pageParam,
        pageSize,
      },
      { signal }
    )
  }

  const reactQueryKey = ['assets', params]

  const {
    data,
    error,
    fetchNextPage,
    hasNextPage,
    isFetching,
    isFetchingNextPage,
    refetch,
  } = useInfiniteQuery(reactQueryKey, doFetch, {
    getNextPageParam: (lastPage, pages) => {
      if (lastPage.length < pageSize) {
        // If we didn't receive a full page of items, that means we're finished.
        return undefined
      }
      // Otherwise, increment the page number. We don't have direct access to
      // the current page number here, but we know that the next page number to
      // get is the current number of pages we have retrieved.
      return pages.length
    },
    enabled: siteRequest.data != null,
  })

  const lastElementRef = useIntersectionObserverRef(
    {
      onView: fetchNextPage,
      getObservableElements,
    },
    [data]
  )

  const hasItems = data?.pages.some((page) => page.length > 0)

  return (
    <>
      {data &&
        data.pages.map((page, pageIndex) => {
          return (
            // eslint-disable-next-line react/no-array-index-key
            <React.Fragment key={pageIndex}>
              {page.map((asset, assetIndex) => {
                return (
                  <AssetComponent
                    asset={asset}
                    key={asset.id}
                    ref={
                      pageIndex === data.pages.length - 1 &&
                      assetIndex === page.length - 1
                        ? lastElementRef
                        : null
                    }
                  />
                )
              })}
            </React.Fragment>
          )
        })}

      {/*
        The server applies (*) filtering after applying pagination, so if none
        of the items in the first page match the query, the server will return
        an empty list (and a continuation token) even if there will be items
        that match the query. When this happens, there is no "last" element to
        attach the listener to, so we create a dummy at the end and listen to
        that. Perhaps we could *just* have this one element at the end and not
        worry about attaching listeners to the last element, but it does seem
        nice to fetch when any part of the last element is visible as opposed
        to the an element beyond the end of the last element.

        (*) this is no longer true, so this block of code is now entirely
        defensive.
       */}
      {data && !hasItems && !isFetching && !isFetchingNextPage && (
        <>
          {hasNextPage ? (
            <Flex ref={lastElementRef} />
          ) : !avoidingQuery ? (
            <div tw="mt-16">
              <Flex align="center middle" size="medium">
                <Icon icon="warning" color="inherit" size="large" />
                <Text type="message">{t('plainText.noAssetsFound')}</Text>
              </Flex>
            </div>
          ) : null}
        </>
      )}

      <Tail
        isLoading={isFetching || isFetchingNextPage}
        isError={error}
        onRetry={refetch}
      />
    </>
  )
}
