import { useState, useEffect, useMemo } from 'react'
import * as configcat from 'configcat-js'
import { FeatureFlagContext } from './FeatureFlagContext'
import { useConfig } from '../ConfigProvider/ConfigProvider'
import { useUser } from '../UserProvider/UserProvider'

export { useFeatureFlag } from './FeatureFlagContext'

const localSettingsKey = 'configCatSettings'

// We use local storage to let Woggle 2 know what the feature flags and base values are,
// and allow it to override them. See the Woggle 2 README for more details.

function getLocalSettings() {
  const s = localStorage.getItem(localSettingsKey)
  if (s != null) {
    return JSON.parse(s)
  } else {
    return {
      baseSettings: {},
      overrideSettings: {},
    }
  }
}

function setLocalBaseSettings(nextBaseSettings) {
  localStorage.setItem(
    localSettingsKey,
    JSON.stringify({
      ...getLocalSettings(),
      baseSettings: nextBaseSettings,
    })
  )
}

export function FeatureFlagProvider({ children }) {
  const [flags, setFlags] = useState([])
  const [isLoaded, setIsLoaded] = useState(false)
  const [isError, setIsError] = useState(false)
  const { configCatKey } = useConfig()
  const { customer, email, id } = useUser()

  const configCatClient = useMemo(
    () =>
      configCatKey ? configcat.createClientWithLazyLoad(configCatKey) : null,
    [configCatKey]
  )

  // Loads all toggles
  useEffect(() => {
    async function f() {
      if (id && configCatKey) {
        try {
          const nextFlags = await configCatClient?.getAllValuesAsync({
            identifier: email,
            custom: {
              customerName: customer?.name,
              domain: email.split('@')[1],
            },
          })
          setLocalBaseSettings(nextFlags)
          setFlags(nextFlags)
          setIsLoaded(true)
        } catch (error) {
          setIsError(true)
          // eslint-disable-next-line no-console
          console.error(error)
        }
      }
    }
    f()
  }, [configCatClient, configCatKey, customer?.name, email, id])

  function hasFeatureToggle(featureName) {
    try {
      const s = getLocalSettings()
      if (featureName in s.overrideSettings) {
        return s.overrideSettings[featureName]
      } else {
        return (
          flags.find((f) => f.settingKey === featureName)?.settingValue ?? false
        )
      }
    } catch (error) {
      // eslint-disable-next-line no-console
      console.error(error)
    }
    return false
  }

  const context = {
    hasFeatureToggle,
    isLoaded,
    isError,
  }

  return (
    <FeatureFlagContext.Provider value={context}>
      {children}
    </FeatureFlagContext.Provider>
  )
}
