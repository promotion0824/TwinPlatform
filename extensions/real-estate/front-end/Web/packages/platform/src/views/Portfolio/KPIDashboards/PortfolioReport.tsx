import {
  DatePickerBusinessRangeOptions,
  DatePickerDayRangeOptions,
  reduceQueryStatuses,
  useScopeSelector,
} from '@willow/ui'
import { UseQueryResult } from 'react-query'
import { EmbedGroup } from '../../../components/Reports/ReportsLayout'
import { useGetEmbedUrls } from '../../../hooks/index'
import {
  SigmaWidget,
  WidgetsResponse,
} from '../../../services/Widgets/WidgetsService'
import DashboardContent from '../../Command/Performance/DashboardContent'
import { Portfolio, usePortfolio } from '../PortfolioContext'
import { User } from './HeaderControls/HeaderControls'

/** The selected report content for Portfolio */
export default function PortfolioDashboardContent({
  user,
  widgetsResponse,
  selectedReport,
  dateRange,
  selectedDayRange,
  selectedBusinessHourRange,
  selectedDashboardReport,
  shouldAuthReportEnabled = false,
  ...rest
}: {
  user: User
  selectedBusinessHourRange?: DatePickerBusinessRangeOptions
  selectedDayRange?: DatePickerDayRangeOptions
  dateRange: [string, string]
  widgetsResponse: UseQueryResult<WidgetsResponse>
  selectedReport?: SigmaWidget
  selectedDashboardReport?: EmbedGroup
  shouldAuthReportEnabled?: boolean
}) {
  const portfolio: Portfolio = usePortfolio()
  const { location, descendantSiteIds } = useScopeSelector()
  const scopeId = location?.twin?.id
  const { filteredSiteIds } = portfolio

  const embedUrlsResponse = useGetEmbedUrls(
    {
      start: dateRange[0],
      end: dateRange[1],
      scopeId: location?.twin.id,
      customerId: user?.customer?.id,
      selectedDayRange:
        selectedDayRange ?? DatePickerDayRangeOptions.ALL_DAYS_KEY,
      selectedBusinessHourRange:
        selectedBusinessHourRange ??
        DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
      /**
       *  TODO : Remove siteIds in future.
       *  Link : https://dev.azure.com/willowdev/Unified/_workitems/edit/126484
       */
      siteIds: scopeId ? descendantSiteIds ?? [] : filteredSiteIds,
      url: `/api/sigma/portfolios/${user?.portfolios?.[0]?.id}/embedurls`,
      reportId: selectedReport?.id,
      reportName: selectedDashboardReport?.name,
    },
    {
      enabled: shouldAuthReportEnabled,
      // expect embedUrls to have exactly 1 item meeting criteria
      select: (embedUrls) =>
        embedUrls.filter(
          (embedUrl) =>
            selectedDashboardReport?.embedPath != null &&
            embedUrl?.url?.startsWith(selectedDashboardReport.embedPath)
        ),
      cacheTime: 0,
    }
  )

  const reducedQueryStatus = reduceQueryStatuses([
    widgetsResponse.status,
    embedUrlsResponse.status,
  ])

  return (
    <DashboardContent
      isFetchOrAuthError={reducedQueryStatus === 'error'}
      isFetchOrAuthLoading={
        reducedQueryStatus === 'loading' &&
        !(
          widgetsResponse.status === 'success' &&
          selectedDashboardReport == null
        )
      }
      isGetWidgetsSuccess={widgetsResponse.isSuccess}
      selectedReport={selectedReport}
      selectedDashboardReport={selectedDashboardReport}
      isAuthReportSuccess={
        embedUrlsResponse.isSuccess && !!embedUrlsResponse.data?.[0]
      }
      authenticatedReport={embedUrlsResponse.data?.[0]}
      {...rest}
    />
  )
}
