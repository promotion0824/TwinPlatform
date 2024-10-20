import { useEffect } from 'react'
import { ga } from '@willow/ui'
import { useConfig } from '@willow/ui'

export default function GoogleAnalytics() {
  const config = useConfig()

  useEffect(() => {
    ga.init(config.googleAnalyticsKey)
  }, [])

  return null
}
