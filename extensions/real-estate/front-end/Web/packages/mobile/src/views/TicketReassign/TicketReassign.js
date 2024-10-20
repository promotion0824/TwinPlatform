import { useMemo, useState, useEffect } from 'react'
import { useParams } from 'react-router'
import _ from 'lodash'
import {
  Button,
  Form,
  Loader,
  Option,
  Select,
  useSnackbar,
  useApi,
} from '@willow/mobile-ui'
import { Status, Tab } from '@willow/common/ticketStatus'
import { useLayout, useTickets } from '../../providers'
import TicketLabel from '../TicketAction/TicketLabel'
import TicketStatusPill from '../../components/TicketStatusPill/TicketStatusPill.tsx'
import TicketConfirm from '../TicketConfirm/TicketConfirm'
import styles from './TicketReassign.css'

export default function TicketReassign() {
  const snackbar = useSnackbar()
  const api = useApi()
  const { siteId, ticketId } = useParams()
  const { getTicket, clearTickets, getPossibleTicketAssignees } = useTickets()
  const { setTitle, setShowBackButton } = useLayout()
  const { data: ticket, updateCache } = getTicket(siteId, ticketId)
  const { data: possibleTicketAssignees = [] } =
    getPossibleTicketAssignees(siteId)
  const [isSubmitted, setIsSubmitted] = useState()
  const [ticketAssigneeId, setTicketAssigneeId] = useState('unassigned')

  const orderedPossibleTicketAssignees = _.orderBy(
    possibleTicketAssignees,
    (assignee) => `${assignee.firstName} ${assignee.lastName}`.toLowerCase()
  )

  useEffect(() => {
    setTicketAssigneeId(ticket?.assigneeId ?? 'unassigned')
  }, [ticket])

  const reassignTicket = async () => {
    if (ticket?.assigneeId === ticketAssigneeId) {
      return
    }

    try {
      let data
      if (ticketAssigneeId === 'unassigned') {
        data = {
          assigneeType: 'noAssignee',
        }
      } else {
        data = {
          assigneeId: ticketAssigneeId,
          assigneeType: 'customerUser',
        }
      }
      const nextTicket = await api.put(
        `/api/sites/${siteId}/tickets/${ticketId}/assignee`,
        data
      )

      updateCache(nextTicket)
      clearTickets(siteId, Tab.open)
      setIsSubmitted(true)
    } catch {
      snackbar.show('An error has occurred while reassigning ticket')
    }
  }

  const pageTitle = useMemo(() => {
    if (isSubmitted) {
      return null
    }

    return ticket ? ticket.sequenceNumber : 'Reassign Ticket'
  }, [isSubmitted, ticket])

  setTitle(pageTitle)
  setShowBackButton(true)

  if (!ticket) {
    return <Loader />
  }

  return isSubmitted ? (
    <TicketConfirm siteId={siteId} title="Ticket Reassigned" />
  ) : (
    <Form
      defaultValue={ticket}
      onSubmit={reassignTicket}
      showSubmitted={false}
      showSuccessful={false}
    >
      <TicketStatusPill isHeader status={Status.reassign} />
      <div className={styles.ticketContent}>
        <TicketLabel label="Reassign To">
          <Select
            value={ticketAssigneeId}
            text={(value) => {
              const assignee = orderedPossibleTicketAssignees.find(
                (x) => x.id === value
              )
              return assignee
                ? `${assignee.firstName} ${assignee.lastName}`
                : 'Unassigned'
            }}
            onChange={(nextValue) => setTicketAssigneeId(nextValue)}
          >
            <Option key="unassigned" value="unassigned">
              Unassigned
            </Option>
            {orderedPossibleTicketAssignees.map((nextValue) => (
              <Option key={nextValue.id} value={nextValue.id}>
                {`${nextValue.firstName} ${nextValue.lastName}`}
              </Option>
            ))}
          </Select>
        </TicketLabel>
      </div>
      <footer className={styles.footer}>
        <Button
          size="large"
          color="white"
          type="submit"
          className={styles.button}
          disabled={(ticket?.assigneeId ?? 'unassigned') === ticketAssigneeId}
        >
          Reassign
        </Button>
      </footer>
    </Form>
  )
}
