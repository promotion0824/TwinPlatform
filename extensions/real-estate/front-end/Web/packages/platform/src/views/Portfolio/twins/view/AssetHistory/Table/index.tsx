import { useTranslation } from 'react-i18next'
import {
  getInsightStatusTranslatedName,
  getTicketStatusTranslatedName,
} from '@willow/ui'
import { styled } from 'twin.macro'
import { TFunction } from 'i18next'
import { Badge, DataGrid } from '@willowinc/ui'
import _ from 'lodash'
import { useMemo } from 'react'
import { Row } from 'react-table'
import {
  PriorityBadge,
  TextWithTooltip,
} from '@willow/common/insights/component'
import { calculatePriority } from '@willow/common/insights/costImpacts/getInsightPriority'
import {
  formatDateTime,
  getPriorityByRange,
} from '@willow/common/insights/costImpacts/utils'
import { titleCase } from '@willow/common'
import TicketStatusPill from '../../../../../../components/TicketStatusPill/TicketStatusPill'
import InsightWorkflowStatusPill from '../../../../../../components/InsightStatusPill/InsightWorkflowStatusPill'

/**
 * AssetHistoryTable is the table that displays the list of standard tickets,
 * scheduled tickets, insights, and inspections.
 */
export default function AssetHistoryTable({
  assetHistory,
  assetHistoryQueryStatus,
  onSelectItem,
  filterType,
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const columns = useMemo(
    () => [
      {
        headerName: t('labels.type'),
        field: 'assetHistoryType',
        id: 'assetHistoryType',
        renderCell: ({ row }: { row: Row }) => (
          <Badge size="lg" variant="muted" color="gray">
            <TextWithTooltip
              text={_.startCase(
                t(`plainText.${typeTranslationKeys[row.assetHistoryType]}`)
              )}
              tooltipWidth="124px"
              isTitleCase={false}
            />
          </Badge>
        ),
        minWidth: 90,
        maxWidth: 200,
        flex: 1,
      },
      {
        headerName: t('labels.status'),
        field: 'status',
        id: 'status',
        renderCell: ({ value, row }: { value: string; row: Row }) => {
          const title = getTranslatedStatus(
            t,
            row.assetHistoryType ?? filterType,
            filterType === 'insight' || row.assetHistoryType === 'insight'
              ? row.lastStatus
              : value
          )

          if (row.assetHistoryType === 'insight') {
            return (
              <InsightWorkflowStatusPill
                size="md"
                lastStatus={row.lastStatus}
              />
            )
          }

          if (
            ['standardTicket', 'scheduledTicket'].includes(row.assetHistoryType)
          ) {
            return <TicketStatusPill statusCode={row.statusCode} />
          }

          const statusColors = {
            due: 'orange',
            overdue: 'red',
            completed: 'green',
          }

          // inspection
          return (
            <Badge
              variant="dot"
              size="md"
              color={statusColors[row.status] ?? 'gray'}
            >
              {title && _.capitalize(title)}
            </Badge>
          )
        },
        minWidth: 90,
        maxWidth: 150,
        flex: 1,
      },
      {
        headerName: titleCase({
          text:
            filterType === 'all'
              ? `${t('plainText.skill')} / ${t('labels.summary')}`
              : filterType === 'insight'
              ? t('plainText.skill')
              : t('labels.summary'),
          language,
        }),
        field: 'name',
        id: 'name',
        renderCell: ({ row }) => (
          <TextWithTooltip
            text={row.name || row.description}
            tooltipWidth="200px"
            isTitleCase={false}
          />
        ),
        minWidth: 150,
        flex: 1,
      },
      {
        headerName: t('labels.priority'),
        field: 'priority',
        id: 'priority',
        renderCell: ({ row }) => (
          <PriorityBadge
            priority={getPriorityByRange(
              calculatePriority({
                impactScores: row.impactScores,
                language,
                insightPriority: row.priority,
              })
            )}
          />
        ),
        minWidth: 90,
        maxWidth: 130,
        flex: 1,
      },
      {
        headerName: titleCase({
          language,
          text:
            filterType === 'insight'
              ? t('plainText.lastFaultedOccurrence')
              : `${t('plainText.lastFaultedOccurrence')} / ${t(
                  'plainText.dateCreated'
                )}`,
        }),
        field: 'date',
        id: 'date',
        valueGetter: ({ row }) =>
          formatDateTime({
            value:
              row.assetHistoryType === 'insight' ? row.occurredDate : row.date,
            language,
            timeZone: row?.timeZone,
          }),
        flex: 1,
        minWidth: 150,
        maxWidth: 300,
      },
    ],
    [filterType, language, t]
  )

  return (
    <StyledDataGrid
      loading={assetHistoryQueryStatus === 'loading'}
      rows={assetHistory}
      columns={columns}
      disableRowSelectionOnClick={false}
      disableMultipleRowSelection
      onRowClick={({ row }) => onSelectItem(row)}
      noRowsOverlayMessage={_.startCase(t('plainText.noAssetFound'))}
      autosizeOnMount
      autosizeOptions={{
        includeHeaders: true,
      }}
    />
  )
}

const StyledDataGrid = styled(DataGrid)({
  '&&&': {
    border: '0px',
  },
})

const inspectionsTranslationKeys = {
  due: 'due',
  overdue: 'overdue',
  completed: 'completed',
  missed: 'missed',
  notRequired: 'notRequired',
}

const typeTranslationKeys = {
  standardTicket: 'ticket',
  scheduledTicket: 'scheduledTicket',
  inspection: 'inspection',
  insight: 'insight',
}

export function getTranslatedStatus(
  t: TFunction,
  type: string,
  status: string
) {
  switch (type) {
    case 'insight':
      return getInsightStatusTranslatedName(t, status)
    case 'inspection':
      return t(`plainText.${inspectionsTranslationKeys[status]}`)
    default:
      return getTicketStatusTranslatedName(t, status)
  }
}
