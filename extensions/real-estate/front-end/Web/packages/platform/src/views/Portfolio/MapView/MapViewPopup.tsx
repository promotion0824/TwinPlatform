import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { TwinChip } from '@willow/ui'
import { Button, Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import tw, { css } from 'twin.macro'
import InsightWorkflowStatusPill from '../../../components/InsightStatusPill/InsightWorkflowStatusPill'
import routes from '../../../routes'
import { MapViewInsight, MapViewItem, isMapViewTwin } from './types'

export default function MapViewPopup({
  name,
  insights = [],
  item,
  modelOfInterest,
  headerChip,
}: {
  name?: string
  insights?: MapViewInsight[]
  item: MapViewItem
  modelOfInterest?: ModelOfInterest
  headerChip?: React.ReactNode
}) {
  const isTwin = isMapViewTwin(item)
  const hasInsights = (insights?.length ?? 0) > 0
  const earliestOccurredDate = Math.min(...insights.map((i) => i.occurredDate))
  const earliestDateDiffInDays = Math.ceil(
    (Date.now() - earliestOccurredDate) / (1000 * 60 * 60 * 24)
  )

  return (
    <PanelGroup>
      <Panel
        title={
          <div tw="flex flex-col pt-[8px] pr-[8px]">
            <div tw="flex gap-[8px]">
              <span
                css={css(({ theme }) => ({
                  ...theme.font.heading.lg,
                  color: theme.color.neutral.fg.default,
                  lineHeight: '1.5rem',
                }))}
              >
                {name}
              </span>
              {headerChip}
            </div>
            {modelOfInterest && (
              <div
                css={css(({ theme }) => ({
                  paddingTop: theme.spacing.s8,
                  '& *': {
                    height: 'auto',
                  },
                }))}
              >
                <TwinChip modelOfInterest={modelOfInterest} text={name} />
              </div>
            )}
          </div>
        }
        tw="border-none"
        hideHeaderBorder={!hasInsights}
      >
        <PanelContent>
          <div
            className="map-view-popup"
            css={css`
              display: flex;
              flex-direction: column;
              padding: 16px;
              padding-top: 0px;
              & > * {
                padding: 8px 0px;
                border-bottom: 1px solid #3b3b3b;
                ${tw`truncate`}
              }
            `}
          >
            {hasInsights && (
              <div
                css={css(({ theme }) => ({
                  ...theme.font.heading.md,
                  paddingTop: theme.spacing.s16,
                }))}
              >
                Active Insights
              </div>
            )}
            {/* limited space, only display 3 insights on popup */}
            {(insights ?? []).slice(0, 3)?.map((insight) => (
              <Button
                css={css(({ theme }) => ({
                  '&:focus': {
                    outline: 'none',
                  },
                  width: '100%',
                  display: 'flex',
                  '&&& > div': {
                    margin: 0,
                  },
                  '& *': {
                    textDecoration: 'none',
                    color: theme.color.neutral.fg.default,
                    '&:hover': {
                      color: theme.color.neutral.fg.highlight,
                    },
                  },
                  // the badge should not take full container space
                  '& *:not(.mantine-Badge-root)': {
                    width: '100%',
                  },
                }))}
                kind="secondary"
                background="transparent"
              >
                <a
                  href={`${routes.sites__siteId_insights__insightId(
                    isTwin ? item.siteId : item.id,
                    insight.id
                  )}?days=${earliestDateDiffInDays}${
                    isTwin ? `&twinId=${item.id}` : ''
                  }`}
                  tw="flex gap-[8px]"
                >
                  {insight.lastStatus && (
                    <InsightWorkflowStatusPill
                      // setting width: 100% on container somehow makes the
                      // dot inside this "Badge" component skewed, we apply
                      // min-width to fix it
                      css={css`
                        min-width: fit-content;
                        &::before {
                          min-width: 0.625rem;
                        }
                      `}
                      lastStatus={insight.lastStatus}
                    />
                  )}
                  <div tw="leading-[24px] truncate">
                    {insight.ruleName ?? insight.name}
                  </div>
                </a>
              </Button>
            ))}
            {(insights?.length ?? 0) > 3 && (
              <div tw="border-none">{`+ ${insights.length - 3} more`}</div>
            )}
            {hasInsights && (
              <div tw="border-none flex justify-end pb-0">
                <Button
                  kind="secondary"
                  background="transparent"
                  css={css(({ theme }) => ({
                    border: `1px solid ${theme.color.neutral.border.default}`,
                    '& *': {
                      textDecoration: 'none',
                      color: theme.color.neutral.fg.default,
                      '&:hover': {
                        color: theme.color.neutral.fg.highlight,
                      },
                    },
                  }))}
                >
                  {/* 
                        include days and twinId in query params
                        to ensure the insights user see on MapViewMap
                        will show up in Insights Page
                    */}
                  <a
                    href={`${routes.sites__siteId_insights(
                      isTwin ? item.siteId : item.id
                    )}?days=${earliestDateDiffInDays}${
                      isTwin ? `&twinId=${item.id}` : ''
                    }`}
                  >
                    {`View ${insights.length} Insights`}
                  </a>
                </Button>
              </div>
            )}
          </div>
        </PanelContent>
      </Panel>
    </PanelGroup>
  )
}
