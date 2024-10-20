import { useLatest } from '@willow/common'
import {
  passToFunction,
  useApi,
  useHasUnmountedRef,
  useTimer,
  useUniqueId,
} from '@willow/ui'
import Error from 'components/Error/Error'
import Progress from 'components/Progress/Progress'
import { useFetchRefresh } from 'providers/FetchRefreshProvider/FetchRefreshProvider'
import { useEffect, useState } from 'react'
import NotFound from '../../NotFound/NotFound'

export default function Fetch({
  name,
  requests,
  shouldReturnArray,
  poll,
  progress = <Progress />,
  error = <Error />,
  children,
  onResponse,
  onError,
}) {
  const api = useApi()
  const fetchId = useUniqueId()
  const fetchRefresh = useFetchRefresh()
  const hasUnmountedRef = useHasUnmountedRef()
  const timer = useTimer()

  const [state, setState] = useState({
    isLoading: true,
    responses: [],
    error: undefined,
  })

  function getResponse(responses) {
    return shouldReturnArray ? responses : responses[0]
  }

  const fetch = useLatest(async (isPolling) => {
    try {
      setState((prevState) => {
        const isLoading = prevState.error != null || !isPolling

        return {
          ...prevState,
          isLoading,
          responses: !isLoading ? prevState.responses : [],
          error: undefined,
        }
      })

      const responses = await Promise.all(
        requests.map((request) =>
          api.ajax(request.url, {
            method: request.method ?? 'get',
            params: request.params,
            body: request.body,
            headers: request.headers,
            ...(request.responseType != null
              ? { responseType: request.responseType }
              : {}),
            cache: request.cache,
            handleAbort: request.handleAbort,
            mock: request.mock,
            mockTimeout: request.mockTimeout,
          })
        )
      )

      try {
        await onResponse?.(getResponse(responses))
      } catch (err) {
        console.error(`Fetch: error raised in onResponse: ${err}`) // eslint-disable-line
      }

      if (hasUnmountedRef.current) {
        return
      }

      setState({
        isLoading: false,
        responses,
        error: undefined,
      })

      if (poll != null) {
        await timer.sleep(poll)

        fetch(true)
      }
    } catch (err) {
      try {
        await onError?.(err)
      } catch (errOnError) {
        console.error(`Fetch: error raised in onError: ${errOnError}`) // eslint-disable-line
      }

      if (hasUnmountedRef.current) {
        return
      }

      setState({
        isLoading: false,
        responses: [],
        error: err ?? {},
      })
    }
  })

  useEffect(() => {
    fetchRefresh.registerFetch({
      fetchId,
      name,
      fetch,
    })

    return () => {
      fetchRefresh.unregisterFetchId(fetchId)
    }
  }, [name])

  useEffect(() => {
    fetch()
  }, [JSON.stringify(requests)])

  if (state.isLoading && progress != null) {
    return progress
  }

  if (state.error != null && error != null) {
    return error
  }

  const notFound = requests.find(
    (request, i) => request.notFound != null && state.responses[i]?.length === 0
  )?.notFound

  if (notFound != null) {
    return <NotFound>{notFound}</NotFound>
  }

  const context = {
    isLoading: state.isLoading,
    error: state.error,
  }

  return passToFunction(children, getResponse(state.responses), context)
}
