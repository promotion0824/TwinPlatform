import { api } from '@willow/ui'
import { toPairs } from 'lodash'
import { useQuery, UseQueryOptions } from 'react-query'

export type TicketCountsByDate = {
  counts: Record<string, number>
}

const useTicketCountsByDate = ({
  twinId,
  startDate,
  endDate,
  options = {},
}: {
  twinId: string
  startDate: string
  endDate: string
  options?: UseQueryOptions<
    TicketCountsByDate,
    unknown,
    ReturnType<typeof formateData>
  >
}) =>
  useQuery({
    queryKey: ['ticketCountsByDateChartTile', twinId],
    queryFn: async () => {
      const { data } = await api.get(
        `/tickets/twins/${twinId}/ticketCountsByDate`,
        {
          params: { startDate, endDate },
        }
      )
      return data
    },
    select: formateData,
    ...options,
  })

const formateData = (data: TicketCountsByDate) =>
  toPairs(data.counts).map(([date, count]) => ({ date, count }))

export default useTicketCountsByDate
