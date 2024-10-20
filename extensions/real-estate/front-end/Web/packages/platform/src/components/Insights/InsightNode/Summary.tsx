/* eslint-disable complexity */
import {
  InsightCostImpactPropNames,
  getImpactScore,
  titleCase,
} from '@willow/common'
import {
  InsightTypeBadge,
  PriorityName,
} from '@willow/common/insights/component'
import {
  checkIsWalmartAlert,
  formatDateTime,
  getRollUpTotalImpactScores,
} from '@willow/common/insights/costImpacts/utils'
import { Insight } from '@willow/common/insights/insights/types'
import { getModelInfo } from '@willow/common/twins/utils'
import {
  caseInsensitiveEquals,
  TwinChip,
  getContainmentHelper,
} from '@willow/ui'
import {
  Icon,
  Loader,
  Panel,
  PanelContent,
  PanelGroup,
  Group,
} from '@willowinc/ui'
import _ from 'lodash'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import { css, styled } from 'twin.macro'
import { useSites } from '../../../providers/index'
import routes from '../../../routes'
import SiteChip from '../../../views/Portfolio/twins/page/ui/SiteChip'

const insightDetailsContainer = 'insightDetailsContainer'
const { containerName, getContainerQuery } = getContainmentHelper(
  insightDetailsContainer
)

const Summary = ({
  insight,
  modelInfo,
  status,
  firstOccurredDate,
}: {
  insight: Insight
  modelInfo?: ReturnType<typeof getModelInfo>
  status: string
  firstOccurredDate: string
}) => {
  const history = useHistory()
  const translation = useTranslation()

  const {
    t,
    i18n: { language },
  } = translation
  const sites = useSites()
  const site = sites.find((s) => s.id === insight.siteId)
  const totalImpactScores = useMemo(
    () =>
      getRollUpTotalImpactScores({
        insight,
        timeZone: site?.timeZone ?? 'utc',
        t,
        language,
        firstOccurredDate,
      }),
    [insight, firstOccurredDate, site, language]
  )

  // Show disclaimer section, if all values are empty or content is hidden
  const isImpactSectionDisabled = totalImpactScores.every(
    ({ value, hidden }) => value.includes('--') || hidden
  )

  return (
    <StyledPanelGroup
      direction="vertical"
      css={`
        container-type: size;
        container-name: ${containerName};
      `}
    >
      <PanelWrapper>
        <Panel
          id="insightDetailsPanel"
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
        >
          <StyledPanelContent tw="flex flex-col">
            {[
              {
                heading: t('labels.twin'),
                content:
                  insight?.twinId == null ? null : status === 'loading' ? (
                    <Loader />
                  ) : status === 'error' ? (
                    <Icon
                      icon="warning"
                      css={{
                        color: 'red',
                      }}
                    />
                  ) : (
                    <TwinModelChipContainer
                      onClick={() => {
                        history.push(
                          routes.portfolio_twins_view__siteId__twinId(
                            insight.siteId,
                            insight.twinId
                          )
                        )
                      }}
                    >
                      <TwinChip
                        variant="instance"
                        modelOfInterest={modelInfo?.modelOfInterest}
                        text={insight.equipmentName}
                        highlightOnHover
                      />
                    </TwinModelChipContainer>
                  ),
              },
              {
                heading: t('plainText.skill'),
                content: insight?.ruleName,
              },
              {
                heading: t('labels.description'),
                content: insight?.description,
              },
              {
                heading: t('labels.recommendation'),
                content: insight?.recommendation ? (
                  <span
                    css={css`
                      white-space: pre-wrap;
                    `}
                  >
                    {insight.recommendation}
                  </span>
                ) : caseInsensitiveEquals(insight.sourceName, 'inspection') ? (
                  <NoRecommendationContainer>
                    {t('plainText.noRecommendationInsight')}
                  </NoRecommendationContainer>
                ) : (
                  <span>
                    {titleCase({ text: t('plainText.notAvailable'), language })}
                  </span>
                ),
              },
              {
                heading: t('labels.priority'),
                content: (
                  <PriorityName
                    impactScorePriority={getImpactScore({
                      impactScores: insight.impactScores ?? [],
                      scoreName: InsightCostImpactPropNames.priorityScore,
                      language,
                    })}
                    insightPriority={insight.priority}
                    size="sm"
                  />
                ),
              },
              {
                heading: t('labels.category'),
                content: <InsightTypeBadge type={insight.type} />,
              },
              {
                heading: t('labels.source'),
                content: checkIsWalmartAlert(insight?.ruleId)
                  ? 'Walmart'
                  : insight.sourceName,
              },
              {
                heading: titleCase({
                  text: t('plainText.lastFaultedOccurrence'),
                  language,
                }),
                content: formatDateTime({
                  value: insight.occurredDate,
                  language,
                  timeZone: site?.timeZone ?? 'utc',
                }),
              },
            ].map(
              ({ heading, content }) =>
                content && (
                  <InsightDetail
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
                  `
                    )}
                  />
                )
            )}
          </StyledPanelContent>
        </Panel>
      </PanelWrapper>
      <PanelWrapper>
        <Panel
          id="insightImpactPanel"
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
        >
          {isImpactSectionDisabled ? (
            <DisclaimerContainer tw="flex gap-[10px]">
              <Icon
                icon="info"
                size={20}
                css={css(({ theme }) => ({
                  color: theme.color.neutral.fg.muted,
                }))}
              />
              {t('plainText.noImpactDataAvailable')}
            </DisclaimerContainer>
          ) : (
            <StyledPanelContent>
              {totalImpactScores.map(
                (totalImpactScore) =>
                  !totalImpactScore.hidden &&
                  totalImpactScore.value && (
                    <InsightDetail
                      key={`${totalImpactScore.name}-${totalImpactScore.value}`}
                      heading={totalImpactScore.name}
                      content={totalImpactScore.value}
                      tooltipText={totalImpactScore.tooltip}
                    />
                  )
              )}
            </StyledPanelContent>
          )}
        </Panel>
      </PanelWrapper>
      <PanelWrapper>
        <Panel
          id="insightLocationPanel"
          title={
            <div tw="flex gap-[10px]">
              <Icon icon="location_on" />
              <span>
                {titleCase({
                  text: t('plainText.location'),
                  language,
                })}
              </span>
            </div>
          }
          collapsible
        >
          <StyledPanelContent>
            <InsightDetail
              key={t('labels.site')}
              heading={t('labels.site')}
              content={<SiteChip siteName={site?.name} />}
              css={css(
                ({ theme }) =>
                  `${getContainerQuery(`width < ${theme.breakpoints.mobile}`)} {
                  flex-direction: column;
                };
                `
              )}
            />
          </StyledPanelContent>
        </Panel>
      </PanelWrapper>
    </StyledPanelGroup>
  )
}

export default Summary

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

/**
 * rectangular card with 3 rows of text
 */
export const Card = ({
  firstRow,
  secondRow,
  thirdRow,
  tooltip,
  className,
  isDisabled = false,
}: {
  firstRow: string
  secondRow?: string
  thirdRow: string
  tooltip?: string
  className?: string
  isDisabled?: boolean
}) => {
  const {
    i18n: { language },
  } = useTranslation()
  return (
    <CardContainer className={className} $isDisabled={isDisabled}>
      <div
        css={css(({ theme }) => ({
          display: 'flex',
          gap: theme.spacing.s4,
        }))}
      >
        <span>
          {titleCase({
            text: firstRow,
            language,
          })}
        </span>
        <Icon
          icon="info"
          filled
          data-tooltip={tooltip}
          data-tooltip-position="top"
          data-tooltip-width="250px"
        />
      </div>
      <div>{secondRow}</div>
      <div>{_.startCase(thirdRow)}</div>
    </CardContainer>
  )
}

const CardContainer = styled.div<{ $isDisabled: boolean }>(
  ({ $isDisabled, theme }) => ({
    padding: theme.spacing.s8,
    display: 'flex',
    flexDirection: 'column',
    flexGrow: 1,
    borderRadius: theme.spacing.s4,
    background: theme.color.neutral.bg.accent.default,
    boxShadow: theme.shadow.s2,
    color: $isDisabled ? theme.color.state.disabled.fg : 'inherit',

    '&&&': {
      border: `1px solid ${theme.color.neutral.border.default}`,
    },
    '& > *:nth-child(1)': {
      ...theme.font.heading.xs,
    },
    '& > *:nth-child(2)': {
      ...theme.font.display.md.medium,
    },
    '& > *:nth-child(3)': {
      ...theme.font.body.md.regular,
    },
  })
)

export const InsightDetail = ({
  heading,
  content,
  tooltipText,
  className,
}: {
  heading: string
  content?: React.ReactNode
  className?: string
  tooltipText?: string
}) => (
  <InsightDetailContainer className={className}>
    <HeaderSection>
      <Group w="240px" align="start" gap="s4">
        {_.startCase(heading)}
        {tooltipText && (
          <Icon
            icon="info"
            filled
            data-tooltip={tooltipText}
            data-tooltip-position="top"
            data-tooltip-width="250px"
          />
        )}
      </Group>
    </HeaderSection>
    <div>{content}</div>
  </InsightDetailContainer>
)

const InsightDetailContainer = styled.div(({ theme }) => ({
  display: 'flex',
  width: '100%',
  paddingBottom: theme.spacing.s8,
  paddingTop: theme.spacing.s8,
  gap: theme.spacing.s4,
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
}))

const HeaderSection = styled.div(({ theme }) => ({
  color: theme.color.neutral.fg.muted,
  width: '240px',
  display: 'flex',
}))

const TwinModelChipContainer = styled.div(({ theme }) => ({
  '& *:hover': {
    color: theme.color.neutral.fg.highlight,
    cursor: 'pointer',
  },
}))

const StyledPanelGroup = styled(PanelGroup)`
  container-type: inline-size;
  container-name: insightSummaryPanelContainer;
  overflow-y: auto !important;
`

const DisclaimerContainer = styled.div(({ theme }) => ({
  padding: `0 ${theme.spacing.s16} ${theme.spacing.s16} ${theme.spacing.s16}`,
  overflowX: 'hidden',
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
}))

const NoRecommendationContainer = styled.span(({ theme }) => ({
  color: theme.color.neutral.fg.subtle,
  ...theme.font.body.md.regular,
}))
