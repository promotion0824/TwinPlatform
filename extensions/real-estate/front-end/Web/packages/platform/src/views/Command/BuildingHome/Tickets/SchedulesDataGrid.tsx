import { DataGrid, GridColDef, Icon } from '@willowinc/ui'
import {
  AssetPill,
  Text,
  Time,
  User,
  MoreButton,
  MoreButtonDropdownOption,
  dateComparator,
  MoreButtonDropdown,
} from '@willow/ui'
import _ from 'lodash'
import { titleCase, FullSizeLoader } from '@willow/common'
import { useMemo, MouseEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { ScheduleTicket } from '../../../../services/Tickets/TicketsService'
import NotFound from '../../../../components/Insights/ui/NotFound'
import getRecurrence from '../../Tickets/Schedules/Recurrence'

export default function SchedulesDataGrid({
  schedules,
  onSelectedScheduleId,
  onShowModal,
  status,
  isArchived,
}: {
  schedules: ScheduleTicket[]
  onSelectedScheduleId: (id: string) => void
  onShowModal: (e: MouseEvent<HTMLButtonElement>, schedule: string) => void
  status: string
  isArchived: boolean
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'summary',
        headerName: t('plainText.name'),
        valueGetter: ({ row: schedule }) =>
          `${t('plainText.scheduleOf')} ${schedule.summary}`,
        flex: 1.6,
        minWidth: 200,
      },
      {
        field: 'assets',
        headerName: t('plainText.asset'),
        minWidth: 200,
        flex: 3,
        sortComparator: (array1, array2) =>
          _.sortBy(array1, 'assetName')[0]?.assetName.localeCompare(
            _.sortBy(array2, 'assetName')[0]?.assetName
          ),
        renderCell: ({ row: schedule }) => (
          <div tw="flex">
            {schedule?.assets?.slice(0, 2).map((asset) => (
              <AssetPill key={asset.id}>{asset.assetName}</AssetPill>
            ))}
            {schedule?.assets?.length > 2 && (
              <Text size="tiny">
                +{schedule.assets.length - 2} {t('labels.more')}
              </Text>
            )}
          </div>
        ),
      },
      {
        field: 'nextTicketDate',
        headerName: t('plainText.nextTicketDate'),
        minWidth: 100,
        flex: 0.8,
        sortComparator: dateComparator,
        renderCell: ({ row: schedule }) => (
          <Time format="date" value={schedule.nextTicketDate} />
        ),
      },
      {
        field: 'recurrence',
        headerName: t('plainText.recurrence'),
        minWidth: 100,
        flex: 0.8,
        valueGetter: ({ row: schedule }) =>
          getRecurrence({ recurrence: schedule.recurrence, t }),
      },
      {
        field: 'assignee',
        headerName: t('plainText.assignedTo'),
        minWidth: 100,
        flex: 1.4,
        valueGetter: ({ row: schedule }) => schedule.assignee?.name,
        renderCell: ({ row: schedule }) =>
          schedule.assignee != null && (
            <User user={schedule.assignee} displayAsText />
          ),
      },
      {
        field: 'startDate',
        headerName: t('labels.startDate'),
        minWidth: 100,
        flex: 0.8,
        disableSortBy: false,
        sortComparator: dateComparator,
        valueGetter: ({ row: schedule }) => schedule.recurrence.startDate,
        renderCell: ({ row: schedule }) => (
          <Time value={schedule.recurrence.startDate} format="date" />
        ),
      },
      {
        field: 'createdDate',
        headerName: t('labels.createdDate'),
        minWidth: 130,
        flex: 0.8,
        sortComparator: dateComparator,
        renderCell: ({ row: schedule }) => (
          <Time value={schedule.createdDate} />
        ),
      },
      {
        field: 'updatedDate',
        headerName: t('plainText.lastUpdatedDate'),
        minWidth: 130,
        flex: 0.8,
        sortComparator: dateComparator,
        renderCell: ({ row: schedule }) => (
          <Time value={schedule.updatedDate} />
        ),
      },
      ...(!isArchived
        ? [
            {
              field: 'id',
              headerName: '',
              flex: 0.4,
              minWidth: 44,
              renderCell: ({ row: schedule }) => (
                <MoreButtonDropdown withinPortal>
                  <MoreButtonDropdownOption
                    suffix={<Icon icon="arrow_forward" />}
                    onClick={() => onSelectedScheduleId(schedule.id)}
                  >
                    {t('plainText.scheduleSettings')}
                  </MoreButtonDropdownOption>
                  <MoreButtonDropdownOption
                    suffix={<Icon icon="delete" />}
                    onClick={(e: MouseEvent<HTMLButtonElement>) =>
                      onShowModal(e, schedule)
                    }
                  >
                    {t('headers.archiveSchedule')}
                  </MoreButtonDropdownOption>
                </MoreButtonDropdown>
              ),
            },
          ]
        : []),
    ],
    [onShowModal, isArchived, onSelectedScheduleId, t]
  )

  return status === 'loading' ? (
    <FullSizeLoader />
  ) : (
    <DataGrid
      columns={columns}
      rows={schedules}
      initialState={{
        pinnedColumns: {
          right: ['id'],
        },
        sorting: {
          sortModel: [{ field: 'updatedDate', sort: 'desc' }],
        },
      }}
      slots={{
        noRowsOverlay: () => (
          <NotFound
            message={titleCase({
              language,
              text: t('plainText.noSchedulesFound'),
            })}
          />
        ),
      }}
    />
  )
}
