import { useTicketStatuses } from '@willow/common'
import { isTicketStatusEquates, Status, Tab } from '@willow/common/ticketStatus'
import {
  Dropdown,
  DropdownContent,
  DropdownButton,
  Icon,
} from '@willow/mobile-ui'
import { TICKET_STATUS_DISPLAY_NAMES } from '../../utils/ticketStatus'
import styles from './TicketFilter.css'

export default function TicketFilter({
  value,
  onTicketStatusChanged,
  /**
   * Ticket type - Standard or scheduled
   */
  type = 'standard',
}) {
  const ticketStatuses = useTicketStatuses()
  const openStatuses =
    ticketStatuses.data?.filter(
      (ticketStatus) =>
        ticketStatus.tab === Tab.open &&
        (type === 'scheduled'
          ? isTicketStatusEquates(
              ticketStatus.status,
              Status.limitedAvailability
            )
          : true)
    ) || []

  return openStatuses.length ? (
    <Dropdown type="none" showBorder={false} className={styles.dropdown}>
      <Icon className={styles.icon} icon="filter" />
      <DropdownContent position="right">
        {openStatuses.map((ticketStatus) => (
          <DropdownButton
            key={ticketStatus.statusCode}
            onClick={() =>
              onTicketStatusChanged(
                ticketStatus.statusCode === value
                  ? undefined
                  : ticketStatus.statusCode
              )
            }
            selected={ticketStatus.statusCode === value}
          >
            {TICKET_STATUS_DISPLAY_NAMES[ticketStatus.status]}
          </DropdownButton>
        ))}
      </DropdownContent>
    </Dropdown>
  ) : null
}
