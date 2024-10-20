import { useEffect, useState } from 'react'
import { useHistory, useLocation } from 'react-router'
import _ from 'lodash'
import { useEffectOnceMounted } from '@willow/common'

export default function useQueryStringState(getState, getUrl) {
  const history = useHistory()
  const location = useLocation()

  const [state, setState] = useState(() => getState())

  useEffect(() => {
    history.replace(getUrl(state))
  }, [])

  useEffectOnceMounted(() => {
    const nextUrl = getUrl(state)

    if (location.search.slice(1) !== nextUrl.split('?')[1]) {
      history.push(nextUrl)
    }
  }, [state])

  useEffectOnceMounted(() => {
    const nextState = getState()

    if (!_.isEqual(state, nextState)) {
      setState(nextState)
    }
  }, [location])

  return [state, setState]
}
