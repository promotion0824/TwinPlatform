import { useState, useEffect } from 'react'
import { useFetchRefresh, Modal, Fetch, useUser } from '@willow/ui'
import { useTicketStatuses, priorities } from '@willow/common'
import { Status } from '@willow/common/ticketStatus'
import { useTranslation } from 'react-i18next'
import GetAttachments from './GetAttachments'
import TicketForm from './TicketForm/TicketForm'

const mediumPriorityId = priorities.find(
  (priority) => priority.name === 'Medium'
).id

/**
 * Modal that displays ticket details, which can be use to create a new ticket,
 * edit existing ticket and view readOnly ticket.
 *
 * A ticket must be linked to a site, and required to have a summary, description,
 * contact number and email of requestor. Ticket also have the following site-related
 * fields which are disabled when site is not specified (i.e. when creating new ticket
 * from All sites view): Assignee, Category, Requestor and Comments.
 */
export default function TicketModal({
  siteId,
  ticketId,
  ticket = undefined,
  onClose,
  showNavigationButtons,
  onPreviousItem,
  onNextItem,
  dataSegmentPropPage,
  isTicketUpdated,
  selectedInsight,
}) {
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()
  const ticketStatuses = useTicketStatuses()
  const { email, company, mobile, firstName, lastName } = useUser()

  const [currentTicket, setCurrentTicket] = useState(() =>
    ticketId != null
      ? {
          siteId,
          ticketId,
        }
      : undefined
  )

  useEffect(() => {
    setCurrentTicket(() =>
      ticketId != null
        ? {
            siteId,
            ticketId,
          }
        : undefined
    )
  }, [ticketId, siteId])

  function handleTicketChange(nextTicket) {
    if (nextTicket != null) {
      setCurrentTicket(nextTicket)
    } else {
      fetchRefresh('ticket')
    }

    fetchRefresh('tickets')
  }

  // returns formatted value of name to be displayed in Creator Name field....
  function formatName() {
    const unformattedName = `${firstName ?? ''} ${lastName ?? ''}`
    return unformattedName === ' ' ? undefined : unformattedName.trim()
  }

  return (
    <Modal
      header={t('headers.ticket')}
      size="medium"
      onClose={onClose}
      showNavigationButtons={showNavigationButtons}
      onPreviousItem={onPreviousItem}
      onNextItem={onNextItem}
      closeOnClickOutside={false}
    >
      <GetAttachments loadAttachments={ticket?.loadAttachments}>
        {(attachments) => (
          <Fetch
            name="ticket"
            url={
              // If currentTicket is present, send GET request to fetch the existing ticket.
              currentTicket != null
                ? `/api/sites/${currentTicket.siteId}/tickets/${currentTicket.ticketId}`
                : undefined
            }
          >
            {(response) => (
              <TicketForm
                ticket={
                  response
                    ? { ...response, siteId: currentTicket.siteId }
                    : {
                        id: null,
                        siteId,
                        template: false,
                        sequenceNumber: '',
                        createdDate: null,
                        updatedDate: null,
                        reporterId: null,
                        reporterName: '',
                        reporterPhone: '',
                        reporterEmail: '',
                        reporterCompany: '',
                        creator: {
                          email,
                          company,
                          mobile,
                          name: formatName(),
                        },
                        statusCode: ticketStatuses.getByStatus(Status.open)
                          ?.statusCode,
                        summary: '',
                        description: '',
                        cause: '',
                        solution: '',
                        resolvedDate: null,
                        attachments,
                        floorCode: '',
                        issueId: null,
                        issueType: null,
                        issueName: null,
                        assignee: null,
                        priority: mediumPriorityId,
                        dueDate: null,
                        insightName: null,
                        comments: [],
                        comment: '',
                        sourceType: null,
                        ...ticket,
                      }
                }
                onTicketChange={handleTicketChange}
                dataSegmentPropPage={dataSegmentPropPage}
                isTicketUpdated={isTicketUpdated}
                onClose={onClose}
                selectedInsight={selectedInsight}
              />
            )}
          </Fetch>
        )}
      </GetAttachments>
    </Modal>
  )
}
