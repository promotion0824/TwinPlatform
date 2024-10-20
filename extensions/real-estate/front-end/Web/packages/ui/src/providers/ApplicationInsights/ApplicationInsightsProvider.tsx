import React from 'react'
import {
  ApplicationInsights,
  DistributedTracingModes,
} from '@microsoft/applicationinsights-web'
import {
  AppInsightsContext,
  ReactPlugin,
} from '@microsoft/applicationinsights-react-js'
import { cookie } from '../../utils'
import { createBrowserHistory } from 'history'
import { useConfig } from '../ConfigProvider/ConfigProvider'

type Region = 'au' | 'us' | 'eu'

/**
 * Sets up application insights using the application insights reactjs component
 * Connection string and role name are read from application configuration
 */
export default function ApplicationInsightsProvider(props) {
  const region: Region = cookie.get('api')
  const config: {
    roleName
    applicationInsights: { [Property in Region]: string }
  } = useConfig()
  const connString: string | undefined = config?.applicationInsights?.[region]

  const reactPlugin = new ReactPlugin()
  if (connString) {
    const browserHistory = createBrowserHistory()
    const appInsights = new ApplicationInsights({
      config: {
        connectionString: connString,
        extensions: [reactPlugin],
        distributedTracingMode: DistributedTracingModes.W3C,
        excludeRequestFromAutoTrackingPatterns: [
          /^.*\.mapbox\.com.*$/,
          /^.*\.segment\.io.*$/,
          /^.*\.segment\.com.*$/,
          /^.*\.zendesk\.com.*$/,
          /^.*\.configcat\.com.*$/,
          /^.*\.zdassets\.com.*$/,
          /^.*\.mixpanel\.com.*$/,
        ],
        extensionConfig: {
          [reactPlugin.identifier]: { history: browserHistory },
        },
        disableFetchTracking: false,
      },
    })
    const telemetryInitializer = (envelope) => {
      // eslint-disable-next-line no-param-reassign
      envelope.tags['ai.application.ver'] = process.env.BUILD_BUILDNUMBER
      // eslint-disable-next-line no-param-reassign
      envelope.tags['ai.cloud.role'] = config.roleName
      // eslint-disable-next-line no-param-reassign
      envelope.tags['ai.cloud.roleInstance'] = window.location.hostname
    }
    appInsights.loadAppInsights()
    appInsights.addTelemetryInitializer(telemetryInitializer)
  }

  return <AppInsightsContext.Provider {...props} value={reactPlugin} />
}
