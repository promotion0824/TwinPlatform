import { useState } from 'react'
import {
  useDateTime,
  useForm,
  useUser,
  Checkbox,
  DatePicker,
  Fieldset,
  Flex,
} from '@willow/ui'
import OverduePill from 'components/OverduePill/OverduePill'
import { useTranslation } from 'react-i18next'

export default function TicketSettings() {
  const dateTime = useDateTime()
  const form = useForm()
  const user = useUser()
  const { t } = useTranslation()

  const [minDate] = useState(() => dateTime.now().startOfDay().format())

  return (
    <Fieldset icon="settings" legend={t('plainText.ticketSettings')}>
      {user.customer.features.isDynamicsIntegrationEnabled &&
        form.data.id == null && (
          <Checkbox
            label={t('labels.dynamics')}
            value={form.data.sourceType === 'Dynamics'}
            onChange={(checked) => {
              form.setData((prevData) => ({
                ...prevData,
                sourceType: checked ? 'Dynamics' : null,
              }))
            }}
          />
        )}
      <Flex horizontal fill="equal" size="large">
        <DatePicker
          name="dueDate"
          data-cy="ticket-duedate"
          label={t('labels.dueDate')}
          min={minDate}
        />
        <Flex align="left bottom" padding="tiny 0">
          <OverduePill ticket={form.data} />
        </Flex>
      </Flex>
    </Fieldset>
  )
}
