import { Input, Icon } from '@willow/ui'
import {
  getImpactScore,
  InsightCostImpactPropNames as Metrics,
  InsightMetric,
  titleCase,
} from '@willow/common'
import { InsightDetail, Container } from '@willow/common/insights/component'
import { styled } from 'twin.macro'
import { TFunction, useTranslation } from 'react-i18next'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import { ImpactScore } from '@willow/common/insights/insights/types'
import { ImpactMetricsDisclaimer } from '../../../../Insights/ui/RollupSummary'

export default function ImpactMetrics({
  impactScores,
  language,
  t,
}: {
  impactScores: ImpactScore[]
  language: Language
  t: TFunction
}) {
  const getImpactMetricsContent = () => (
    <>
      <ImpactMetricsDisclaimer />
      {[InsightMetric.cost, InsightMetric.energy].map((item) => (
        <FlexContainer flex="2 1 0%" flexFlow="row" key={item}>
          <MetricsGroup
            item={item}
            impactScores={impactScores}
            language={language}
            t={t}
          />
        </FlexContainer>
      ))}
    </>
  )

  return (
    <Container $hidePaddingBottom>
      <InsightDetail
        headerIcon="impactMetrics"
        headerText={t('plainText.impact')}
      >
        {getImpactMetricsContent()}
      </InsightDetail>
    </Container>
  )
}

export const FlexContainer = styled.div<{
  flex: string
  flexFlow?: string
  isExtraMarginLeft?: boolean
}>(({ flexFlow = 'column', isExtraMarginLeft }) => ({
  flexFlow,
  display: 'flex',
  marginLeft: isExtraMarginLeft ? '16px' : 0,

  '* label': {
    width: '100%',
    paddingBottom: '2px',
  },
}))

/**
 * A component that returns input metrics section row of three fields (daily, yearly, total)
 * Currently used for both cost and energy metrics fields.
 */
const MetricsGroup = ({
  item,
  impactScores,
  language,
  t,
}: {
  item: InsightMetric
  impactScores: ImpactScore[]
  language: Language
  t: TFunction
}) => {
  const yearlyValue = getImpactScore({
    impactScores,
    scoreName:
      item === InsightMetric.cost
        ? Metrics.dailyAvoidableCost
        : Metrics.dailyAvoidableEnergy,
    multiplier: 365,
    language,
    decimalPlaces: 0,
  })

  const totalValue = getImpactScore({
    impactScores,
    scoreName:
      item === InsightMetric.cost
        ? Metrics.totalCostToDate
        : Metrics.totalEnergyToDate,
    multiplier: 1,
    language,
    decimalPlaces: 0,
  })

  const totalTooltipText =
    item === InsightMetric.cost
      ? t('interpolation.timelyCostToolTip', { timely: 'total' })
      : t('interpolation.timelyEnergyToolTip', { timely: 'total' })

  const yearlyTooltipText =
    item === InsightMetric.cost
      ? t('interpolation.timelyCostToolTip', { timely: 'yearly' })
      : t('interpolation.timelyEnergyToolTip', { timely: 'yearly' })

  const yearlyLabel = t('interpolation.avoidableExpensePerYear', {
    expense: item,
  })

  const totalLabel = t('interpolation.expenseToDate', {
    expense: item,
  })

  const metricsContent = [
    {
      label: yearlyLabel,
      tooltipText: yearlyTooltipText,
      value: yearlyValue,
      testId:
        item === InsightMetric.cost
          ? Metrics.yearlyAvoidableCost
          : Metrics.yearlyAvoidableEnergy,
    },
    {
      label: totalLabel,
      tooltipText: totalTooltipText,
      value: totalValue,
      testId:
        item === InsightMetric.cost
          ? Metrics.totalCostToDate
          : Metrics.totalEnergyToDate,
    },
  ]

  return (
    <>
      {metricsContent.map(({ label, tooltipText, value, testId }, index) => (
        <FlexContainer flex="1" isExtraMarginLeft={index !== 0} key={testId}>
          <Input
            label={<MetricsLabel label={label} tooltipText={tooltipText} />}
            value={value}
            readOnly
            data-testid={testId}
          />
        </FlexContainer>
      ))}
    </>
  )
}

const MetricsLabel = ({
  label,
  tooltipText,
}: {
  label: string
  tooltipText: string
}) => {
  const {
    i18n: { language },
  } = useTranslation()
  return (
    <>
      {titleCase({ text: label, language })}
      <IconContainer
        data-tooltip={tooltipText}
        data-tooltip-position="top"
        data-tooltip-z-index={10}
      >
        <Icon icon="questionMark" size="medium" tw="stroke-0" />
      </IconContainer>
    </>
  )
}

const IconContainer = styled.span({
  display: 'inline-block',
  float: 'right',
})
