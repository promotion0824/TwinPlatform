import { FullSizeLoader, titleCase } from '@willow/common'
import {
  PriorityBadge,
  iconMap as insightTypeMap,
} from '@willow/common/insights/component/index'
import { calculatePriority } from '@willow/common/insights/costImpacts/getInsightPriority'
import { getPriorityByRange } from '@willow/common/insights/costImpacts/utils'
import {
  selectOccurrences,
  makeFaultyTimes,
} from '@willow/common/utils/insightUtils'
import {
  DiagnosticOccurrence,
  Insight,
  Occurrence,
  PointTwinDto,
} from '@willow/common/insights/insights/types'
import { Time, TooltipWhenTruncated, api, useAnalytics } from '@willow/ui'
import {
  Badge,
  Indicator,
  useTheme,
  Stack,
  Group,
  Card,
  Box,
  IconButton,
  Icon,
} from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useQuery } from 'react-query'
import styled, { css } from 'styled-components'
import 'twin.macro'
import { useState } from 'react'
import routes from '../../../../../../../routes'
import {
  ButtonLink,
  Flex,
  GrayDot,
  INSIGHT_CATEGORY,
  INSIGHT_NAME,
  INSIGHT_PRIORITY,
  TWIN_CATEGORY,
  TWIN_NAME,
} from './shared'

/**
 * A single insight panel to be used in 3D viewer
 * that displays the insight details
 */
export default function OneInsightPanel({
  insight,
  siteId,
  twinName,
  twinCategory,
}: {
  insight: Insight
  siteId: string
  twinName: string
  twinCategory: string
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const analytics = useAnalytics()
  const theme = useTheme()
  const [isOpen, setIsOpen] = useState(false)

  return (
    <Card radius="r4" shadow="s2" mb="s12">
      <Box
        p="s8"
        bg="neutral.bg.accent.default"
        onClick={() => setIsOpen(!isOpen)}
        tw="cursor-pointer"
      >
        <div
          css={`
            display: flex;
            justify-content: space-between;
          `}
        >
          <Flex tw="gap-1">
            {insight.lastStatus === 'new' && (
              <Indicator position="middle-center" pl="s8">
                <span
                  css={`
                    opacity: 0;
                  `}
                >
                  {/* Indicator component from PUI has limited ability to position the indicator.
                      so to fine tune the position of Indicator component, we test out the right
                      position with placeholder text. The actual text content does not matter
                      it just help to position the indicator correctly.
                      */}
                  xxx
                </span>
              </Indicator>
            )}
            <Stack gap={0}>
              <span
                css={{
                  ...theme.font.heading.sm,
                  color: theme.color.neutral.fg.default,
                  textWrap: 'wrap',
                }}
              >
                {insight.ruleName ?? insight.name}
              </span>
              <Flex tw="gap-0.5 items-center">
                <Time
                  css={{
                    ...theme.font.body.xs.regular,
                    color: theme.color.neutral.fg.muted,
                  }}
                  value={insight.occurredDate}
                  format="ago"
                />
                <GrayDot />
                <span
                  css={{
                    ...theme.font.body.xs.regular,
                    color: theme.color.neutral.fg.muted,
                  }}
                >
                  {insightTypeMap[insight.type].value ?? insight.type}
                </span>
              </Flex>
            </Stack>
          </Flex>

          <Flex>
            <PriorityBadge
              css={`
                align-self: center;
                min-width: fit-content;
              `}
              priority={getPriorityByRange(
                calculatePriority({
                  impactScores: insight?.impactScores,
                  language,
                  insightPriority: insight.priority,
                })
              )}
              size="sm"
            />
            <IconButton
              kind="secondary"
              background="transparent"
              css={`
                align-self: center;
                min-width: fit-content;
              `}
            >
              <Icon
                icon={isOpen ? 'keyboard_arrow_up' : 'keyboard_arrow_down'}
              />
            </IconButton>
          </Flex>
        </div>
      </Box>
      {isOpen && (
        <Box p="s12" bg="neutral.bg.panel.default">
          <OneInsightPanelContent insight={insight} />
          <Group w="100%" justify="flex-end">
            <ButtonLink
              to={routes.sites__siteId_insights__insightId(siteId, insight.id)}
              text={titleCase({
                text: t('plainText.viewInsight'),
                language,
              })}
              onClick={() =>
                analytics.track('3D Viewer - View Insight Clicked', {
                  [TWIN_NAME]: twinName,
                  [TWIN_CATEGORY]: twinCategory,
                  [INSIGHT_NAME]: insight.ruleName ?? insight.name,
                  [INSIGHT_CATEGORY]:
                    insightTypeMap[insight.type].value ?? insight.type,
                  [INSIGHT_PRIORITY]: getPriorityByRange(
                    calculatePriority({
                      impactScores: insight?.impactScores,
                      language,
                      insightPriority: insight.priority,
                    })
                  ),
                })
              }
            />
          </Group>
        </Box>
      )}
    </Card>
  )
}

const OneInsightPanelContent = ({ insight }: { insight: Insight }) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const occurrencesQuery = useQuery(
    ['insightOccurrences', insight.siteId, insight.id],
    async (): Promise<Occurrence[]> => {
      const { data } = await api.get(
        `/sites/${insight.siteId}/insights/${insight.id}/occurrences`
      )
      return data
    },
    {
      select: selectOccurrences,
      enabled: !!insight.id && !!insight.siteId,
    }
  )

  // Calculate the start and end date for the first faulty time and
  // use that to query about diagnostic to be consistent
  // with InsightNode page.
  const faultyTimes = makeFaultyTimes({
    occurrences:
      occurrencesQuery.status === 'success' ? occurrencesQuery.data : [],
    language,
  })
  const { start = insight.occurredDate, end = new Date().toISOString() } =
    faultyTimes?.[0] ?? {}

  const insightDiagnosticsQuery = useQuery(
    ['insightDiagnostics', insight.id],
    async (): Promise<Array<DiagnosticOccurrence & PointTwinDto>> => {
      const { data = [] } = await api.get(
        `/insights/${insight.id}/occurrences/diagnostics`,
        {
          params: {
            start,
            end,
          },
        }
      )

      return data
    },
    {
      enabled: !!insight.id && occurrencesQuery.status !== 'loading',
      select: (data) => _.uniqBy(data, 'id'),
    }
  )

  return insightDiagnosticsQuery.status === 'loading' ? (
    <FullSizeLoader />
  ) : (
    <>
      {insight && <InnerHeader>{t('labels.description')}</InnerHeader>}
      <StyledText
        css={css(({ theme }) => ({
          paddingBottom: theme.spacing.s8,
        }))}
      >
        {insight?.description ?? ''}
      </StyledText>
      {(insightDiagnosticsQuery.data ?? []).length > 0 && (
        <>
          <InnerHeader>
            {titleCase({
              text: t('plainText.diagnostics'),
              language,
            })}
          </InnerHeader>
          <Flex
            css={`
              flex-direction: column;
            `}
          >
            {insightDiagnosticsQuery.data?.map(({ id, ruleName, check }) => (
              <Flex key={id}>
                <Badge
                  size="xs"
                  color={check ? 'green' : 'red'}
                  variant="subtle"
                  css={`
                    min-width: fit-content;
                    align-self: center;
                  `}
                >
                  {_.capitalize(
                    check ? t('plainText.pass') : t('plainText.fail')
                  )}
                </Badge>
                <Flex
                  css={`
                    flex-direction: column;
                    justify-content: center;
                    width: 100%;
                  `}
                >
                  <TooltipWhenTruncated
                    css={`
                      max-width: 80%;
                    `}
                    label={ruleName ?? ''}
                  >
                    <StyledText
                      css={`
                        white-space: nowrap;
                      `}
                    >
                      {ruleName ?? ''}
                    </StyledText>
                  </TooltipWhenTruncated>
                </Flex>
              </Flex>
            ))}
          </Flex>
        </>
      )}
    </>
  )
}

const StyledText = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  textOverflow: 'ellipsis',
  overflow: 'hidden',
}))
const InnerHeader = styled(StyledText)(({ theme }) => ({
  color: theme.color.neutral.fg.muted,
}))
