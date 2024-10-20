import {
  InsightCostImpactPropNames,
  InsightMetric,
  formatDateTime,
  getImpactScore,
  titleCase,
} from '@willow/common'
import {
  InsightTypeBadge,
  PriorityName,
} from '@willow/common/insights/component'
import { getTotalImpactScoreSummary } from '@willow/common/insights/costImpacts/utils'
import { getModelInfo } from '@willow/common/twins/utils'
import TwinModelChip from '@willow/common/twins/view/TwinModelChip'
import { getContainmentHelper } from '@willow/ui'
import { Icon, Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useQuery } from 'react-query'
import { css } from 'styled-components'
import { styled } from 'twin.macro'
import { useInsightsContext } from '../CardViewInsights/InsightsContext'
import { Card, InsightDetail } from '../InsightNode/Summary'

const insightTypeNodeContainer = 'insightTypeNodeContainer'
const { containerName, getContainerQuery } = getContainmentHelper(
  insightTypeNodeContainer
)

const Summary = () => {
  const {
    isLoading,
    t,
    language,
    impactView,
    isWalmartAlert,
    cards = [],
    ontologyQuery: { data: ontology },
    modelsOfInterestQuery: { data: { items: modelsOfInterest } = {} },
  } = useInsightsContext()
  const translation = useTranslation()

  const modelQuery = useQuery(
    ['models'],
    () => {
      const model = ontology?.getModelById(cards[0]?.primaryModelId as string)
      return (
        model &&
        ontology &&
        modelsOfInterest &&
        getModelInfo(model, ontology, modelsOfInterest, translation)
      )
    },
    {
      enabled:
        cards[0]?.primaryModelId != null &&
        ontology != null &&
        modelsOfInterest != null,
    }
  )

  const totalImpactScore = cards[0]?.impactScores
    ? getTotalImpactScoreSummary({
        impactScores: cards[0]?.impactScores,
        scoreName:
          impactView === InsightMetric.cost
            ? InsightCostImpactPropNames.dailyAvoidableCost
            : InsightCostImpactPropNames.dailyAvoidableEnergy,
        language,
        multiplier: 365,
        decimalPlaces: 0,
        isRollUpTotal: true,
      })?.totalImpactScore
    : '--'

  return (
    <Panel
      collapsible
      defaultSize={22}
      title={t('labels.summary')}
      data-testid="insightTypeNodePageLeftPanel"
      id="insight-type-node-page-left-summary-panel"
    >
      {isLoading ? null : (
        <PanelContent tw="h-full">
          <StyledPanelGroup
            direction="vertical"
            css={`
              container-type: size;
              container-name: ${containerName};
            `}
          >
            {totalImpactScore !== '--' ? (
              <PanelWrapper>
                <Panel
                  title={
                    <div tw="flex gap-[10px]">
                      <Icon icon="speed" />
                      <span>
                        {titleCase({
                          text: t('plainText.impacts'),
                          language,
                        })}
                      </span>
                    </div>
                  }
                  collapsible
                  id="insight-type-node-page-left-summary-impacts-panel"
                >
                  <StyledPanelContent tw="flex gap-[12px] flex-wrap">
                    <Card
                      tw="w-[346px]"
                      key={t('plainText.totalSavingsPerYearTooltip')}
                      firstRow={_.startCase(
                        t('interpolation.estimatedAvoidable', {
                          expense: impactView,
                        })
                      )}
                      secondRow={totalImpactScore}
                      thirdRow={_.startCase(t('plainText.totalPerYear'))}
                      tooltip={t('plainText.totalSavingsPerYearTooltip')}
                    />
                  </StyledPanelContent>
                </Panel>
              </PanelWrapper>
            ) : (
              // to satisfy PanelGroup's child prop type
              <></>
            )}
            <PanelWrapper>
              <Panel
                title={
                  <div tw="flex gap-[10px]">
                    <Icon icon="subject" />
                    <span>
                      {titleCase({
                        text: t('headers.details'),
                        language,
                      })}
                    </span>
                  </div>
                }
                collapsible
                id="insight-type-node-page-left-summary-details-panel"
              >
                <StyledPanelContent tw="flex flex-col">
                  {[
                    {
                      heading: t('labels.priority'),
                      content: (
                        <PriorityName
                          impactScorePriority={getImpactScore({
                            impactScores: cards[0]?.impactScores,
                            scoreName: InsightCostImpactPropNames.priorityScore,
                            language,
                          })}
                          insightPriority={cards[0]?.priority}
                          size="sm"
                        />
                      ),
                    },
                    {
                      heading: t('plainText.twin'),
                      content: (
                        <TwinModelChip
                          model={modelQuery.data?.model}
                          modelOfInterest={modelQuery.data?.modelOfInterest}
                        />
                      ),
                      isVisible: !!(
                        modelQuery.data?.model &&
                        modelQuery.data?.modelOfInterest
                      ),
                    },
                    {
                      heading: t('labels.category'),
                      content: (
                        <InsightTypeBadge type={cards[0]?.insightType} />
                      ),
                    },
                    {
                      heading: t('plainText.totalInsights'),
                      content: cards[0]?.insightCount,
                    },
                    {
                      heading: t('labels.source'),
                      content: isWalmartAlert
                        ? 'Walmart'
                        : cards[0]?.sourceName || t('plainText.multiple'),
                    },
                    {
                      heading: titleCase({
                        text: t('plainText.lastOccurrence'),
                        language,
                      }),
                      content: formatDateTime({
                        value: cards[0]?.lastOccurredDate,
                        language,
                      }),
                    },
                  ].map(
                    ({ heading, content, isVisible = true }) =>
                      isVisible &&
                      content && (
                        <StyledInsightDetail
                          key={heading}
                          heading={heading}
                          content={content}
                          css={css(
                            ({ theme }) =>
                              `
                          ${getContainerQuery(
                            `width < ${theme.breakpoints.mobile}`
                          )} {
                            flex-direction: column;
                          };
                          && {
                            gap: ${theme.spacing.s4};
                          }
                        `
                          )}
                        />
                      )
                  )}
                  {cards[0]?.recommendation && (
                    <>
                      <span>
                        {titleCase({
                          text: t('labels.recommendation'),
                          language,
                        })}
                      </span>
                      <List>{cards[0]?.recommendation}</List>
                    </>
                  )}
                </StyledPanelContent>
              </Panel>
            </PanelWrapper>
          </StyledPanelGroup>
        </PanelContent>
      )}
    </Panel>
  )
}

export default Summary

const StyledPanelGroup = styled(PanelGroup)`
  container-type: inline-size;
  container-name: insightSummaryPanelContainer;
  overflow-y: auto !important;
`

const StyledInsightDetail = styled(InsightDetail)({
  justifyContent: 'space-between',
  '&& > *:nth-child(1) > *': {
    maxWidth: '240px',
    width: 'auto',
  },
})

const List = styled.div(({ theme }) => ({
  marginTop: theme.spacing.s4,
  paddingLeft: theme.spacing.s16,
  color: theme.color.neutral.fg.default,
  whiteSpace: 'pre-wrap',
  ...theme.font.body.md.regular,
}))

const PanelWrapper = styled.div(({ theme }) => ({
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  // remove border on outermost div of <Panel /> and <PanelHeader />
  '&&& > div, &&& > div > div > div': {
    border: 'none',
  },
}))

const StyledPanelContent = styled(PanelContent)(({ theme }) => ({
  padding: `0 ${theme.spacing.s16} ${theme.spacing.s16} ${theme.spacing.s16}`,
  overflowX: 'hidden',
}))
