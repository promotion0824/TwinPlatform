import { useState } from 'react'
import tw, { styled } from 'twin.macro'
import { Button, useSnackbar, Loader } from '@willow/mobile-ui'
import { useTicketStatuses } from '@willow/common'
import { Status } from '@willow/common/ticketStatus'
import ActionButton from '../../components/ActionButton/ActionButton'
import priorities from '../../components/priorities.json'
import {
  POSSIBLE_STATUS_TRANSITIONS,
  STATUS_BUTTON_COLORS,
  TICKET_STATUS_DISPLAY_NAMES,
  SpecialStatus,
} from '../../utils/ticketStatus'

const Footer = styled.footer({
  alignItems: 'center',
  display: 'flex',
  height: '70px',
  backgroundColor: 'var(--theme-color-neutral-bg-panel-default)',
  padding: '0 var(--padding-extra-large)',
})

const StyledButton = styled(Button)({
  flex: '1',

  '&:not(:only-child):not(:last-child)': {
    marginRight: '10px',
  },
})

function TicketActionButton({ targetStatus, onChange, ...rest }) {
  const snackbar = useSnackbar()
  const [isLoading, setIsLoading] = useState()

  const updateStatus = async () => {
    setIsLoading(true)

    try {
      await onChange(targetStatus)
    } catch {
      snackbar.show('Failed to update ticket status')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <StyledButton
      size="large"
      color={STATUS_BUTTON_COLORS[targetStatus] || 'blue'}
      loading={isLoading}
      onClick={updateStatus}
      {...rest}
    />
  )
}

/**
 * The footer of ticket to provide possible statuses that the ticket
 * can be updated to based on the current ticket's status.
 * @see possibleStatusTransitions For list of possible status options
 */
export default function TicketFooter({
  ticket,
  onStatusChange,
  onReassignTicket,
  ticketType = 'standard',
}) {
  const { priority, statusCode, assigneeType } = ticket
  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(statusCode)

  const nextPossibleStatuses =
    POSSIBLE_STATUS_TRANSITIONS[ticketType][ticketStatus?.status]
  const nextStatuses =
    nextPossibleStatuses &&
    nextPossibleStatuses.nextStatuses.filter(
      (nextStatus) =>
        nextStatus === SpecialStatus.reject ||
        ticketStatuses.getByStatus(nextStatus) != null
    )

  if (ticketStatuses.isLoading) {
    return <Loader />
  } else if (!nextStatuses?.length) {
    return null
  }

  return (
    <Footer>
      {nextPossibleStatuses.isDropdown ? (
        <ActionButton
          tw="flex[1]"
          size="large"
          color="blue"
          items={nextStatuses.map((targetStatus) => ({
            name: targetStatus,
            size: 'large',
            color: STATUS_BUTTON_COLORS[targetStatus],
            text: TICKET_STATUS_DISPLAY_NAMES[targetStatus],
            onClick: () => onStatusChange(targetStatus),
            'data-segment': `Mobile ${
              ticketType === 'scheduled' ? 'Scheduled' : ''
            } Ticket ${targetStatus} Clicked`,
            'data-segment-props': JSON.stringify(
              ticketType === 'standard'
                ? {
                    priority: priorities.find((item) => item.id === priority)
                      ?.name,
                    status: TICKET_STATUS_DISPLAY_NAMES[ticketStatus?.status],
                  }
                : { summary: ticket.summary, asset: ticket.issueName }
            ),
          }))}
        >
          {TICKET_STATUS_DISPLAY_NAMES[ticketStatus?.status]}
        </ActionButton>
      ) : (
        nextStatuses.map((nextStatus) => {
          // We always render TicketActionButton unless nextStatus is "reject" and
          // assigneeType is neither noAssignee or workGroup,
          if (
            nextStatus !== SpecialStatus.reject ||
            !['noAssignee', 'workGroup'].includes(assigneeType)
          ) {
            return (
              <TicketActionButton
                key={nextStatus}
                onChange={
                  ticketStatus?.status === Status.reassign
                    ? onReassignTicket
                    : onStatusChange
                }
                targetStatus={nextStatus}
                data-segment={dataSegment[ticketType][nextStatus]}
                data-segment-props={JSON.stringify({
                  priority: priorities.find((item) => item.id === priority)
                    ?.name,
                  status: TICKET_STATUS_DISPLAY_NAMES[ticketStatus?.status],
                })}
              >
                {ticketActionButtonText[nextStatus] ||
                  TICKET_STATUS_DISPLAY_NAMES[nextStatus]}
              </TicketActionButton>
            )
          }
          return null
        })
      )}
    </Footer>
  )
}

const dataSegment = {
  standard: {
    [Status.inProgress]: 'Mobile Ticket Accepted',
    [Status.reassign]: 'Mobile Ticket Accepted',
    [Status.onHold]: 'Mobile Ticket On Hold',
    [SpecialStatus.reject]: 'Mobile Ticket Rejected',
  },
  scheduled: {
    [Status.inProgress]: 'Mobile Scheduled Accepted',
    [Status.reassign]: 'Mobile Scheduled Accepted',
    [Status.onHold]: 'Mobile Scheduled On Hold',
    [SpecialStatus.reject]: 'Mobile Scheduled Rejected',
  },
}

const ticketActionButtonText = {
  [Status.inProgress]: 'Accept',
  [Status.reassign]: 'Accept',
  [SpecialStatus.reject]: 'Reject',
}
