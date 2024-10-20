import {
  NotificationData,
  notifications,
  NotificationsStore as SnackbarsStore,
} from '@mantine/notifications'
import { createContext, forwardRef, useContext } from 'react'
import styled, { useTheme } from 'styled-components'

import { Intent } from '../../common'
import { Icon, IconName } from '../../misc/Icon'
import { Loader } from '../Loader'

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
interface SnackbarDataBaseProps {
  /** Notification id, can be used to close or update notification */
  id?: string
  /**
   * Title of snackbar component.
   */
  title?: React.ReactNode
  /**
   * Dictates the color of the snackbar.
   * @default 'primary'
   */
  intent?: Intent
  /**
   * Actions which can be performed on snackbars.
   */
  actions?: React.ReactNode
  /**
   * Description which will be displayed in the component.
   */
  description?: React.ReactNode
  /**
   * Auto close timeout for all notifications in ms, false to disable auto close,
   * can be overwritten for individual notifications in notifications.show function,
   * @default 4000
   */
  autoClose?: NotificationData['autoClose']
  /** Called when notification closes */
  onClose?: (props: NotificationData) => void
  /** Called when notification opens */
  onOpen?: (props: NotificationData) => void
  loading?: boolean
}
export const SnackbarDataPropsDiv = forwardRef<
  HTMLDivElement,
  SnackbarDataBaseProps
>(() => <div />)

export interface SnackbarData
  extends Omit<
      NotificationData,
      'title' | 'color' | 'message' | 'icon' | 'loading'
    >,
    SnackbarDataBaseProps {}

interface SnackbarContextProps {
  /** Adds given snackbar to the snackbars list or queue. */
  show: (props: SnackbarData, store?: SnackbarsStore) => void
  /** Removes snackbar with given id from the notifications state and queue. */
  hide: (id: string, store?: SnackbarsStore) => void
  /** Updates snackbar that was previously added to the state or queue. */
  update: (props: SnackbarData, store?: SnackbarsStore) => void
  /** Removes all snackbars from the snackbars state and queue */
  clean: (store?: SnackbarsStore) => void
  /** Removes all notifications from the queue */
  cleanQueue: (store?: SnackbarsStore) => void
}
/** For displaying storybook props */
export const SnackbarMethodPropsDiv = forwardRef<
  HTMLDivElement,
  SnackbarContextProps
>(() => <div />)

const SnackbarContext = createContext<SnackbarContextProps | undefined>(
  undefined
)

export function useSnackbar() {
  const context = useContext(SnackbarContext)
  if (!context) {
    throw new Error('useSnackbar requires a SnackbarProvider')
  }
  return context
}

export function SnackbarProvider({ children }: { children: React.ReactNode }) {
  const theme = useTheme()
  const context = {
    /**
     * Adds snackbar to snackbar list or queue depending on current state and limit
     */
    show(props: SnackbarData) {
      const {
        intent = 'primary',
        description,
        actions,
        loading,
        ...restProps
      } = props
      notifications.show({
        ...restProps,
        color: theme.color.intent[intent].fg.default,
        title: <Title title={props.title} intent={intent} loading={loading} />,
        message: <Description description={description} actions={actions} />,
      })
    },

    /**
     * Removes snackbar based on its id from snackbar state and queue
     */
    hide(id: string) {
      notifications.hide(id)
    },

    /**
     * Updates snackbar that was previously added to the state or queue and display it based on new content provided that the id of the snackbar is same.
     */
    update(props: SnackbarData) {
      const {
        intent = 'primary',
        description,
        actions,
        loading,
        ...restProps
      } = props
      notifications.update({
        ...restProps,
        color: theme.color.intent[intent].fg.default,
        title: <Title title={props.title} intent={intent} loading={loading} />,
        message: <Description description={description} actions={actions} />,
      })
    },

    /**
     * Removes all snackbar from state and queue
     */
    clean() {
      notifications.clean()
    },

    cleanQueue() {
      notifications.cleanQueue()
    },
  }

  return (
    <SnackbarContext.Provider value={context as SnackbarContextProps}>
      {children}
    </SnackbarContext.Provider>
  )
}

/**
 * Title component will display the color and icons based on intent passed.
 */
export function Title({
  title,
  intent,
  loading = false,
}: {
  title?: React.ReactNode
  intent: Intent
  loading?: boolean
}) {
  return (
    <StyledTitle $intent={intent}>
      {loading ? (
        <Loader intent="secondary" />
      ) : (
        <Icon icon={getIconMapping(intent)} size={20} />
      )}
      {title}
    </StyledTitle>
  )
}

/**
 * Description component will display the description text and actions to be carried out.
 */
export function Description({
  description,
  actions,
}: {
  description?: React.ReactNode
  actions?: React.ReactNode
}) {
  return description || actions ? (
    <StyledDescription>
      <div>{description}</div>
      <div>{actions}</div>
    </StyledDescription>
  ) : null
}

// Mapping of icons based on intent.
export const getIconMapping = (intent: Intent) => {
  const iconsMapping: Record<Intent, IconName> = {
    primary: 'info',
    secondary: 'info',
    negative: 'warning',
    positive: 'check_circle',
    notice: 'report',
  }
  return iconsMapping[intent]
}

const StyledTitle = styled.div<{ $intent: Intent }>(({ theme, $intent }) => ({
  display: 'flex',
  alignItems: 'center',
  ...theme.font.heading.sm,
  color: theme.color.neutral.fg.default,

  '> span': {
    marginRight: theme.spacing.s8,
    color: theme.color.intent[$intent].fg.default,
  },
}))

const StyledDescription = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
  marginLeft: '28px',
}))
