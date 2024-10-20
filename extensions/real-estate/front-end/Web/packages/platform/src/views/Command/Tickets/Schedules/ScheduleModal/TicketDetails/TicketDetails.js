import { Fieldset, Flex, Input, TextArea, useForm } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import Assignee from './Assignee'
import CategorySelect from './CategorySelect'
import PrioritySelect from '../../../../../../components/PrioritySelect/PrioritySelect'
import TicketStatusSelect from '../../../../../../components/TicketStatusSelect/TicketStatusSelect.tsx'
import Tasks from './Tasks'

export default function TicketDetails({ isReadOnly }) {
  const { t } = useTranslation()
  const form = useForm()

  return (
    <Fieldset icon="details" legend={t('plainText.ticketDetails')}>
      <Flex horizontal fill="equal" size="large">
        <Flex horizontal fill="equal" size="large">
          <PrioritySelect />
          <TicketStatusSelect
            initialStatusCode={form.data.statusCode}
            readOnly
          />
        </Flex>
        <Assignee />
      </Flex>
      <Flex horizontal fill="equal" size="large">
        <Input name="summary" label={t('labels.summary')} required />
        <CategorySelect />
      </Flex>
      <TextArea name="description" label={t('labels.description')} required />
      <Tasks isReadOnly={isReadOnly} />
    </Fieldset>
  )
}
