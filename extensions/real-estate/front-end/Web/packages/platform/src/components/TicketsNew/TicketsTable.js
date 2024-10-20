/* eslint-disable complexity */
import { intersection, noop } from 'lodash'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory, useParams } from 'react-router'
import { styled } from 'twin.macro'

import { useTicketStatuses } from '@willow/common'
import { useConfig, useDateTime, useScopeSelector } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'

import AssetDetailsModal from 'components/AssetDetailsModal/AssetDetailsModal.tsx'
import ManageTicketCategoriesModal from 'components/TicketsNew/ManageTicketCategoriesModal/ManageTicketCategoriesModal'
import { useSites } from '../../providers'
import { useTickets } from './TicketsContext'
import TicketsDataGrid from './TicketsDataGrid'
import filterTicketByDueDate from './filterByDueDate'

/**
 * this handler will enable user to click on
 * asset name on Ticket Detail Drawer and navigate to
 * twin explorer of that asset, once user click on "back"
 * button, user will come back to Ticket page with
 * same Ticket Detail Drawer opened.
 *
 * Note: this behavior is only relevant to
 * - packages\platform\src\components\TicketsNew\TicketsTable.js (main Tickets Page)
 * - packages\platform\src\views\Command\Dashboard\FloorViewer\Floor\SidePanel\Content\InProgress.js (Classic explorer page)
 * - packages\platform\src\components\TimeSeries\PointSelector\AssetModal\Content\Tickets.js (Ticket page)
 *
 */
function useGetHandleSelectedTicket({
  siteId,
  isScheduled,
  onSelectedTicketIdChange,
  setShowNewTicket,
  isScopeSelectorEnabled,
  scopeId,
}) {
  const history = useHistory()

  return (ticketToSelect) => {
    const pathnameForSite = `/sites/${siteId}/tickets${
      isScheduled ? '/scheduled-tickets' : ''
    }${ticketToSelect?.id ? `/${ticketToSelect.id}` : ''}`
    const pathnameForPortfolio = `/tickets${
      isScheduled ? '/scheduled-tickets' : ''
    }${ticketToSelect?.id ? `/${ticketToSelect.id}` : ''}`

    const scopedPathSuffix = ticketToSelect?.id
      ? `${isScheduled ? '/scheduled-tickets' : '/ticket'}/${ticketToSelect.id}`
      : `${isScheduled ? '/scheduled-tickets' : ''}`
    const pathnameForScope = `/tickets/scope/${scopeId}${scopedPathSuffix}`
    const pathnameForAllScopes = `/tickets${scopedPathSuffix}`

    const isTimeSeries = history.location?.pathname?.includes('time-series')
    const isClassicExplorer = history.location.pathname.includes('/floors/')

    // update ticketId query string param when user is in time series or Classic explorer page
    if (isTimeSeries || isClassicExplorer) {
      onSelectedTicketIdChange?.(ticketToSelect?.id)
    } else {
      if (isScopeSelectorEnabled) {
        history.push({
          pathname: scopeId ? pathnameForScope : pathnameForAllScopes,
          search: new URLSearchParams(history.location.search).toString(),
        })
      } else {
        history.push({
          pathname: siteId ? pathnameForSite : pathnameForPortfolio,
          search: new URLSearchParams(history.location.search).toString(),
        })
      }
      setShowNewTicket(false)
    }
  }
}

export default function TicketsTable({ onSelectedTicketIdChange }) {
  const {
    isLoading,
    ticketsList = [],
    siteId,
    selectedTicket,
    dataSegmentPropsPage,
    isScheduled,
    filters,
    showSourceId,
    selectedLink,
    setSelectedLink,
  } = useTickets()
  const sites = useSites()
  const ticketStatuses = useTicketStatuses()
  const dateTime = useDateTime()
  const { scopeId, isScopeSelectorEnabled, location, isScopeUsedAsBuilding } =
    useScopeSelector()

  const handleSelectedTicket = useGetHandleSelectedTicket({
    siteId,
    isScheduled,
    onSelectedTicketIdChange,
    setShowNewTicket: noop,
    scopeId,
    isScopeSelectorEnabled,
  })

  // Exclude selectedSources and selectedCategories that are not in tickets list.
  // - We stored all selected filters from all the tabs in tickets page, so applied filters will persist
  //   when switching tabs.
  const selectedCategories = filters.selectedCategories.filter((category) =>
    filters.categories.includes(category)
  )
  const selectedSources = filters.selectedSources.filter((source) =>
    filters.sources.includes(source)
  )
  const selectedAssignees = filters.selectedAssignees.filter((assignee) =>
    filters.assignees.includes(assignee)
  )
  const selectedStatuses = intersection(
    filters.selectedStatuses,
    filters.statuses.map((statues) => statues.statusCode)
  )
  const items = ticketsList
    .filter(
      (ticket) =>
        ticket.sequenceNumber
          .toLowerCase()
          .includes(filters.search.toLowerCase()) ||
        ticket.summary.toLowerCase().includes(filters.search.toLowerCase()) ||
        // assignedTo is a nullable field
        ticket?.assignedTo
          ?.toLowerCase()
          ?.includes(filters.search.toLowerCase())
    )
    .filter(
      (ticket) =>
        filters.selectedPriorities.length === 0 ||
        filters.selectedPriorities.includes(ticket.priority)
    )
    .filter(
      (ticket) =>
        selectedSources.length === 0 ||
        selectedSources.includes(ticket.sourceName)
    )
    .filter(
      (ticket) =>
        selectedCategories.length === 0 ||
        selectedCategories.includes(ticket.category)
    )
    .filter(
      (ticket) =>
        selectedAssignees.length === 0 ||
        selectedAssignees.includes(ticket.assignedTo)
    )
    .filter(
      (ticket) =>
        selectedStatuses.length === 0 ||
        selectedStatuses.includes(ticket.statusCode)
    )
    .filter(
      (ticket) =>
        filters.dueBy.length === 0 ||
        filters.selectedDueBy == null ||
        filterTicketByDueDate(
          ticket,
          filters.selectedDueBy,
          dateTime,
          ticketStatuses
        )
    )
    .map((ticket) => {
      const idAsInteger = parseInt(ticket.externalId, 10)
      return {
        ...ticket,
        site: sites.find((site) => site.id === ticket.siteId)?.name ?? '-',
        status: ticketStatuses.getByStatusCode(ticket.statusCode)?.status,
        // When sorting the external id column we sort by the value of the sourceId
        sourceId: Number.isNaN(idAsInteger) ? ticket.externalId : idAsInteger,
      }
    })

  return (
    <>
      <TicketTableWrapper>
        <TicketsDataGrid
          response={items}
          isScheduled={isScheduled}
          isLoading={isLoading || ticketStatuses.isLoading}
          showSourceId={showSourceId}
          dataSegmentPropsPage={dataSegmentPropsPage}
          selectedTicket={selectedTicket}
          setSelectedTicket={handleSelectedTicket}
          // If current scope is used as building, it means there can possibly be 1 location,
          // otherwise, there can be multiple locations
          includeSiteColumn={!isScopeUsedAsBuilding(location)}
        />
      </TicketTableWrapper>
      {selectedTicket != null && (
        <AssetDetailsModal
          siteId={selectedTicket?.siteId ?? siteId}
          item={{ ...selectedTicket, modalType: 'ticket' }}
          onClose={handleSelectedTicket}
          isUpdatedTicket
          navigationButtonProps={{
            items,
            selectedItem: selectedTicket,
            setSelectedItem: handleSelectedTicket,
          }}
        />
      )}
      {/* do not display this modal when selectedTicket is defined,
          it'll avoid displaying two modals at the same time */}
      {selectedLink.insightId &&
        selectedLink.siteId &&
        selectedTicket == null && (
          <AssetDetailsModal
            siteId={selectedLink.siteId}
            item={{ id: selectedLink.insightId, modalType: 'insight' }}
            onClose={() => setSelectedLink({ insightId: null, siteId: null })}
          />
        )}
    </>
  )
}

export const TicketsControls = ({ isScheduled }) => {
  const { scopeId, isScopeSelectorEnabled } = useScopeSelector()
  const { siteId } = useParams()
  const config = useConfig()
  const { t } = useTranslation()
  const [showManageTicketCategoriesModal, setShowManageTicketCategoriesModal] =
    useState(false)
  const [showNewTicket, setShowNewTicket] = useState(false)

  const handleSelectedTicket = useGetHandleSelectedTicket({
    siteId,
    isScheduled,
    onSelectedTicketIdChange: undefined,
    setShowNewTicket,
    scopeId,
    isScopeSelectorEnabled,
  })

  return (
    <>
      {config.hasFeatureToggle('wp-ticket-categories-enabled') &&
        siteId != null && (
          <Button
            kind="secondary"
            onClick={() => setShowManageTicketCategoriesModal(true)}
          >
            {t('plainText.manageTicketCategories')}
          </Button>
        )}
      {showManageTicketCategoriesModal && (
        <ManageTicketCategoriesModal
          siteId={siteId}
          onClose={() => setShowManageTicketCategoriesModal(false)}
        />
      )}

      <Button
        prefix={<Icon icon="add" />}
        onClick={() => setShowNewTicket(true)}
      >
        {t('plainText.addTicket')}
      </Button>
      {showNewTicket && (
        <>
          <AssetDetailsModal
            siteId={siteId}
            item={{ modalType: 'ticket' }}
            onClose={handleSelectedTicket}
            isUpdatedTicket={false}
            navigationButtonProps={{
              items: [],
              selectedItem: undefined,
              setSelectedItem: handleSelectedTicket,
            }}
          />
        </>
      )}
    </>
  )
}

const TicketTableWrapper = styled.div({
  display: 'flex',
  height: '100%',
  flexFlow: 'column',
  flexShrink: '0',
  // because we apply 2 sorting on tickets table by default (priority and createdDate),
  // there is a small number indicator that appears on the right of the column header
  // we move it a bit down so it is visible
  '& .MuiBadge-anchorOriginTopRightRectangular': {
    top: 5,
  },
})
