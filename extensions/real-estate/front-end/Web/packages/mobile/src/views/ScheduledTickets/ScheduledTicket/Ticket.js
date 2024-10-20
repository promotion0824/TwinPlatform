import { useMemo } from 'react'
import { useParams, useHistory } from 'react-router'
import {
  Spacing,
  useDateTime,
  useApi,
  useUser,
  useAnalytics,
} from '@willow/mobile-ui'
import { useTicketStatuses } from '@willow/common'
import { isTicketStatusEquates, Status, Tab } from '@willow/common/ticketStatus'
import cx from 'classnames'
import { native } from 'utils'
import { useLayout, useTickets } from '../../../providers'
import Comments from '../../../components/Comments/Comments'
import TicketTopBar from './TicketTopBar'
import TicketHeader from './TicketHeader'
import TicketTasks from './TicketTasks'
import TicketImages from './TicketImages'
import TicketFooter from '../../Ticket/TicketFooter'
import TicketSection from './TicketSection'
import TicketLabel from '../Common/TicketLabel'
import styles from './Ticket.css'
import { SpecialStatus } from '../../../utils/ticketStatus'

export default function Ticket({ ticket, updateTicket }) {
  const analytics = useAnalytics()
  const api = useApi()
  const history = useHistory()
  const { siteId, ticketId } = useParams()
  const dateTime = useDateTime()
  const { clearScheduledTickets } = useTickets()
  const user = useUser()
  const { setTitle, setShowBackButton } = useLayout()

  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(ticket.statusCode)

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

      updateTicket({
        ...ticket,
        comments: nextComments,
      })
    }
  }

  const handleStatusChange = async (status) => {
    if (status === SpecialStatus.reject) {
      history.push(`/sites/${siteId}/scheduled-tickets/reject/${ticketId}`)
    } else if (Status.reassign === status) {
      history.push(`/sites/${siteId}/scheduled-tickets/reassign/${ticketId}`)
    } else if (
      [Status.limitedAvailability, Status.resolved, Status.onHold].includes(
        status
      )
    ) {
      history.push(
        `/sites/${siteId}/scheduled-tickets/action/${ticketId}?reason=${status}`
      )
    } else if (ticket) {
      const nextTicketStatus = ticketStatuses.getByStatus(status)
      const nextTicket = await api.put(
        `/api/sites/${siteId}/tickets/${ticketId}/status`,
        {
          statusCode: nextTicketStatus?.statusCode,
        }
      )

      updateTicket(nextTicket)
      clearScheduledTickets(siteId, Tab.open)
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

    updateTicket(nextTicket)
    clearScheduledTickets(siteId, Tab.open)
  }

  const building = useMemo(() => {
    let ticketBuilding

    if (user && siteId) {
      const { sites } = user
      ticketBuilding = sites.find((x) => x.id === siteId)
    }

    return ticketBuilding || {}
  }, [user, siteId])

  const showSolution =
    ticket &&
    [Status.limitedAvailability, Status.resolved, Status.closed].includes(
      ticketStatus?.status
    )

  setTitle(`${ticket ? ticket.sequenceNumber : 'Ticketssss'}`)
  setShowBackButton(true)

  const mainContentClassName = cx(styles.mainContent, {
    [styles.isOpenTicket]: isTicketStatusEquates(ticketStatus, Status.open),
  })

  return (
    <Spacing type="content">
      <TicketTopBar ticket={ticket} />
      <div
        className={mainContentClassName}
        onScroll={(e) => native.scroll(e.target.scrollTop)}
      >
        <TicketHeader ticket={ticket} />
        <TicketTasks ticket={ticket} updateTicket={updateTicket} />
        <TicketSection icon="group" title="Ticket Details">
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
            <TicketLabel value={ticket.notes} label="Notes" />
            <TicketLabel
              value={dateTime(ticket.resolvedDate).format('dateTimeLong')}
              label="Completed"
            />
          </TicketSection>
        )}
        <TicketSection icon="attachment" title="Attachments &amp; Screenshots">
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
        ticketType="scheduled"
      />
    </Spacing>
  )
}
