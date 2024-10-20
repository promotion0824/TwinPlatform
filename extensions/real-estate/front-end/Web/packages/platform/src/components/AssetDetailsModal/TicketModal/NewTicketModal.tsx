import { priorities, useTicketStatuses } from '@willow/common'
import { Status } from '@willow/common/ticketStatus'
import { api, Message, Modal, NotFound, Progress, useUser } from '@willow/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery } from 'react-query'
import { styled } from 'twin.macro'
import GetAttachments from './GetAttachments'
import TicketForm from './TicketForm/TicketForm'

const defaultPriorityId = priorities.find((p) => p.name === 'Medium')?.id

/**
 * Modal that displays ticket details, which can be use to create a
 * new ticket and populate relevant information in modal by fetching insight details.
 *
 * A ticket must be linked to a site, and required to have a summary, description,
 * contact number and email of requestor. Ticket also have the following site-related
 * fields which are disabled when site is not specified (i.e. when creating new ticket
 * from All sites view): Assignee, Category, Requestor and Comments.
 */
export default function NewTicketModal({
  insightId,
  ticket,
  insightName,
  siteId,
  dataSegmentPropPage,
  onClose,
}) {
  const [currentTicket, setCurrentTicket] = useState({ siteId, insightId })

  // Fetching insight details based on site id and insight id
  const insightQuery = useQuery(
    ['insightDetails', insightId],
    async () => {
      const response = await api.get(
        `/sites/${currentTicket.siteId}/insights/${currentTicket.insightId}`
      )
      return response.data
    },
    { enabled: insightId !== null }
  )
  const { isLoading, isError, isSuccess, data = [] } = insightQuery
  const { t } = useTranslation()
  const ticketStatuses = useTicketStatuses()
  const { id, email, company, mobile, firstName, lastName } = useUser()

  function handleTicketChange(nextTicket) {
    if (nextTicket != null) {
      setCurrentTicket(nextTicket)
    }
  }

  // returns formatted value of name to be displayed in Creator Name field....
  function formatName() {
    const unformattedName = `${firstName ?? ''} ${lastName ?? ''}`
    return unformattedName === ' ' ? undefined : unformattedName.trim()
  }

  return (
    <Modal header={t('headers.ticket')} size="medium" onClose={onClose}>
      <GetAttachments loadAttachments={ticket?.loadAttachments ?? []}>
        {(attachments = []) =>
          isError ? (
            <StyledMessage icon="error">
              {t('plainText.errorOccurred')}
            </StyledMessage>
          ) : isLoading ? (
            <Progress />
          ) : isSuccess && data.length === 0 ? (
            <NotFound>{t('plainText.pageNotFound')}</NotFound>
          ) : (
            <TicketForm
              ticket={{
                siteId,
                template: false,
                sequenceNumber: '',
                createdDate: null,
                updatedDate: null,
                creator: {
                  email,
                  company,
                  mobile,
                  name: formatName(),
                },
                statusCode: ticketStatuses.getByStatus(Status.open)?.statusCode,
                reporterId: id,
                reporterName: `${firstName} ${lastName}`,
                reporterPhone: mobile,
                reporterEmail: email,
                reporterCompany: company,
                summary: data.name,
                description: data.description,
                floorCode: data.floorCode,
                insightId: data.id,
                insightName,
                loadAttachments: true,
                priority: defaultPriorityId,
                issueId: data?.equipmentId,
                issueName: data?.equipmentName,
                issueType: 'asset',
                cause: '',
                solution: '',
                resolvedDate: null,
                attachments,
                assignee: null,
                dueDate: null,
                comments: [],
                comment: '',
                sourceType: null,
                status: data.lastStatus,
                insightStatus: data.status,
                ...data,
                id: null, // a new ticket should not have an id
              }}
              onTicketChange={handleTicketChange}
              dataSegmentPropPage={dataSegmentPropPage}
              onClose={onClose}
              isTicketUpdated={false}
              selectedInsight={{
                id: data.id,
                name: data.name,
                ruleName: data.ruleName,
              }}
            />
          )
        }
      </GetAttachments>
    </Modal>
  )
}

const StyledMessage = styled(Message)({
  height: '100%',
})
