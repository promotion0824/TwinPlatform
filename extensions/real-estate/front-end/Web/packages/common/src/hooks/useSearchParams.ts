// Copy-pasted from React Router 6, so can be replaced when upgraded.
//
// See this for usage
// https://reactrouter.com/docs/en/v6/api#usesearchparams
//
// The following changes were made:
// - navigate replaced with history.push
// - ts was removed - can add back in later with a re-copy-paste

import React from 'react'
import { useHistory, useLocation } from 'react-router'

function warning(cond, message) {
  if (!cond) {
    // eslint-disable-next-line no-console
    if (typeof console !== 'undefined') console.warn(message)

    try {
      // Welcome to debugging React Router!
      //
      // This error is thrown as a convenience so you can more easily
      // find the source for a warning that appears in the console by
      // enabling "pause on exceptions" in your JavaScript debugger.
      throw new Error(message)
      // eslint-disable-next-line no-empty
    } catch (e) {}
  }
}

/**
 * Creates a URLSearchParams object using the given initializer.
 *
 * This is identical to `new URLSearchParams(init)` except it also
 * supports arrays as values in the object form of the initializer
 * instead of just strings. This is convenient when you need multiple
 * values for a given key, but don't want to use an array initializer.
 *
 * **Note for JS users**: this function won't filter out falsy values
 * for example:
 * `{ modelId: undefined }` will be converted to `"?modelId=undefined"`.
 * Use _.pickBy if you need to filter out falsy values.
 *
 * For example, instead of:
 *
 *   let searchParams = new URLSearchParams([
 *     ['sort', 'name'],
 *     ['sort', 'price']
 *   ]);
 *
 * you can do:
 *
 *   let searchParams = createSearchParams({
 *     sort: ['name', 'price']
 *   });
 */
export function createSearchParams(
  init:
    | string
    | URLSearchParams
    | string[][]
    | Record<string, string | string[]> = ''
): URLSearchParams {
  return new URLSearchParams(
    typeof init === 'string' ||
    Array.isArray(init) ||
    init instanceof URLSearchParams
      ? init
      : Object.keys(init).reduce((memo: [string, string][], key) => {
          const value = init[key]
          return memo.concat(
            Array.isArray(value) ? value.map((v) => [key, v]) : [[key, value]]
          )
        }, [])
  )
}

/**
 * A convenient wrapper for reading and writing search parameters via the
 * URLSearchParams interface.
 */
export default function useSearchParams(defaultInit = '') {
  warning(
    typeof URLSearchParams !== 'undefined',
    `You cannot use the \`useSearchParams\` hook in a browser that does not ` +
      `support the URLSearchParams API. If you need to support Internet ` +
      `Explorer 11, we recommend you load a polyfill such as ` +
      `https://github.com/ungap/url-search-params\n\n` +
      `If you're unsure how to load polyfills, we recommend you check out ` +
      `https://polyfill.io/v3/ which provides some recommendations about how ` +
      `to load polyfills only for users that need them, instead of for every ` +
      `user.`
  )

  const defaultSearchParamsRef = React.useRef(createSearchParams(defaultInit))

  const location = useLocation()
  const searchParams = React.useMemo(() => {
    const searchParams = createSearchParams(location.search)

    for (let key of defaultSearchParamsRef.current.keys()) {
      if (!searchParams.has(key)) {
        defaultSearchParamsRef.current.getAll(key).forEach((value) => {
          searchParams.append(key, value)
        })
      }
    }

    return searchParams
  }, [location.search])

  const history = useHistory()
  const setSearchParams = React.useCallback(
    (
      nextInit,
      navigateOptions // { replace?: boolean; state?: any }
    ) => {
      const navigate = navigateOptions?.replace ? history.replace : history.push
      navigate('?' + createSearchParams(nextInit), navigateOptions?.state)
    },
    [history]
  )

  return [searchParams, setSearchParams]
}
