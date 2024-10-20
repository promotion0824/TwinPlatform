/* eslint-disable no-console, @typescript-eslint/no-unused-vars, @typescript-eslint/no-empty-function */
import { useMemo } from 'react'
import ConsoleContext from './ConsoleContext'

const isTesting =
  typeof process !== 'undefined' && process.env?.NODE_ENV === 'test'

function noop(..._args) {}

/**
 * A wrapper for the most popular methods of the `console` object, but which by
 * default does not log anything while running unit tests. Unit tests can set the
 * `debug` prop to `true` to force logs to come through.
 */
export default function ConsoleProvider({
  debug = false,
  children,
}: {
  /**
   * Do logging, even if running tests
   */
  debug: boolean
  children: JSX.Element
}) {
  const context = useMemo(() => {
    if (!isTesting || debug) {
      return {
        log: console.log,
        warn: console.warn,
        error: console.error,
        trace: console.trace,
      }
    } else {
      return {
        log: noop,
        warn: noop,
        error: noop,
        trace: noop,
      }
    }
  }, [debug])

  return (
    <ConsoleContext.Provider value={context}>
      {children}
    </ConsoleContext.Provider>
  )
}
