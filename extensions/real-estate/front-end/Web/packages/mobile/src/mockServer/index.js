import { setupWorker } from 'msw'
import * as inspections from './inspections'

/**
 * Sets up mock service worker with list of restful handlers.
 * (Adapted from platform/mockServer for consistency)
 */
export default function makeWorker() {
  return setupWorker(...inspections.handlers)
}
