import { ResizeObserverContainer } from '@willow/common'
import { FILTER_PANEL_BREAKPOINT } from '@willow/ui'
import { PanelGroup } from '@willowinc/ui'
import { useState } from 'react'
import { useParams } from 'react-router'
import { styled } from 'twin.macro'
import TicketsContent from './TicketsContent'
import TicketsFilter from './TicketsFilter'
import TicketsHeaderContent from './TicketsHeaderContent'
import TicketsProvider from './TicketsProvider'

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))

export default function Tickets({
  siteId,
  showSite = true,
  showHeader = true,
  isScheduled,
  tab,
  onTabChange,
}) {
  const { ticketId } = useParams()
  const [currentPageWidth, setCurrentPageWidth] = useState(Infinity)
  const showFiltersPanel = currentPageWidth > FILTER_PANEL_BREAKPOINT

  return (
    <TicketsProvider
      siteId={siteId}
      showSite={showSite}
      showHeader={showHeader}
      isScheduled={isScheduled}
      selectedTicketId={ticketId}
      tab={tab}
      onTabChange={onTabChange}
      showSourceId
    >
      <ResizeObserverContainer onContainerWidthChange={setCurrentPageWidth}>
        <StyledPanelGroup>
          {showFiltersPanel ? (
            <TicketsFilter isWithinPortal={!showFiltersPanel} />
          ) : (
            <TicketsHeaderContent />
          )}
          <TicketsContent />
        </StyledPanelGroup>
      </ResizeObserverContainer>
    </TicketsProvider>
  )
}
