import tw from 'twin.macro'
import axios from 'axios'
import { useQuery, UseQueryOptions } from 'react-query'
import { Icon, Select, Progress, Option, getUrl, useForm } from '@willow/ui'
import { useTranslation } from 'react-i18next'

/**
 * Makes a request to PortalXL to get the IDs and titles of available ArcGIS
 * maps. If the request succeeds, renders a dropdown to allow the user to
 * choose which one they want to use for the site. If the `isArcGisEnabled`
 * feature is not enabled for the site, we don't display anything.
 *
 * This component must be used from within a @willow/ui `Form`. It binds to the
 * `webMapId` field in the form.
 */
export default function ArcGisMapSelector({ siteId }: { siteId: string }) {
  const form = useForm()
  const enabled = form?.data?.features?.isArcGisEnabled
  const query = useArcGisMaps(siteId, { enabled })
  const { t } = useTranslation()

  if (query.status === 'loading') {
    return <Progress />
  } else if (query.status === 'error') {
    return (
      <div tw="flex items-center gap[8px]">
        <div tw="flex-initial">
          <Icon icon="error" />
        </div>
        <div tw="flex-1">{t('plainText.arcGisMapsRetrievalError')}</div>
      </div>
    )
  } else if (
    enabled &&
    query.status === 'success' &&
    query.data !== 'notEnabled'
  ) {
    // We need to check for `enabled` even if the query.status is "success"
    // because we want to hide the dropdown if the user disabled the ArcGIS
    // feature after the list of maps is downloaded.
    return (
      <Select name="webMapId" label={t('plainText.arcGisMap')}>
        {query.data?.map((m) => (
          <Option key={m.id} value={m.id}>
            {m.title}
          </Option>
        ))}
      </Select>
    )
  } else {
    return null
  }
}

/**
 * A particular map returned by the arcGisMaps endpoint
 */
type ArcGisMap = { id: string; title: string }

/**
 * The complete response from the arcGisMaps endpoint
 */
type ArcGisMapsResponse = {
  maps: ArcGisMap[]
}

/**
 * For useArcGisMaps, we differentiate between a request failing because ArcGIS
 * is not enabled for the site or customer (in which case we just display
 * nothing) from other failures, where we display an error message.
 */
type QueryResponse = 'notEnabled' | ArcGisMap[]

/**
 * Query the ArcGIS maps for the specified site.
 */
function useArcGisMaps(
  siteId: string,
  options?: UseQueryOptions<QueryResponse>
) {
  return useQuery<QueryResponse>(
    ['siteArcGisMaps', siteId],
    async () => {
      try {
        return (
          await axios.get<ArcGisMapsResponse>(
            getUrl(`/api/sites/${siteId}/arcGisMaps`)
          )
        ).data.maps
      } catch (e) {
        if (e.response?.status === 400) {
          return 'notEnabled'
        } else {
          throw e
        }
      }
    },
    options
  )
}
