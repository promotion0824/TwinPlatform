import { Flex, ModalHeader, Text, Time } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function InsightHeader({ insight }) {
  const { t } = useTranslation()
  return (
    <ModalHeader>
      <Flex horizontal fill="header">
        <Flex horizontal size="small">
          <Text size="extraTiny" color="grey">
            {t('plainText.insightId')}
          </Text>
          <Text size="extraTiny" color="grey">
            {insight.sequenceNumber}
          </Text>
        </Flex>
        <Flex horizontal size="small">
          <Text size="extraTiny" color="grey">
            {t('labels.created')}:
          </Text>
          <Text size="extraTiny" color="grey">
            <Time value={insight.createdDate} />
          </Text>
        </Flex>
      </Flex>
    </ModalHeader>
  )
}
