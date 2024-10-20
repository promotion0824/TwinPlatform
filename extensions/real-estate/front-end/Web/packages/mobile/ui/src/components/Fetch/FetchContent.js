import { useEffect, useRef, useState } from 'react'
import _ from 'lodash'
import { useGlobalFetch } from 'providers'
import { useApi, useUniqueId } from 'hooks'
import Error from 'components/Error/Error'
import Loader from 'components/Loader/Loader'
import { useEffectOnceMounted, useLatest } from '@willow/common'
import { FetchContext } from './FetchContext'

export default function FetchContent({
  name,
  method = 'get',
  url,
  params,
  data,
  headers,
  ignoreGlobalPrefix,
  mock,
  mockToggle,
  mockTimeout,
  cache = false,
  cancel = true,
  handleKey = true,
  poll,
  loader = <Loader />,
  error = <Error />,
  children,
  onResponse = () => {},
  onError = () => {},
}) {
  const globalFetch = useGlobalFetch()
  const fetchId = useUniqueId()
  const api = useApi()

  const pollTimerRef = useRef()
  const hasCancelledRef = useRef(false)

  const [state, setState] = useState({
    response: undefined,
    isLoading: url != null,
    error: undefined,
  })

  const latestOnResponse = useLatest(onResponse)
  const latestOnError = useLatest(onError)

  const latestFetch = useLatest(async (isPolling = false) => {
    try {
      if (url == null) {
        api.cancel()

        setState({
          response: undefined,
          isLoading: false,
          error: undefined,
        })

        return
      }

      if (!isPolling) {
        setState({
          response: undefined,
          isLoading: true,
          error: undefined,
        })
      } else if (!handleKey) {
        setState((prevState) => ({
          ...prevState,
          isLoading: true,
        }))
      }

      const response = await api.ajax(method, url, data, {
        params,
        headers,
        ignoreGlobalPrefix,
        mock,
        mockToggle,
        mockTimeout,
        cache,
        cancel,
      })

      try {
        await latestOnResponse(response)
      } catch (err) {
        console.error(`Fetch: error raised in onResponse: ${err}`) // eslint-disable-line
      }

      if (!hasCancelledRef.current) {
        setState({
          response,
          isLoading: false,
          error: undefined,
        })

        pollFetch() // eslint-disable-line
      }
    } catch (err) {
      if (!isPolling) {
        try {
          await latestOnError(err)
        } catch (errOnError) {
          console.error(`Fetch: error raised in onError: ${errOnError}`) // eslint-disable-line
        }

        if (!hasCancelledRef.current) {
          setState({
            response: undefined,
            isLoading: false,
            error: err,
          })
        }
      }
    }
  })

  function pollFetch() {
    window.clearTimeout(pollTimerRef.current)

    if (poll) {
      pollTimerRef.current = window.setTimeout(() => latestFetch(true), poll)
    }
  }

  useEffect(() => {
    latestFetch()

    globalFetch.registerFetch({ fetchId, name, fetch: latestFetch })

    return () => {
      globalFetch.unregisterFetch(fetchId)

      hasCancelledRef.current = true
      window.clearTimeout(pollTimerRef.current)
    }
  }, [])

  useEffectOnceMounted(() => {
    latestFetch(!handleKey)
  }, [url, JSON.stringify(params), JSON.stringify(data)])

  if (loader && state.isLoading) {
    return loader
  }

  if (error && state.error) {
    return error
  }

  const context = {
    response: state.response,
    isLoading: state.isLoading,
    error: state.error,

    fetch: latestFetch,
  }

  return (
    <FetchContext.Provider value={context}>
      {_.isFunction(children)
        ? children(context.response, context) // eslint-disable-line
        : children ?? null}
    </FetchContext.Provider>
  )
}
