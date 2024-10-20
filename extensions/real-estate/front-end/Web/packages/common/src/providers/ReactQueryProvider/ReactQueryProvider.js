import { authService } from '@willow/common'
import { QueryCache, QueryClient, QueryClientProvider } from 'react-query'
import { createWebStoragePersistor } from 'react-query/createWebStoragePersistor-experimental'
import { persistQueryClient } from 'react-query/persistQueryClient-experimental'

const queryCache = new QueryClient({
  defaultOptions: {
    queries: {
      cacheTime: 1000 * 60 * 60 * 24, // 24 hours
      refetchOnWindowFocus: false,
      retry: false,
      staleTime: 30000,
    },
  },
  queryCache: new QueryCache({
    onError: (error, query) => {
      // If we get a 401 error from our own API, redirect to login page and clear cache. Don't
      // do anything if it's a 401 from some different API, eg. Forge.
      // In addition, any api call that is persisted should not redirect to login page,
      // and this is to avoid '/me' query failure with 401 redirecting to login page which
      // will cause infinite loop.
      if (
        !query?.meta?.persist &&
        error.isAxiosError &&
        error.response.status === 401 &&
        error.request.responseURL.startsWith(
          `${window.location.protocol}//${window.location.host}`
        )
      ) {
        sessionStorage.clear()
        window.location = authService.getLoginPath()
      }
    },
  }),
})

const sessionStoragePersistor = createWebStoragePersistor({
  storage: window.sessionStorage,
})

persistQueryClient({
  dehydrateOptions: {
    shouldDehydrateQuery: (query) => query.meta?.persist,
  },
  queryClient: queryCache,
  persistor: sessionStoragePersistor,
})

export { queryCache }

export function ReactQueryProvider({ children }) {
  return (
    <QueryClientProvider client={queryCache}>{children}</QueryClientProvider>
  )
}
