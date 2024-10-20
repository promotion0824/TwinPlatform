/* eslint-disable complexity */
import { useUser } from '@willow/ui'
import { useEffect } from 'react'
import PerformanceViewHeader from '../../Command/Performance/PerformanceViewHeader'
import { usePortfolio } from '../PortfolioContext'

export default function KPIDashboardHeader({
  analytics,
  disableDatePicker = false,
  hideBusinessHourRange,
}: {
  analytics: {
    track: (
      key: string,
      // eslint-disable-next-line camelcase
      value: { customer_name?: string; tab?: string }
    ) => void
  }
  disableDatePicker?: boolean
  hideBusinessHourRange?: boolean
}) {
  const user = useUser()
  const {
    quickOptionSelected,
    handleQuickOptionChange,
    handleDayRangeChange,
    selectedDayRange,
    handleDateRangeChange,
    handleBusinessHourChange,
    handleResetClick,
    selectedBusinessHourRange,
    dateRange,
  } = usePortfolio()

  useEffect(() => {
    analytics.track('Site List Page Landing', {
      customer_name: user.customer.name,
    })
  }, [])

  return (
    <PerformanceViewHeader
      quickOptionSelected={quickOptionSelected}
      onQuickOptionChange={handleQuickOptionChange}
      selectedDayRange={selectedDayRange}
      onDayRangeChange={handleDayRangeChange}
      onBusinessHourRangeChange={handleBusinessHourChange}
      onResetClick={handleResetClick}
      selectedBusinessHourRange={selectedBusinessHourRange}
      dateRange={dateRange}
      handleDateRangeChange={handleDateRangeChange}
      hideBusinessHourRange={hideBusinessHourRange}
      disableDatePicker={disableDatePicker}
    />
  )
}
