import { useForm, Fieldset, Flex, Input } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import Requestor from './Requestor'

export default function RequestorDetails() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Fieldset icon="user" legend={t('plainText.requestorDetails')}>
      <Flex horizontal fill="equal" size="large">
        <Requestor siteId={form.data.siteId} />
        <Input
          name="reporterPhone"
          data-cy="requestorDetails-phone"
          label={t('labels.contactNumber')}
        />
      </Flex>
      <Flex horizontal fill="equal" size="large">
        <Input
          type="email"
          data-cy="requestorDetails-email"
          name="reporterEmail"
          label={t('labels.contactEmail')}
          required
        />
        <Input name="reporterCompany" label={t('labels.company')} />
      </Flex>
    </Fieldset>
  )
}
