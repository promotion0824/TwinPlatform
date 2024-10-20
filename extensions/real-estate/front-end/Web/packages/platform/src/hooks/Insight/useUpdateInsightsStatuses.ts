import {
  Insight,
  InsightWorkflowStatus,
} from '@willow/common/insights/insights/types'
import { api } from '@willow/ui'
import { useMutation, useQueryClient } from 'react-query'

/**
 * this is the hook to update "lastStatus" of an insight or multiple insights
 * belonging to the same site
 */
export default function useUpdateInsightsStatuses(
  {
    siteId,
    insightIds,
    newStatus,
    reason,
  }: {
    siteId: string
    insightIds: string[]
    newStatus: InsightWorkflowStatus
    reason?: string
  },
  options?: {
    onError?: (error: unknown) => void
    onMutate?: () => SnapshotInsights
    onSettled?: () => void
    onSuccess?: (data: Insight) => void
    enabled?: boolean
  }
) {
  const queryClient = useQueryClient()

  return useMutation<Insight, unknown, void, SnapshotInsights>(
    async () => {
      const isUpdatingMultipleInsights = insightIds.length > 1
      const url = isUpdatingMultipleInsights
        ? `/v2/sites/${siteId}/insights/status`
        : `/v2/sites/${siteId}/insights/${insightIds[0]}/status`

      // https://wil-dev-plt-aue1-portalxl.azurewebsites.net/swagger/index.html?urls.primaryName=PlatformPortalXL%20API%20V2
      // as per swagger, POST is used to update multiple insights and PUT is used to update a single insight
      const method = isUpdatingMultipleInsights ? api.post : api.put

      const { data } = await method(url, {
        // include ids only if updating multiple insights,
        // insightId will be part of url if updating a single insight
        ...(isUpdatingMultipleInsights && { ids: insightIds }),
        status: newStatus,
        // include reason only when insight is resolved
        ...(reason && { reason }),
      })
      return data
    },
    {
      mutationKey: ['insightsStatuses', siteId, insightIds, newStatus],
      onMutate: async () => {
        // cancel queries that might impact the PUT request
        await queryClient.cancelQueries('insights')
        const snapshotOfPreviousInsights =
          queryClient.getQueryData<Insight[]>('insights')

        // optimistic update
        queryClient.setQueryData<Insight[]>('insights', (oldInsights) => {
          const insightsFound = oldInsights?.filter((insight) =>
            insightIds.includes(insight.id)
          )

          if (insightsFound && insightsFound?.length > 0) {
            return [
              ...(oldInsights?.filter((i) => !insightIds.includes(i.id)) ?? []),
              ...insightsFound.map((insight) => ({
                ...insight,
                lastStatus: newStatus,
              })),
            ]
          } else {
            return oldInsights ?? []
          }
        })

        // return snapshot to be able to rollback in case of error
        return {
          snapshotOfPreviousInsights,
        }
      },
      onError: (error, _, context) => {
        queryClient.setQueryData(
          'insights',
          context?.snapshotOfPreviousInsights ?? []
        )
      },
      // Always refetch after error or success:
      onSettled: () => {
        queryClient.invalidateQueries('insights')
        queryClient.invalidateQueries('all-insights')
      },
      ...options,
    }
  )
}

// SnapshotInsights is used to rollback in case of error
type SnapshotInsights = {
  snapshotOfPreviousInsights?: Insight[]
}
