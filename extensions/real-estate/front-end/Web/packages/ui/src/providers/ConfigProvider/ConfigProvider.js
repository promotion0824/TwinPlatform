import { useState } from 'react'
import cookie from 'utils/cookie'
import { useSnackbar } from 'providers/SnackbarProvider/SnackbarContext'
import { useApi } from '@willow/ui'
import { ConfigContext } from './ConfigContext'

export { useConfig } from './ConfigContext'

export function ConfigProvider({ configPrefix, children }) {
  const api = useApi()
  const snackbar = useSnackbar()

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
        const nextConfig = await api.get('/public/config.json', {
          ignoreGlobalPrefix: true,
        })

        const combinedConfig = {
          ...nextConfig,
          // Convert to a boolean from the original environment variable source
          isSingleTenant: nextConfig.isSingleTenant === 'true',
          hasMobilePwa: nextConfig.hasMobilePwa === 'true',
          ...featureToggles,
        }

        if (combinedConfig.hasMobilePwa) {
          // If the customer has the mobile PWA enabled, create a PWA manifest file
          // pointing to and set the the href of the <link rel="manifest"> element
          // (which we created in the index.html) to a data: url containing the manifest
          // content. This is a neat Javascript-only way to conditionally set a PWA manifest.
          const baseUrl = `${location.protocol}//${location.host}`
          const mobileAppUrl = `${baseUrl}/mobile-web`
          const manifest = {
            name: 'Willow Mobile',
            short_name: 'Willow Mobile',
            start_url: mobileAppUrl + '/',
            display: 'standalone',
            background_color: '#000',
            description: 'Command Mobile',
            icons: [
              {
                src: `${mobileAppUrl}/public/pwa-icon-large.png`,
                sizes: '256x256',
              },
            ],
          }

          const blob = new Blob([JSON.stringify(manifest)], {
            type: 'application/json',
          })
          const manifestURL = URL.createObjectURL(blob)
          document
            .getElementById('willow-manifest-placeholder')
            .setAttribute('href', manifestURL)
        }
        setConfig(combinedConfig)

        return combinedConfig
      } catch (err) {
        snackbar.show('An error has occurred loading application config')

        return config
      }
    },

    hasFeatureToggle(featureToggle) {
      return config[featureToggle] === true
    },

    language:
      window.localStorage.getItem('i18nextLng') ||
      window.navigator.language ||
      'en',
  }

  return (
    <ConfigContext.Provider value={context}>{children}</ConfigContext.Provider>
  )
}
