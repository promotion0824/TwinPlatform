import tw from 'twin.macro'
import { useTranslation } from 'react-i18next'
import { Fieldset, Flex, Input, Pill, Select, TextArea } from '@willow/ui'
import { InsightCostImpactPropNames, getImpactScore } from '@willow/common'
import { PriorityValue } from '@willow/common/insights/component'
import InsightStatusPill from '../../../InsightStatusPill/InsightStatusPill'
import { useSites } from '../../../../providers'

export default function InsightDetails({ insight, language }) {
  const { t } = useTranslation()
  const sites = useSites()

  const priorityValue = getImpactScore({
    impactScores: insight.impactScores,
    scoreName: InsightCostImpactPropNames.priorityScore,
    language,
  })

  return (
    <Fieldset icon="details" legend={t('plainText.insightDetails')}>
      <Flex horizontal>
        <Flex size="large" flex={2}>
          <Flex horizontal fill="equal" size="large">
            <Flex horizontal fill="equal" size="large">
              <Select
                label={t('labels.priority')}
                header={() => <PriorityValue value={priorityValue} />}
                readOnly
              />
              <div tw="min-w-max">
                <Select
                  label={t('labels.status')}
                  header={() => <InsightStatusPill status={insight.status} />}
                  isPillSelect
                  readOnly
                />
              </div>
            </Flex>
            <div />
          </Flex>
          <Flex horizontal fill="equal" size="large">
            <Input
              label={t('labels.site')}
              value={sites?.find((s) => s.id === insight.siteId)?.name}
              readOnly
            />
            <div />
          </Flex>
          <Flex horizontal size="large">
            <Flex flex={3}>
              <Input
                label={t('labels.summary')}
                value={insight.name}
                readOnly
              />
            </Flex>
            <Flex flex={1}>
              <Select
                label={t('labels.insightType')}
                header={() => <Pill>{insight.type}</Pill>}
                isPillSelect
                readOnly
              />
            </Flex>
          </Flex>
          <TextArea
            label={t('labels.description')}
            value={insight.description}
            readOnly
          />
          {insight?.recommendation && (
            <TextArea
              label={t('labels.recommendation')}
              value={insight.recommendation}
              readOnly
            />
          )}
          <Flex horizontal fill="equal" size="large">
            <Input
              label={t('labels.source')}
              value={insight.sourceName}
              readOnly
            />
            {insight.sourceType === 'inspection' &&
            insight.createdUser != null ? (
              <Input
                label={t('labels.createdBy', 'Created by')}
                value={insight.createdUser.name}
                readOnly
              />
            ) : (
              <div />
            )}
          </Flex>
        </Flex>
        <Flex flex={1} />
      </Flex>
    </Fieldset>
  )
}
