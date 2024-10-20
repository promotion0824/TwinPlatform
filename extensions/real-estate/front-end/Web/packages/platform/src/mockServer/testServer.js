import { setupServer } from 'msw/node'
import { makeRouteCollections } from './routes'
import { withoutRegion } from './utils'

/* eslint-disable import/prefer-default-export */
export function setupTestServer() {
  const handlerItems = makeRouteCollections()
  const server = setupServer(
    ...handlerItems
      .map((item) => item.handlers)
      .flat()
      .map(withoutRegion)
  )
  return {
    server,
    reset: () => {
      for (const item of handlerItems) {
        item.reset()
      }
    },
  }
}
