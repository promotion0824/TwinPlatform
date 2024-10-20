import { Site, titleCase } from '@willow/common'
import { InsightCardGroups } from '@willow/common/insights/insights/types'
import { useScopeSelector } from '@willow/ui'
import { Popover, useDisclosure, useTheme } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import styled from 'styled-components'
import CountSummary from './CountSummary'
import routes from '../../../routes'

type CountTypes =
  | Site['insightsStats']
  | Site['insightsStatsByStatus']
  | Site['ticketStats']
  | Site['ticketStatsByStatus']

type Variant = 'critical' | 'priority' | 'status'

export enum SummaryType {
  insights = 'insights',
  tickets = 'tickets',
}

const Indicator = styled.div<{ $color: string }>(({ $color, theme }) => ({
  backgroundColor: $color,
  borderRadius: theme.radius.round,
  height: '10px',
  width: '10px',
}))

const IndicatorContainer = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s4,
}))

const IndicatorLabel = styled.div(({ theme }) => ({
  ...theme.font.body.sm.regular,
  color: theme.color.neutral.fg.default,
}))

const IndicatorRow = styled.div(({ theme }) => ({
  display: 'flex',
  gap: theme.spacing.s8,
}))

const PopoverContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s4,
  padding: theme.spacing.s8,
}))

const PopoverHeading = styled.div(({ theme }) => ({
  ...theme.font.heading.xs,
  color: theme.color.neutral.fg.default,
}))

// Returns the count if it is defined, or 0 if it is undefined.
function getCount(counts: CountTypes, key: string): number {
  return counts?.[key] ?? 0
}

function CountIndicator({
  color,
  count,
  label,
}: {
  color: string
  count: number
  label: string
}) {
  if (!count) return null

  return (
    <IndicatorContainer>
      <Indicator $color={color} />
      <IndicatorLabel>{`${count.toLocaleString()} ${label}`}</IndicatorLabel>
    </IndicatorContainer>
  )
}

export const getTotalCount = (
  summaryType: SummaryCountProps['summaryType'],
  variant: Variant,
  counts: CountTypes
) => {
  if (summaryType === SummaryType.tickets) {
    return getCount(counts, 'openCount')
  }

  return variant === 'critical'
    ? getCount(counts, 'urgentCount')
    : variant === 'priority'
    ? getCount(counts, 'lowCount') +
      getCount(counts, 'mediumCount') +
      getCount(counts, 'highCount') +
      getCount(counts, 'urgentCount')
    : getCount(counts, 'inProgressCount') +
      getCount(counts, 'newCount') +
      getCount(counts, 'openCount')
}

interface SummaryCountProps {
  counts: CountTypes
  hideCount?: boolean
  showSummaryName?: boolean
  /**
   * It will have default navigation to the corresponding insights or
   * tickets page under the siteId provided.
   */
  siteId?: string
  summaryType: SummaryType
  variant?: Variant
  /**
   * Callback when the counts get clicked.
   * If not provided, it will default to the Scope's tickets or all insights or
   * critical insights page.
   */
  onClick?: (event: React.MouseEvent<Element, MouseEvent>) => void
}

// eslint-disable-next-line complexity
export default function SummaryCount({
  counts,
  hideCount = false,
  showSummaryName = true,
  siteId,
  summaryType,
  variant = 'status',
  onClick,
}: SummaryCountProps) {
  const history = useHistory()
  const { isScopeSelectorEnabled, scopeLookup } = useScopeSelector()
  const theme = useTheme()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [opened, { close, open }] = useDisclosure()

  // TODO: we can split out getTotalCount, and remove counts props from this
  // component in the future, so that this component will only be responsible
  // for rendering count number that passed in. And do not need to maintain the logic
  // of calculating counts and labels.
  const totalCount = getTotalCount(summaryType, variant, counts)

  const fullSummaryLabel = titleCase({
    language,
    text: t(
      summaryType === SummaryType.tickets
        ? 'interpolation.ticketsCount'
        : variant === 'critical'
        ? 'interpolation.criticalInsightsCount'
        : variant === 'priority'
        ? 'interpolation.totalInsightsCount'
        : 'interpolation.insightsCount',
      {
        count: totalCount,
      }
    ),
  })

  return (
    <Popover opened={opened} position="top" withArrow>
      <Popover.Target>
        <CountSummary
          count={totalCount}
          onClick={(event) => {
            event.stopPropagation()
            if (onClick) {
              onClick(event)
              return
            }

            if (!siteId) {
              // Just in case we cannot get siteId in RelationsMap for locatedTwin
              return
            }

            if (summaryType === SummaryType.insights) {
              const route = isScopeSelectorEnabled
                ? routes.insights_scope__scopeId(scopeLookup[siteId].twin.id)
                : routes.sites__siteId_insights(siteId)

              const fullRoute =
                variant === 'critical'
                  ? `${route}?groupBy=${InsightCardGroups.ALL_INSIGHTS}&priorities=1`
                  : `${route}?groupBy=${InsightCardGroups.INSIGHT_TYPE}`

              history.push(fullRoute)
            } else {
              const route = isScopeSelectorEnabled
                ? routes.tickets_scope__scopeId(scopeLookup[siteId].twin.id)
                : routes.sites__siteId_tickets(siteId)

              history.push(route)
            }
          }}
          onMouseLeave={close}
          onMouseOver={open}
          summaryType={summaryType}
          label={
            hideCount
              ? undefined
              : showSummaryName
              ? fullSummaryLabel
              : // Split the number out of the full summary label so that its formatting is preserved
                fullSummaryLabel.split(' ')[0]
          }
          intent={variant === 'critical' ? 'negative' : 'secondary'}
        />
      </Popover.Target>
      <Popover.Dropdown>
        <PopoverContainer>
          <PopoverHeading>
            {summaryType === SummaryType.insights
              ? titleCase({ language, text: t('headers.activeInsights') })
              : titleCase({ language, text: t('headers.openTickets') })}
          </PopoverHeading>
          {!!totalCount && (
            <IndicatorRow>
              {variant === 'critical' ? (
                <CountIndicator
                  color={theme.color.core.red.bg.bold.default}
                  count={getCount(counts, 'urgentCount')}
                  label={t('plainText.critical')}
                />
              ) : variant === 'priority' ? (
                <>
                  <CountIndicator
                    color={theme.color.core.blue.fg.default}
                    count={getCount(counts, 'lowCount')}
                    label={t('plainText.low')}
                  />
                  <CountIndicator
                    color={theme.color.core.yellow.fg.default}
                    count={getCount(counts, 'mediumCount')}
                    label={t('plainText.medium')}
                  />
                  <CountIndicator
                    color={theme.color.core.orange.fg.default}
                    count={getCount(counts, 'highCount')}
                    label={t('plainText.high')}
                  />
                  <CountIndicator
                    color={theme.color.core.red.bg.bold.default}
                    count={getCount(counts, 'urgentCount')}
                    label={t('plainText.critical')}
                  />
                </>
              ) : summaryType === SummaryType.insights ? (
                <>
                  <CountIndicator
                    color={theme.color.core.yellow.fg.default}
                    count={getCount(counts, 'newCount')}
                    label={t('plainText.new')}
                  />
                  <CountIndicator
                    color={theme.color.core.yellow.fg.default}
                    count={getCount(counts, 'openCount')}
                    label={t('plainText.open')}
                  />
                  <CountIndicator
                    color={theme.color.core.blue.fg.default}
                    count={getCount(counts, 'inProgressCount')}
                    label={titleCase({
                      language,
                      text: t('plainText.inProgress'),
                    })}
                  />
                </>
              ) : (
                <CountIndicator
                  color={theme.color.core.yellow.fg.default}
                  count={getCount(counts, 'openCount')}
                  label={t('plainText.open')}
                />
              )}
            </IndicatorRow>
          )}
        </PopoverContainer>
      </Popover.Dropdown>
    </Popover>
  )
}
