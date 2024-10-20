import { useAnalytics, useFeatureFlag } from '@willow/ui'
import EChartsMiniTimeSeries from 'components/EChartsMiniTimeSeries/MiniTimeSeries'
import MiniTimeSeries from 'components/MiniTimeSeries/MiniTimeSeries'
import { useEffect } from 'react'
import { useParams } from 'react-router'
import { useFloor } from '../../FloorContext'

export default function TimeSeries({ assetId }) {
  const analytics = useAnalytics()
  const params = useParams()
  const floor = useFloor()
  const featureFlags = useFeatureFlag()

  useEffect(() => {
    analytics.track('Time Series Viewed', { context: 'Floor Asset View' })
  }, [analytics])

  return featureFlags.hasFeatureToggle('eChartsMiniTimeseries') ? (
    <EChartsMiniTimeSeries
      siteEquipmentId={`${params.siteId}_${assetId}`}
      equipmentName={floor?.selectedAsset?.name}
    />
  ) : (
    <MiniTimeSeries
      siteEquipmentId={`${params.siteId}_${assetId}`}
      equipmentName={floor?.selectedAsset?.name}
    />
  )
}
