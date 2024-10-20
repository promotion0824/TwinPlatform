import { titleCase } from '@willow/common'
import { formatEnergy } from '@willow/common/insights/costImpacts/utils'
import { useScopeSelector } from '@willow/ui'
import { Group } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import { DataTile } from '../../../../../components/LocationHome/DataTile/DataTile'
import { useGetInsightTypes } from '../../../../../hooks'
import {
  FilterOperator,
  statusMap,
} from '../../../../../services/Insight/InsightsService'

const Value = styled.span(({ theme }) => ({
  ...theme.font.body.md.semibold,
}))

const renderImpactScoreValue = (value: number | string, unit: string) => (
  <Group gap="s4" wrap="nowrap">
    <Value>{value}</Value>
    <span>{unit}</span>
  </Group>
)

// eslint-disable-next-line complexity
const ImpactScoresTile = ({
  showActiveAvoidableCost,
  showActiveAvoidableEnergy,
  showAverageDuration,
}: {
  showActiveAvoidableCost: boolean
  showActiveAvoidableEnergy: boolean
  showAverageDuration: boolean
}) => {
  const { scopeId } = useScopeSelector()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const insightsTypesQuery = useGetInsightTypes({
    filterSpecifications: [
      {
        field: 'scopeId',
        operator: FilterOperator.equalsLiteral,
        value: scopeId,
      },
      {
        field: 'status',
        operator: FilterOperator.containedIn,
        value: statusMap.default,
      },
    ],
  })

  if (
    !showActiveAvoidableCost &&
    !showActiveAvoidableEnergy &&
    !showAverageDuration
  )
    return null

  if (!insightsTypesQuery.isSuccess) return null

  const currencyFormatter = Intl.NumberFormat(language, { notation: 'compact' })

  const { impactScoreSummary } = insightsTypesQuery.data

  const dailyAvoidableCost = impactScoreSummary.find(
    (item) => item.fieldId === 'daily_avoidable_cost'
  )

  const dailyAvoidableEnergy = impactScoreSummary.find((item) =>
    item.fieldId.includes('daily_avoidable_energy')
  )

  // TODO: Replace with data from backend when available.
  // https://dev.azure.com/willowdev/Unified/_workitems/edit/134871
  const duration = 6.33

  const convertDailyAvoidableEnergyValue = (unconvertedValue: number) => {
    const [value, unit] = formatEnergy({
      language,
      value: unconvertedValue * 365,
    }).split(' ')

    return [
      value,
      t('interpolation.unitPerYear', {
        unit,
      }),
    ] as const
  }

  return (
    <DataTile
      fields={[
        ...(showActiveAvoidableCost
          ? [
              {
                icon: 'paid' as const,
                iconFilled: false,
                label: titleCase({
                  language,
                  text: t('labels.activeAvoidableCost'),
                }),
                tooltip: t('interpolation.totalSavingsPerYear', {
                  item: t('plainText.cost').toLowerCase(),
                }),
                value:
                  dailyAvoidableCost && dailyAvoidableCost.value > 0 ? (
                    renderImpactScoreValue(
                      currencyFormatter.format(dailyAvoidableCost.value * 365),
                      t('interpolation.unitPerYear', {
                        unit: dailyAvoidableCost.unit,
                      })
                    )
                  ) : (
                    <Value>--</Value>
                  ),
              },
            ]
          : []),
        ...(showActiveAvoidableEnergy
          ? [
              {
                icon: 'offline_bolt' as const,
                iconFilled: false,
                label: titleCase({
                  language,
                  text: t('labels.activeAvoidableEnergy'),
                }),
                tooltip: t('interpolation.totalSavingsPerYear', {
                  item: t('plainText.energyUsage'),
                }),
                value:
                  dailyAvoidableEnergy && dailyAvoidableEnergy.value > 0 ? (
                    renderImpactScoreValue(
                      ...convertDailyAvoidableEnergyValue(
                        dailyAvoidableEnergy.value
                      )
                    )
                  ) : (
                    <Value>--</Value>
                  ),
              },
            ]
          : []),
        ...(showAverageDuration
          ? [
              {
                icon: 'timelapse' as const,
                iconFilled: false,
                label: titleCase({
                  language,
                  text: t('labels.averageDuration'),
                }),
                tooltip: t('plainText.insightsAverageDurationDescription'),
                value:
                  duration && duration > 0 ? (
                    renderImpactScoreValue(duration, t('plainText.hours'))
                  ) : (
                    <Value>--</Value>
                  ),
              },
            ]
          : []),
      ]}
      title={titleCase({ language, text: t('headers.impactScores') })}
    />
  )
}

export default ImpactScoresTile
