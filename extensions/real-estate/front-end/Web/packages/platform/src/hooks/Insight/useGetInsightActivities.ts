import { useQuery } from 'react-query'
import { api } from '@willow/ui'
import { InsightWorkflowActivity } from '@willow/common/insights/insights/types'

const useGetInsightActivities = (siteId: string, insightId: string) =>
  useQuery<InsightWorkflowActivity[]>(
    ['insightActivities', siteId, insightId],
    async () => {
      const { data = [] } = await api.get(
        `/sites/${siteId}/insights/${insightId}/activities`
      )

      return data
    }
  )

export default useGetInsightActivities
