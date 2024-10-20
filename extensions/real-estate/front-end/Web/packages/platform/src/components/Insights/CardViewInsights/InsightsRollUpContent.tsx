import styled from 'styled-components'
import _ from 'lodash'
import { getTotalImpactScoreSummary } from '@willow/common/insights/costImpacts/utils'
import { Icon, Panel, PanelContent } from '@willowinc/ui'
import { InsightMetric, InsightCostImpactPropNames } from '@willow/common'
import { InsightTableControls } from '@willow/common/insights/insights/types'
import { useInsightsContext } from './InsightsContext'

/**
 * A horizontal banner above insight table containing
 * some cards to show summarized information of the table
 *
 * Reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/88974
 */
const InsightsRollUpContent = () => {
  const {
    language,
    t,
    impactView = InsightMetric.cost,
    excludedRollups = [],
    impactScoreSummary = [],
  } = useInsightsContext()

  /**
   * The values here are just placeholder for now
   * Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/88974
   */
  const rollupData = [
    {
      value: getTotalImpactScoreSummary({
        impactScores: impactScoreSummary,
        scoreName:
          impactView === InsightMetric.cost
            ? InsightCostImpactPropNames.dailyAvoidableCost
            : InsightCostImpactPropNames.dailyAvoidableEnergy,
        language,
        multiplier: 365,
        decimalPlaces: 0,
        isRollUpTotal: true,
      }).totalImpactScore,
      header: _.startCase(
        t('interpolation.estimatedAvoidable', {
          expense: impactView,
        })
      ),
      tooltipText: t('interpolation.totalSavingsPerYear', {
        item:
          impactView === InsightMetric.cost
            ? t('plainText.cost').toLowerCase()
            : t('plainText.energyUsage').toLowerCase(),
      }),
      footer: _.startCase(t('plainText.totalPerYear')),
      isVisible: !excludedRollups.includes(
        InsightTableControls.showEstimatedAvoidable
      ),
    },
  ]
  const isContentVisible = rollupData.some((data) => data.isVisible)

  return (
    <>
      {isContentVisible && (
        <StyledPanel id="insightRollupContentPanel" defaultSize={128}>
          <StyledPanelContent>
            <RollupSummaryContainer data-testid="rollupSummary">
              <CardsContainer>
                {rollupData.map(
                  ({ value, header, isVisible, tooltipText, footer }) =>
                    isVisible && (
                      <Card key={header}>
                        <CardHeader>
                          {header}
                          <StyledIcon
                            icon="info"
                            filled={false}
                            data-tooltip={tooltipText ?? undefined}
                            data-tooltip-position="top"
                            data-tooltip-width="242px"
                            data-tooltip-time={500}
                          />
                        </CardHeader>
                        <StyledText>{value}</StyledText>
                        <CardFooter>{footer}</CardFooter>
                      </Card>
                    )
                )}
              </CardsContainer>
            </RollupSummaryContainer>
          </StyledPanelContent>
        </StyledPanel>
      )}
    </>
  )
}

export default InsightsRollUpContent

const StyledPanel = styled(Panel)(({ theme }) => ({
  marginBottom: theme.spacing.s12,
}))

const StyledPanelContent = styled(PanelContent)({
  height: '100%',
})

const CardsContainer = styled.div(({ theme }) => ({
  display: 'flex',
  gap: theme.spacing.s12,
  height: '100%',
}))

const Card = styled.div(({ theme }) => ({
  width: '242px',
  gap: '4px',
  boxShadow: theme.shadow.s2,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: '4px',
  padding: theme.spacing.s8,
  background: theme.color.neutral.bg.accent.default,
  color: theme.color.neutral.fg.default,
  whiteSpace: 'nowrap',
  overflow: 'hidden',
}))

const CardHeader = styled.div(({ theme }) => ({
  display: 'flex',
  ...theme.font.heading.xs,
}))

const CardFooter = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
}))

const StyledText = styled.span(({ theme }) => ({
  ...theme.font.display.md.medium,
  display: 'flex',
  alignItems: 'center',
  height: '36px',
}))

const RollupSummaryContainer = styled.div(({ theme }) => ({
  padding: theme.spacing.s16,
  height: '100%',
}))

const StyledIcon = styled(Icon)(({ theme }) => ({
  marginLeft: theme.spacing.s6,
}))
