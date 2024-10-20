import { InsightDetailEmptyState } from '@willow/common/insights/component'
import {
  Insight,
  TimeSeriesTwinInfo,
} from '@willow/common/insights/insights/types'
import { useAnalytics, useFeatureFlag } from '@willow/ui'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import EChartsMiniTimeSeriesComponent from '../../../../EChartsMiniTimeSeries/MiniTimeSeriesComponent'
import MiniTimeSeriesComponent from '../../../../MiniTimeSeries/MiniTimeSeriesComponent'

/**
 * A variant of the MiniTimeSeries component that is used in the InsightWorkflowModal
 * with shading for abnormal/faulty/bad data occurrences and display points for time series
 * and impact score points coming from rules engine.
 */
export default function InsightWorkflowTimeSeries({
  insight,
  start,
  end,
  shadedDurations,
  twinInfo,
  isViewingDiagnostic = false,
  insightTab,
  period,
  onPeriodChange,
  diagnosticBoundaries,
}: {
  insight: Insight
  start: string
  end: string
  shadedDurations: Array<{ start: string; end: string; color: string }>
  twinInfo?: TimeSeriesTwinInfo
  isViewingDiagnostic?: boolean
  insightTab?: string
  period?: string
  onPeriodChange?: (period: string | null) => void
  diagnosticBoundaries?: string[]
}) {
  const analytics = useAnalytics()
  const { t } = useTranslation()
  const featureFlags = useFeatureFlag()

  useEffect(() => {
    analytics.track('Time Series Viewed', { context: 'Insight View' })
  }, [analytics])

  return (
    <FlexContainer>
      <SubContainer>
        {insight?.twinId == null && insight?.equipmentId == null ? (
          <div tw="mt-[20px]">
            <InsightDetailEmptyState
              heading={t('plainText.timeSeriesNotAvailable')}
              subHeading={t('plainText.timeSeriesVisibleWillowActivate')}
            />
          </div>
        ) : featureFlags.hasFeatureToggle('eChartsMiniTimeseries') ? (
          <EChartsMiniTimeSeriesComponent
            siteEquipmentId={`${insight.siteId}_${
              insight?.equipmentId ?? insight.twinId
            }`}
            times={[start, end]}
            equipmentName={insight.equipmentName}
            shadedDurations={shadedDurations}
            twinInfo={twinInfo}
            // start and end covers the entire time range of the insight
            // which is unlikely to be any of the predefined quick ranges
            isDefaultQuickRange={false}
            className={undefined}
            hideEquipments={false}
            isViewingDiagnostic={isViewingDiagnostic}
            insightTab={insightTab}
            period={period}
            onPeriodChange={onPeriodChange}
            diagnosticBoundaries={diagnosticBoundaries}
          />
        ) : (
          <MiniTimeSeriesComponent
            siteEquipmentId={`${insight.siteId}_${
              insight?.equipmentId ?? insight.twinId
            }`}
            times={[start, end]}
            equipmentName={insight.equipmentName}
            shadedDurations={shadedDurations}
            twinInfo={twinInfo}
            // start and end covers the entire time range of the insight
            // which is unlikely to be any of the predefined quick ranges
            isDefaultQuickRange={false}
            className={undefined}
            hideEquipments={false}
            isViewingDiagnostic={isViewingDiagnostic}
            insightTab={insightTab}
            period={period}
            onPeriodChange={onPeriodChange}
            diagnosticBoundaries={diagnosticBoundaries}
          />
        )}
      </SubContainer>
    </FlexContainer>
  )
}

const FlexContainer = styled.div({
  display: 'flex',
  height: '100%',
  '& #time-series-graph-header': {
    height: '89px',
  },
  '& #time-series-graph-labels-wrapper': {
    marginTop: '50px',
  },
})

const SubContainer = styled.div({
  display: 'flex',
  overflow: 'hidden',
  flex: '1 1 0%',
  flexFlow: 'column',
})
