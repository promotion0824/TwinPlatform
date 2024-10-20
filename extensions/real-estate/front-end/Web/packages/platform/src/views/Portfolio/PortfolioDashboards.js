import {
  DashboardReportCategory,
  DocumentTitle,
  Progress,
  useAnalytics,
  useFeatureFlag,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { selectReport, useGetWidgets } from '../../hooks/index'
import {
  getHeaderPanelCategories,
  getWidgetsToDisplay,
} from '../Command/Dashboard/Dashboard/views/CategoriesHeaderPanel'
import PerformanceSidePanel from '../Command/Performance/SidePanel'
import KPIDashboardHeader from './KPIDashboards/KPIDashboardHeader'
import PortfolioDashboardContent from './KPIDashboards/PortfolioReport'
import { usePortfolio } from './PortfolioContext'

export default function PortfolioDashboards() {
  const user = useUser()
  const featureFlags = useFeatureFlag()
  const analytics = useAnalytics()
  const {
    category,
    selectedDayRange,
    selectedBusinessHourRange,
    dateRange,
    selectedDashboard,
    handleReportSelection,
  } = usePortfolio()
  const { t } = useTranslation()
  const { locationName } = useScopeSelector()

  const widgetsResponse = useGetWidgets(
    {
      baseUrl: '/api/portfolios',
      id: user?.portfolios?.[0]?.id,
    },
    {
      enabled: user?.portfolios?.[0]?.id != null,
      onError: (err) => console.error(err),
      select: (data) => selectReport({ data, category, selectedDashboard }),
    }
  )

  const { selectedReport, selectedDashboardReport } = widgetsResponse.data ?? {}

  // TODO: future improvement: this function seems duplicated in
  // packages/platform/src/views/Command/Dashboard/Dashboard/Dashboard.js
  const headerPanelCategories = getHeaderPanelCategories(
    [
      {
        category: DashboardReportCategory.OPERATIONAL,
        condition: true,
      },
      {
        category: DashboardReportCategory.DATA_QUALITY,
        condition: featureFlags?.hasFeatureToggle('dataQualityView'),
      },
      {
        category: DashboardReportCategory.OCCUPANCY,
        condition: featureFlags?.hasFeatureToggle('occupancyView'),
      },
      {
        category: DashboardReportCategory.TENANT,
        condition: true,
      },
      {
        category: DashboardReportCategory.MANAGEMENT,
        condition: featureFlags?.hasFeatureToggle('isManagementViewEnabled'),
      },
      {
        category: DashboardReportCategory.SUSTAINABILITY,
        condition: featureFlags?.hasFeatureToggle('sustainabilityView'),
      },
      {
        category: DashboardReportCategory.SAVINGS,
        condition: true,
      },
      {
        category: DashboardReportCategory.PRE_OPERATIONAL,
        condition: true,
      },
    ],
    widgetsResponse
  )
  const isAuthReportEnabled =
    widgetsResponse.isSuccess &&
    selectedReport?.id != null &&
    user?.portfolios?.[0]?.id != null &&
    !!selectedDashboardReport

  const shouldTrackDashboardReport =
    selectedReport != null && selectedDashboardReport?.name

  useEffect(() => {
    if (shouldTrackDashboardReport) {
      analytics.track('Portfolio Drilldown', {
        category: selectedReport?.metadata?.category,
        customer_name: user.customer?.name,
        button_name: selectedDashboardReport?.name,
      })
    }
  }, [
    analytics,
    selectedDashboardReport?.name,
    selectedReport?.metadata?.category,
    shouldTrackDashboardReport,
    user.customer?.name,
  ])

  useEffect(() => {
    analytics?.track(`Portfolio Dashboard Page`, {
      customer_name: user.customer.name,
      page: category,
    })
  }, [analytics, category, user.customer.name])

  // error state will be handled by CommonTab
  if (widgetsResponse.isLoading) {
    return <Progress />
  }

  const widgetsToDisplay = getWidgetsToDisplay(
    headerPanelCategories,
    widgetsResponse?.data?.widgets
  )

  const selectedCategory =
    category ??
    selectedReport?.metadata
      ?.category /* use first category if no category selected */

  return (
    <DashboardContainer>
      <DocumentTitle
        scopes={[
          selectedDashboardReport?.name,
          t('headers.dashboards'),
          locationName,
        ]}
      />

      {/* Hide SidePanel if report not found */}
      {widgetsToDisplay.length > 0 && (
        <PerformanceSidePanel
          selectedDashboardReport={selectedDashboardReport}
          selectedCategory={selectedCategory}
          widgets={widgetsToDisplay}
          onReportChange={handleReportSelection}
          css={{ flexShrink: 0 }}
        />
      )}
      <ContentContainer>
        {/* Hide Headers if report not found */}
        {widgetsToDisplay.length > 0 && (
          <KPIDashboardHeader
            analytics={analytics}
            disableDatePicker={
              !!selectedDashboardReport?.disableDatePicker ||
              selectedCategory === DashboardReportCategory.DATA_QUALITY
            }
            hideBusinessHourRange={
              selectedCategory === DashboardReportCategory.SUSTAINABILITY
            }
          />
        )}

        <PortfolioDashboardContent
          user={user}
          selectedDayRange={selectedDayRange}
          selectedBusinessHourRange={selectedBusinessHourRange}
          dateRange={dateRange}
          widgetsResponse={widgetsResponse}
          selectedReport={selectedReport}
          selectedDashboardReport={selectedDashboardReport}
          shouldAuthReportEnabled={isAuthReportEnabled}
        />
      </ContentContainer>
    </DashboardContainer>
  )
}

const DashboardContainer = styled.div({
  height: '100%',
  width: '100%',
  display: 'flex',
})

const ContentContainer = styled.div`
  width: 100%;
  display: flex;
  flex-direction: column;
`
