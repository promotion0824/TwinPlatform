import { FullSizeLoader, titleCase } from '@willow/common'
import {
  ErrorReload,
  NoNotifications,
  NotificationComponent,
  useGetNotifications,
  useMakeNotificationsWithHeader,
  useNotificationMutations,
} from '@willow/common/notifications'
import { DateProximity } from '@willow/common/notifications/types'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  Badge,
  Box,
  Button,
  Group,
  Icon,
  IconButton,
  Menu,
  Panel,
  PanelContent,
  PanelGroup,
  Stack,
  useTheme,
} from '@willowinc/ui'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import { Link } from 'react-router-dom'
import { css } from 'styled-components'
import useOntologyInPlatform from '../../../../../hooks/useOntologyInPlatform'
import routes from '../../../../../routes'

/**
 * The component displays up to 20 most recent notifications when user clicks on the "Bell" icon
 * on Header, and allows the user to mark all notifications or single notification as read.
 */
const NotificationsMenu = ({
  isOpened,
  onOpen,
  onChange,
  newCountSinceLastOpen = 0,
}: {
  isOpened: boolean
  onOpen: () => void
  onChange: () => void
  newCountSinceLastOpen?: number
}) => {
  const history = useHistory()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const theme = useTheme()
  const { data: { items: modelsOfInterest } = {} } = useModelsOfInterest()
  const { data: ontology } = useOntologyInPlatform()
  const notificationsQuery = useGetNotifications(undefined, {
    enabled: isOpened,
  })
  const notifications = useMakeNotificationsWithHeader({
    query: notificationsQuery,
    limit: 20,
  })
  const {
    handleUpdateNotificationsStatuses,
    handleMarkAllNotificationsAsRead,
    isIntendedToMarkAllAsRead,
    canMarkAllAsRead,
  } = useNotificationMutations()

  // The following hook will prevent the notification menu from closing
  // when clicking on the notification snackbar (e.g. user wants to clear a snackbar)
  useEffect(() => {
    const isElementWithinNotificationSnackbar = (element): boolean => {
      if (!element) {
        return false
      }
      if (element?.classList?.contains('mantine-Notifications-notification')) {
        return true
      }
      return isElementWithinNotificationSnackbar(element.parentElement)
    }

    const pointerdownHandler = (e: PointerEvent) => {
      if (
        e.button === 0 && // Check if the left mouse button is clicked
        isElementWithinNotificationSnackbar(e.target)
      ) {
        e.preventDefault()
        e.stopPropagation()
      }
    }

    document.addEventListener('pointerdown', pointerdownHandler)

    return () => {
      document.removeEventListener('pointerdown', pointerdownHandler)
    }
  }, [])

  return (
    <Menu
      position="bottom-end"
      width={398}
      opened={isOpened}
      onChange={onChange}
      onOpen={onOpen}
    >
      <Menu.Target>
        <Group pos="relative" pr="s8">
          {newCountSinceLastOpen > 0 && (
            <Badge
              pos="absolute"
              top={0}
              left="s24"
              color="purple"
              css={{
                zIndex: theme.zIndex.toast,
                '& *': {
                  fontSize: theme.font.body.xs.regular.fontSize,
                },
              }}
            >
              {newCountSinceLastOpen}
            </Badge>
          )}

          <IconButton
            background="transparent"
            icon="notifications"
            kind="secondary"
            data-testid="bell-icon"
            css={{
              '& *': {
                fontSize: theme.spacing.s24,
              },
            }}
          />
        </Group>
      </Menu.Target>
      <Menu.Dropdown
        mih="120px"
        mah="90vh"
        css={{
          overflowY: 'auto',
        }}
      >
        <PanelGroup>
          <Panel
            css={{
              border: 'none',
            }}
            hideHeaderBorder
            title={
              <Group w="100%" justify="space-between" py="s12">
                <Box
                  css={{
                    ...theme.font.heading.lg,
                    color: theme.color.neutral.fg.default,
                  }}
                >
                  {t('labels.notifications')}
                </Box>
                <Group>
                  <Button
                    kind="secondary"
                    prefix={<Icon icon="inbox" />}
                    tw="mr-2"
                  >
                    <Link
                      css={css({
                        textDecoration: 'none',
                        color: theme.color.neutral.fg.default,
                      })}
                      to={routes.notifications}
                      onClick={onChange}
                    >
                      {titleCase({ text: t('plainText.viewAll'), language })}
                    </Link>
                  </Button>
                  <Menu position="bottom-end" width={200}>
                    <Menu.Target>
                      <IconButton
                        icon="more_vert"
                        kind="secondary"
                        background="transparent"
                      />
                    </Menu.Target>
                    <Menu.Dropdown>
                      <Menu.Item
                        prefix={<Icon icon="settings" />}
                        onClick={() => {
                          history.push(routes.admin_notification_settings)
                        }}
                      >
                        {titleCase({
                          text: t('headers.notificationSettings'),
                          language,
                        })}
                      </Menu.Item>
                    </Menu.Dropdown>
                  </Menu>
                </Group>
              </Group>
            }
          >
            <PanelContent mih={120}>
              {notificationsQuery.status === 'loading' ? (
                <FullSizeLoader
                  css={{
                    flex: 1,
                  }}
                />
              ) : notificationsQuery.status === 'error' ? (
                <ErrorReload onReload={notificationsQuery.refetch} />
              ) : notificationsQuery.status === 'success' &&
                notificationsQuery.data.pages[0].total === 0 ? (
                <NoNotifications />
              ) : (
                notifications.map((notification, index) => {
                  if (Object.keys(DateProximity).includes(notification.id)) {
                    return (
                      <Box
                        css={{
                          position: 'sticky',
                          top: 0,
                          zIndex: theme.zIndex.sticky,
                        }}
                        key={notification.id}
                      >
                        <Group w="100%" justify="space-between" gap={0}>
                          <Stack
                            css={{
                              ...theme.font.heading.group,
                              color: theme.color.intent.secondary.fg.default,
                              flex: index === 0 ? 1 : 0,
                            }}
                            justify="center"
                            miw="100px"
                            h="s32"
                            pl="s16"
                            bg="neutral.bg.panel.default"
                          >
                            {t(
                              `plainText.${notification.id.toLocaleLowerCase()}`
                            ).toUpperCase()}
                          </Stack>
                          {/* A clickable section to mark all notifications as read,
                          we only need to display this section for the first group */}
                          {index === 0 && canMarkAllAsRead && (
                            <Stack
                              justify="center"
                              pr="s16"
                              h="s32"
                              css={{
                                ...theme.font.body.xs.regular,
                                color: theme.color.intent.primary.fg.default,
                                '&:hover': {
                                  cursor: 'pointer',
                                  color: theme.color.intent.primary.fg.hovered,
                                },
                              }}
                              bg="neutral.bg.panel.default"
                              onClick={() => {
                                handleMarkAllNotificationsAsRead(
                                  notificationsQuery.data?.pages[0].total || 0
                                )
                              }}
                            >
                              {titleCase({
                                text: t('plainText.markAllAsRead'),
                                language,
                              })}
                            </Stack>
                          )}
                        </Group>
                      </Box>
                    )
                  } else {
                    return (
                      <NotificationComponent
                        modelsOfInterest={modelsOfInterest}
                        ontology={ontology}
                        key={notification.id}
                        notification={notification}
                        onNotificationsStatusesChange={
                          handleUpdateNotificationsStatuses
                        }
                        isMarkingAsRead={isIntendedToMarkAllAsRead}
                      />
                    )
                  }
                })
              )}
            </PanelContent>
          </Panel>
        </PanelGroup>
      </Menu.Dropdown>
    </Menu>
  )
}

export default NotificationsMenu
