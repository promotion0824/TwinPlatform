import { FullSizeContainer, FullSizeLoader } from '@willow/common'
import { Message, useAnalytics, useLanguage } from '@willow/ui'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useDashboard } from '../../Dashboard/DashboardContext'
import CostInsightsTable, {
  formatInsight,
} from './CostInsightsTable/CostInsightsTable'

/**
 * TODO: Remove this component once home page revamp is complete.
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/132321
 * */
export default function Insights({ metric, onSelectedInsightChange }) {
  const analytics = useAnalytics()
  const { insightsQuery, insights, insightsQueryFilterSpec } = useDashboard()
  const { language } = useLanguage()
  const { t } = useTranslation()

  const insightsData = useMemo(
    () => formatInsight(insights, language),
    [insights, language]
  )

  return insightsQuery.isError ? (
    <FullSizeContainer>
      <Message icon="error">{t('plainText.errorOccurred')}</Message>
    </FullSizeContainer>
  ) : insightsQuery.isLoading ? (
    <FullSizeLoader />
  ) : insightsQuery.isSuccess ? (
    <CostInsightsTable
      // Force re-render when filter changes to get the correct total count
      key={JSON.stringify(insightsQueryFilterSpec)}
      insights={insightsData ?? []}
      metric={metric}
      analytics={analytics}
      language={language}
      t={t}
      onSelectedInsightChange={onSelectedInsightChange}
      fetchNextPage={() => {
        if (insightsQuery.hasNextPage && !insightsQuery.isFetchingNextPage) {
          insightsQuery.fetchNextPage()
        }
      }}
      total={insightsQuery.data?.pages?.at(-1)?.total ?? 0}
    />
  ) : null
}
