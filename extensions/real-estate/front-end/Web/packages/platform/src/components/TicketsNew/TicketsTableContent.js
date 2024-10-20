import styled from 'styled-components'
import {
  AssetPill,
  Flex,
  ProgressTotal,
  Table,
  Head,
  Body,
  Row,
  Cell,
  Time,
  User,
  Link,
} from '@willow/ui'
import { useTicketStatuses, priorities } from '@willow/common'
import { useSites } from 'providers'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import OverduePill from '../OverduePill/OverduePill'
import TicketStatusPill from '../TicketStatusPill/TicketStatusPill'
import { useTickets } from './TicketsContext'

export default function TicketsTableContent({
  response,
  selectedTicket,
  setSelectedTicket,
  onItemsSortChange,

  /**
   * Whether to display the site name for each ticket. You can turn this off if
   * all the tickets are from the same site.
   */
  includeSiteColumn,
  isLoading,
  isError,
}) {
  const sites = useSites()
  const tickets = useTickets()
  const { t } = useTranslation()
  const ticketStatuses = useTicketStatuses()
  const {
    dataSegmentPropsPage,
    filters,
    isScheduled,
    showSourceId,
    setSelectedLink,
  } = tickets

  // Exclude selectedSources and selectedCategories that are not in tickets list.
  // - We stored all selected filters from all the tabs in tickets page, so applied filters will persist
  //   when switching tabs.
  const selectedCategories = filters.selectedCategories.filter((category) =>
    filters.categories.includes(category)
  )
  const selectedSources = filters.selectedSources.filter((source) =>
    filters.sources.includes(source)
  )
  const selectedAssignees = tickets.filters.selectedAssignees.filter(
    (assignee) => tickets.filters.assignees.includes(assignee)
  )

  const items = response
    .filter(
      (ticket) =>
        ticket.sequenceNumber
          .toLowerCase()
          .includes(tickets.filters.search.toLowerCase()) ||
        ticket.summary
          .toLowerCase()
          .includes(tickets.filters.search.toLowerCase()) ||
        // assignedTo is a nullable field
        ticket?.assignedTo
          ?.toLowerCase()
          ?.includes(tickets.filters.search.toLowerCase())
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
    <Table
      items={items}
      defaultSort={['priority asc', 'createdDate desc']}
      notFound={t('plainText.notTicketsFound')}
      onItemsSortChange={onItemsSortChange}
      isLoading={isLoading || ticketStatuses.isLoading}
      isError={isError}
    >
      {(ticketsItems) => (
        <>
          <Head>
            <Row>
              <Cell sort="sequenceNumber">{t('plainText.id')}</Cell>
              <Cell sort="summary" width="1fr">
                {t('labels.summary')}
              </Cell>
              {includeSiteColumn && <Cell sort="site">{t('labels.site')}</Cell>}
              {isScheduled && (
                <Cell sort="issueName">{t('plainText.asset')}</Cell>
              )}
              {isScheduled ? (
                <Cell sort="dueDate">{t('plainText.scheduledDate')}</Cell>
              ) : (
                <Cell sort="dueDate">{t('labels.dueDate')}</Cell>
              )}
              <Cell sort="status" width="minmax(141px, min-content)">
                {t('labels.status')}
              </Cell>
              {isScheduled && <Cell>{t('plainText.progress')}</Cell>}
              <Cell sort="sourceName">{t('labels.source')}</Cell>
              {showSourceId && (
                <Cell sort="sourceId">{t('plainText.sourceId')}</Cell>
              )}
              <Cell sort="category" width="minmax(106px, min-content)">
                {t('labels.category')}
              </Cell>
              <Cell sort="assignedTo">{t('plainText.assignedTo')}</Cell>
              <Cell sort="createdDate">{t('labels.created')}</Cell>
              <Cell sort="updatedDate">{t('labels.lastUpdated')}</Cell>
            </Row>
          </Head>
          <Body>
            {ticketsItems.map((ticket) => {
              const isInsightExist =
                ticket.insightId != null && ticket.insightId !== ''

              return (
                <Row
                  key={ticket.id}
                  color={`var(--priority${ticket.priority})`}
                  selected={selectedTicket?.id === ticket.id}
                  onClick={() => setSelectedTicket(ticket)}
                  data-segment="Ticket Selected"
                  data-testid="ticket-result"
                  data-segment-props={JSON.stringify({
                    priority: priorities.find(
                      (priority) => priority.id === ticket.priority
                    )?.name,
                    status: ticket.status,
                    page: dataSegmentPropsPage || 'Tickets Page',
                  })}
                >
                  <Cell>{ticket.sequenceNumber}</Cell>
                  <Cell>{ticket.summary}</Cell>
                  {includeSiteColumn && <Cell>{ticket.site}</Cell>}
                  {isScheduled && (
                    <Cell type="fill">
                      {ticket.issueName != null && (
                        <AssetPill>{ticket.issueName}</AssetPill>
                      )}
                    </Cell>
                  )}
                  {isScheduled && (
                    <Cell sort="scheduledDate" type="fill">
                      <Flex horizontal align="middle right" size="medium">
                        <Time value={ticket.scheduledDate} format="date" />
                        <OverduePill ticket={ticket} />
                      </Flex>
                    </Cell>
                  )}
                  {!isScheduled && (
                    <Cell sort="dueDate" type="fill">
                      <Flex horizontal align="middle right" size="medium">
                        <Time value={ticket.dueDate} format="date" />
                        <OverduePill ticket={ticket} />
                      </Flex>
                    </Cell>
                  )}
                  <Cell type="fill">
                    <TicketStatusPill statusCode={ticket.statusCode} />
                  </Cell>
                  {isScheduled && (
                    <Cell type="fill">
                      {ticket.tasks.length > 0 && (
                        <ProgressTotal
                          value={
                            ticket.tasks.filter((task) => task.isCompleted)
                              .length
                          }
                          total={ticket.tasks.length}
                        />
                      )}
                    </Cell>
                  )}
                  <StyledCell>
                    {isInsightExist ? (
                      <StyledLink
                        onClick={(e) => {
                          e.stopPropagation()
                          setSelectedLink({
                            insightId: ticket.insightId,
                            siteId: ticket.siteId,
                          })
                        }}
                      >
                        {ticket.sourceName}
                      </StyledLink>
                    ) : (
                      ticket.sourceName
                    )}
                  </StyledCell>
                  {showSourceId && <Cell>{ticket.externalId}</Cell>}
                  <Cell>
                    {t(`ticketCategory.${_.camelCase(ticket.category)}`, {
                      defaultValue: ticket.category,
                    })}
                  </Cell>
                  <Cell type="fill">
                    <User
                      user={{
                        name: ticket.assignedTo,
                      }}
                      displayAsText
                    />
                  </Cell>
                  <Cell>
                    <Time value={ticket.createdDate} />
                  </Cell>
                  <Cell>
                    <Time value={ticket.updatedDate} />
                  </Cell>
                </Row>
              )
            })}
          </Body>
        </>
      )}
    </Table>
  )
}

const StyledLink = styled(Link)({
  font: '400 12px Poppins',
  color: 'var(--light)',
  textDecoration: 'underline',

  '&:hover': {
    color: 'var(--light)',
  },
})

// to ensure the inner content takes up the full height of the cell
// so that click inside the cell will always target the inner content
// instead of triggering row click (which opens the ticket drawer)
const StyledCell = styled(Cell)({
  padding: '0 8px',
  // <Cell /> coming from @willow/ui will wrap the content in a <Text />
  // and it has line-height of 1.4, we overwrite it here to ensure the text is
  // vertically centered
  '&&& *': {
    height: '100%',
    lineHeight: '48px',
  },
})
