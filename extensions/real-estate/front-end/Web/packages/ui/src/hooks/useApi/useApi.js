import { useEffect, useRef } from 'react'
import useHasUnmountedRef from 'hooks/useHasUnmountedRef/useHasUnmountedRef'
import useTimer from 'hooks/useTimer/useTimer'
import AbortError from './AbortError'
import ApiError from './ApiError'
import getOptions from './getOptions/getOptions'
import getUrl from '../../utils/getUrl'

const cache = {}
let initOptions = {}

export function initApi(options) {
  initOptions = options
}

export { ApiError }

/**
 * @deprecated please do the following instead
 * import { api } from '@willow/ui'
 */
export default function useApi() {
  const hasUnmountedRef = useHasUnmountedRef()
  const timer = useTimer()

  const requestsRef = useRef([])

  useEffect(
    () => () => {
      requestsRef.current.forEach((request) => request.abort())
    },
    []
  )

  async function ajaxRequest(url, options) {
    return new Promise(async (resolve, reject) => {
      // eslint-disable-line
      try {
        if (options.mockTimeout != null) {
          await timer.sleep(options.mockTimeout)

          if (hasUnmountedRef.current) {
            throw new AbortError()
          }
        }

        if (options.mock !== undefined) {
          resolve(options.mock)
          return
        }

        if (url == null) {
          resolve()
          return
        }

        if (options.cache && options.method === 'get') {
          if (Object.prototype.hasOwnProperty.call(cache, url)) {
            // Push cached data to act async so if an entire page has cached data, it will not be rendered sync all at once, which blocks the page.
            await timer.sleep()

            if (hasUnmountedRef.current) {
              throw new AbortError()
            }

            resolve(cache[url])

            return
          }
        }

        const request = new XMLHttpRequest()
        requestsRef.current = [...requestsRef.current, request]

        request.addEventListener('load', () => {
          requestsRef.current = requestsRef.current.filter(
            (prevRequest) => prevRequest !== request
          )

          if (hasUnmountedRef.current) {
            if (options.handleAbort) {
              reject(new AbortError())
            }

            return
          }

          const data = request.response

          if (request.status < 200 || request.status >= 300) {
            const err = new ApiError({
              url,
              status: request.status,
              data,
              response: request,
            })

            try {
              const isErrorHandled = initOptions?.onError?.(err, url)
              if (isErrorHandled) {
                return
              }
            } catch (subErr) {
              // do nothing
            }

            reject(err)

            return
          }

          if (options.cache && options.method === 'get') {
            cache[url] = data
          }

          resolve(data)
        })

        request.addEventListener('error', () => {
          requestsRef.current = requestsRef.current.filter(
            (prevRequest) => prevRequest !== request
          )

          if (hasUnmountedRef.current) {
            if (options.handleAbort) {
              reject(new AbortError())
            }

            return
          }

          reject(
            new ApiError({
              url,
              status: request.status,
              data: null,
              response: request,
            })
          )
        })

        // Handles abort event that is triggered in the cleanup function of useEffect
        request.addEventListener('abort', () => {
          if (options.handleAbort) {
            reject(new AbortError())
          }
        })

        request.open(options.method, url)

        request.responseType = options.responseType

        Object.entries(options.headers).map((entry) =>
          request.setRequestHeader(entry[0], entry[1])
        )

        request.send(options.body)
      } catch (err) {
        // If handleAbort is true, then we reject this promise with AbortError.
        if (err?.name === 'AbortError' && !options.handleAbort) {
          return
        }

        try {
          const isErrorHandled = initOptions?.onError?.(err, url)
          if (isErrorHandled) {
            return
          }
        } catch (subErr) {
          // do nothing
        }

        reject(err)
      }
    })
  }

  function ajax(url, options) {
    const nextUrl = getUrl(url, options)
    const nextOptions = getOptions(options)

    return ajaxRequest(nextUrl, nextOptions)
  }

  return {
    ajax,

    get(url, options) {
      return ajax(url, {
        ...options,
        method: 'get',
      })
    },

    post(url, body, options) {
      return ajax(url, {
        body,
        ...options,
        method: 'post',
      })
    },

    put(url, body, options) {
      return ajax(url, {
        body,
        ...options,
        method: 'put',
      })
    },

    delete(url, options) {
      return ajax(url, {
        ...options,
        method: 'delete',
      })
    },

    patch(url, body, options) {
      return ajax(url, {
        body,
        ...options,
        // Note that HTTP methods are technically case sensitive and so the
        // other methods above should probably be changed to uppercase like
        // this one. https://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html
        method: 'PATCH',
      })
    },
  }
}
