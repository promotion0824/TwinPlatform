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
} from '@willow/mobile-ui'
import { qs, useTicketStatuses } from '@willow/common'
import { isTicketStatusEquates, Status, Tab } from '@willow/common/ticketStatus'
import { useLayout, useTickets } from '../../providers'
import priorities from '../../components/priorities.json'
import TicketImages from '../Ticket/TicketImages'
import TicketLabel from './TicketLabel'
import TicketConfirm from '../TicketConfirm/TicketConfirm'
import styles from './TicketAction.css'
import TicketStatusPill from '../../components/TicketStatusPill/TicketStatusPill.tsx'

function getResolveKey(status, targetStatus) {
  return `${status}2${stringUtils.capitalizeFirstLetter(targetStatus)}`
}

export default function TicketAction() {
  const analytics = useAnalytics()
  const api = useApi()
  const { siteId, ticketId } = useParams()
  const { getTicket, clearTickets } = useTickets()
  const { setTitle, setShowBackButton } = useLayout()
  const { data: ticket, updateCache } = getTicket(siteId, ticketId)
  const [isCauseError, setIsCauseError] = useState()
  const [isSolutionError, setIsSolutionError] = useState()
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

    if (stringUtils.isNullOrEmpty(form.value.cause)) {
      setIsCauseError(true)
      hasError = true
    } else {
      setIsCauseError(false)
    }

    if (stringUtils.isNullOrEmpty(form.value.solution)) {
      setIsSolutionError(true)
      hasError = true
    } else {
      setIsSolutionError(false)
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
            cause: form.value.cause,
            solution: form.value.solution,
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
            cause: form.value.cause,
            solution: form.value.solution,
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
    clearTickets(siteId, Tab.open)

    if (reason === Status.resolved) {
      clearTickets(siteId, Tab.resolved)
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
    <Form
      defaultValue={ticket}
      onSubmit={resolveTicket}
      readOnly={isResolved}
      showSubmitted={false}
      showSuccessful={false}
    >
      <TicketStatusPill isHeader status={reason} />
      <div className={styles.ticketContent}>
        <TicketLabel label="Cause">
          <TextArea
            name="cause"
            error={isCauseError}
            className={styles.textarea}
          />
        </TicketLabel>

        <TicketLabel label="Solution">
          <TextArea
            name="solution"
            error={isSolutionError}
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
  )
}
