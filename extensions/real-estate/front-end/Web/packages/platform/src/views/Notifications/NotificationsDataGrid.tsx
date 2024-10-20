import { FullSizeContainer, FullSizeLoader, titleCase } from '@willow/common'
import {
  NoNotifications,
  NotificationComponent,
} from '@willow/common/notifications'
import {
  DateProximity,
  Notification,
  NotificationStatus,
  noMoreNotificationsId,
} from '@willow/common/notifications/types'
import { Ontology } from '@willow/common/twins/view/models'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { DataGrid, GridColDef, Stack } from '@willowinc/ui'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'styled-components'

/**
 * The NotificationsDataGrid component displays a list of notifications; we choose to
 * use DataGrid as it comes with virtualization and can easily cooperate with infinite
 * scroll.
 */
export default function NotificationsDataGrid({
  total,
  search,
  isLoading,
  notifications,
  modelsOfInterest,
  ontology,
  fetchNextPage,
  onUpdateNotificationStatus,
  onMarkAllNotificationsAsRead,
  isMarkingAsRead,
  canMarkAllAsRead,
}: {
  total?: number
  search?: string
  isLoading: boolean
  notifications: Array<Notification>
  fetchNextPage: () => void
  modelsOfInterest?: ModelOfInterest[]
  ontology?: Ontology
  onUpdateNotificationStatus: (ids: [string], state: NotificationStatus) => void
  onMarkAllNotificationsAsRead: () => void
  isMarkingAsRead: boolean
  canMarkAllAsRead: boolean
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'id',
        headerName: titleCase({
          text: t('plainText.notification'),
          language,
        }),
        flex: 1,
        renderCell: ({ row }) =>
          // End of the list message
          row.id === noMoreNotificationsId ? (
            <NoNotifications
              w={254}
              message={t('plainText.thatIsAllYourNotifications')}
            />
          ) : isCustomHeaderRow(row) ? (
            <NotificationComponent
              modelsOfInterest={modelsOfInterest}
              ontology={ontology}
              notification={row}
              onNotificationsStatusesChange={onUpdateNotificationStatus}
              isMarkingAsRead={isMarkingAsRead}
            />
          ) : (
            <Stack
              css={css(({ theme }) => ({
                ...theme.font.heading.group,
              }))}
              c="intent.secondary.fg.default"
              pl="s16"
            >
              {t(`plainText.${row.id.toLowerCase()}`).toUpperCase()}
            </Stack>
          ),
      },
    ],
    [
      isMarkingAsRead,
      language,
      modelsOfInterest,
      onUpdateNotificationStatus,
      ontology,
      t,
    ]
  )

  return (
    <>
      {!isLoading && canMarkAllAsRead && (
        <Stack
          pos="absolute"
          h="s48"
          left={350}
          justify="center"
          css={css(({ theme }) => ({
            ...theme.font.body.xs.regular,
            color: theme.color.intent.primary.fg.default,
            '&:hover': {
              cursor: 'pointer',
              color: theme.color.intent.primary.fg.hovered,
            },
            zIndex: theme.zIndex.sticky,
          }))}
          onClick={() => {
            onMarkAllNotificationsAsRead()
          }}
        >
          {titleCase({
            text: t('plainText.markAllAsRead'),
            language,
          })}
        </Stack>
      )}
      {!isLoading && total === 0 ? (
        <FullSizeContainer>
          <NoNotifications search={search} />
        </FullSizeContainer>
      ) : (
        <StyledDataGrid
          loading={isLoading}
          columns={columns}
          rows={notifications}
          onRowsScrollEnd={fetchNextPage}
          isRowSelectable={({ row }) => !isCustomHeaderRow(row.id)}
          hideFooter
          slots={{
            loadingOverlay: () => <FullSizeLoader />,
          }}
          getRowHeight={({ id }) => {
            if (Object.keys(DateProximity).includes(id.toString())) {
              return 48
            } else if (id === noMoreNotificationsId) {
              return 254
            } else {
              return 94
            }
          }}
        />
      )}
    </>
  )
}

const isCustomHeaderRow = (row: { id: string }) =>
  !Object.keys(DateProximity).includes(row.id)

const StyledDataGrid = styled(DataGrid)({
  '&&&': {
    border: '0px',
  },
  // We target the custom header rows and disable background color change on hover
  // as they are not clickable per design
  [[...Object.keys(DateProximity), noMoreNotificationsId]
    .map(
      (proximity) =>
        `[data-id="${proximity}"].Mui-hovered, [data-id="${proximity}"].MuiDataGrid-row`
    )
    .join(', ')]: {
    backgroundColor: 'transparent',
  },
  '.MuiDataGrid-cell': {
    border: 'none',
    '&:focus-within': {
      outline: 'none',
    },
  },
  '.MuiDataGrid-columnHeaders': {
    display: 'none',
  },
  '.MuiDataGrid-virtualScroller': {
    overflowX: 'hidden',
  },
})
