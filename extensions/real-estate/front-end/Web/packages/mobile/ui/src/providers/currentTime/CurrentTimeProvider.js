import { useInterval } from '@willow/common'
import { useState } from 'react'
import { CurrentTimeContext } from './CurrentTimeContext'

export default function CurrentTimeProvider(props) {
  const [currentTime, setCurrentTime] = useState(new Date().toISOString())

  useInterval(() => {
    setCurrentTime(new Date().toISOString())
  }, 1000)

  return <CurrentTimeContext.Provider {...props} value={currentTime} />
}
