import { Button, useSnackbar } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQueryClient } from 'react-query'
import {
  useGetNotificationsStats,
  useUpdateNotificationsStatuses,
} from './hooks'
import { DateProximity, NotificationStatus } from './types'

/**
 * Accepts a date string and returns a string value ('Today', 'Yesterday', or 'Older')
 * based on the date's proximity to the current date.
 */
export const makeDateTextValue = (date: string): DateProximity => {
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const yesterday = new Date(
    today.getFullYear(),
    today.getMonth(),
    today.getDate() - 1
  )
  const notificationDate = new Date(date)

  let textValue: DateProximity
  if (notificationDate >= today) {
    textValue = DateProximity.Today
  } else if (notificationDate >= yesterday) {
    textValue = DateProximity.Yesterday
  } else {
    textValue = DateProximity.Older
  }

  return textValue
}

/**
 * This custom hook returns:
 * a) handler for updating single notification's status
 * b) handler for marking all notifications as read
 * c) a Boolean value of whether the user intends to mark all notifications as read
 */
export const useNotificationMutations = () => {
  const { t } = useTranslation()
  const snackbar = useSnackbar()
  const queryClient = useQueryClient()
  const [isIntendedToMarkAllAsRead, setIsIntendedToMarkAllAsRead] =
    useState(false)

  const notificationsStatsQuery = useGetNotificationsStats()
  const canBeMarkedAsReadNotificationCount = notificationsStatsQuery.data?.find(
    ({ state }) => state === NotificationStatus.New
  )?.count

  const notificationStateMutation = useUpdateNotificationsStatuses({
    onSuccess: async () => {
      notificationsStatsQuery.refetch()
      await queryClient.invalidateQueries(['notifications'])
    },
    onError: () => {
      snackbar.show({
        title: t('plainText.anErrorOccurred'),
        description: t('plainText.pleaseTryAgain'),
        intent: 'negative',
      })
    },
  })

  const handleUpdateNotificationsStatuses = (
    notificationIds: string[],
    state: NotificationStatus
  ) => {
    notificationStateMutation.mutate({ notificationIds, state })
  }

  const markAllNotificationsAsReadMutation = useUpdateNotificationsStatuses({
    onSuccess: async () => {
      notificationsStatsQuery.refetch()
      await queryClient.invalidateQueries(['notifications'])
      setIsIntendedToMarkAllAsRead(false)
    },
    onError: () => {
      snackbar.show({
        title: t('plainText.anErrorOccurred'),
        description: t('plainText.pleaseTryAgain'),
        intent: 'negative',
      })
      setIsIntendedToMarkAllAsRead(false)
    },
  })

  // We immediately mark all notifications as read with a local state when this handler is called,
  // and show a snackbar to allow the user to undo the action, if
  // the user does not undo the action within 4 seconds (default autoclose time for snackbar),
  // we will send a request to the server to mark all notifications as read, if user
  // undoes the action, we will clear the local state and do nothing.
  const handleMarkAllNotificationsAsRead = (count: number) => {
    setIsIntendedToMarkAllAsRead(true)
    let timeId: NodeJS.Timeout
    snackbar.show({
      id: 'markAllAsRead',
      title: t('interpolation.countOfNotificationsMarkedAsRead', {
        count: canBeMarkedAsReadNotificationCount ?? count,
      }),
      intent: 'positive',
      actions: (
        <Button
          kind="primary"
          background="transparent"
          onClick={() => {
            snackbar.hide('markAllAsRead')
            clearTimeout(timeId)
            setIsIntendedToMarkAllAsRead(false)
          }}
        >
          {t('plainText.undo')}
        </Button>
      ),
    })
    timeId = setTimeout(() => {
      markAllNotificationsAsReadMutation.mutate({
        notificationIds: [],
        state: NotificationStatus.Open,
      })
    }, 4000)
  }

  return {
    handleUpdateNotificationsStatuses,
    handleMarkAllNotificationsAsRead,
    isIntendedToMarkAllAsRead,
    canMarkAllAsRead: (canBeMarkedAsReadNotificationCount ?? 0) > 0,
  }
}
