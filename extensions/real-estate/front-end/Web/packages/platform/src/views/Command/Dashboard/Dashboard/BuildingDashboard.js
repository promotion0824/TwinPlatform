/* eslint-disable complexity */
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import useSaveUserPreferences from '@willow/common/hooks/useSaveUserPreferences'
import {
  DashboardReportCategory,
  DatePickerBusinessRangeOptions,
  Error,
  Progress,
  useDateTime,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { useEffect } from 'react'
import { Redirect, useParams } from 'react-router'
import { selectReport, useGetWidgets } from '../../../../hooks/index'
import {
  getHeaderPanelCategories,
  getWidgetsToDisplay,
} from './views/CategoriesHeaderPanel'

import routes from '../../../../routes'
import SitePerformance from '../../Performance/SitePerformance'

const defaultQuickRange = '1M'

/**
 * Dashboard for a selected building, (used to be called Performance View).
 */
export default function BuildingDashboard({ site, analytics, featureFlags }) {
  const { isScopeSelectorEnabled, location: scope } = useScopeSelector()
  const { siteId: siteIdFromParams } = useParams()
  const user = useUser()
  const dateTime = useDateTime()
  const defaultStartDate = dateTime.now().addDays(-30).format('dateLocal')
  const defaultEndDate = dateTime.now().format('dateLocal')
  const [searchParams, setSearchParams] = useMultipleSearchParams([
    'startDate',
    'endDate',
    'quickOptionSelected',
    'category',
    'selectedDashboard',
    'selectedDayRange',
    'selectedBusinessHourRange',
  ])

  const {
    category, // please refer to packages\ui\src\constants\index.js for possible values
    startDate: paramStartDate,
    endDate: paramEndDate,
    selectedDashboard,
    quickOptionSelected = defaultQuickRange,
    selectedDayRange = user?.options?.selectedDayRange,
    selectedBusinessHourRange = user?.options?.selectedBusinessHourRange,
  } = searchParams

  const selectedDates = [
    paramStartDate ?? defaultStartDate,
    paramEndDate ?? defaultEndDate,
  ]

  const widgetsResponse = useGetWidgets(
    {
      baseUrl: '/api/sites',
      id: site.id,
    },
    {
      enabled: !!site?.id,
      onError: (err) => console.error(err),
      cacheTime: 0,
      select: (data) => selectReport({ data, category, selectedDashboard }),
    }
  )

  const { selectedReport, selectedDashboardReport } = widgetsResponse.data ?? {}

  const handleReportSelection = (newReport, newCategory) => {
    setSearchParams({
      category: newCategory,
      selectedDashboard: newReport.name,
      selectedBusinessHourRange:
        newCategory === DashboardReportCategory.SUSTAINABILITY
          ? DatePickerBusinessRangeOptions.ALL_HOURS_KEY
          : selectedBusinessHourRange,
    })
  }

  const handleDateRangeChange = (newDateRange, newQuickOption) => {
    if (quickOptionSelected) {
      analytics?.track('Date Range Filter Clicked', {
        customer_name: user.customer.name,
        date_range_filter: quickOptionSelected,
        ...(site?.id == null
          ? {}
          : {
              site,
            }),
      })
    }
    // Set date to localtimezone so the datepicker date and url param date are same.
    setSearchParams({
      startDate: dateTime(newDateRange[0]).format('dateTimeLocal'),
      endDate: dateTime(newDateRange[1]).format('dateTimeLocal'),
      quickOptionSelected: newQuickOption,
    })
  }

  const handleQuickOptionChange = (newQuickOption) => {
    setSearchParams({ quickOptionSelected: newQuickOption })
  }

  const handleDayRangeChange = (newSelectedDayRange) => {
    setSearchParams({
      startDate: selectedDates[0],
      endDate: selectedDates[1],
      selectedDayRange: newSelectedDayRange,
    })
  }

  const handleBusinessHourChange = (newBusinessHour) => {
    setSearchParams({
      startDate: selectedDates[0],
      endDate: selectedDates[1],
      selectedBusinessHourRange: newBusinessHour,
    })
  }

  const handleResetClick = () => {
    user.saveOptions('selectedDayRange', undefined)
    user.saveOptions('selectedBusinessHourRange', undefined)
    setSearchParams({
      startDate: undefined,
      endDate: undefined,
      selectedDayRange: undefined,
      selectedBusinessHourRange: undefined,
      quickOptionSelected: undefined,
    })
  }

  // TODO: future improvement: this function should be invoked inside useGetWidgets,
  // which will return filtered widgets with valid categories and reports to be display in SidePanel
  const headerPanelCategories = getHeaderPanelCategories(
    [
      { category: DashboardReportCategory.OPERATIONAL, condition: true },
      {
        category: DashboardReportCategory.DATA_QUALITY,
        condition: featureFlags?.hasFeatureToggle('dataQualityView'),
      },
      {
        category: DashboardReportCategory.TENANT,
        condition: true,
      },
      {
        category: DashboardReportCategory.OCCUPANCY,
        condition: featureFlags?.hasFeatureToggle('occupancyView'),
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

  const widgetsToDisplay = getWidgetsToDisplay(
    headerPanelCategories,
    widgetsResponse?.data?.widgets
  )

  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/71367
  const shouldTrackDashboardReport =
    selectedReport != null && selectedDashboardReport?.name

  useEffect(() => {
    if (shouldTrackDashboardReport) {
      analytics.track('Building Drilldown', {
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

  useSaveUserPreferences({
    preferenceNames: ['selectedDayRange', 'selectedBusinessHourRange'],
    preferencesToSave: [
      searchParams?.selectedDayRange,
      searchParams?.selectedBusinessHourRange,
    ],
    preferenceValues: [
      user?.options?.selectedDayRange,
      user?.options?.selectedBusinessHourRange,
    ],
    save: user.saveOptions,
  })

  const defaultCategory = selectedReport?.metadata?.category

  if (isScopeSelectorEnabled && siteIdFromParams && scope) {
    return <Redirect to={routes.dashboards_scope__scopeId(scope.twin.id)} />
  }

  return widgetsResponse.isLoading ? (
    <Progress />
  ) : widgetsResponse.isError ? (
    <Error />
  ) : (
    <>
      {/* TODO: Data Quality page, typescript conversion and tests will be addressed by
          https://dev.azure.com/willowdev/Unified/_workitems/edit/68411
      */}
      <SitePerformance
        featureFlags={featureFlags}
        site={site}
        dateRange={selectedDates}
        selectedDayRange={selectedDayRange}
        selectedBusinessHourRange={selectedBusinessHourRange}
        onBusinessHourRangeChange={handleBusinessHourChange}
        onResetClick={handleResetClick}
        onDayRangeChange={handleDayRangeChange}
        quickOptionSelected={quickOptionSelected}
        onQuickOptionChange={handleQuickOptionChange}
        handleDateRangeChange={handleDateRangeChange}
        widgetsResponse={widgetsResponse}
        selectedReport={selectedReport}
        selectedDashboardReport={selectedDashboardReport}
        selectedCategory={
          // category will be initialized as undefined, use default category as initial value when no param set
          category ?? defaultCategory
        }
        onUserOptionsSave={user?.saveOptions}
        userSavedTenants={site?.id ? user?.options?.[`tenants-${site.id}`] : []}
        hideBusinessHourRange={
          category === DashboardReportCategory.SUSTAINABILITY
        }
        widgetsToDisplay={widgetsToDisplay}
        onReportSelection={handleReportSelection}
        disableDatePicker={!!selectedDashboardReport?.disableDatePicker}
      />
    </>
  )
}
