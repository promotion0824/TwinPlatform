import { useState } from 'react'
import cookie from 'utils/cookie'
import { useSnackbar } from 'providers/snackbar/SnackbarContext'
import { useApi } from 'hooks'
import { ConfigContext } from './ConfigContext'

export default function ConfigProvider(props) {
  const { configPrefix } = props

  const snackbar = useSnackbar()
  const api = useApi()

  const [config, setConfig] = useState({})

  const context = {
    ...config,

    async loadConfig() {
      let featureToggles = {}
      try {
        const cookies = cookie.parse()
        featureToggles = Object.keys(cookies)
          .filter((key) => key.startsWith(configPrefix))
          .reduce(
            (acc, key) => ({
              ...acc,
              [key]: cookies[key],
            }),
            {}
          )
      } catch (err) {
        console.error('An error has occurred parsing feature toggle cookies') // eslint-disable-line
      }

      try {
        const nextConfig = await api.get(
          '/mobile-web/public/config.json',
          null,
          {
            ignoreGlobalPrefix: true,
          }
        )

        const combinedConfig = {
          ...nextConfig,
          // Convert to a boolean from the original environment variable source
          isSingleTenant: nextConfig.isSingleTenant === 'true',
          ...featureToggles,
        }

        setConfig(combinedConfig)

        return combinedConfig
      } catch (err) {
        snackbar.show('An error has occurred loading application config.')

        return config
      }
    },

    hasFeatureToggle(featureToggle) {
      return config[featureToggle] === true
    },
  }

  return <ConfigContext.Provider {...props} value={context} />
}
