import { useMemo } from 'react'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { titleCase, useTicketStatuses } from '@willow/common'
import { Status, Tab } from '@willow/common/ticketStatus'
import {
  Select,
  Option,
  getTicketStatusTranslatedName,
  Progress,
} from '@willow/ui'
import TicketStatusPill from '../TicketStatusPill/TicketStatusPill'

/**
 * List of excluded status options for all possible ticket statuses
 *
 * Note about special status(es) which not all customers have.
 * - onHold (See https://dev.azure.com/willowdev/Unified/_workitems/edit/71863/)
 *   - When a ticket status is "onHold", the possible options are "open" and "inProgress"
 *   - A ticket can be moved to "onHold" if the current status is "open" or "inProgress"
 *   - By default, when ticket has no status, don't show onHold option.
 */
const statusExclusions = {
  [Status.open]: [],
  [Status.inProgress]: [],
  [Status.onHold]: [
    Status.limitedAvailability,
    Status.reassign,
    Status.resolved,
    Status.closed,
  ],
  default: [Status.onHold],
}

const selectName = 'statusCode'

/**
 * Ticket status selector used within Form. The dropdown shows
 * a list of ticket statuses, ordered by the tab that the status
 * is associated to.
 */
export default function TicketStatusSelect({
  readOnly = false,
  onChange,
  initialStatusCode,
  hideLabel = false,
  isPillSelect = true,
  disabled = false,
  nextValidStatus = [],
}: {
  readOnly?: boolean
  onChange?: (nextValue: number) => void
  /**
   * The status of the ticket from the API regardless of any unsaved
   * changes made (i.e. value found in Form).
   */
  initialStatusCode?: number
  hideLabel?: boolean
  isPillSelect?: boolean
  disabled?: boolean
  nextValidStatus?: number[]
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const ticketStatuses = useTicketStatuses()
  const initialTicketStatus =
    initialStatusCode != null
      ? ticketStatuses.getByStatusCode(initialStatusCode)
      : undefined
  // if nextValidStatus is empty, allSortedStatuses will be all ticket statuses
  // else filter ticket statuses based on nextValidStatus
  // nextValidStatus is status transitions configured in the backend
  const allSortedStatuses = useMemo(() => {
    const validStatus =
      nextValidStatus.length > 0
        ? ticketStatuses?.data?.filter((status) =>
            nextValidStatus.includes(status.statusCode)
          )
        : ticketStatuses.data
    return validStatus
      ? _.sortBy(validStatus, (status) => {
          switch (status.tab) {
            case Tab.open:
              return 1
            case Tab.resolved:
              return 2
            default:
              return 3
          }
        })
      : []
  }, [ticketStatuses.data, nextValidStatus])
  return (
    <Select
      name={selectName}
      label={!hideLabel && t('labels.status')}
      header={(value?: number) => {
        const ticketStatus = value ?? initialStatusCode
        return (
          ticketStatus != null && <TicketStatusPill statusCode={ticketStatus} />
        )
      }}
      isPillSelect={isPillSelect}
      unselectable
      readOnly={readOnly}
      onChange={onChange}
      disabled={disabled}
    >
      {ticketStatuses.isLoading ? (
        <Progress />
      ) : (
        allSortedStatuses
          .filter(
            (ticketStatus) =>
              !(
                (initialTicketStatus != null &&
                  statusExclusions[initialTicketStatus.status]) ||
                statusExclusions.default
              ).includes(ticketStatus.status)
          )
          .map((ticketStatus) => (
            <Option
              key={ticketStatus.statusCode}
              value={ticketStatus.statusCode}
            >
              {titleCase({
                text:
                  getTicketStatusTranslatedName(t, ticketStatus.status) ?? '',
                language,
              })}
            </Option>
          ))
      )}
    </Select>
  )
}
