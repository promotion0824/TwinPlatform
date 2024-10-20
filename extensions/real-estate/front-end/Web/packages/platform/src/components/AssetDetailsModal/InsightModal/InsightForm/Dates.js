import _ from 'lodash'
import { DatePicker, Fieldset, Flex } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useSites } from '../../../../providers'

export default function Dates({ insight }) {
  const { t } = useTranslation()
  const sites = useSites()
  const timeZone = sites.find((s) => s.id === insight.siteId)?.timeZone

  return (
    <Fieldset icon="date" legend={t('plainText.dates')}>
      <Flex horizontal>
        <Flex size="large" flex={2}>
          <Flex horizontal fill="equal" size="large">
            <DatePicker
              label={t('labels.createdDate')}
              type="date-time"
              value={insight.createdDate}
              timezone={timeZone}
              readOnly
            />
            <DatePicker
              label={t('labels.updatedDate')}
              type="date-time"
              value={insight.updatedDate}
              timezone={timeZone}
              readOnly
            />
          </Flex>
          <Flex horizontal fill="equal" size="large">
            <DatePicker
              label={_.capitalize(t('plainText.lastOccurredDate'))}
              type="date-time"
              value={insight.occurredDate}
              timezone={timeZone}
              readOnly
            />
            <DatePicker
              label={_.capitalize(t('plainText.firstOccurredDate'))}
              type="date-time"
              value={insight.detectedDate}
              timezone={timeZone}
              readOnly
            />
          </Flex>
        </Flex>
        <Flex flex={1} />
      </Flex>
    </Fieldset>
  )
}
