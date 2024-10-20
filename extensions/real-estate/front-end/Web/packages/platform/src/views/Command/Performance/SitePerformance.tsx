/* eslint-disable complexity */
import { qs } from '@willow/common'
import { ParamsDict } from '@willow/common/hooks/useMultipleSearchParams'
import { Site } from '@willow/common/site/site/types'
import {
  api,
  CollapsablePanel,
  DashboardReportCategory,
  DatePickerBusinessRangeOptions,
  DatePickerDayRangeOptions,
  DocumentTitle,
  reduceQueryStatuses,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import _ from 'lodash'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery, UseQueryResult } from 'react-query'
import { styled } from 'twin.macro'
import { EmbedGroup } from '../../../components/Reports/ReportsLayout'
import {
  useGetAuthenticatedReport,
  usePostAuthenticatedReport,
} from '../../../hooks/index'
import {
  SigmaWidget,
  WidgetsResponse,
} from '../../../services/Widgets/WidgetsService'
import { FeatureFlags } from '../../Portfolio/KPIDashboards/HeaderControls/HeaderControls'
import DashboardContent from './DashboardContent'
import PerformanceViewHeader from './PerformanceViewHeader'
import PerformanceSidePanel from './SidePanel'

export default function SitePerformance({
  site,
  dateRange,
  quickOptionSelected,
  onQuickOptionChange,
  handleDateRangeChange,
  featureFlags,
  widgetsResponse,
  selectedReport,
  selectedDashboardReport,
  disableDatePicker = false,
  selectedDayRange,
  selectedBusinessHourRange,
  onDayRangeChange,
  onBusinessHourRangeChange,
  onUserOptionsSave,
  userSavedTenants = [],
  onResetClick,
  hideBusinessHourRange,
  selectedCategory,
  onReportSelection,
  widgetsToDisplay,
}: {
  site: Site
  dateRange: [string, string]
  quickOptionSelected?: string
  onQuickOptionChange?: (quickOptionSelected: string) => void
  handleDateRangeChange: (params: ParamsDict) => void
  featureFlags: FeatureFlags
  widgetsResponse: UseQueryResult<WidgetsResponse>
  selectedReport?: SigmaWidget
  selectedDashboardReport?: EmbedGroup
  disableDatePicker?: boolean
  selectedDayRange?: DatePickerDayRangeOptions
  selectedBusinessHourRange?: DatePickerBusinessRangeOptions
  onDayRangeChange?: (selectedDayRange: DatePickerDayRangeOptions) => void
  onBusinessHourRangeChange?: (
    selectedBusinessHourRange: DatePickerBusinessRangeOptions
  ) => void
  onUserOptionsSave?: (key: string, options: string[]) => void
  userSavedTenants?: string[]
  onResetClick?: () => void
  hideBusinessHourRange?: boolean
  selectedCategory: DashboardReportCategory
  onReportSelection: (report: EmbedGroup, category: string) => void
  widgetsToDisplay: SigmaWidget[]
}) {
  const { location, locationName } = useScopeSelector()
  const { customer } = useUser()

  const [isPanelOpen, setIsPanelOpen] = useState(true)
  const [tenantIds, setTenantIds] = useState<string[]>(userSavedTenants)
  const { t } = useTranslation()

  const authPostReportResponse = usePostAuthenticatedReport(
    {
      reportId: selectedDashboardReport?.widgetId,
      reportName: selectedDashboardReport?.name,
      scopeId: location?.twin.id,
      customerId: customer.id,
      start: dateRange[0],
      end: dateRange[1],
      selectedDayRange:
        selectedDayRange ?? DatePickerDayRangeOptions.ALL_DAYS_KEY,
      selectedBusinessHourRange:
        selectedBusinessHourRange ??
        DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
      /**
       *  TODO : Remove siteIds in future.
       *  Link : https://dev.azure.com/willowdev/Unified/_workitems/edit/126484
       */
      url: `/api/sigma/sites/${site.id}/embedurl`,
      tenantIds,
    },
    {
      enabled:
        featureFlags?.hasFeatureToggle('businessHourRange') &&
        !!selectedDashboardReport?.widgetId &&
        !!selectedDashboardReport?.name,
    }
  )

  // TODO: remove 'useGetAuthenticatedReport' once 'businessHourRange' FF becomes stale.
  const authGetReportResponse = useGetAuthenticatedReport(
    {
      url: qs.createUrl(`/api/sigma/sites/${site.id}/embedurl`, {
        reportId: selectedDashboardReport?.widgetId,
        reportName: selectedDashboardReport?.name,
        start: dateRange[0],
        end: dateRange[1],
        tenantIds,
      }),
    },
    {
      enabled:
        !featureFlags?.hasFeatureToggle('businessHourRange') &&
        !!selectedDashboardReport?.widgetId &&
        !!selectedDashboardReport?.name,
    }
  )

  const authReportResponse = featureFlags?.hasFeatureToggle('businessHourRange')
    ? authPostReportResponse
    : authGetReportResponse

  const reducedQueryStatus = reduceQueryStatuses([
    widgetsResponse.status,
    authReportResponse.status,
  ])

  const tenantsQuery = useQuery<
    Array<{ tenantId: string; tenantName: string }>
  >(
    ['tenants', site.id],
    async () => {
      const response = await api.get(`/tenants?siteIds=${site.id}`)

      return response.data
    },
    {
      select: (data) =>
        _.orderBy(_.uniqBy(data ?? [], 'tenantId'), 'tenantName'),
    }
  )

  const allTenants = t('plainText.allTenants')
  const handleTenantsChange = ({
    id,
    isAllTenants,
    isEveryTenantsChecked,
  }: {
    id: string
    isAllTenants: boolean
    isEveryTenantsChecked: boolean
  }) => {
    // if "All Tenants" option is selected,
    // - uncheck all other options when all other options are checked,
    //   otherwise check all other options
    // if "All Tenants" option is not selected
    // - simply add/remove the new option to/from the tenantIds array
    const nextTenantIds = isAllTenants
      ? isEveryTenantsChecked
        ? []
        : tenantsQuery.data?.map(({ tenantId }) => tenantId) ?? []
      : _.xor(tenantIds, [id])

    onUserOptionsSave?.(`tenants-${site.id}`, nextTenantIds)
    setTenantIds(nextTenantIds)
  }

  return (
    <DashboardFlexContainer>
      <DocumentTitle
        scopes={[
          selectedDashboardReport?.name,
          t('headers.dashboards'),
          locationName,
        ]}
      />

      {/* Hide SidePanel if no report found */}
      {widgetsToDisplay.length > 0 && (
        <PerformanceSidePanel
          selectedDashboardReport={selectedDashboardReport}
          selectedCategory={selectedCategory}
          widgets={widgetsToDisplay}
          onReportChange={onReportSelection}
          css={{ flexShrink: 0 }}
        />
      )}
      <Container>
        {/* Hide Headers if no report found */}
        {widgetsToDisplay.length > 0 && (
          <PerformanceViewHeader
            quickOptionSelected={quickOptionSelected}
            onQuickOptionChange={onQuickOptionChange}
            selectedDayRange={selectedDayRange}
            onDayRangeChange={onDayRangeChange}
            onBusinessHourRangeChange={onBusinessHourRangeChange}
            onResetClick={onResetClick}
            selectedBusinessHourRange={selectedBusinessHourRange}
            dateRange={dateRange}
            handleDateRangeChange={handleDateRangeChange}
            hideBusinessHourRange={hideBusinessHourRange}
            disableDatePicker={disableDatePicker}
          />
        )}

        <DashboardContent
          isFetchOrAuthError={reducedQueryStatus === 'error'}
          isFetchOrAuthLoading={
            reducedQueryStatus === 'loading' &&
            // when selectedDashboardReport is null,
            // useGetAuthenticatedReport hook is disable and its status is
            // always "idle" and combined status is always loading,
            // to avoid this, we need to check if selectedDashboardReport is null
            !(
              widgetsResponse.status === 'success' &&
              selectedDashboardReport == null
            )
          }
          isGetWidgetsSuccess={widgetsResponse?.isSuccess}
          selectedReport={selectedReport}
          selectedDashboardReport={selectedDashboardReport}
          isAuthReportSuccess={reducedQueryStatus === 'success'}
          authenticatedReport={authReportResponse.data}
        />
      </Container>
    </DashboardFlexContainer>
  )
}

const Container = styled.div({
  width: '100%',
  height: '100%',
  '> div': {
    minWidth: 'fit-content',
  },
  display: 'flex',
  flexFlow: 'column',
})

const DashboardFlexContainer = styled.div({
  height: '100%',
  display: 'flex',
})

const StyledCollapsablePanel = styled(CollapsablePanel)<{ isOpen: boolean }>(
  ({ isOpen }) => ({
    display: 'block',
    marginRight: '4px',
    minWidth: isOpen ? '235px' : '40px',
  })
)
