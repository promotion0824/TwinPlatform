import { useMemo } from 'react'
import {
  Error,
  useUser,
  Progress,
  useDateTime,
  DashboardReportCategory,
  DatePickerDayRangeOptions,
  DatePickerBusinessRangeOptions,
} from '@willow/ui'
import { useSites } from 'providers'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import useSaveUserPreferences from '@willow/common/hooks/useSaveUserPreferences'
import PortfolioContent from './PortfolioContent'
import useGetPerformanceData from '../../hooks/KpiDashboard/useGetPerformanceData'
import useManagedPortfolios from './useManagedPortfolios'

export default function Portfolio({ children }) {
  const sites = useSites()
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

  const selectedDates = [
    searchParams.startDate ?? defaultStartDate,
    searchParams.endDate ?? defaultEndDate,
  ]

  const performanceDataQuery = useGetPerformanceData({
    startDate: selectedDates[0],
    endDate: selectedDates[1],
    portfolioId: user.portfolios?.[0]?.id,
    customerId: user?.customer?.id,
    selectedDayRange: [
      searchParams.selectedDayRange ??
        user?.options?.selectedDayRange ??
        DatePickerDayRangeOptions.ALL_DAYS_KEY,
    ],
    selectedBusinessHourRange: [
      searchParams.selectedBusinessHourRange ??
        user?.options?.selectedBusinessHourRange ??
        DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
    ],
    url: '/api/kpi/building_data',
    options: {
      enabled:
        searchParams?.category == null ||
        searchParams?.category === DashboardReportCategory?.OPERATIONAL,
      onError: (err) => console.error(err),
    },
  })

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

  /**
   * data returned by useGetPerformanceData comes in shape of following where
   * xValue is site's name and yValue is a score:
   *
   * [{xValue: '60 Martin Street', yValue: 0.5}, {xValue: '59 Martin Street', yValue: 0.4}]
   *
   * we standardize this to return data in the form of:
   * Array<{siteId?: string, score?: number}>
   *
   * and default to be empty array if neither hook returns data
   */
  const buildingScores = useMemo(() => {
    if (
      searchParams?.category === DashboardReportCategory.OPERATIONAL ||
      searchParams?.category == null
    ) {
      const scores =
        performanceDataQuery?.data
          ?.find((s) => s.name === 'ComfortScore_LastValue')
          ?.values.map((v) => ({
            siteId: sites.find((site) => site.name === v.xValue)?.id,
            comfort: v.yValue,
          })) ?? []

      performanceDataQuery?.data
        ?.find((s) => s.name === 'EnergyScore_LastValue')
        ?.values.forEach((v) => {
          const siteId = sites.find((site) => site.name === v.xValue)?.id
          const scoreSite = scores.find((s) => s.siteId === siteId)
          if (scoreSite) {
            scoreSite.energy = v.yValue
          } else {
            scores.push({ siteId, energy: v.yValue })
          }
        })

      return scores.map((score) => {
        let performanceScore = 0
        const availableScores = [score.comfort, score.energy].filter(
          (value) => value != null
        )
        if (availableScores.length > 0) {
          const sumOfScores = availableScores.reduce((sum, one) => sum + one, 0)
          performanceScore = sumOfScores / availableScores.length
        }

        // We consider 0 score as valid score, and null or undefined as invalid score.
        return { ...score, performance: performanceScore }
      })
    }
    return []
  }, [performanceDataQuery, searchParams?.category, sites])

  const isBuildingScoresLoading = performanceDataQuery.isLoading

  const managedPortfoliosQuery = useManagedPortfolios()

  if (managedPortfoliosQuery.isLoading) {
    return <Progress />
  }
  if (managedPortfoliosQuery.isError) {
    return <Error />
  }

  const filteredPortfolios = managedPortfoliosQuery.data

  const filteredPortfoliosSiteIds = filteredPortfolios
    .flatMap((portfolio) => portfolio.sites)
    .map((site) => site.siteId)

  const filteredSites = sites.filter((site) =>
    filteredPortfoliosSiteIds.includes(site.id)
  )

  return (
    <PortfolioContent
      sites={filteredSites}
      searchParams={searchParams}
      buildingScores={buildingScores}
      isBuildingScoresLoading={isBuildingScoresLoading}
      setSearchParams={setSearchParams}
      dateRange={selectedDates}
    >
      {children}
    </PortfolioContent>
  )
}
