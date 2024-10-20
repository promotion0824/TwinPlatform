import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useDuration, useSnackbar, Fetch } from '@willow/ui'
import { useTimeSeries } from '../TimeSeriesContext'

export default function Point({ point }) {
  const duration = useDuration()
  const snackbar = useSnackbar()
  const timeSeries = useTimeSeries()
  const { t } = useTranslation()

  useEffect(() => {
    timeSeries.addLoadingSitePointId(point.sitePointId)

    return () => {
      timeSeries.removeLoadingSitePointId(point.sitePointId)
    }
  }, [JSON.stringify(timeSeries.times), timeSeries.granularity])

  return (
    <Fetch
      url={`/api/sites/${point.siteId}/points/${point.pointId}/liveData`}
      progress={null}
      params={{
        start: timeSeries.times[0],
        end: timeSeries.times[1],
        interval: duration(timeSeries.granularity).toDotnetString(),
      }}
      onResponse={(response) => {
        timeSeries.removeLoadingSitePointId(point.sitePointId)
        timeSeries.updatePoint(point.sitePointId, response)
      }}
      onError={(err) => {
        timeSeries.removeLoadingSitePointId(point.sitePointId)

        if (err?.name === 'AbortError') {
          timeSeries.removeSitePoints([point.sitePointId])
          return
        }

        timeSeries.removeSitePoints([point.sitePointId])

        snackbar.show(t('plainText.errorLoadingPoint'), {
          description: `PointId: ${point.pointId}`,
        })
      }}
    />
  )
}
