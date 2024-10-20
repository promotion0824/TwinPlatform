import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'

import { Tab as TicketTab } from '@willow/common/ticketStatus'
import { Panel, PanelContent, Tabs } from '@willowinc/ui'
import { useTicketStatuses } from '@willow/common'
import { useTickets } from './TicketsContext'
import TicketsTable from './TicketsTable'

const StyledPanelContent = styled(PanelContent)({
  height: '100%',
})

export default function TicketsContent() {
  const tickets = useTickets()
  const ticketStatus = useTicketStatuses()
  const { t } = useTranslation()

  const handleTabChange = (newTab) => {
    if (tickets.tab !== newTab) {
      tickets.onTabChange(newTab)
    }
  }
  const isTicketStatusConfigured = ticketStatus.data?.length > 0
  const ticketTabs = [
    {
      header: t('headers.open'),
      tab: TicketTab.open,
      value: 'tickets-tab-open',
    },
    /**
     * Resolved will be called Completed.
     * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80429
     * team has settled on using "Resolved" instead of "Completed" on Frontend only,
     * so we are setting the tab to be "Resolved" but display "Completed" on UI.
     * */
    {
      header: t('headers.completed'),
      tab: TicketTab.resolved,
      value: 'tickets-tab-resolved',
    },
    {
      header: t('headers.closed'),
      tab: TicketTab.closed,
      value: 'tickets-tab-closed',
    },
  ]
  return (
    <Panel
      id="tickets-content-panel"
      tabs={
        <Tabs
          defaultValue={ticketTabs[0].value}
          onTabChange={handleTabChange}
          value={
            ticketTabs.find((ticketTab) => ticketTab.tab === tickets.tab).tab
          }
        >
          <Tabs.List>
            {ticketTabs.map(({ header, tab, value }) => {
              // The shouldRenderTab is true if ticket status is not configured or if it's configured and the tab included in the configuration
              const shouldRenderTab =
                !isTicketStatusConfigured ||
                ticketStatus.data?.some((x) => x.tab === tab)
              return (
                shouldRenderTab && (
                  <Tabs.Tab data-testid={value} key={value} value={tab}>
                    {header}
                  </Tabs.Tab>
                )
              )
            })}
          </Tabs.List>
          <StyledPanelContent>
            <TicketsTable />
          </StyledPanelContent>
        </Tabs>
      }
    />
  )
}
