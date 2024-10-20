import { Fragment } from 'react'
import tw, { styled, css } from 'twin.macro'
import { TFunction } from 'react-i18next'
import {
  titleCase,
  getImpactScore,
  InsightMetric,
  InsightCostImpactPropNames,
} from '@willow/common'
import { InsightTypesGroupedByDate } from '@willow/common/insights/insights/types'
import { Badge, Icon } from '@willowinc/ui'
import FullSizeLoader from '@willow/common/components/FullSizeLoader'
import { iconMap } from '@willow/common/insights/component'
import { useInsightsContext } from '../InsightsContext'
import IndividualCard from './IndividualCard'
import NotFound from '../../ui/NotFound'

const InsightTypeCards = ({
  noData,
  insightTypesGroupedByDate,
  t,
  language,
  isLoading,
  impactView,
}: {
  noData: boolean
  t: TFunction
  insightTypesGroupedByDate: InsightTypesGroupedByDate
  language: string
  isLoading: boolean
  impactView: string
}) => {
  const isCostView = impactView === InsightMetric.cost

  const { handleInsightTypeClick } = useInsightsContext()

  return isLoading ? (
    <FullSizeLoader />
  ) : noData ? (
    <NotFound
      message={titleCase({
        language,
        text: t('plainText.noSkillsFound'),
      })}
    />
  ) : (
    <>
      {insightTypesGroupedByDate?.map(
        (item) =>
          item.insightTypes.length > 0 && (
            <Fragment key={item.title}>
              <DateSection> {item.title}</DateSection>
              <GridLayout>
                {item.insightTypes.map((card, index) => {
                  const {
                    ruleId,
                    insightType,
                    ruleName,
                    insightCount,
                    priority,
                    lastOccurredDate,
                    impactScores,
                  } = card
                  const isTransparent = insightType === 'multiple'
                  const { icon, color, value } = iconMap[insightType] ?? {}

                  return (
                    <IndividualCard
                      key={ruleId ?? `${ruleName}${index}`}
                      title={ruleName}
                      type={insightType}
                      insightCount={insightCount}
                      priority={priority}
                      lastOccurred={lastOccurredDate}
                      impactTitle={
                        isCostView
                          ? InsightCostImpactPropNames.avoidableCostPerYear
                          : InsightCostImpactPropNames.avoidableEnergyPerYear
                      }
                      impactScore={
                        impactScores
                          ? getImpactScore({
                              impactScores,
                              scoreName: isCostView
                                ? InsightCostImpactPropNames.dailyAvoidableCost
                                : InsightCostImpactPropNames.dailyAvoidableEnergy,
                              language,
                              multiplier: 365,
                              decimalPlaces: 0,
                            })
                          : undefined
                      }
                      t={t}
                      language={language}
                      onClick={() => handleInsightTypeClick(card)}
                      insightTypeBadge={
                        // render <Icon /> as prefix somehow cause
                        // misalignment on parent container vertically
                        // so we always render prefix but make it transparent
                        // when not needed
                        <Badge
                          css={css(({ theme }) => ({
                            '&&& > *:first-child': {
                              marginRight: isTransparent ? 0 : theme.spacing.s8,
                            },
                          }))}
                          size="sm"
                          variant="subtle"
                          color={color}
                          prefix={
                            <Icon
                              css={css(({ theme }) => ({
                                width: icon ? theme.spacing.s16 : 0,
                                color: isTransparent
                                  ? 'transparent'
                                  : undefined,
                              }))}
                              size={16}
                              icon={icon ?? 'unknown_2'}
                            />
                          }
                        >
                          {value ?? ''}
                        </Badge>
                      }
                    />
                  )
                })}
              </GridLayout>
            </Fragment>
          )
      )}
    </>
  )
}

const GridLayout = styled.div(({ theme }) => ({
  display: 'grid',
  gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
  gap: theme.spacing.s12,
  padding: theme.spacing.s16,
  width: '100%',
}))

const DateSection = styled.div(({ theme }) => ({
  ...theme.font.heading.group,
  color: theme.color.neutral.fg.muted,
  textTransform: 'uppercase',
  padding: `${theme.spacing.s8} ${theme.spacing.s16}`,
}))

export default InsightTypeCards
