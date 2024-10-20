/* eslint-disable complexity */
import { FullSizeContainer, titleCase } from '@willow/common'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import { getModelInfo } from '@willow/common/twins/utils'
import TwinModelChip from '@willow/common/twins/view/TwinModelChip'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  Number,
  Time,
  api,
  useAnalytics,
  getContainmentHelper,
  TooltipWhenTruncated,
} from '@willow/ui'
import {
  Badge,
  Group,
  IconButton,
  Panel,
  PanelContent,
  PanelGroup,
  Tabs,
  useTheme,
} from '@willowinc/ui'
import _ from 'lodash'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery } from 'react-query'
import styled from 'styled-components'
import 'twin.macro'
import useGetInsights from '../../../../../../../hooks/Insight/useGetInsights'
import { TicketsResponse } from '../../../../../../../services/Tickets/TicketsService'
import useOntologyInPlatform from '../../../../../../../hooks/useOntologyInPlatform'
import routes from '../../../../../../../routes'
import { FilterOperator } from '../../../../../../../services/Insight/InsightsService'
import OneInsightPanel from './OneInsightPanel'
import OneTicketPanel from './OneTicketPanel'
import {
  customLocalStorage,
  assetDetailPanelInsightsPanelKey,
  assetDetailPanelTicketsPanelKey,
  OPEN_INSIGHTS,
  TWIN_NAME,
  TWIN_CATEGORY,
  IN_PROGRESS_INSIGHTS,
  TICKETS_COUNT,
  ButtonLink,
  Flex,
  MoreOrLessExpander,
} from './shared'

const assetDetailPanelHeaderPanel = 'assetDetailPanelHeaderPanel'
const { containerName, getContainerQuery } = getContainmentHelper(
  assetDetailPanelHeaderPanel
)

/**
 * A Panel containing detailed information about the selected asset
 * to be used in the Floor Viewer page.
 */
const AssetDetailPanel = ({
  siteId,
  twinIdOfSelectedAsset,
  asset,
  onCloseClick,
  selectedDataTab = 'insights',
  onSelectedDataTabChange,
}: {
  siteId: string
  twinIdOfSelectedAsset?: string
  asset: {
    id: string
    name: string
  }
  onCloseClick: () => void
  selectedDataTab?: string
  onSelectedDataTabChange: (tab: string) => void
}) => {
  const theme = useTheme()
  const [isLivePointsExpanded, setLivePointsExpanded] = useState(false)
  const [selectInsightsTab, setSelectedInsightsTab] = useState<string | null>(
    'open'
  )
  const translation = useTranslation()
  const {
    t,
    i18n: { language },
  } = translation
  const analytics = useAnalytics()

  const liveDataQuery = useQuery<
    {
      title: string
      liveDataPoints: Array<{
        id: string
        tag: string
        unit: string
        liveDataValue?: number
        liveDataTimestamp: string
      }>
    },
    Error
  >(
    ['liveData', siteId, asset?.id],
    async () => {
      const response = await api.get(
        `/sites/${siteId}/assets/${asset?.id}/pinOnLayer`
      )

      return response.data
    },
    {
      enabled: !!asset?.id,
      select: (data) => ({
        ...data,
        liveDataPoints: _.uniqBy(data.liveDataPoints, (p) => p.id),
      }),
    }
  )
  const liveDataPoints = liveDataQuery.data?.liveDataPoints ?? []

  const assetInsightsQuery = useGetInsights(
    {
      filterSpecifications: [
        {
          field: 'siteId',
          operator: FilterOperator.equalsLiteral,
          value: siteId,
        },
        {
          field: 'status',
          operator: FilterOperator.containedIn,
          value: ['open', 'inProgress', 'new'],
        },
        {
          field: 'equipmentId',
          operator: FilterOperator.equalsLiteral,
          value: asset?.id,
        },
      ],
    },
    {
      enabled: asset?.id != null && siteId !== null,
      // a business logic to not present diagnostic insights as they will be fetched
      // with a separate query.
      select: (data) => data.filter((insight) => insight.type !== 'diagnostic'),
    }
  )

  const assetTicketsQuery = useQuery<TicketsResponse>(
    ['tickets', [siteId, asset?.id]],
    async () => {
      const response = await api.get(`/sites/${siteId}/tickets`, {
        params: {
          assetId: asset?.id,
          tab: TicketTab.open,
        },
      })
      return response.data
    },
    {
      enabled: !!siteId && !!asset?.id,
      select: (data) =>
        data?.filter(
          (ticket) =>
            twinIdOfSelectedAsset && ticket?.twinId === twinIdOfSelectedAsset
        ),
    }
  )

  // use cognitiveSearch endpoint to avoid relying on siteId
  const twinQuery = useQuery<{
    twins: Array<{
      id: string
      modelId: string
      externalId: string
      name: string
    }>
  }>(
    ['cognitively-searching-a-twin', twinIdOfSelectedAsset],
    async () => {
      const response = await api.get('/twins/cognitiveSearch', {
        params: {
          term: twinIdOfSelectedAsset,
        },
      })
      return response.data
    },
    { enabled: !!twinIdOfSelectedAsset }
  )
  const { data: { items: modelsOfInterest } = {} } = useModelsOfInterest()
  const { data: ontology } = useOntologyInPlatform()
  const twin = (twinQuery?.data?.twins ?? []).find(
    (nextTwin) => nextTwin.id === twinIdOfSelectedAsset
  )
  const { modelId: twinModelId } = twin ?? {}

  const modelQuery = useQuery(
    ['models', twinModelId],
    () => {
      if (twinModelId != null && ontology != null && modelsOfInterest != null) {
        const model = ontology.getModelById(twinModelId)
        return getModelInfo(model, ontology, modelsOfInterest, translation)
      }
    },
    {
      enabled:
        twinModelId != null && ontology != null && modelsOfInterest != null,
    }
  )

  const {
    open: openInsights = [],
    inProgress: inProgressInsights = [],
    new: newInsights = [],
  } = _.groupBy(assetInsightsQuery?.data ?? [], 'lastStatus')
  const nonInProgressInsights = [...openInsights, ...newInsights]
  const twinName = twin?.name ?? asset?.name ?? liveDataQuery?.data?.title
  const twinCategory = modelQuery.data?.modelOfInterest?.name ?? ''
  const hasNoInsights =
    assetInsightsQuery.isSuccess && assetInsightsQuery.data?.length === 0
  const hasNoTickets =
    assetTicketsQuery.isSuccess && assetTicketsQuery.data?.length === 0

  useEffect(() => {
    const isModalQuerySuccess = twinIdOfSelectedAsset
      ? modelQuery.isSuccess
      : true
    if (
      liveDataQuery.isSuccess &&
      isModalQuerySuccess &&
      assetInsightsQuery.isSuccess
    ) {
      analytics.track('3D Viewer - Twin Summary Panel Viewed', {
        [TWIN_NAME]: twinName,
        [TWIN_CATEGORY]: twinCategory,
        [OPEN_INSIGHTS]: nonInProgressInsights.length,
        [IN_PROGRESS_INSIGHTS]: inProgressInsights.length,
        [TICKETS_COUNT]: assetTicketsQuery.data?.length ?? 0,
      })
    }
  }, [
    liveDataQuery.isSuccess,
    twinIdOfSelectedAsset,
    modelQuery.isSuccess,
    assetInsightsQuery,
    twinName,
    twinCategory,
    nonInProgressInsights.length,
    inProgressInsights.length,
    analytics,
    assetTicketsQuery.data?.length,
  ])

  const handleCloseClick = () => {
    analytics.track('3D Viewer - 3D Twin Summary Panel Closed', {
      [TWIN_NAME]: twinName,
      [TWIN_CATEGORY]: twinCategory,
    })
    onCloseClick()
  }

  return (
    <Panel
      defaultSize={30}
      id={`asset-detail-panel-${asset.id}`}
      // apply min-height to the header panel to fit in the TwinChip
      css={{
        '&&': {
          minWidth: 'auto',
        },
        '& > div > div': {
          minHeight: 'fit-content',
          padding: theme.spacing.s16,
        },
        containerType: 'size',
        containerName,
      }}
      title={
        <div
          css={`
            display: flex;
            flex-direction: column;
          `}
        >
          <Group mb="s4" tw="justify-between">
            <TooltipWhenTruncated label={twinName} key={asset.id}>
              <div
                css={{
                  ...theme.font.heading.xl2,
                  whiteSpace: 'nowrap',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                }}
              >
                {twinName}
              </div>
            </TooltipWhenTruncated>
            <div>
              {twinIdOfSelectedAsset && (
                <ButtonLink
                  to={routes.portfolio_twins_view__siteId__twinId(
                    siteId,
                    twinIdOfSelectedAsset
                  )}
                  onClick={() =>
                    analytics.track('3D Viewer - Go to Twin', {
                      [TWIN_NAME]: twinName,
                      [TWIN_CATEGORY]: twinCategory,
                    })
                  }
                  text={titleCase({
                    text: t('plainText.goToTwin'),
                    language,
                  })}
                />
              )}
              <IconButton
                onClick={handleCloseClick}
                icon="close"
                kind="secondary"
                css={`
                  outline: none;
                `}
              />
            </div>
          </Group>
          {modelQuery.data && (
            <TwinModelChip
              model={modelQuery.data.model}
              modelOfInterest={modelQuery.data.modelOfInterest}
              css={`
                width: fit-content;

                & > div {
                  height: 100%;
                  display: flex;
                  flex-direction: column;
                  justify-content: center;
                  margin: -1px;
                }
              `}
            />
          )}
          {liveDataPoints.length > 0 && (
            <Flex
              css={{
                width: '100%',
                flexDirection: 'column',
                marginTop: theme.spacing.s16,
              }}
            >
              {(
                liveDataPoints.slice(
                  0,
                  isLivePointsExpanded
                    ? liveDataPoints.length
                    : initialLivePointExpandThreshold
                ) ?? []
              ).map(({ id, tag, unit, liveDataValue, liveDataTimestamp }) => (
                <div
                  key={id}
                  css={`
                    width: 100%;
                    display: flex;
                    justify-content: space-between;
                    ${getContainerQuery(`width < 300px`)} {
                      flex-wrap: wrap;
                    }
                  `}
                >
                  <TooltipWhenTruncated label={tag}>
                    <StyledText
                      css={{
                        color: theme.color.neutral.fg.muted,
                      }}
                    >
                      {tag}
                    </StyledText>
                  </TooltipWhenTruncated>
                  <div
                    css={`
                      display: flex;
                    `}
                  >
                    {liveDataValue == null ? (
                      <div>{t('plainText.noDataInLastHour')}</div>
                    ) : (
                      <>
                        {unit === 'Bool' ? (
                          <StyledText>
                            {liveDataValue === 0
                              ? t('plainText.off')
                              : t('plainText.on')}
                          </StyledText>
                        ) : (
                          <>
                            <Number
                              css={{
                                ...theme.font.body.md.regular,
                                color: theme.color.neutral.fg.default,
                                marginRight: theme.spacing.s4,
                              }}
                              value={liveDataValue}
                              format="0.[00]"
                            />
                            <StyledText>{unit}</StyledText>
                          </>
                        )}
                        <Time
                          css={{
                            marginLeft: theme.spacing.s8,
                            lineHeight: theme.spacing.s20,
                          }}
                          value={liveDataTimestamp}
                          format="ago"
                        />
                      </>
                    )}
                  </div>
                </div>
              ))}
              {liveDataPoints.length > initialLivePointExpandThreshold && (
                <MoreOrLessExpander
                  onClick={() => setLivePointsExpanded(!isLivePointsExpanded)}
                  expanded={isLivePointsExpanded}
                />
              )}
            </Flex>
          )}
        </div>
      }
    >
      <Tabs
        defaultValue={selectedDataTab}
        onTabChange={onSelectedDataTabChange}
        pb="0"
      >
        <Tabs.List>
          {[
            [assetInsightsQuery.data?.length ?? 0, 'insights'],
            [assetTicketsQuery.data?.length ?? 0, 'tickets'],
          ].map(([count, value]: [number, string]) => (
            <Tabs.Tab
              key={value}
              suffix={count > 0 && <Badge>{count}</Badge>}
              value={value}
            >
              {titleCase({
                text: t(`headers.${value}`),
                language,
              })}
            </Tabs.Tab>
          ))}
        </Tabs.List>
      </Tabs>
      <PanelContent
        css={{
          padding: theme.spacing.s16,
          height: '100%',
        }}
      >
        {selectedDataTab === 'insights' && !hasNoInsights && (
          <div
            css={{
              paddingBottom: theme.spacing.s12,
              display: 'flex',
              flexDirection: 'column',
            }}
          >
            <div
              css={{
                ...theme.font.heading.md,
                color: theme.color.neutral.fg.default,
              }}
            >
              {titleCase({
                text: t('headers.activeInsights'),
                language,
              })}
            </div>
            <Tabs
              defaultValue={selectInsightsTab}
              onTabChange={(value) => setSelectedInsightsTab(value)}
            >
              <Tabs.List>
                <Tabs.Tab
                  value="open"
                  suffix={<Badge>{nonInProgressInsights.length}</Badge>}
                >
                  {titleCase({
                    text: t('plainText.open'),
                    language,
                  })}
                </Tabs.Tab>
                <Tabs.Tab
                  value="inProgress"
                  suffix={<Badge>{inProgressInsights.length}</Badge>}
                >
                  {titleCase({
                    text: t('plainText.inProgress'),
                    language,
                  })}
                </Tabs.Tab>
              </Tabs.List>
            </Tabs>
          </div>
        )}
        <>
          <PanelGroup
            key={
              selectedDataTab === 'insights'
                ? selectInsightsTab?.toString() || 'insightsPanelGroup'
                : asset.id
            }
            direction="vertical"
            css={`
              height: fit-content !important;
            `}
            storage={customLocalStorage}
            autoSaveId={
              selectedDataTab === 'insights'
                ? assetDetailPanelInsightsPanelKey
                : assetDetailPanelTicketsPanelKey
            }
          >
            {/*
              Display In Progress Insights when the selected tab is Insights and the selected insights tab is In Progress;
              display Other Insights when the selected tab is Insights and the selected insights tab is Open;
              display Tickets when the selected tab is not Insights.
            */}
            {(selectedDataTab === 'insights'
              ? selectInsightsTab === 'inProgress'
                ? inProgressInsights
                : nonInProgressInsights
              : assetTicketsQuery.data ?? []
            ).map((item) =>
              selectedDataTab === 'insights' ? (
                <OneInsightPanel
                  key={item.id}
                  insight={item}
                  siteId={siteId}
                  twinName={twinName}
                  twinCategory={twinCategory}
                />
              ) : (
                <OneTicketPanel
                  key={item.id}
                  ticket={item}
                  siteId={siteId}
                  twinName={twinName}
                  twinCategory={twinCategory}
                />
              )
            )}
          </PanelGroup>
          {(selectedDataTab === 'insights' && hasNoInsights) ||
          (selectedDataTab === 'tickets' && hasNoTickets) ? (
            <NoDataContainer>
              {titleCase({
                text: t(
                  selectedDataTab === 'insights'
                    ? 'plainText.noInsights'
                    : 'plainText.noTickets'
                ),
                language,
              })}
            </NoDataContainer>
          ) : null}
        </>
      </PanelContent>
    </Panel>
  )
}

export default AssetDetailPanel

const initialLivePointExpandThreshold = 5
const StyledText = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  whiteSpace: 'nowrap',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
}))
const NoDataContainer = styled(FullSizeContainer)(({ theme }) => ({
  ...theme.font.heading.lg,
  color: theme.color.neutral.fg.default,
}))
