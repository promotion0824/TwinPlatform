import { DebouncedSearchInput, titleCase } from '@willow/common'
import useMultipleSearchParams, {
  ParamsDict,
} from '@willow/common/hooks/useMultipleSearchParams'
import {
  ErrorReload,
  useGetNotifications,
  useMakeNotificationsWithHeader,
  useNotificationMutations,
} from '@willow/common/notifications'
import {
  NotificationFilterOperator,
  NotificationStatus,
} from '@willow/common/notifications/types'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { DocumentTitle, getContainmentHelper } from '@willow/ui'
import {
  Badge,
  Box,
  Button,
  Checkbox,
  CheckboxGroup,
  Drawer,
  Group,
  Icon,
  Indicator,
  PageTitle,
  PageTitleItem,
  Panel,
  PanelContent,
  PanelGroup,
  Stack,
  useDisclosure,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useTheme } from 'styled-components'
import useOntologyInPlatform from '../../hooks/useOntologyInPlatform'
import HeaderWithTabs from '../Layout/Layout/HeaderWithTabs'
import NotificationsDataGrid from './NotificationsDataGrid'

const notificationContainer = 'notificationContainer'
const { containerName, getContainerQuery } = getContainmentHelper(
  notificationContainer
)

/**
 * The Notifications page shows a list of notifications for the user,
 * and allows the user to filter, search, save, and clear notifications.
 */
function Notifications() {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const [params, setSearchParams] = useMultipleSearchParams([
    {
      name: 'status',
      type: 'array',
    },
    'search',
  ])
  const {
    status,
    search,
  }: {
    status?: NotificationStatus[]
    search?: string
  } = params
  const theme = useTheme()

  const [drawerOpened, { close: closeDrawer, open: openDrawer }] =
    useDisclosure(false)

  const notificationsQuery = useGetNotifications({
    page: 1,
    pageSize: 20,
    sortSpecifications: [
      { field: 'notification.createdDateTime', sort: 'desc' },
    ],
    filterSpecifications: [
      ...((status ?? []).length > 0
        ? [
            {
              field: 'state',
              operator: NotificationFilterOperator.ContainedIn,
              value: status,
            },
          ]
        : []),
      ...(search
        ? [
            {
              field: 'notification.title',
              operator: NotificationFilterOperator.Contains,
              value: search,
            },
          ]
        : []),
    ],
  })
  const total = notificationsQuery?.data?.pages?.[0]?.total || 0
  const after = notificationsQuery?.data?.pages?.at(-1)?.after || 0
  const notifications = useMakeNotificationsWithHeader({
    query: notificationsQuery,
    after,
    search,
    total,
  })
  const { data: { items: modelsOfInterest } = {} } = useModelsOfInterest()
  const { data: ontology } = useOntologyInPlatform()
  const {
    handleUpdateNotificationsStatuses,
    handleMarkAllNotificationsAsRead,
    isIntendedToMarkAllAsRead,
    canMarkAllAsRead,
  } = useNotificationMutations()

  return (
    <>
      <DocumentTitle
        scopes={[
          titleCase({
            language,
            text: t('labels.notifications'),
          }),
        ]}
      />
      <HeaderWithTabs
        css={`
          border-bottom: none;
        `}
        titleRow={[
          <PageTitle
            id="notifications-page-titles"
            key="notifications-page-titles"
            mr="auto"
            mb="auto"
          >
            {[
              {
                text: 'labels.notifications',
                to: window.location.pathname,
              },
            ].map(({ text, to }) => {
              const commonSuffix = titleCase({
                text: t(text),
                language,
              })
              return (
                <PageTitleItem key={text}>
                  {to ? <Link to={to}>{commonSuffix}</Link> : commonSuffix}
                </PageTitleItem>
              )
            })}
          </PageTitle>,
          <Stack key="notifications-header-controls">
            <Indicator
              disabled={(!status || status.length === 0) && !search}
              // + 32 because the Panel to be shown when screen size is less than tabletLandscape
              // has a 16px padding on both sides
              css={`
                @media (min-width: ${`${
                    Number.parseInt(theme.breakpoints.tabletLandscape, 10) + 32
                  }px`}) {
                  display: none;
                }
              `}
            >
              <Button
                w="100%"
                kind="secondary"
                onClick={openDrawer}
                prefix={<Icon icon="filter_list" />}
              >
                {t('headers.filters')}
              </Button>
            </Indicator>
          </Stack>,
        ]}
      />
      <Drawer
        header={t('headers.filters')}
        opened={drawerOpened}
        onClose={closeDrawer}
        size="xs"
      >
        <Box p="s16" pt="0">
          <NotificationsFilter
            {...makeNotificationFilterProps(params, setSearchParams)}
          />
        </Box>
      </Drawer>
      <PanelGroup
        m="s16"
        w="auto"
        direction="horizontal"
        css={{
          containerType: 'size',
          containerName,
        }}
      >
        <Panel
          defaultSize={292}
          collapsible
          title={t('headers.filters')}
          css={`
            ${getContainerQuery(
              `width < ${theme.breakpoints.tabletLandscape}`
            )} {
              display: none;
            }
          `}
        >
          <PanelContent p="s16" pt="0">
            <NotificationsFilter
              {...makeNotificationFilterProps(params, setSearchParams)}
            />
          </PanelContent>
        </Panel>
        <Panel
          title={
            <Group gap={8}>
              {t('labels.notifications')}
              {notificationsQuery.status === 'success' && total > 0 && (
                <Badge>{total}</Badge>
              )}
            </Group>
          }
          className="notifications-data-grid"
          css={`
            ${getContainerQuery(
              `width < ${theme.breakpoints.tabletLandscape}`
            )} {
              margin-left: 0 !important;
            }
          `}
        >
          <PanelContent h="100%">
            {notificationsQuery.isError ? (
              <ErrorReload onReload={notificationsQuery.refetch} />
            ) : (
              <NotificationsDataGrid
                total={total}
                search={search}
                isLoading={notificationsQuery.isFetching}
                notifications={notifications}
                fetchNextPage={() => {
                  if (
                    notificationsQuery.hasNextPage &&
                    !notificationsQuery.isFetchingNextPage
                  ) {
                    notificationsQuery.fetchNextPage()
                  }
                }}
                modelsOfInterest={modelsOfInterest}
                ontology={ontology}
                onUpdateNotificationStatus={handleUpdateNotificationsStatuses}
                onMarkAllNotificationsAsRead={() =>
                  handleMarkAllNotificationsAsRead(
                    notificationsQuery.data?.pages?.[0]?.total ?? 0
                  )
                }
                isMarkingAsRead={isIntendedToMarkAllAsRead}
                canMarkAllAsRead={canMarkAllAsRead}
              />
            )}
          </PanelContent>
        </Panel>
      </PanelGroup>
    </>
  )
}

const makeNotificationFilterProps = (
  params: ParamsDict,
  setSearchParams: (nextParams: {
    [key: string]: string | string[] | null
  }) => void
) => ({
  search: params.search as string | undefined,
  status: params.status as NotificationStatus[],
  onFilterChange: (params: ParamsDict) =>
    setSearchParams({
      ...params,
      status: params.status || null,
    }),
  onSearch: (params: ParamsDict) =>
    setSearchParams({
      ...params,
      search: params.search || null,
    }),
})

const NotificationsFilter = ({
  search,
  status,
  onFilterChange,
  onSearch,
}: {
  search?: string
  status?: NotificationStatus[]
  onFilterChange?: (params: ParamsDict) => void
  onSearch?: (params: ParamsDict) => void
}) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <>
      <DebouncedSearchInput
        label={t('labels.search')}
        key={(search ?? '').toString()}
        onDebouncedSearchChange={onSearch}
        value={search?.toString()}
        mt="s12"
      />
      <CheckboxGroup
        mt="s12"
        label={titleCase({
          text: t('labels.state'),
          language,
        })}
        onChange={(nextStatus) => {
          onFilterChange?.({
            status: nextStatus,
          })
        }}
        value={status}
      >
        {[
          [NotificationStatus.New, 'plainText.unread'],
          [NotificationStatus.Open, 'plainText.read'],
        ].map(([state, text]) => (
          <Checkbox
            key={state}
            label={titleCase({
              text: t(text),
              language,
            })}
            value={state}
            checked
          />
        ))}
      </CheckboxGroup>
    </>
  )
}

export default Notifications
