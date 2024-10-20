import { api } from '@willow/ui'
import { useQuery, UseQueryOptions } from 'react-query'

export type TopActiveTicketCategories = {
  categoryCounts: Array<{
    categoryName: string
    count: number
  }>
  otherCount: number
}

const useTopActiveTicketCategories = ({
  limit,
  twinId,
  options = {},
}: {
  twinId: string
  limit?: number
  options: UseQueryOptions<
    TopActiveTicketCategories,
    unknown,
    ReturnType<typeof filterNonZeroCategoriesByCounts>
  >
}) =>
  useQuery({
    queryKey: ['TopActiveTicketCategoriesChartTile', twinId],
    queryFn: async () => {
      // returns `categoryCounts` as a descending sorted array by counts
      const { data } = await api.get(
        `/tickets/twins/${twinId}/ticketCountsByCategory`,
        {
          params: { limit },
        }
      )
      return data
    },
    select: filterNonZeroCategoriesByCounts,
    ...options,
  })

export const filterNonZeroCategoriesByCounts = (
  ticketCategories: TopActiveTicketCategories
): Array<{
  categoryName: string
  count: number
}> =>
  [
    ...ticketCategories.categoryCounts,
    { categoryName: 'Other', count: ticketCategories.otherCount },
  ].filter((category) => category.count > 0)

export default useTopActiveTicketCategories
