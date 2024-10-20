import _ from 'lodash'
import { setupWorker } from 'msw'
import { makeRouteCollections } from './routes'

export default function makeWorker() {
  return setupWorker(
    ...makeRouteCollections()
      .map((item) => item.handlers)
      .flat()
  )
}
