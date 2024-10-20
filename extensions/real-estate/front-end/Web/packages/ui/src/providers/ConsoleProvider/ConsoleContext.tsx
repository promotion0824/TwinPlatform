/* eslint-disable @typescript-eslint/no-explicit-any */
import { createContext, useContext } from 'react'

const ConsoleContext = createContext<
  | {
      log: (...args: any[]) => void
      warn: (...args: any[]) => void
      error: (...args: any[]) => void
      trace: (...args: any[]) => void
    }
  | undefined
>(undefined)

export default ConsoleContext

/**
 * A wrapper for the most popular methods of the `console` object, but which by
 * default does not log anything while running unit tests. If the `debug`
 * attribute from the ConsoleProvider is set to true, logs will come through
 * even in tests.
 *
 * Usage:
 *
 * const console = useConsole();
 * console.log("some stuff", 123); // or warn, error, or trace
 *
 * This will behave the same as regular console.log in the running app, but
 * will not log anything in tests (unless `debug` is set to `true` in the
 * provider).
 */
export function useConsole() {
  return useContext(ConsoleContext)
}
