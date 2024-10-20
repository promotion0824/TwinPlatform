import { Flex, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function FormattedDuration({ duration }) {
  const { t } = useTranslation()
  if (!duration.days && !duration.hours && !duration.minutes) {
    return null
  }

  const formattedDuration = [
    duration.days
      ? `${duration.days} day${duration.days !== 1 ? 's' : ''}`
      : undefined,
    duration.hours
      ? `${duration.hours} hour${duration.hours !== 1 ? 's' : ''}`
      : undefined,
    duration.minutes
      ? `${duration.minutes} minute${duration.minutes !== 1 ? 's' : ''}`
      : undefined,
  ]
    .filter((str) => str != null)
    .join(', ')

  return (
    <Flex size="tiny">
      <Text>
        <span>{t('plainText.changeSetPointFor')} </span>
        {formattedDuration}
        <span>, {t('plainText.revertToOriginalSettings')}</span>
      </Text>
    </Flex>
  )
}
