import { useTicketStatuses } from '@willow/common'
import { isTicketStatusIncludes, Status } from '@willow/common/ticketStatus'
import { useForm, DatePicker, Fieldset, Input, TextArea } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function SolutionDetails() {
  const form = useForm()
  const { t } = useTranslation()
  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(form.data.statusCode)

  const showSolutionDetails =
    (ticketStatus &&
      isTicketStatusIncludes(ticketStatus, [
        Status.onHold,
        Status.limitedAvailability,
        Status.resolved,
        Status.closed,
      ])) ||
    (form.data.template
      ? form.initialData.notes !== ''
      : form.initialData.cause !== '')

  if (!showSolutionDetails) {
    return null
  }

  return (
    <Fieldset icon="details" legend={t('plainText.solutionDetails')}>
      {form.data.template ? (
        <TextArea name="notes" label={t('labels.notes')} required />
      ) : (
        <>
          <Input name="cause" label={t('labels.cause')} required />
          <TextArea name="solution" label={t('labels.solution')} required />
        </>
      )}
      {form.data.resolvedDate != null && (
        <DatePicker
          label={t('headers.completed')}
          type="date-time"
          value={form.data.resolvedDate}
          readOnly
        />
      )}
    </Fieldset>
  )
}
