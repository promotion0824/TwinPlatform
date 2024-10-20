import {
  DateProximity,
  noMoreNotificationsId,
  Notification,
  NotificationFilterOperator,
  NotificationResponse,
  NotificationSource,
  NotificationStatus,
} from '@willow/common/notifications/types'
import { api } from '@willow/ui'
import { useMemo } from 'react'
import {
  useInfiniteQuery,
  UseInfiniteQueryOptions,
  UseInfiniteQueryResult,
  useMutation,
  UseMutationOptions,
  useQuery,
  UseQueryOptions,
} from 'react-query'
import { makeDateTextValue } from './utils'

/**
 * Hook returns all notifications for the user with pagination
 */
export const useGetNotifications = (
  params: {
    pageSize?: number
    page?: number
    sortSpecifications?: Array<{
      field: string
      sort: 'asc' | 'desc'
    }>
    filterSpecifications?: Array<{
      field: string
      operator: NotificationFilterOperator
      value?: string | string[]
    }>
  } = {
    pageSize: 20,
    page: 1,
    sortSpecifications: [
      { field: 'notification.createdDateTime', sort: 'desc' },
    ],
  },
  options?: UseInfiniteQueryOptions<NotificationResponse>
) =>
  useInfiniteQuery(
    ['notifications', params],
    async ({ pageParam = params.page }) => {
      const response = await api.post('/notifications/all', {
        ...params,
        page: pageParam,
      })
      return response.data
    },
    {
      ...options,
      getNextPageParam: ({ after, total }) => {
        if (after === 0) {
          return undefined
        }
        return (total - after) / (params.pageSize || 10) + 1
      },
    }
  )

interface MutationParams {
  notificationIds?: string[]
  state: NotificationStatus
}

/**
 * Hook to update the status of notifications
 */
export const useUpdateNotificationsStatuses = (
  options?: UseMutationOptions<number, Error, MutationParams>
) =>
  useMutation<number, Error, MutationParams>(
    async ({ notificationIds = [], state }: MutationParams) => {
      const response = await api.put('/notifications/state', {
        notificationIds,
        state,
      })
      return response.status
    },
    {
      ...options,
    }
  )

/**
 * Taken the notifications query and return the notifications with header row,
 * the content of the header doesn't matter as we just want to show either
 * 'today', 'yesterday', or 'older'
 */
export const useMakeNotificationsWithHeader = ({
  query,
  limit,
  after,
  search,
  total = 0,
}: {
  query: UseInfiniteQueryResult<NotificationResponse>
  limit?: number
  after?: number
  search?: string
  total?: number
}) =>
  useMemo(() => {
    let headerText: undefined | DateProximity
    const flattenedNotifications =
      query.data?.pages?.flatMap((page) => page.items) || []

    const limitedNotifications = (
      (limit ?? 0) > 0
        ? flattenedNotifications.slice(0, limit)
        : flattenedNotifications
    ).map((notification) => {
      const {
        entityId: id,
        twinId,
        name: twinName,
        priority,
        twinCategoryId: twinModelId,
        modelId,
      } = JSON.parse(notification.propertyBagJson || '{}')

      return {
        ...notification,
        properties: {
          id,
          twinId,
          twinName,
          modelId: modelId ?? twinModelId,
          priority,
        },
      }
    })
    const notificationsWithHeaderRow: Notification[] = []

    for (const notification of limitedNotifications) {
      if (
        !headerText ||
        makeDateTextValue(notification.createdDateTime) !== headerText
      ) {
        headerText = makeDateTextValue(notification.createdDateTime)
        // We don't actually care about the content of the
        // notification as this functions as a header row only
        notificationsWithHeaderRow.push(makePlaceholderNotification(headerText))
      }
      notificationsWithHeaderRow.push(notification)
    }
    if (after === 0 && !search && total > 0) {
      // We use this placeholder notification to render a custom end of notifications list message
      notificationsWithHeaderRow.push(
        makePlaceholderNotification(noMoreNotificationsId)
      )
    }

    return notificationsWithHeaderRow
  }, [after, limit, query.data?.pages, search, total])

const makePlaceholderNotification = (id: string) => ({
  id,
  source: NotificationSource.Insight,
  properties: {
    id,
    priority: 4,
  },
  userId: '',
  state: NotificationStatus.New,
  createdDateTime: '',
})

/**
 * Hook to get the notification stats for the current user
 */
export const useGetNotificationsStats = (
  params?: Array<{
    field: string
    operator: NotificationFilterOperator
    value: string
  }>,
  options?: UseQueryOptions<
    Array<{
      state: NotificationStatus
      count: number
    }>
  >
) =>
  useQuery(
    ['notifications-states-stats', params],
    async () => {
      const response = await api.post('/notifications/states/stats', params)
      return response.data
    },
    {
      ...options,
    }
  )
