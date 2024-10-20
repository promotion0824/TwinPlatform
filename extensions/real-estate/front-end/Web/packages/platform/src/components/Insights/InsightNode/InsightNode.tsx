/* eslint-disable complexity */
import { titleCase } from '@willow/common'
import FullSizeLoader from '@willow/common/components/FullSizeLoader'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { getInsightPoints } from '@willow/common/insights/costImpacts/utils'
import {
  ActivityKey,
  DiagnosticOccurrence,
  Insight,
  InsightPointsDto,
  InsightTab,
  InsightWorkflowActivity,
  Occurrence,
  ParamsDictionary,
  PointTwinDto,
  PointType,
  SortBy,
  TimeSeriesTwinInfo,
} from '@willow/common/insights/insights/types'
import { getModelInfo } from '@willow/common/twins/utils'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  makeFaultyTimes,
  selectOccurrences,
} from '@willow/common/utils/insightUtils'
import {
  api,
  DocumentTitle,
  Message,
  reduceQueryStatuses,
  useDateTime,
  useFeatureFlag,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import {
  Badge,
  Button,
  Icon,
  IconButton,
  Indicator,
  PageTitle,
  PageTitleItem,
  Panel,
  PanelContent,
  PanelGroup,
  Tabs,
} from '@willowinc/ui'
import _, { set } from 'lodash'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  QueryClient,
  QueryKey,
  useQueries,
  useQuery,
  useQueryClient,
  UseQueryResult,
} from 'react-query'
import { useHistory, useParams } from 'react-router'
import { Link } from 'react-router-dom'
import { styled } from 'twin.macro'
import useUpdateInsightsStatuses from '../../../hooks/Insight/useUpdateInsightsStatuses'
import useOntologyInPlatform from '../../../hooks/useOntologyInPlatform'
import { useSites } from '../../../providers'
import routes from '../../../routes'
import { TicketSimpleDto } from '../../../services/Tickets/TicketsService'
import HeaderWithTabs from '../../../views/Layout/Layout/HeaderWithTabs'
import AssetDetailsModal, {
  Item,
} from '../../AssetDetailsModal/AssetDetailsModal'
import Activities from '../../AssetDetailsModal/InsightModal/InsightMetricsForm/Tab/Activities'
import InsightWorkflowTimeSeries from '../../AssetDetailsModal/InsightModal/InsightMetricsForm/Tab/InsightWorkflowTimeSeries'
import Occurrences from '../../AssetDetailsModal/InsightModal/InsightMetricsForm/Tab/Occurrences'
import InsightWorkflowStatusPill from '../../InsightStatusPill/InsightWorkflowStatusPill'
import { SelectedPointsProvider, useSelectedPoints } from '../../MiniTimeSeries'
import ActionsViewControl, { InsightActions } from '../ui/ActionsViewControl'
import NotFound from '../ui/NotFound'
import Actions from './Actions'
import Diagnostics from './Diagnostics'
import { ErrorMessage } from './shared'
import Summary from './Summary'

const InsightNode = ({
  enableDiagnostics = false,
}: {
  enableDiagnostics?: boolean
}) => {
  const groupByQueryParam = useUser()?.localOptions?.insightsGroupBy
  const { insightId } = useParams<{ insightId: string }>()
  const insightDetailQuery = useQuery<
    Insight,
    {
      response: { status: number }
    }
  >(['insightInfo', insightId], async (): Promise<Insight> => {
    const { data } = await api.get(`/insights/${insightId}`)
    return data
  })

  return (
    <SelectedPointsProvider>
      <InsightNodeContent
        enableDiagnostics={enableDiagnostics}
        insightQuery={insightDetailQuery}
        groupByQueryParam={groupByQueryParam}
      />
    </SelectedPointsProvider>
  )
}

/**
 * This is the component dedicated to a single insight, called a "InsightNode",
 * user can click on an insight row on insights table to open this component
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/87166
 */
const InsightNodeContent = ({
  insightQuery,
  enableDiagnostics = false,
  groupByQueryParam,
}: {
  insightQuery: UseQueryResult<
    Insight,
    {
      response: { status: number }
    }
  >
  enableDiagnostics?: boolean
  groupByQueryParam?: string
}) => {
  const { isScopeSelectorEnabled, location, locationName } = useScopeSelector()
  const scopeId = location?.twin?.id
  const selectedPointsContext = useSelectedPoints()
  const sites = useSites()
  const featureFlags = useFeatureFlag()

  const { params: diagnosticQueryParams } = selectedPointsContext
  const history = useHistory()
  const dateTime = useDateTime()
  const queryClient = useQueryClient()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const { insightId, siteId: siteIdFromPath } = useParams<{
    insightId: string
    siteId?: string
  }>()
  const [
    { insightTab = InsightTab.Summary, ticketId, action, period },
    setSearchParams,
  ] = useMultipleSearchParams(['insightTab', 'ticketId', 'action', 'period'])
  const cachedInsight = getCachedInsight(insightId, queryClient)

  const { siteId } = cachedInsight || insightQuery?.data || {}
  const enabledInsightInfoQueries = siteId != null && insightId != null

  const site = sites.find((s) => s.id === siteId)

  const insightInfoQueries = useQueries([
    {
      queryKey: ['insightTickets', siteId, insightId],
      queryFn: async (): Promise<TicketSimpleDto[]> => {
        const { data } = await api.get(
          `/sites/${siteId}/insights/${insightId}/tickets`
        )
        return data
      },
      enabled:
        enabledInsightInfoQueries && !site?.features?.isTicketingDisabled,
    },
    {
      queryKey: ['insightOccurrences', siteId, insightId],
      queryFn: async (): Promise<Occurrence[]> => {
        const { data } = await api.get(
          `/sites/${siteId}/insights/${insightId}/occurrences`
        )
        return data
      },
      enabled: enabledInsightInfoQueries,
      select: selectOccurrences,
    },
    {
      queryKey: ['insightPoints', siteId, insightId],
      queryFn: async (): Promise<InsightPointsDto> => {
        const { data } = await api.get(
          `/sites/${siteId}/insights/${insightId}/points`
        )
        return data
      },
      enabled: enabledInsightInfoQueries,
      select: getInsightPoints,
    },
    {
      queryKey: ['insightActivities', siteId, insightId],
      queryFn: async (): Promise<InsightWorkflowActivity[]> => {
        const { data = [] } = await api.get(
          `/sites/${siteId}/insights/${insightId}/activities`
        )

        return data
      },
      enabled: enabledInsightInfoQueries,
    },
  ])

  const [insightTicketsQuery, occurrencesQuery, pointsQuery, activitiesQuery] =
    insightInfoQueries

  const insight = insightQuery?.data
  const { data: modelInfo, status } = useGetInsightTwinModal({
    modelId: insight?.primaryModelId,
  })
  // simplified insight would help on speeding up the rendering of the time series
  // so it doesn't have to wait for the detailed insight to be fetched
  const simplifiedInsight = insight || cachedInsight

  // business requirement to update insight.lastStatus from "New" => "Open"
  // the first time when user viewing the insight node page
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/78997
  const mutation = useUpdateInsightsStatuses(
    {
      siteId: siteIdFromPath || insight?.siteId || '',
      insightIds: [insightId],
      newStatus: 'open',
    },
    {
      enabled: !!(siteIdFromPath || insight?.siteId),
      onSettled: () => {
        queryClient.invalidateQueries('insights')
        queryClient.invalidateQueries('all-insights')
        queryClient.invalidateQueries('insightInfo')
      },
    }
  )
  useEffect(() => {
    if (insight?.lastStatus === 'new') {
      mutation.mutate()
    }
  }, [insight])

  const reducedQueryStatus = reduceQueryStatuses([
    // only consider the loading status of insightQuery when it is initial fetching
    // so that a PUT request to only update insight status will not cause the page to show loading state
    insightQuery.isFetching && !insightQuery.isRefetching
      ? 'loading'
      : 'success',
    ...insightInfoQueries.map((query) => query.status),
  ])

  const occurrences = occurrencesQuery.data ?? []
  // occurrences that are either invalid or faulted are considered abnormal
  const abnormalOccurrences = occurrences.filter(
    (occurrence: Occurrence) => !occurrence.isValid || occurrence.isFaulted
  )
  const shadedDurations = _.uniqBy(
    abnormalOccurrences.map(({ started, ended, isValid }) => ({
      start: started,
      end: ended,
      color: isValid ? 'red' : 'orange',
    })),
    ({ start, end, color }) => `${start}-${end}-${color}`
  )

  const currentDateInISO = new Date().toISOString()
  const faultedOccurrence = occurrences.filter(({ isFaulted }) => isFaulted)
  const earliestFaultedOccurrence = _.minBy(faultedOccurrence, 'started')

  // to be used by diagnostics
  const faultyOccurrenceTimes = makeFaultyTimes({
    occurrences,
    language,
    timeZone: site?.timeZone,
  })

  // determine whether the period is a faulty occurrence time,
  // if so, use it as the diagnostic time period and add 10% padding
  // to the start and end of the time period
  const matchedTimes = faultyOccurrenceTimes.find(
    (time) => time.value === period
  )
  const [diagnosticStart, diagnosticEnd] = useDiagnosticTimePeriod({
    dateTime,
    period,
    faultyOccurrenceTimes,
    fallBackTimes: diagnosticQueryParams,
    padding: matchedTimes ? diagnosticTimePeriodPadding : 0,
  })

  const insightDiagnosticsQuery = useQuery(
    [
      'insightDiagnostics',
      simplifiedInsight?.id,
      diagnosticStart,
      diagnosticEnd,
      diagnosticQueryParams?.interval,
    ],
    async (): Promise<Array<DiagnosticOccurrence & PointTwinDto>> => {
      const { data = [] } = await api.get(
        `/insights/${simplifiedInsight?.id}/occurrences/diagnostics`,
        {
          params: {
            start: diagnosticStart,
            end: diagnosticEnd,
            interval: diagnosticQueryParams?.interval,
          },
        }
      )

      return data
    },
    {
      enabled:
        enableDiagnostics &&
        simplifiedInsight?.id != null &&
        faultyOccurrenceTimes.length > 0 &&
        !!diagnosticStart &&
        !!diagnosticEnd &&
        !!diagnosticQueryParams?.interval,
      select: (data) => {
        // build hierarchy by matching parentId and id so that we have a map like:
        // {
        //   grandParentId: [grandParentId],
        //   parentId: [grandParentId, parentId],
        //   childId: [grandParentId, parentId, childId],
        // }

        // For some of the cases, we are getting duplicate ids so adding a safe check to filter only unique ids
        const uniqueData = _.uniqBy(data, 'id')
        const hierarchyMap = uniqueData.reduce((acc, curr) => {
          const { id, parentId } = curr

          if (parentId !== insight?.id) {
            acc[id] = [parentId, ...acc[id]]
          }

          return acc
        }, Object.fromEntries(uniqueData.map((diagnosticOccurrence) => [diagnosticOccurrence.id, [diagnosticOccurrence.id]])))

        return uniqueData.map((diagnosticOccurrence) => ({
          ...diagnosticOccurrence,
          hierarchy: hierarchyMap[diagnosticOccurrence.id],
          pointTwinId: diagnosticOccurrence.twinId,
          trendId: diagnosticOccurrence.occurrenceLiveData.pointId,
          name:
            diagnosticOccurrence.ruleName ||
            diagnosticOccurrence.occurrenceLiveData.pointName ||
            diagnosticOccurrence.twinName,
          unit: diagnosticOccurrence.occurrenceLiveData.unit,
          type: PointType.DiagnosticPoint,
          entityId: diagnosticOccurrence.occurrenceLiveData.pointEntityId,
          defaultOn: false,
        }))
      },
    }
  )

  const diagnosticRows = useMemo<Array<DiagnosticOccurrence & PointTwinDto>>(
    () => insightDiagnosticsQuery.data ?? [],
    [insightDiagnosticsQuery.data]
  )

  const failedDiagnosticsCount =
    diagnosticRows?.length > 0
      ? diagnosticRows.filter((row) => !row.check).length
      : undefined

  const diagnosticPoints = diagnosticRows.map(
    (row) => `${row.siteId}_${row.id}`
  )
  const diagnosticPointsSelected = useMemo(
    () =>
      diagnosticPoints.filter((point) =>
        selectedPointsContext.pointIds.includes(point)
      ),
    [diagnosticPoints, selectedPointsContext.pointIds]
  )
  const diagnosticPointsNotSelected = useMemo(
    () => _.difference(diagnosticPoints, diagnosticPointsSelected),
    [diagnosticPoints, diagnosticPointsSelected]
  )
  const isAllDiagnosticsSelected = useMemo(
    () => diagnosticPointsNotSelected.length === 0,
    [diagnosticPointsNotSelected.length]
  )

  const lastSelectedFaultyOccurrenceTime = useRef<string[]>()
  const diagnosticTimes = useMemo(() => {
    // if user just turned on diagnostics where period is not defined,
    // then the diagnostic time period should be the first faulty occurrence time
    // since faultyOccurrenceTimes is already sorted with latest first
    if (
      diagnosticPointsSelected.length > 0 &&
      faultyOccurrenceTimes[0] &&
      !period
    ) {
      const result = [
        faultyOccurrenceTimes[0].start,
        faultyOccurrenceTimes[0].end,
      ]
      lastSelectedFaultyOccurrenceTime.current = result
      return result
    }

    // if user is viewing diagnostics where period is defined,
    // check if the period is a faulty occurrence time,
    // if so, use it as the diagnostic time period
    if (diagnosticPointsSelected.length > 0 && matchedTimes) {
      const result = [matchedTimes.start, matchedTimes.end]
      lastSelectedFaultyOccurrenceTime.current = result
      return result
    }

    // if there isn't any diagnostic points selected,
    // return undefined, otherwise, return the last selected faulty occurrence time
    return lastSelectedFaultyOccurrenceTime.current
  }, [
    diagnosticPointsSelected.length,
    faultyOccurrenceTimes,
    matchedTimes,
    period,
  ])

  const twinInfo: TimeSeriesTwinInfo = {
    twinName: insight?.equipmentName,
    twinId: insight?.twinId,
    isInsightPointsLoading: pointsQuery.isLoading,
    insightPoints: pointsQuery.data?.insightPoints ?? [],
    impactScorePoints: pointsQuery.data?.impactScorePoints ?? [],
    diagnosticPoints: insightDiagnosticsQuery.data ?? [],
    siteInsightId:
      insight?.id && siteId ? `${insight.id}-${siteId}` : undefined,
    diagnosticStart,
    diagnosticEnd,
    isAnyDiagnosticSelected: diagnosticPointsSelected.length > 0,
  }
  const [actionsViewControlOpen, setActionsViewControlOpen] = useState(false)

  // following states are used to control the modal (ticket, ticket creation or resolve confirmation)
  const [selectedItem, setSelectedItem] = useState<Item | undefined>(undefined)

  const handleTabChange = (newTab) => {
    // clicking away from diagnostics tab should clear the selected diagnostic points
    if (newTab !== InsightTab.Diagnostics) {
      for (const point of diagnosticPointsSelected) {
        selectedPointsContext.onSelectPoint(point, false)
      }
    }

    if (insightTab !== newTab) {
      setSearchParams({
        insightTab: newTab,
        period: null,
      })
    }
  }

  // normally we close the modal when user click on the close button;
  // but in this case, we want to keep the resolve confirmation modal open
  // after user close a ticket modal because all tickets need to be closed
  // before we can resolve an insight
  const handleModalClose = () => {
    if (
      selectedItem?.modalType === 'ticket' &&
      action === InsightActions.resolve
    ) {
      setSelectedItem({
        id: insightId,
        modalType: 'resolveInsightConfirmation',
      })
    } else {
      setSelectedItem(undefined)
      setSearchParams({
        ticketId: null,
        action: null,
      })
    }
  }

  const handleDiagnosticPeriodChange = (nextPeriod: string | null) => {
    setSearchParams({
      period: nextPeriod,
    })
  }

  // we set selected item in this effect block as opposed to trigger it
  // when user click on the action because we want to make sure e.g.
  // when user paste a url with action=resolve as query param,
  // the insight resolve confirmation modal will be open
  useEffect(() => {
    if (action === InsightActions.resolve && insight?.id) {
      setSelectedItem({
        id: insight.id,
        modalType: 'resolveInsightConfirmation',
      })
    }

    if (action === InsightActions.newTicket && insight?.id) {
      setSelectedItem({
        insightId: insight.id,
        modalType: InsightActions.newTicket,
      })
    }

    if (action === InsightActions.report && insight?.id) {
      setSelectedItem({
        insightId: insight.id,
        modalType: InsightActions.report,
      })
    }

    if (
      (insightTab === InsightTab.Actions ||
        insightTab === InsightTab.Activity ||
        insightTab === InsightTab.Summary) &&
      action === InsightActions.ticket &&
      ticketId != null
    ) {
      const selectedTicket =
        (insightTicketsQuery.data ?? [])?.find(
          (ticket) => ticket.id === ticketId
        ) ?? undefined
      setSelectedItem({ ...selectedTicket, modalType: 'ticket' })
    }
  }, [
    action,
    insight?.id,
    insightId,
    insightTab,
    insightTicketsQuery.data,
    ticketId,
  ])

  // Panel does not expose a prop to control collapsibility, we use state to control it
  const [collapsibilities, setCollapsibilities] = useState({
    left: true,
    right: true,
  })
  const onPanelCollapse = (isLeftPanel: boolean) => {
    const panelToAdjustCollapsibility = isLeftPanel ? 'right' : 'left'
    setCollapsibilities((prev) => ({
      ...prev,
      [panelToAdjustCollapsibility]: !prev[panelToAdjustCollapsibility],
    }))
  }

  // count activity that has a "InsightActivity" type and status is one of "new", "inProgress", "resolved", or "ignored" as per business requirement
  const activitiesCount = activitiesQuery.data?.filter((activity) => {
    if (activity.activityType === 'InsightActivity') {
      const insightStatus = _.lowerFirst(
        _.find(activity.activities, { key: ActivityKey.Status })?.value ?? ''
      )

      // The business requirement is to exclude :
      // <*> open insights, since account managers viewing it (insight is new, insight is open, insight is set back to new) might consider it be noise.
      // <*> deleted insights, since  deleted insights would not show they were deleted in activity tab.
      return ['new', 'inProgress', 'resolved', 'ignored'].includes(
        insightStatus
      )
    }

    return true
  })?.length

  // Getting faulty occurrence count to show in Occurrence tab header
  const faultyOccurrenceCount = occurrences.filter(
    ({ isFaulted, isValid }) => isFaulted && isValid
  ).length

  const tabs = makeInsightTabs({
    actionCount: insightTicketsQuery.data?.length,
    occurrenceCount: faultyOccurrenceCount,
    activitiesCount,
    enableDiagnostics,
    failedDiagnosticsCount,
  })

  const ruleName = insight?.ruleId
    ? insight?.ruleName
    : _.capitalize('ungrouped')

  const [sortBy, setSortByChange] = useState(SortBy.desc)

  const handleSortByChange = (option) => {
    setSortByChange(option)
  }

  const handleTicketLinkClick = (param: ParamsDictionary) => {
    setSearchParams(param)
  }

  const handleRowClick = (params: { id: string }) => {
    const { id: nextInsightId } = params
    if (isScopeSelectorEnabled) {
      history.push({
        pathname: scopeId
          ? routes.insights_scope__scopeId_insight__insightId(
              scopeId,
              nextInsightId
            )
          : routes.insights_insight__insightId(nextInsightId),
        search: new URLSearchParams({
          insightTab: InsightTab.Diagnostics,
        }).toString(),
      })
    } else if (!siteIdFromPath) {
      history.push({
        pathname: routes.insights_insightId(nextInsightId),
      })
    } else {
      history.push({
        pathname: routes.sites__siteId_insights__insightId(
          siteId,
          nextInsightId
        ),
        search: new URLSearchParams({
          insightTab: InsightTab.Diagnostics,
        }).toString(),
      })
    }
  }

  const baseRoute = isScopeSelectorEnabled
    ? scopeId
      ? routes.insights_scope__scopeId(scopeId)
      : routes.insights
    : siteIdFromPath
    ? routes.sites__siteId_insights(siteIdFromPath)
    : routes.insights

  const baseRuleRoute = isScopeSelectorEnabled
    ? scopeId
      ? routes.insights_scope__scopeId_rule__ruleId(
          scopeId,
          insight?.ruleId ?? ruleName
        )
      : routes.insights_rule__ruleId(insight?.ruleId ?? ruleName)
    : siteIdFromPath
    ? routes.sites__siteId_insight_rule__ruleId(
        siteIdFromPath,
        insight?.ruleId ?? ruleName
      )
    : routes.insights_rule__ruleId(insight?.ruleId ?? ruleName)

  const isReadyToResolve =
    featureFlags.hasFeatureToggle('readyToResolve') &&
    insight?.lastStatus === InsightActions.readyToResolve

  const insightRuleName = insight && ruleName ? ruleName : ''
  const assetName =
    simplifiedInsight?.equipmentName ?? simplifiedInsight?.asset?.name ?? ''

  return (
    <>
      <DocumentTitle
        scopes={[
          assetName,
          insightRuleName,
          t('headers.insights'),
          locationName,
        ]}
      />

      <InsightNodeContainer>
        <HeaderWithTabs
          css={{ borderBottom: 'none' }}
          titleRow={[
            <PageTitle key="pageTitle">
              {[
                {
                  text: t('headers.insights'),
                  to: `${baseRoute}${
                    groupByQueryParam ? `?groupBy=${groupByQueryParam}` : ''
                  }`,
                },
                // use insight here because the cached insight might not have the ruleId
                ...(insightRuleName
                  ? [
                      {
                        text: insightRuleName,
                        to: baseRuleRoute,
                      },
                    ]
                  : []),
                ...(assetName && simplifiedInsight
                  ? [
                      {
                        text: assetName,
                        suffix: (
                          <StyledFlex tw="ml-[16px]">
                            <InsightWorkflowStatusPill
                              lastStatus={simplifiedInsight.lastStatus}
                            />
                          </StyledFlex>
                        ),
                      },
                    ]
                  : []),
              ].map(({ text, to, suffix }) => (
                // only the last item has suffix
                <PageTitleItem key={`${text}-${to}-${suffix}`} suffix={suffix}>
                  {to ? <Link to={to}>{text}</Link> : text}
                </PageTitleItem>
              ))}
            </PageTitle>,
            ...(siteId && insight
              ? [
                  <ActionsViewControl
                    key="actionsViewControl"
                    selectedInsight={insight}
                    siteId={siteId}
                    lastStatus={insight?.lastStatus}
                    assetId={insight?.equipmentId ?? insight?.asset?.id}
                    floorId={insight?.floorId}
                    canDeleteInsight={false}
                    onCreateTicketClick={() => {
                      setSearchParams({
                        action: InsightActions.newTicket,
                      })
                    }}
                    onResolveClick={() => {
                      setSearchParams({
                        action: InsightActions.resolve,
                      })
                    }}
                    onReportClick={() =>
                      setSearchParams({
                        action: InsightActions.report,
                      })
                    }
                    opened={actionsViewControlOpen}
                    onToggleActionsView={setActionsViewControlOpen}
                  >
                    <Indicator
                      intent="primary"
                      hasBorder={isReadyToResolve}
                      color={isReadyToResolve ? 'violet' : 'transparent'}
                      position="top-end"
                    >
                      <StyledButton
                        className="insightActionIcon"
                        prefix={<Icon icon="more_vert" />}
                        onClick={() =>
                          setActionsViewControlOpen(!actionsViewControlOpen)
                        }
                        disabled={
                          reducedQueryStatus === 'loading' &&
                          !site?.features?.isTicketingDisabled
                        }
                      >
                        {t('plainText.actions')}
                      </StyledButton>
                    </Indicator>
                  </ActionsViewControl>,
                ]
              : []),
          ]}
        />
        {insightQuery.status === 'error' ? (
          insightQuery?.error?.response?.status === 404 ? (
            <NotFound
              message={titleCase({
                language,
                text: t('plainText.noInsightCanBeFound'),
              })}
            />
          ) : (
            <ErrorMessage />
          )
        ) : (
          <StyledPanelGroup resizable data-testid="insightNodePage">
            <Panel
              id="insightNodePageLeftPanel"
              collapsible={collapsibilities.left}
              onCollapse={() => onPanelCollapse(true)}
              tw="min-w-[354px]"
              tabs={
                <Tabs
                  defaultValue={tabs[0].value}
                  onTabChange={handleTabChange}
                  value={tabs.find((i) => i.tab === insightTab)?.value}
                  data-testid="insightNodeLeftPanelTabs"
                >
                  <Tabs.List>
                    {tabs.map(({ header, tab, value, suffix }) => (
                      <Tabs.Tab key={tab} value={value} suffix={suffix}>
                        {titleCase({
                          text: t(header),
                          language,
                        })}
                      </Tabs.Tab>
                    ))}
                  </Tabs.List>
                  <PanelContent tw="h-full">
                    {(activitiesQuery.status === 'loading' &&
                      insightTab === InsightTab.Activity) ||
                    (insightTicketsQuery.status === 'loading' &&
                      insightTab === InsightTab.Actions) ||
                    (occurrencesQuery.status === 'loading' &&
                      insightTab === InsightTab.Occurrences) ||
                    (insightDiagnosticsQuery.status === 'loading' &&
                      insightTab === InsightTab.Diagnostics) ||
                    simplifiedInsight == null ? (
                      <FullSizeLoader />
                    ) : (
                      [
                        {
                          isError: activitiesQuery.status === 'error',
                          node: (
                            <Activities
                              key="activity-tab"
                              activities={activitiesQuery.data}
                              onSortByChange={handleSortByChange}
                              sortBy={sortBy}
                              onTicketLinkClick={handleTicketLinkClick}
                              timeZone={site?.timeZone}
                            />
                          ),
                          tab: InsightTab.Activity,
                        },
                        {
                          isError: insightTicketsQuery.status === 'error',
                          node: (
                            <Actions
                              key="actions-tab"
                              tickets={insightTicketsQuery.data ?? []}
                              onTicketClick={(action) => {
                                setSearchParams({
                                  action,
                                })
                              }}
                            />
                          ),
                          tab: InsightTab.Actions,
                        },
                        {
                          isError: occurrencesQuery.status === 'error',
                          node: (
                            <Occurrences
                              key="occurrence-tab"
                              occurrences={occurrences}
                              onSortByChange={handleSortByChange}
                              sortBy={sortBy}
                              timeZone={site?.timeZone}
                            />
                          ),
                          tab: InsightTab.Occurrences,
                        },
                        {
                          node: simplifiedInsight && (
                            <Summary
                              key="summary-tab"
                              insight={{
                                ...simplifiedInsight,
                              }}
                              firstOccurredDate={
                                earliestFaultedOccurrence?.started ??
                                new Date(
                                  simplifiedInsight.occurredDate
                                ).toISOString()
                              }
                              modelInfo={modelInfo}
                              status={status}
                            />
                          ),
                          tab: InsightTab.Summary,
                        },
                        ...(enableDiagnostics
                          ? [
                              {
                                isError:
                                  insightDiagnosticsQuery.status === 'error',
                                node: insight && (
                                  <Diagnostics
                                    key="diagnostics-tab"
                                    insightDiagnosticsQuery={
                                      insightDiagnosticsQuery
                                    }
                                    insight={{
                                      ...insight,
                                      tickets: insightTicketsQuery.data ?? [],
                                    }}
                                    diagnosticPeriods={faultyOccurrenceTimes}
                                    onDiagnosticPeriodChange={
                                      handleDiagnosticPeriodChange
                                    }
                                    selectedPeriod={
                                      typeof period === 'string'
                                        ? period
                                        : `${faultyOccurrenceTimes[0]?.value}`
                                    }
                                    modelInfo={modelInfo}
                                    status={status}
                                    onRowClick={handleRowClick}
                                    rows={diagnosticRows}
                                    pointsSelected={diagnosticPointsSelected}
                                    pointsNotSelected={
                                      diagnosticPointsNotSelected
                                    }
                                    isAllPointsSelected={
                                      isAllDiagnosticsSelected
                                    }
                                  />
                                ),
                                tab: InsightTab.Diagnostics,
                              },
                            ]
                          : []),
                      ].map(({ node, tab, isError }) => {
                        if (
                          insightTab === tab ||
                          (insightTab == null && tab === InsightTab.Summary)
                        ) {
                          return isError ? <ErrorMessage /> : node
                        }
                      })
                    )}
                  </PanelContent>
                </Tabs>
              }
            />
            <Panel
              id="insightNodePageRightPanel"
              title={
                <div tw="flex gap-[8px]">
                  {t('headers.timeSeries')}
                  {diagnosticPointsSelected.length > 0 && (
                    <Badge
                      variant="subtle"
                      color="purple"
                      size="sm"
                      suffix={
                        <IconButton
                          kind="secondary"
                          background="transparent"
                          icon="close"
                          onClick={() => {
                            handleDiagnosticPeriodChange(null)
                            for (const point of diagnosticPointsSelected) {
                              selectedPointsContext.onSelectPoint(point, false)
                            }
                          }}
                        />
                      }
                    >
                      {titleCase({
                        text: t('plainText.monitoringDiagnostics'),
                        language,
                      })}
                    </Badge>
                  )}
                </div>
              }
              tw="min-w-[387px]"
              collapsible={collapsibilities.right}
              onCollapse={() => onPanelCollapse(false)}
            >
              {pointsQuery.status === 'loading' ||
              occurrencesQuery.status === 'loading' ||
              !simplifiedInsight ? (
                <FullSizeLoader />
              ) : // failure of other queries do not mean we can't show time series
              pointsQuery.status === 'error' ? (
                <Message tw="h-full" icon="error">
                  {t('plainText.errorOccurred')}
                </Message>
              ) : (
                pointsQuery.status === 'success' &&
                simplifiedInsight && (
                  <InsightWorkflowTimeSeries
                    insight={simplifiedInsight}
                    start={dateTime(currentDateInISO).addDays(-30).format()}
                    end={currentDateInISO}
                    shadedDurations={shadedDurations}
                    twinInfo={twinInfo}
                    isViewingDiagnostic={diagnosticPointsSelected.length > 0}
                    insightTab={
                      typeof insightTab === 'string' ? insightTab : undefined
                    }
                    period={typeof period === 'string' ? period : undefined}
                    onPeriodChange={handleDiagnosticPeriodChange}
                    diagnosticBoundaries={
                      diagnosticPointsSelected.length > 0 &&
                      faultyOccurrenceTimes[0]
                        ? diagnosticTimes
                        : undefined
                    }
                  />
                )
              )}
            </Panel>
          </StyledPanelGroup>
        )}
      </InsightNodeContainer>
      {/* check if valid ticked is selected, if so show the modal */}
      {insight &&
        ((ticketId != null && selectedItem?.id != null) ||
          (insight != null &&
            action != null &&
            selectedItem?.modalType != null)) && (
          <AssetDetailsModal
            siteId={insight.siteId}
            item={
              selectedItem?.modalType === 'ticket'
                ? {
                    ...selectedItem,
                    ...(insight
                      ? {
                          selectedInsight: {
                            id: insight.id,
                            name: insight.name,
                            ruleName: insight.ruleName,
                          },
                        }
                      : {}),
                  }
                : {
                    ...selectedItem,
                    insightId: insight?.id,
                    modalType: selectedItem?.modalType,
                  }
            }
            isUpdatedTicket={selectedItem?.modalType === 'ticket'}
            onClose={handleModalClose}
            navigationButtonProps={{
              items: [],
              selectedItem: undefined,
              setSelectedItem: _.noop,
            }}
            selectedInsightIds={insight ? [insight.id] : []}
            dataSegmentPropPage="insight node"
          />
        )}
    </>
  )
}

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))

const InsightNodeContainer = styled.div(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  display: 'flex',
  flexDirection: 'column',
  // min width for left panel is 354px, right panel is 387px;
  // along with 16px padding on both sides of the container
  // the total min width is 773px
  // https://dev.azure.com/willowdev/Unified/_workitems/edit/89717
  '& > *': {
    minWidth: '773px',
  },
}))

export default InsightNode

const makeInsightTabs = ({
  actionCount,
  occurrenceCount,
  activitiesCount,
  enableDiagnostics = false,
  failedDiagnosticsCount,
}: {
  actionCount?: number
  occurrenceCount?: number
  activitiesCount?: number
  enableDiagnostics?: boolean
  failedDiagnosticsCount?: number
}) => [
  {
    tab: InsightTab.Summary,
    header: 'labels.summary',
    value: 'summary',
  },
  {
    tab: InsightTab.Actions,
    header: 'plainText.actions',
    value: 'actions',
    suffix: typeof actionCount === 'number' && <Badge>{actionCount}</Badge>,
  },
  {
    tab: InsightTab.Activity,
    header: 'plainText.activity',
    value: 'activity',
    suffix: typeof activitiesCount === 'number' && (
      <Badge>{activitiesCount}</Badge>
    ),
  },
  {
    tab: InsightTab.Occurrences,
    header: 'plainText.occurrences',
    value: 'occurrences',
    suffix: typeof occurrenceCount === 'number' && (
      <Badge>{occurrenceCount}</Badge>
    ),
  },
  ...(enableDiagnostics
    ? [
        {
          tab: InsightTab.Diagnostics,
          header: 'plainText.diagnostics',
          value: 'diagnostics',
          suffix: typeof failedDiagnosticsCount === 'number' && (
            <Badge>{failedDiagnosticsCount}</Badge>
          ),
        },
      ]
    : []),
]

// click event should only be triggered on the Button, not the inner span
const StyledButton = styled(Button)({
  '& .mantine-Button-inner': {
    pointerEvents: 'none',
  },
})

const StyledFlex = styled.div({
  display: 'flex',
  alignItems: 'center',
})

/**
 * local hook to be used in InsightNode to get the model info for the insight twin
 */
const useGetInsightTwinModal = ({ modelId }: { modelId?: string }) => {
  const translation = useTranslation()
  const ontologyQuery = useOntologyInPlatform()
  const modelsOfInterestQuery = useModelsOfInterest()
  const insightModelQueryEnabled =
    modelId != null &&
    ontologyQuery.data != null &&
    modelsOfInterestQuery.data?.items != null
  const modelQuery = useQuery(
    ['insightTwinModel', modelId],
    () => {
      if (insightModelQueryEnabled) {
        const ontology = ontologyQuery.data
        const model = ontology.getModelById(modelId)
        return getModelInfo(
          model,
          ontology,
          modelsOfInterestQuery.data!.items,
          translation
        )
      }
    },
    {
      enabled: insightModelQueryEnabled,
    }
  )

  const reducedStatus = reduceQueryStatuses([
    ontologyQuery.status,
    modelsOfInterestQuery.status,
    modelQuery.status,
  ])

  return { ...modelQuery, status: reducedStatus }
}

/**
 * 10% padding for diagnostic time period
 */
const diagnosticTimePeriodPadding = 0.1
/**
 * local hook to calculate diagnostic time period and
 * add 10% padding to the start and end of the time period
 */
const useDiagnosticTimePeriod = ({
  dateTime,
  period,
  faultyOccurrenceTimes,
  fallBackTimes,
  padding = diagnosticTimePeriodPadding,
}: {
  dateTime: ReturnType<typeof useDateTime>
  period?: string | null | string[]
  faultyOccurrenceTimes: Array<{
    start: string
    end: string
    value: string
    label: string
  }>
  fallBackTimes?: {
    start: string
    end: string
  }
  padding?: number
}): [string, string] =>
  useMemo(() => {
    const [baseDiagnosticStart, baseDiagnosticEnd] =
      typeof period === 'string' ? period.split(' - ') : [null, null]
    const baseMillisecondsDiff =
      dateTime.now().differenceInMilliseconds(baseDiagnosticStart) -
      dateTime.now().differenceInMilliseconds(baseDiagnosticEnd)
    const millisecondsDiff = Number.isNaN(baseMillisecondsDiff)
      ? 0
      : Math.floor(padding * baseMillisecondsDiff)
    const diagnosticStart = baseDiagnosticStart
      ? dateTime(baseDiagnosticStart)
          .addMilliseconds(-millisecondsDiff)
          .format()
      : faultyOccurrenceTimes[0]?.start || fallBackTimes?.start
    const diagnosticEnd = baseDiagnosticEnd
      ? dateTime(baseDiagnosticEnd).addMilliseconds(millisecondsDiff).format()
      : faultyOccurrenceTimes[0]?.end || fallBackTimes?.end
    return [diagnosticStart, diagnosticEnd]
  }, [
    dateTime,
    fallBackTimes?.end,
    fallBackTimes?.start,
    faultyOccurrenceTimes,
    padding,
    period,
  ])

const getCachedInsight = (insightId: string, queryClient: QueryClient) => {
  const cachedQueries = queryClient
    .getQueriesData<{
      insights: { items: Insight[] }
    }>(['all-insights'])
    .filter(
      (query) => query && query?.[1] // filter out queries that do not have data, query[1] is the data
    )
  const normalizedCachedQueries: Array<[QueryKey, Insight[]]> =
    cachedQueries.map((query) => [
      query?.[0],
      query?.[1]?.insights?.items ?? [],
    ])
  // time series, classic viewer etc
  const cachedQueriesFromLegacyTables = queryClient
    .getQueriesData<Insight[]>(['asset-insights'])
    .filter((query) => query && query?.[1])

  for (const query of [
    ...normalizedCachedQueries,
    ...cachedQueriesFromLegacyTables,
  ]) {
    const insight = query?.[1]?.find?.((i) => i.id === insightId)

    if (insight) {
      return insight
    }
  }

  return undefined
}
