import { useCallback, useState, useEffect, useMemo } from 'react'
import * as configcat from 'configcat-js'
import { FeatureFlagContext } from './FeatureFlagContext'
import { useConfig } from '../config/ConfigContext'
import { useUser } from '../user/UserContext'

/**
 * Provider for ConfigCat feature flags. Almost the same as FeatureFlagProvider
 * in packages/ui (and we should factor and move this to packages/common).
 */
export default function FeatureFlagProvider({ children }) {
  const [flags, setFlags] = useState<
    { [flagName: string]: boolean } | undefined
  >()
  const { configCatKey } = useConfig()
  const { email } = useUser()

  const configCatClient = useMemo(
    () =>
      configCatKey ? configcat.createClientWithLazyLoad(configCatKey) : null,
    [configCatKey]
  )

  // Loads all toggles
  useEffect(() => {
    async function getFlagState() {
      try {
        const values = await configCatClient?.getAllValuesAsync({
          identifier: email,
          custom: {
            domain: email.split('@')[1],
          },
        })
        if (values != null) {
          setFlags(
            Object.fromEntries(
              values.map((v) => [v.settingKey, v.settingValue as boolean])
            )
          )
        }
      } catch (error) {
        // eslint-disable-next-line no-console
        console.error(error)
      }
    }
    if (configCatClient && email && configCatKey) {
      getFlagState()
    }
  }, [email, configCatKey, configCatClient])

  /**
   * Is the feature with the specified name enabled for the current user?
   */
  const hasFeatureToggle = useCallback(
    (featureName: string) => flags?.[featureName] ?? false,
    [flags]
  )

  const context = useMemo(
    () => ({
      hasFeatureToggle,
      isLoaded: flags != null,
    }),
    [hasFeatureToggle, flags]
  )

  return (
    <FeatureFlagContext.Provider value={context}>
      {children}
    </FeatureFlagContext.Provider>
  )
}
