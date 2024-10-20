import { Fieldset, Flex, Input } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import Requestor from './Requestor'

export default function RequestorDetails() {
  const { t } = useTranslation()
  return (
    <Fieldset icon="user" legend={t('plainText.requestorDetails')}>
      <Flex horizontal fill="equal" size="large">
        <Requestor />
        <Input name="reporterPhone" label={t('labels.contactNumber')} />
      </Flex>
      <Flex horizontal fill="equal" size="large">
        <Input name="reporterEmail" label={t('labels.contactEmail')} required />
        <Input name="reporterCompany" label={t('labels.company')} />
      </Flex>
    </Fieldset>
  )
}
