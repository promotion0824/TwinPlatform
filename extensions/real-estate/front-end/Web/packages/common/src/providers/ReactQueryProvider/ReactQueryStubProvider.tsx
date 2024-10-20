import { ReactNode, useRef } from 'react'
import { QueryClient, QueryClientProvider, setLogger } from 'react-query'

setLogger({
  log: console.log,
  warn: console.warn,
  error: () => {
    // Do nothing. We suppress React Query error logging during tests because
    // sometimes tests unexpectedly run queries after a test is complete, which
    // will fail the whole test run even if all the tests succeeded.
  },
})

/**
 * Builds the QueryClient and QueryClient Provider for each usage so that test
 * is completely isolated from other tests.
 * Find out more from https://react-query.tanstack.com/guides/testing
 */
const ReactQueryStubProvider = ({ children }: { children?: ReactNode }) => {
  const { current: queryClient } = useRef(
    new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    })
  )
  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

export default ReactQueryStubProvider
