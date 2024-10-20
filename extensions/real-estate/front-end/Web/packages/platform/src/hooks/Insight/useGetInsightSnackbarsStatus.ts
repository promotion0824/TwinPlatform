import { useQuery, UseQueryOptions } from 'react-query'
import { api } from '@willow/ui'
import {
  FilterOperator,
  InsightSnackbarsStatus,
} from '../../services/Insight/InsightsService'

/**
 * Fetch insight counts based on auto resolve state
 */
export default function useGetInsightSnackbarsStatus(
  lastInsightStatusCountDate?: string,
  options?: UseQueryOptions<InsightSnackbarsStatus[]>
) {
  return useQuery(
    ['insight-snackbars-status', lastInsightStatusCountDate],
    async () => {
      const response = await api.post(`/insights/snackbars/status`, [
        {
          field: 'updatedDate',
          operator: FilterOperator.greaterThan,
          value: lastInsightStatusCountDate,
        },
        {
          field: 'status',
          operator: FilterOperator.containedIn,
          value: ['readyToResolve', 'resolved'],
        },
        {
          field: 'sourceType',
          operator: FilterOperator.equalsShort,
          value: 'app',
        },
      ])
      return response.data
    },
    options
  )
}
