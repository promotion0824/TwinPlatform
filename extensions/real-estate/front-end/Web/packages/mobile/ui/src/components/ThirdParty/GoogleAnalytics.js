import { useEffect } from 'react'
import { ga } from 'utils'
import { useConfig } from 'providers'

export default function GoogleAnalytics() {
  const config = useConfig()

  useEffect(() => {
    ga.init(config.googleAnalyticsKey)
  }, [])

  return null
}
