import { useMemo, useState } from 'react'
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

  const orderedAssignees = _.orderBy(possibleTicketAssignees, (assignee) =>
    assignee.name.toLowerCase()
  )
  const customerUserAssignees = orderedAssignees.filter(
    (assignee) => assignee.type === 'customerUser'
  )
  const workgroupAssignees = orderedAssignees.filter(
    (assignee) => assignee.type === 'workGroup'
  )

  const reassignTicket = async (form) => {
    if (ticket?.assigneeId === form.value.assigneeId) {
      return
    }

    try {
      const nextTicket = await api.put(
        `/api/sites/${siteId}/tickets/${ticketId}/assignee`,
        {
          assigneeId: form.value.assigneeId,
          assigneeType: form.value.assigneeType,
        }
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
      {(form) => (
        <>
          <TicketStatusPill isHeader status={Status.reassign} />
          <div className={styles.ticketContent}>
            <TicketLabel label="Reassign To">
              <Select
                value={form.value.assigneeId}
                placeholder="Unassigned"
                header={() => form.value.assignee.name}
                onChange={(assignee) => {
                  form.setValue((prevValue) => ({
                    ...prevValue,
                    assignee,
                    assigneeId: assignee.id,
                    assigneeType: assignee.type,
                  }))
                }}
              >
                <Option key="unassigned" value={{ type: 'noAssignee' }}>
                  Unassigned
                </Option>
                <Option disabled className={styles.headerOption}>
                  Assignees
                </Option>
                {customerUserAssignees.map((nextValue) => (
                  <Option
                    key={nextValue.id}
                    value={nextValue}
                    selected={nextValue.id === form.value.assigneeId}
                  >
                    {nextValue.name}
                  </Option>
                ))}
                <Option disabled className={styles.headerOption}>
                  Workgroups
                </Option>
                {workgroupAssignees.map((nextValue) => (
                  <Option
                    key={nextValue.id}
                    value={nextValue}
                    selected={nextValue.id === form.value.assigneeId}
                  >
                    {nextValue.name}
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
              disabled={
                (ticket?.assigneeId ?? 'unassigned') === form.value.assigneeId
              }
            >
              Reassign
            </Button>
          </footer>
        </>
      )}
    </Form>
  )
}
