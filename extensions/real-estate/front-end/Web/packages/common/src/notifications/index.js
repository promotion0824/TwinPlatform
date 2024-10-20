export {
  ErrorReload,
  NoNotifications,
  default as NotificationComponent,
} from './components'
export {
  useGetNotifications,
  useGetNotificationsStats,
  useMakeNotificationsWithHeader,
  useUpdateNotificationsStatuses,
} from './hooks'
export { makeDateTextValue, useNotificationMutations } from './utils'
export { lastDateTimeOpenNotificationBellKey }

const lastDateTimeOpenNotificationBellKey = 'last-time-open-notification-bell'
