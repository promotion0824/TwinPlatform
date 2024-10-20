import { useMemo, useState } from 'react'
import { useParams } from 'react-router'
import {
  Button,
  Form,
  Loader,
  TextArea,
  Spacing,
  useSnackbar,
  useApi,
  useAnalytics,
  stringUtils,
} from '@willow/mobile-ui'
import { useTicketStatuses } from '@willow/common'
import { Status, Tab } from '@willow/common/ticketStatus'
import { useLayout, useTickets } from '../../../../providers'
import priorities from '../../../../components/priorities.json'
import TicketLabel from '../../Common/TicketLabel'
import TicketHeader from '../../ScheduledTicket/TicketHeader'
import TicketConfirm from '../TicketConfirm/TicketConfirm'
import styles from './ScheduledTicketReject.css'
import TicketTopBar from '../../ScheduledTicket/TicketTopBar'

export default function ScheduledTicketReject() {
  const analytics = useAnalytics()
  const snackbar = useSnackbar()
  const api = useApi()
  const { siteId, ticketId } = useParams()
  const { getScheduledTicket, clearScheduledTickets } = useTickets()
  const { setTitle, setShowBackButton } = useLayout()
  const { data: ticket, updateCache } = getScheduledTicket(siteId, ticketId)
  const [isSubmitted, setIsSubmitted] = useState()
  const [isReasonError, setIsReasonError] = useState()
  const ticketStatuses = useTicketStatuses()

  const rejectTicket = async (form) => {
    let nextTicket
    let nextTicketStatus

    if (stringUtils.isNullOrEmpty(form.value.reason)) {
      setIsReasonError(true)
      return
    }

    setIsReasonError(false)

    try {
      nextTicketStatus = ticketStatuses.getByStatus(Status.reassign)
      nextTicket = await api.put(
        `/api/sites/${siteId}/tickets/${ticketId}/status`,
        {
          statusCode: nextTicketStatus?.statusCode,
          open2Reassign: {
            rejectComment: form.value.reason,
          },
        }
      )
      analytics.track('Mobile Ticket Rejected', {
        priority: priorities.find((item) => item.id === nextTicket.priority)
          ?.name,
        status: Status.reassign,
        reason: form.value.reason,
      })
    } catch {
      snackbar.show('An error has occurred while rejecting ticket')
      return
    }

    updateCache(nextTicket)
    clearScheduledTickets(siteId, Tab.open)

    setIsSubmitted(true)
  }

  const pageTitle = useMemo(() => {
    if (isSubmitted) {
      return null
    }

    return ticket ? ticket.sequenceNumber : 'Reject Ticket'
  }, [isSubmitted, ticket])

  setTitle(pageTitle)
  setShowBackButton(true, `/sites/${siteId}/scheduled-tickets/${Tab.open}`)

  if (!ticket) {
    return <Loader />
  }

  return isSubmitted ? (
    <TicketConfirm siteId={siteId} title="Ticket Rejected" />
  ) : (
    <Spacing type="content" className={styles.main}>
      <TicketTopBar ticket={ticket} />
      <Form
        defaultValue={ticket}
        onSubmit={rejectTicket}
        showSubmitted={false}
        showSuccessful={false}
      >
        <TicketHeader ticket={ticket} />
        <div className={styles.ticketContent}>
          <TicketLabel label="Reject Reason">
            <TextArea
              focusOnError
              name="reason"
              error={isReasonError}
              className={styles.textarea}
            />
          </TicketLabel>
        </div>
        <footer className={styles.footer}>
          <Button
            size="large"
            color="white"
            type="submit"
            className={styles.button}
          >
            Reject
          </Button>
        </footer>
      </Form>
    </Spacing>
  )
}
