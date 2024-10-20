/* eslint-disable class-methods-use-this, max-classes-per-file */
import axios from 'axios'
import _ from 'lodash'
import qs from 'qs'
import { useEffect, useRef } from 'react'
import { getApiGlobalPrefix } from '../../utils/'
import makeRequest from './makeRequest'

function getFormData(data) {
  const formData = new FormData()

  Object.keys(data).forEach((key) => {
    const value = data[key]
    if (value != null) {
      if (_.isArray(value)) {
        value.forEach((item) => {
          formData.append(key, item)
        })
      } else {
        formData.append(key, value)
      }
    }
  })

  return formData
}

/**
 * A basic wrapper around an Application Cache.
 */
class Cache {
  constructor(cacheName) {
    this.cacheName = cacheName
  }

  /**
   * Persist the Axios response to the cache using the url as a key. The
   * Application Cache expects `Response` objects so we convert the Axios
   * response to a `Response` in a rudimentary way. This method accepts an
   * Axios response rather than a `Response` because in testing, the `Response`
   * class does not exist. So by only creating the `Response` object here,
   * `useApi` can use the `DummyCache` in testing and doesn't need to worry
   * about the `Response` class.
   */
  async put(url, axiosResponse) {
    const cache = await caches.open(this.cacheName)
    return cache.put(
      url,
      new Response(JSON.stringify(axiosResponse.data), {
        headers: axiosResponse.headers,
        status: axiosResponse.status,
      })
    )
  }

  /**
   * Gets the parsed JSON content of the request by url. Returns `undefined` if
   * there is no match.
   */
  async get(url) {
    const cache = await caches.open(this.cacheName)
    const match = await cache.match(url)
    if (match != null) {
      const text = await match.text()
      return JSON.parse(text)
    }
  }

  /**
   * Deletes the whole cache.
   */
  clear() {
    return caches.delete(this.cacheName)
  }
}

/**
 * In testing, neither `window.caches` nor the `Response` class exist (and we
 * wouldn't want tests to persist stuff anyway). So `DummyCache` follows the
 * same interface as `Cache` but doesn't do anything.
 */
class DummyCache {
  put(url, axiosResponse) {
    return Promise.resolve()
  }

  async get(url) {
    return Promise.resolve()
  }

  async clear() {
    return Promise.resolve()
  }
}

export default function useApi({ cancelable } = {}) {
  const timerRef = useRef()
  const cancelTokenRef = useRef()
  const hasCancelledRef = useRef(false)

  useEffect(
    () => () => {
      hasCancelledRef.current = true
      window.clearTimeout(timerRef.current)
      cancelTokenRef.current?.cancel()
    },
    []
  )

  const refreshToken = (config) => {
    const refreshTokenUrl = !config.ignoreGlobalPrefix
      ? `${globalPrefix}${'/api/refreshSession'}`
      : '/api/refreshSession'

    return makeRequest('post', refreshTokenUrl, {}, config)
  }

  const globalPrefix = getApiGlobalPrefix()

  const api = {
    // eslint-disable-next-line complexity
    async ajax(method, url, data, config = {}) {
      const derivedUrl = !config.ignoreGlobalPrefix
        ? `${globalPrefix}${url}`
        : url

      const {
        cache: useCache = false,
        cancel = method === 'get' &&
          config.cancelable !== false &&
          cancelable !== false,
        ...axiosConfig
      } = config

      const cache =
        typeof window.caches !== 'undefined'
          ? new Cache('apiCache')
          : new DummyCache()

      return new Promise((resolve, reject) => {
        try {
          const derivedParams =
            method === 'get' ? config.params ?? data : config.params

          window.clearTimeout(timerRef.current)
          cancelTokenRef.current?.cancel()
          cancelTokenRef.current = axios.CancelToken.source()

          let derivedData = data
          const isFormUrlEncoded = config.headers?.[
            Object.keys(config.headers).find(
              (header) => header.toLowerCase() === 'content-type'
            )
          ]?.includes('application/x-www-form-urlencoded')
          const isMultipartFormData = config.headers?.[
            Object.keys(config.headers).find(
              (header) => header.toLowerCase() === 'content-type'
            )
          ]?.includes('multipart/form-data')

          if (isFormUrlEncoded) {
            derivedData = qs.stringify(data)
          } else if (isMultipartFormData) {
            derivedData = getFormData(data)
          }

          const derivedConfig = {
            cancelToken: cancel ? cancelTokenRef.current.token : undefined,
            ...axiosConfig,
            params: derivedParams,
            paramsSerializer(params) {
              return qs.stringify(params, { arrayFormat: 'repeat' })
            },
          }

          /**
           * Try the request. If `retryIf401` is true, and the response is a
           * 401, refresh the access token and try again once.
           */
          // eslint-disable-next-line no-inner-declarations
          function tryRequest(retryIf401) {
            makeRequest(method, derivedUrl, derivedData, derivedConfig)
              .then((response) => {
                if (useCache) {
                  cache.put(url, response)
                }
                resolve(response.data)
              })
              .catch((err) => {
                if (api.isCancel(err)) {
                  return // eslint-disable-line no-useless-return
                } else if (
                  err.response?.status === 401 &&
                  !window.location.pathname.startsWith('/account') &&
                  url !== '/api/auth/user' &&
                  url !== '/api/me'
                ) {
                  if (retryIf401) {
                    refreshToken(derivedConfig)
                      .then(() => {
                        tryRequest(false)
                      })
                      .catch(() => {
                        handleAuthFail()
                      })
                  } else {
                    handleAuthFail()
                  }
                } else if (useCache) {
                  // If we tried a network request and it didn't work, try the
                  // cache. If we have a cache hit, return the result from the
                  // cache and everything is ok. If the cache match is not
                  // successful, return the original error.
                  cache.get(url).then(
                    (result) => {
                      if (result != null) {
                        resolve(result)
                      } else {
                        reject(err)
                      }
                    },
                    () => {
                      reject(err)
                    }
                  )
                } else {
                  reject(err)
                }
              })
          }

          // eslint-disable-next-line no-inner-declarations
          function handleAuthFail() {
            // The cache includes the response from `/api/me`.
            // UserProvider makes this request on loading the page to
            // determine whether the user is logged in. But we aren't
            // logged in (since we just got a 401). So we need to clear
            // the cache, otherwise UserProvider will erroneously think
            // we are still logged in, and then the app will make some
            // other request which will return 401 again and we will get
            // into an infinite redirect loop.
            cache.clear().then(
              () => {},
              (e) => {
                // eslint-disable-next-line no-console
                console.error('Failed to clear cache', e)
              }
            )
            window.location = '/mobile-web/account/login'
          }

          tryRequest(true)
        } catch (err) {
          if (!api.isCancel(err)) {
            reject(err)
          }
        }
      })
    },

    get(url, data, config) {
      return api.ajax('get', url, data, config)
    },

    post(url, data, config) {
      return api.ajax('post', url, data, config)
    },

    put(url, data, config) {
      return api.ajax('put', url, data, config)
    },

    delete(url, data, config) {
      return api.ajax('delete', url, data, config)
    },

    cancel() {
      cancelTokenRef.current?.cancel()
    },

    isCancel(err) {
      return axios.isCancel(err)
    },
  }

  return api
}
