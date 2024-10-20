import { useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useForm, Flex, FormControl } from '@willow/ui'
import { useTicketStatuses } from '@willow/common'
import { isTicketStatusEquates, Status } from '@willow/common/ticketStatus'
import ScheduledTicketTaskDetails from './ScheduledTicketTaskDetails'
import ScheduledTicketAddNewTask from './ScheduledTicketAddNewTask'

export default function ScheduledTicketTasks({ ticket }) {
  const form = useForm()
  const containerRef = useRef()
  const { t } = useTranslation()
  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(ticket.statusCode)

  const isClosed =
    ticketStatus && isTicketStatusEquates(ticketStatus, Status.closed)

  return (
    <FormControl label={t('labels.tasks')}>
      {(props) => (
        <Flex ref={containerRef} size="medium">
          {form.data.tasks.map((task, i) => (
            <>
              <ScheduledTicketTaskDetails
                task={task}
                props={props}
                i={i}
                key={task.id}
                form={form}
              />
            </>
          ))}
          {!isClosed ? <ScheduledTicketAddNewTask form={form} /> : null}
        </Flex>
      )}
    </FormControl>
  )
}
