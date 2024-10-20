import { useMemo } from 'react'
import { useParams, useHistory } from 'react-router'
import {
  Spacing,
  Loader,
  useDateTime,
  useApi,
  useUser,
  useAnalytics,
} from '@willow/mobile-ui'
import { useTicketStatuses } from '@willow/common'
import {
  isTicketStatusEquates,
  isTicketStatusIncludes,
  Status,
  Tab,
} from '@willow/common/ticketStatus'
import { native } from 'utils'
import { useLayout, useTickets } from '../../providers'
import Comments from '../../components/Comments/Comments'
import priorities from '../../components/priorities.json'
import TicketHeader from './TicketHeader'
import TicketImages from './TicketImages'
import TicketFooter from './TicketFooter'
import TicketSection from './TicketSection'
import TicketLabel from './TicketLabel'
import styles from './Ticket.css'
import { SpecialStatus } from '../../utils/ticketStatus'

export default function Ticket() {
  const analytics = useAnalytics()
  const api = useApi()
  const history = useHistory()
  const { siteId, ticketId } = useParams()
  const dateTime = useDateTime()
  const { getTicket, clearTickets } = useTickets()
  const user = useUser()
  const { setTitle, setShowBackButton } = useLayout()
  const { data: ticket, isFetching, updateCache } = getTicket(siteId, ticketId)
  const ticketStatuses = useTicketStatuses()
  const ticketStatus =
    ticket && ticketStatuses.getByStatusCode(ticket?.statusCode)

  const addComment = async (comment) => {
    const addedComment = await api.post(
      `/api/sites/${siteId}/tickets/${ticketId}/comments`,
      {
        text: comment,
      }
    )

    // todo: need update backend api
    if (!addedComment.creator) {
      addedComment.creator = {
        firstName: user.firstName,
        lastName: user.lastName,
      }
    }

    if (ticket) {
      const nextComments = [addedComment, ...(ticket.comments || [])]

      updateCache({
        ...ticket,
        comments: nextComments,
      })
    }
  }

  const handleStatusChange = async (status) => {
    if (status === SpecialStatus.reject) {
      history.push(`/tickets/sites/${siteId}/reject/${ticketId}`)
    } else if (status === Status.reassign) {
      history.push(`/tickets/sites/${siteId}/reassign/${ticketId}`)
    } else if (
      [Status.limitedAvailability, Status.resolved, Status.onHold].includes(
        status
      )
    ) {
      history.push(
        `/tickets/sites/${siteId}/action/${ticketId}?reason=${status}`
      )
    } else if (ticket) {
      const nextTicketStatus = ticketStatuses.getByStatus(status)
      const nextTicket = await api.put(
        `/api/sites/${siteId}/tickets/${ticketId}/status`,
        {
          statusCode: nextTicketStatus?.statusCode,
        }
      )

      updateCache(nextTicket)
      clearTickets(siteId, Tab.open)
    }
  }

  const handleReassignTicket = async () => {
    const nextTicket = await api.put(
      `/api/sites/${siteId}/tickets/${ticketId}/assignee`,
      {
        assigneeId: user.id,
        assigneeType: 'customerUser',
      }
    )

    updateCache(nextTicket)
    clearTickets(siteId, Tab.open)
  }

  const building = useMemo(() => {
    let ticketBuilding

    if (user && siteId) {
      const { sites } = user
      ticketBuilding = sites.find((site) => site.id === siteId)
    }

    return ticketBuilding || {}
  }, [user, siteId])

  const showSolution =
    ticket &&
    ticketStatus &&
    isTicketStatusIncludes(ticketStatus, [
      Status.limitedAvailability,
      Status.resolved,
      Status.closed,
    ])

  setTitle(`${ticket ? ticket.sequenceNumber : 'Ticket'}`)
  setShowBackButton(true)

  return isFetching ? (
    <Loader size="extraLarge" />
  ) : (
    <Spacing type="content">
      <TicketHeader ticket={ticket} />
      <div onScroll={(e) => native.scroll(e.target.scrollTop)}>
        <TicketSection icon="sandClock" title="Due By">
          <p className={styles.dueDate}>
            {dateTime(ticket.dueDate).format('date')}
          </p>
        </TicketSection>
        <TicketSection icon="file" title="Ticket Details">
          <TicketLabel value={ticket.reporterName} label="Requestor" />
          <TicketLabel value={ticket.reporterPhone} label="Contact Number" />
          <TicketLabel value={ticket.reporterEmail} label="Contact Email" />
          <TicketLabel value={ticket.reporterCompany} label="Company" />
          <TicketLabel value={ticket.category} label="Category" />
          <TicketLabel value={ticket.summary} label="Summary" />
          <TicketLabel value={ticket.description} label="Description" />
          <TicketLabel
            value={dateTime(ticket.updatedDate).format('dateTimeLong')}
            label="Last Updated"
          />
        </TicketSection>
        {showSolution && (
          <TicketSection icon="graph" title="Solution Details">
            <TicketLabel value={ticket.cause} label="Cause" />
            <TicketLabel value={ticket.solution} label="Solution" />
            <TicketLabel
              value={dateTime(ticket.resolvedDate).format('dateTimeLong')}
              label="Completed"
            />
          </TicketSection>
        )}
        <TicketSection icon="attachment" title="Attachment &amp; Screenshots">
          <TicketImages siteId={siteId} ticket={ticket} />
        </TicketSection>
        <TicketSection icon="gps" title="Location &amp; Asset">
          <TicketLabel value={building.name} label="Building" />
          <TicketLabel value={building.address} label="Building Address" />
          <TicketLabel value={ticket.floorCode} label="Floor" />
        </TicketSection>
        <TicketSection
          icon="comment"
          title={`Comments (${ticket.comments ? ticket.comments.length : 0})`}
        >
          <Comments
            allowComment={
              ticketStatus &&
              !isTicketStatusEquates(ticketStatus, Status.closed)
            }
            comments={ticket.comments}
            onAddComment={addComment}
          />
        </TicketSection>
      </div>
      <TicketFooter
        ticket={ticket}
        onStatusChange={handleStatusChange}
        onReassignTicket={handleReassignTicket}
      />
    </Spacing>
  )
}
