import {
  Fieldset,
  Flex,
  useAnalytics,
  useDateTime,
  useFeatureFlag,
} from '@willow/ui'
import MiniTimeSeries from 'components/MiniTimeSeries'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import tw from 'twin.macro'
import EChartsMiniTimeSeries from '../../../EChartsMiniTimeSeries'

export default function TimeSeries({ insight }) {
  const analytics = useAnalytics()
  const dateTime = useDateTime()
  const { t } = useTranslation()
  const featureFlags = useFeatureFlag()

  const [times] = useState(() => [
    dateTime(insight.occurredDate).addHours(-48).format(),
    insight.occurredDate,
  ])

  useEffect(() => {
    analytics.track('Time Series Viewed', { context: 'Insight View' })
  }, [analytics])

  if (insight.equipment?.id == null) {
    return null
  }

  return (
    <Flex tw="max-height[100%]">
      <Fieldset
        icon="graph"
        legend={t('headers.timeSeries')}
        padding="extraLarge 0 0"
      />
      <Flex padding="0 large extraLarge" flex="1" tw="overflow-hidden">
        {featureFlags.hasFeatureToggle('eChartsMiniTimeseries') ? (
          <EChartsMiniTimeSeries
            siteEquipmentId={`${insight.siteId}_${insight.equipment.id}`}
            times={times}
            equipmentName={insight.equipment?.name}
          />
        ) : (
          <MiniTimeSeries
            siteEquipmentId={`${insight.siteId}_${insight.equipment.id}`}
            times={times}
            equipmentName={insight.equipment?.name}
          />
        )}
      </Flex>
    </Flex>
  )
}
