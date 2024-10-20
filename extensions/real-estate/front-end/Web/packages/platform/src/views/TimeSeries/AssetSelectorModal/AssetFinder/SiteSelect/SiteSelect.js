import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Select, Option, useAnalytics, useForm } from '@willow/ui'
import { useSites } from 'providers'
import { useTimeSeries } from 'components/TimeSeries/TimeSeriesContext'
import useSearchParams from '@willow/common/hooks/useSearchParams'

export default function SiteSelect() {
  const form = useForm()
  const sites = useSites()
  const timeSeries = useTimeSeries()
  const analytics = useAnalytics()
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const searchSiteId = searchParams.get('siteId')

  function handleSiteSelect(siteId, site) {
    timeSeries.setState((prevState) => ({
      ...prevState,
      siteId,
    }))
    analytics.track('Time Series Site Selected', { Site: site })
  }

  useEffect(() => {
    const site = sites.find(({ id }) => id === searchSiteId)
    if (searchSiteId && site) {
      handleSiteSelect(searchSiteId, site)
    }
  }, [searchSiteId])

  return (
    <Select
      name="site"
      placeholder={t('labels.site')}
      label={t('labels.site')}
      icon="site"
      value={form.data.siteName}
    >
      {sites.map((site) => (
        <Option
          name="siteId"
          key={site.id}
          value={site.id}
          onClick={() => setSearchParams({ siteId: site.id })}
        >
          {site.name}
        </Option>
      ))}
    </Select>
  )
}
