import { useState } from 'react'
import { GlobalFetchContext } from './GlobalFetchContext'

export default function GlobalFetchProvider(props) {
  const [fetches, setFetches] = useState([])

  const context = {
    registerFetch(fetch) {
      setFetches((prevFetches) => [...prevFetches, fetch])
    },

    unregisterFetch(fetchId) {
      setFetches((prevFetches) =>
        prevFetches.filter((prevFetch) => prevFetch.fetchId !== fetchId)
      )
    },

    refresh(name, polling = false) {
      fetches
        .filter((fetch) => fetch.name === name)
        .forEach((fetch) => fetch.fetch(polling))
    },
  }

  return <GlobalFetchContext.Provider {...props} value={context} />
}
