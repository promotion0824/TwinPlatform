import { useForm, DatePicker, Fieldset, Flex } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function Dates() {
  const form = useForm()
  const { t } = useTranslation()

  if (form.data.id == null) {
    return null
  }

  return (
    <Fieldset icon="details" legend={t('plainText.dates')}>
      <Flex horizontal fill="equal" size="large">
        <DatePicker name="createdDate" label={t('labels.created')} readOnly />
        <DatePicker name="updatedDate" label={t('labels.updated')} readOnly />
      </Flex>
    </Fieldset>
  )
}
