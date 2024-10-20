import { useMemo, useState } from 'react'
import { useParams } from 'react-router'
import {
  Button,
  Form,
  Loader,
  TextArea,
  useSnackbar,
  useApi,
  useAnalytics,
  stringUtils,
} from '@willow/mobile-ui'
import { useTicketStatuses } from '@willow/common'
import { Status, Tab } from '@willow/common/ticketStatus'
import { useLayout, useTickets } from '../../providers'
import priorities from '../../components/priorities.json'
import TicketLabel from '../TicketAction/TicketLabel'
import TicketStatusPill from '../../components/TicketStatusPill/TicketStatusPill.tsx'
import TicketConfirm from '../TicketConfirm/TicketConfirm'
import styles from './TicketReject.css'
import { SpecialStatus } from '../../utils/ticketStatus'

export default function TicketReject() {
  const analytics = useAnalytics()
  const snackbar = useSnackbar()
  const api = useApi()
  const { siteId, ticketId } = useParams()
  const { getTicket, clearTickets } = useTickets()
  const { setTitle, setShowBackButton } = useLayout()
  const { data: ticket, updateCache } = getTicket(siteId, ticketId)
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
        status: nextTicketStatus?.status,
        reason: form.value.reason,
      })
      updateCache(nextTicket)
    } catch {
      snackbar.show('An error has occurred while rejecting ticket')
      return
    }

    clearTickets(siteId, Tab.open)

    setIsSubmitted(true)
  }

  const pageTitle = useMemo(() => {
    if (isSubmitted) {
      return null
    }

    return ticket ? ticket.sequenceNumber : 'Reject Ticket'
  }, [isSubmitted, ticket])

  setTitle(pageTitle)
  setShowBackButton(true, `/tickets/sites/${siteId}/${Tab.open}`)

  if (!ticket) {
    return <Loader />
  }

  return isSubmitted ? (
    <TicketConfirm siteId={siteId} title="Ticket Rejected" />
  ) : (
    <Form
      defaultValue={ticket}
      onSubmit={rejectTicket}
      showSubmitted={false}
      showSuccessful={false}
    >
      <TicketStatusPill
        isHeader
        status={SpecialStatus.reject}
        siteId={siteId}
      />
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
  )
}
