import { useTranslation } from 'react-i18next'
import {
  Flex,
  Fieldset,
  Input,
  useDateTime,
  useIntl,
  useLanguage,
} from '@willow/ui'

import { styled } from 'twin.macro'

const StyledInput = styled(Input)`
  input {
    color: var(--light) !important;
  }
`

export default function ScheduleDetails() {
  const { t } = useTranslation()
  const dateTime = useDateTime()
  const { language } = useLanguage()
  const intl = useIntl()

  return (
    <Fieldset icon="details" legend={t('plainText.scheduleDetails')}>
      <Flex horizontal fill="equal" size="large">
        <StyledInput
          name="summary"
          label={t('plainText.scheduleName')}
          disabled
        />
      </Flex>
      <Flex horizontal fill="equal" size="large">
        <StyledInput
          value={dateTime(new Date()).format('date', intl?.timezone, language)}
          label={t('plainText.dateCreated')}
          disabled
        />
      </Flex>
    </Fieldset>
  )
}
