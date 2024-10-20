import { titleCase } from '@willow/common'
import { invariant, useScopeSelector } from '@willow/ui'
import { BarChart } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { ChartTile } from '../../../../../components/LocationHome/ChartTile/ChartTile'
import { useBuildingHomeSlice } from '../../../../../store/buildingHomeSlice'
import useTicketCountsByDate from './useTicketCountsByDate'

const DailyNewTicketsChartTile = () => {
  const {
    i18n: { language },
    t,
  } = useTranslation()
  const { scopeId } = useScopeSelector()
  const { selectedDateRange } = useBuildingHomeSlice()

  invariant(scopeId, 'scopeId is required for DailyNewTicketsChartTile')

  const ticketCountsQuery = useTicketCountsByDate({
    twinId: scopeId,
    startDate: selectedDateRange[0].toISOString(),
    endDate: selectedDateRange[1].toISOString(),
  })

  const [dates = [], values = []] = [
    // We will rely on the value returned in the response,
    // and won't considering local timezone here
    ticketCountsQuery.data?.map(({ date }) => date),
    ticketCountsQuery.data?.map(({ count }) => count),
  ]

  const isEmpty = dates?.length === 0 || values?.length === 0

  // TODO: loading, error and empty state will be handled later in
  // https://dev.azure.com/willowdev/Unified/_workitems/edit/136723
  return isEmpty ? null : (
    <ChartTile
      chart={
        <BarChart
          dataset={[
            {
              data: values,
              name: titleCase({
                language,
                text: t('headers.dailyNewTickets'),
              }),
            },
          ]}
          labels={dates}
          labelsType="time"
          orientation="vertical"
        />
      }
      title={titleCase({
        language,
        text: t('headers.dailyNewTickets'),
      })}
    />
  )
}

export default DailyNewTicketsChartTile
