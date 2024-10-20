import { FullSizeLoader } from '@willow/common'
import { PriorityBadge } from '@willow/common/insights/component/index'
import { getPriorityValue } from '@willow/common/insights/costImpacts/getInsightPriority'
import {
  formatDateTime,
  getPriorityByRange,
} from '@willow/common/insights/costImpacts/utils'
import { Message, NotFound, Pill, Progress, dateComparator } from '@willow/ui'
import { DataGrid, GridColDef } from '@willowinc/ui'
import _ from 'lodash'
import { useMemo } from 'react'
import styled, { css } from 'styled-components'
import AssetDetailsModal from '../../../../components/AssetDetailsModal/AssetDetailsModal'
import { useDashboard } from '../../Dashboard/DashboardContext'

export default function TicketsDataGrid({
  t,
  language,
  selectedTicketId,
  onSelectedTicketIdChange,
}) {
  const { ticketsQuery } = useDashboard()
  const filteredTickets = ticketsQuery.data ?? []
  const selectedTicket = filteredTickets.find(
    (ticket) => ticket.id === selectedTicketId
  )

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'sequenceNumber',
        headerName: t('plainText.id'),
        valueGetter: ({ row }) => row.sequenceNumber,
        width: 130,
      },
      {
        field: 'summary',
        headerName: t('labels.summary'),
        valueGetter: ({ row }) => row.summary,
        width: 120,
      },
      {
        field: 'floorCode',
        headerName: t('labels.floor'),
        renderCell: ({ row }) =>
          row.floorCode !== '' && <Pill>{row.floorCode}</Pill>,
        width: 70,
      },
      {
        field: 'priority',
        headerName: t('labels.priority'),
        renderCell: ({ row }) => (
          <PriorityBadge
            priority={getPriorityByRange(
              getPriorityValue({ insightPriority: row.priority })
            )}
          />
        ),
      },
      {
        field: 'createdDate',
        headerName: t('labels.createdDate'),
        valueGetter: ({ row }) =>
          formatDateTime({
            value: row.createdDate,
            language,
          }),
        sortComparator: dateComparator,
        width: 140,
      },
      {
        field: 'updatedDate',
        headerName: t('labels.lastUpdated'),
        valueGetter: ({ row }) =>
          formatDateTime({
            value: row.createdDate,
            language,
          }),
        sortComparator: dateComparator,
        width: 140,
      },
    ],
    [language, t]
  )

  return (
    <>
      {ticketsQuery.isError ? (
        <div
          css={css`
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100%;
          `}
        >
          <Message icon="error">{t('plainText.errorOccurred')}</Message>
        </div>
      ) : ticketsQuery.isLoading ? (
        <FullSizeLoader />
      ) : (
        <StyledDataGrid
          initialState={{
            sorting: {
              // Setting default sorting state on page load
              sortModel: [
                { field: 'priority', sort: 'asc' },
                { field: 'createdDate', sort: 'desc' },
              ],
            },
          }}
          rows={filteredTickets}
          columns={columns}
          disableRowSelectionOnClick={false}
          rowSelectionModel={selectedTicketId}
          onRowSelectionModelChange={([id]) =>
            onSelectedTicketIdChange(
              filteredTickets.find((ticket) => ticket.id === id)?.id
            )
          }
          slots={{
            loadingOverlay: Progress, // Custom loading to override with MUI loading icon
            noRowsOverlay: () => (
              <NotFound>{t('plainText.notTicketsFound')}</NotFound>
            ),
          }}
          disableMultipleRowSelection
          autosizeOptions={{
            includeHeaders: true,
          }}
        />
      )}
      {selectedTicketId != null && (
        <AssetDetailsModal
          siteId={selectedTicket.siteId}
          item={{ ...selectedTicket, modalType: 'ticket' }}
          onClose={() => onSelectedTicketIdChange(undefined)}
          dataSegmentPropPage="Tickets table"
          navigationButtonProps={{
            items: [],
            selectedItem: undefined,
            setSelectedItem: _.noop,
          }}
        />
      )}
    </>
  )
}

const StyledDataGrid = styled(DataGrid)({
  '&&&': {
    border: '0px',
    height: '100%',
  },

  // Overriding the default sorting icon only for priority column because
  // priority value of 1 is the highest priority (called Critical)
  // so descending sorted priority would be 1, 2, 3, 4, so we want to
  // rotate the icon 180deg to indicate the descending order
  '&&& [data-field="priority"] [title="Sort"]': {
    transform: 'rotate(180deg)',
  },

  '& .MuiBadge-anchorOriginTopRightRectangular': {
    top: 5,
  },
})
