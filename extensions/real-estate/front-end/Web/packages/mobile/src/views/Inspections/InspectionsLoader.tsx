/* eslint-disable no-await-in-loop */
import { useEffect, useState } from 'react'
import { useApi } from '@willow/mobile-ui/hooks'
import { useFeatureFlag } from '../../../ui/src/providers/FeatureFlagProvider/FeatureFlagContext'

/**
 * If the `inspectionsOfflineMode` feature flag is enabled, fetch all the
 * latest inspections data into the cache.
 */
export default function InspectionsLoader() {
  const api = useApi()
  const featureFlags = useFeatureFlag()
  const [hasRegistration, setHasRegistration] = useState(false)

  // eslint-disable-next-line complexity
  // (Note these loops will become a single request soon)
  useEffect(() => {
    async function sync() {
      if (featureFlags.isLoaded) {
        const wantsRegistration = featureFlags.hasFeatureToggle(
          'inspectionsOfflineMode'
        )
        if (wantsRegistration && !hasRegistration) {
          const me = await getJson(api, `/api/me`)
          for (const s of me.sites) {
            const zones = await getJson(
              api,
              `/api/sites/${s.id}/inspectionZones`
            )
            for (const z of zones) {
              const inspections = await getJson(
                api,
                `/api/sites/${s.id}/inspectionZones/${z.id}/inspections`
              )
              for (const inspection of inspections) {
                await getJson(
                  api,
                  `/api/sites/${s.id}/inspections/${inspection.id}/lastRecord`
                )
              }
            }
          }
          setHasRegistration(true)
        }
      }
    }
    sync()
  }, [featureFlags, api, hasRegistration])

  return null
}

/**
 * Get the JSON content from `url` and store it in the persistent cache.
 */
function getJson(api: ReturnType<typeof useApi>, url: string) {
  return api.get(url, undefined, { cache: true })
}
