import { useMemo, useRef, useState } from 'react'
import { useParams } from 'react-router'
import {
  Form,
  Button,
  TextArea,
  useApi,
  useAnalytics,
  stringUtils,
  Loader,
  Spacing,
} from '@willow/mobile-ui'
import { qs, useTicketStatuses } from '@willow/common'
import { Status, Tab, isTicketStatusEquates } from '@willow/common/ticketStatus'
import { useLayout, useTickets } from 'providers'
import priorities from '../../../../components/priorities.json'
import TicketImages from '../../ScheduledTicket/TicketImages'
import TicketLabel from '../../Common/TicketLabel'
import TicketHeader from '../../ScheduledTicket/TicketHeader'
import TicketConfirm from '../TicketConfirm/TicketConfirm'
import styles from './ScheduledTicketAction.css'
import TicketTopBar from '../../ScheduledTicket/TicketTopBar'

function getResolveKey(status, targetStatus) {
  return `${status}2${stringUtils.capitalizeFirstLetter(targetStatus)}`
}

export default function ScheduledTicketAction() {
  const analytics = useAnalytics()
  const api = useApi()
  const { siteId, ticketId } = useParams()
  const { getScheduledTicket, clearScheduledTickets } = useTickets()
  const { setTitle, setShowBackButton } = useLayout()
  const { data: ticket, updateCache } = getScheduledTicket(siteId, ticketId)

  const [isNotesError, setIsNotesError] = useState()
  const [imagesError, setImagesError] = useState(null)
  const [isSubmitted, setIsSubmitted] = useState()
  const refSubmitImageRequest = useRef()
  const reason = qs.get('reason')
  const ticketStatuses = useTicketStatuses()
  const ticketStatus =
    ticket && ticketStatuses.getByStatusCode(ticket.statusCode)

  const isResolved =
    ticketStatus && isTicketStatusEquates(ticketStatus, Status.resolved)

  const resolveTicket = async (form) => {
    if (!ticket) {
      return
    }

    setImagesError(null)
    let hasError = false

    if (stringUtils.isNullOrEmpty(form.value.notes)) {
      setIsNotesError(true)
      hasError = true
    } else {
      setIsNotesError(false)
    }

    if (hasError) {
      return
    }

    let nextAttachments
    let nextTicket
    let nextTicketStatus

    try {
      const resolveKey = getResolveKey(ticketStatus?.status, reason)

      nextAttachments = await refSubmitImageRequest.current()
      nextTicketStatus = ticketStatuses.getByStatus(reason)
      nextTicket = await api.put(
        `/api/sites/${siteId}/tickets/${ticketId}/status`,
        {
          statusCode: nextTicketStatus?.statusCode,
          [resolveKey]: {
            notes: form.value.notes,
          },
        }
      )

      if (reason === Status.limitedAvailability || reason === Status.resolved) {
        analytics.track(
          reason === Status.limitedAvailability
            ? 'Mobile Ticket Partially Fixed'
            : 'Mobile Ticket Completed',
          {
            priority: priorities.find((item) => item.id === nextTicket.priority)
              ?.name,
            status: nextTicketStatus?.status,
            notes: form.value.notes,
          }
        )
      }
    } catch (error) {
      setImagesError(
        error?.response?.data?.items[0]?.message ??
          'An error has occurred while updating ticket attachments'
      )
      return
    }

    updateCache({
      ...nextTicket,
      attachments: nextAttachments,
    })
    clearScheduledTickets(siteId, Tab.open)

    if (reason === Status.resolved) {
      clearScheduledTickets(siteId, Tab.resolved)
    }

    setIsSubmitted(true)
  }

  const pageTitle = useMemo(() => {
    if (isSubmitted) {
      return null
    }

    return ticket ? ticket.sequenceNumber : 'Complete Ticket'
  }, [isSubmitted, ticket])

  setTitle(pageTitle)
  setShowBackButton(true)

  if (!ticket) {
    return <Loader />
  }

  return isSubmitted ? (
    <TicketConfirm
      siteId={siteId}
      title={
        reason === Status.limitedAvailability
          ? 'Ticket Saved'
          : 'Ticket Completed'
      }
    />
  ) : (
    <Spacing type="content" className={styles.main}>
      <TicketTopBar ticket={ticket} />
      <Form
        defaultValue={ticket}
        onSubmit={resolveTicket}
        readOnly={isResolved}
        showSubmitted={false}
        showSuccessful={false}
      >
        <TicketHeader ticket={ticket} />
        <div className={styles.ticketContent}>
          <TicketLabel label="Notes">
            <TextArea
              name="notes"
              placeholder="Add notes ..."
              error={isNotesError}
              className={styles.textarea}
            />
          </TicketLabel>

          <TicketLabel label="Attachments &amp; Screenshots">
            {ticket && (
              <TicketImages
                allowAdd={!isResolved}
                error={imagesError}
                siteId={siteId}
                ticket={ticket}
                refSubmitImageRequest={refSubmitImageRequest}
              />
            )}
          </TicketLabel>
        </div>
        <footer className={styles.footer}>
          {!isResolved && (
            <Button
              size="large"
              color="blue"
              type="submit"
              className={styles.button}
            >
              {reason === Status.resolved ? 'Complete' : 'Save'}
            </Button>
          )}
        </footer>
      </Form>
    </Spacing>
  )
}
