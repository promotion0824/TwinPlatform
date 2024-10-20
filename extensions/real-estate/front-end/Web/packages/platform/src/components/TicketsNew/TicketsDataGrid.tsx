import { FullSizeLoader, priorities, titleCase } from '@willow/common'
import {
  PriorityName,
  TextWithTooltip,
} from '@willow/common/insights/component/index'
import { Link, ProgressTotal, Time, User, useScopeSelector } from '@willow/ui'
import { DataGrid, GridColDef } from '@willowinc/ui'
import _ from 'lodash'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import styled from 'styled-components'
import { useSites } from '../../providers/sites/SitesContext'
import routes, { makeInsightLink } from '../../routes'
import SiteChip from '../../views/Portfolio/twins/page/ui/SiteChip'
import OverduePill from '../OverduePill/OverduePill'
import TicketStatusPill from '../TicketStatusPill/TicketStatusPill'

export default function TicketsDataGrid({
  response,
  isScheduled,
  isLoading,
  showSourceId,
  dataSegmentPropsPage,
  selectedTicket,
  setSelectedTicket,

  /**
   * Whether to display the site name for each ticket. You can turn this off if
   * all the tickets are from the same site.
   */
  includeSiteColumn,
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const history = useHistory()
  const sites = useSites()
  const { isScopeSelectorEnabled, location } = useScopeSelector()

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'status',
        headerName: t('labels.status'),
        renderCell: ({ row: { statusCode } }) => (
          <TicketStatusPill statusCode={statusCode} />
        ),
        minWidth: 140,
        flex: 1.2,
      },
      {
        field: 'summary',
        headerName: t('labels.summary'),
        valueGetter: ({ row }) => row.summary,
        minWidth: 150,
        flex: 2,
      },
      ...(isScheduled
        ? [
            {
              field: 'issueName',
              headerName: t('plainText.asset'),
              minWidth: 140,
              flex: 2,
              renderCell: ({ row: { issueName } }) =>
                issueName && (
                  <StyledPill>
                    <TextWithTooltip text={issueName} />
                  </StyledPill>
                ),
            },
          ]
        : []),
      ...(isScheduled
        ? [
            {
              field: 'scheduledDate',
              flex: 1.6,
              minWidth: 100,
              headerName: t('plainText.scheduledDate'),
              renderCell: ({ row }) => (
                <>
                  <StyledTime value={row.scheduledDate} format="date" />
                  <OverduePill ticket={row} />
                </>
              ),
            },
          ]
        : [
            {
              field: 'dueDate',
              flex: 1.5,
              minWidth: 120,
              headerName: t('labels.dueDate'),
              renderCell: ({ row }) => (
                <>
                  <StyledTime value={row.dueDate} format="date" />
                  <OverduePill ticket={row} />
                </>
              ),
            },
          ]),
      ...(includeSiteColumn
        ? [
            {
              field: 'site',
              headerName: titleCase({
                text: t('plainText.location'),
                language,
              }),
              flex: 1,
              minWidth: 150,
              renderCell: ({ row }) => {
                const siteName = sites?.find((s) => s.id === row.siteId)?.name
                return <SiteChip siteName={siteName} />
              },
            },
          ]
        : []),

      ...(isScheduled
        ? [
            {
              field: '',
              headerName: t('plainText.progress'),
              flex: 0.7,
              minWidth: 100,
              renderCell: ({ row }) => (
                <>
                  {row.tasks.length > 0 && (
                    <ProgressTotal
                      value={
                        row.tasks.filter((task) => task.isCompleted).length
                      }
                      total={row.tasks.length}
                    />
                  )}
                </>
              ),
            },
          ]
        : []),
      ...(!isScheduled
        ? [
            {
              field: 'sourceName',
              headerName: t('labels.source'),
              flex: 0.8,
              minWidth: 100,
              renderCell: ({ row }) =>
                row.insightId ? (
                  <ClickableSourceCell>
                    <StyledLink
                      onClick={(e) => {
                        e.stopPropagation()
                        history.push(
                          makeInsightLink({
                            insightId: row.insightId,
                            siteId: row.siteId,
                            scopeId: location?.twin?.id,
                            isScopeSelectorEnabled,
                          })
                        )
                      }}
                    >
                      {row.sourceName}
                    </StyledLink>
                  </ClickableSourceCell>
                ) : (
                  <TextWithTooltip text={row.sourceName} />
                ),
            },
          ]
        : []),
      ...(showSourceId && !isScheduled
        ? [
            {
              field: 'sourceId',
              flex: 1,
              minWidth: 120,
              headerName: t('plainText.sourceId'),
              valueGetter: ({ row }) =>
                row.sourceName.includes('Willow')
                  ? row.sequenceNumber
                  : row.externalId,
            },
          ]
        : []),
      {
        field: 'category',
        flex: 1,
        minWidth: 100,
        headerName: t('labels.category'),
        valueGetter: ({ row }) =>
          t(`ticketCategory.${_.camelCase(row.category)}`, {
            defaultValue: row.category,
          }),
      },
      {
        field: 'assignedTo',
        flex: 1.2,
        minWidth: 100,
        headerName: t('plainText.assignedTo'),
        renderCell: ({ row }) => (
          <User
            user={{
              name: row.assignedTo,
            }}
            displayAsText
          />
        ),
      },
      {
        field: 'priority',
        flex: 0.9,
        minWidth: 90,
        headerName: t('labels.priority'),
        renderCell: ({ row }) => (
          <PriorityName insightPriority={row.priority} />
        ),
      },
      {
        field: 'createdDate',
        flex: 1.2,
        minWidth: 140,
        headerName: t('labels.created'),
        renderCell: ({ row }) => (
          <EllipsisText>
            <Time value={row.createdDate} />
          </EllipsisText>
        ),
      },
      {
        field: 'updatedDate',
        flex: 1.2,
        minWidth: 140,
        headerName: t('labels.lastUpdated'),
        renderCell: ({ row }) => (
          <EllipsisText>
            <Time value={row.updatedDate} />
          </EllipsisText>
        ),
      },
    ],
    [history, includeSiteColumn, isScheduled, showSourceId, sites, t]
  )

  return isLoading ? (
    // loader is brought in separately, so as to perform autoSizing after data is loaded.
    <FullSizeLoader />
  ) : (
    <StyledDataGrid
      // re-render table if filter collapsed, this is to autoSize columns again.
      initialState={{
        sorting: {
          // Setting default sorting state on page load
          sortModel: [
            { field: 'priority', sort: 'asc' },
            { field: 'createdDate', sort: 'desc' },
          ],
        },
      }}
      data-segment="Ticket Selected"
      data-testid="ticket-result"
      data-segment-props={JSON.stringify({
        priority: priorities.find(
          (priority) => priority.id === selectedTicket?.priority
        )?.name,
        status: selectedTicket?.status,
        page: dataSegmentPropsPage || 'Tickets Page',
      })}
      rows={response}
      columns={columns}
      disableRowSelectionOnClick={false}
      // rowSelectionModel will accept an array of row id or [],
      // need this [] to update row deselection
      rowSelectionModel={selectedTicket?.id ? [selectedTicket?.id] : []}
      onRowSelectionModelChange={([id]) =>
        setSelectedTicket(response.find((ticket) => ticket.id === id))
      }
      disableMultipleRowSelection
      noRowsOverlayMessage={t('plainText.notTicketsFound')}
    />
  )
}

const StyledLink = styled(Link)(({ theme }) => ({
  '&&&': {
    color: theme.color.neutral.fg.default,
    textDecoration: 'underline',
  },

  '&:hover': {
    color: theme.color.neutral.fg.highlight,
  },
}))

const StyledDataGrid = styled(DataGrid)(({ theme }) => ({
  '&&&': {
    border: '0px',

    '.priorityCell': {
      padding: '0', // Overriding padding for Ticket priority cell
    },
  },

  // Overriding the default sorting icon only for priority column because
  // priority value of 1 is the highest priority (called Critical)
  // so descending sorted priority would be 1, 2, 3, 4, so we want to
  // rotate the icon 180deg to indicate the descending order
  '&&& [data-field="priority"] [title="Sort"]': {
    transform: 'rotate(180deg)',
  },
}))

// to ensure the inner content takes up the full height of the cell
// so that click inside the cell will always target the inner content
// instead of triggering row click (which opens the ticket drawer)
const ClickableSourceCell = styled.div({
  // <Cell /> coming from @willow/ui will wrap the content in a <Text />
  // and it has line-height of 1.4, we overwrite it here to ensure the text is
  // vertically centered
  '&&& *': {
    height: '100%',
    lineHeight: '48px',
  },
})

const StyledTime = styled(Time)(({ theme }) => ({
  marginRight: theme.spacing.s8,
}))

const EllipsisText = styled.div({
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
})

const StyledPill = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  alignItems: 'center',
  borderRadius: '2px',
  minWidth: theme.spacing.s32,
  padding: '1px 10px',
  border: `1px solid ${theme.color.neutral.border.default}`,
  textTransform: 'uppercase',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
}))
