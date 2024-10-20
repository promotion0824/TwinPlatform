import { InsightMetric } from '@willow/common'
import { ParamsDict } from '@willow/common/hooks/useMultipleSearchParams'
import {
  Analytics,
  CardSummaryFilters,
  CardSummaryRule,
  ImpactScoreSummary,
  Insight,
  InsightCardGroups,
  InsightTableControls,
  InsightTypesGroupedByDate,
  InsightView,
} from '@willow/common/insights/insights/types'
import { Site } from '@willow/common/site/site/types'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { ALL_SITES, useScopeSelector } from '@willow/ui'
import { GridSortModel } from '@willowinc/ui'
import { History } from 'history'
import { TFunction } from 'i18next'
import { Dispatch, ReactNode, useCallback, useState } from 'react'
import { QueryStatus } from 'react-query'
import useOntologyInPlatform from '../../../hooks/useOntologyInPlatform'
import routes from '../../../routes'

import { FilterSpecification, InsightsContext } from './InsightsContext'
import { InsightsAction, InsightsActionType } from './insightsReducer'

export default function InsightsContextProvider({
  siteId,
  assetId,
  isUngrouped = false,
  isWalmartAlert = false,
  rollupControls,
  nextIncludedRollups,
  groupBy,
  selectedInsight,
  sites,
  sortSpecifications,
  filterSpecifications,
  dispatch,
  insights = [],
  page = 1,
  pageSize = 10,
  totalInsights = 0,
  isLoading = true,
  children,
  impactView = InsightMetric.cost,
  selectedInsightIds = [],
  excludedRollups = [],
  onInsightIdChange,
  onQueryParamsChange,
  view,
  insightTypesGroupedByDate,
  impactScoreSummary,
  cards,
  cardSummaryFilters,
  t,
  language,
  canWillowUserDeleteInsight = false,
  insightId,
  analytics,
  history,
  ontologyQuery,
  modelsOfInterestQuery,
  ruleId,
  isInsightTypeNode = false,
  queryParams = {},
  filterQueryStatus = 'idle',
  lastInsightStatusCountDate,
  onInsightCountDateChange,
}: {
  isUngrouped?: boolean
  isWalmartAlert?: boolean
  rollupControls: Array<{ text: string; control: InsightTableControls }>
  nextIncludedRollups?: InsightTableControls[]
  siteId: string
  sites: Site[]
  selectedInsight?: Insight
  groupBy?: InsightCardGroups
  sortSpecifications: GridSortModel
  filterSpecifications: FilterSpecification[]
  dispatch: Dispatch<InsightsAction>
  assetId?: string
  page?: number
  pageSize?: number
  isLoading: boolean
  insights?: Insight[]
  totalInsights?: number
  cardSummaryFilters?: CardSummaryFilters
  children: ReactNode
  impactView: string
  excludedRollups: string | string[]
  selectedInsightIds?: string[]
  onInsightIdChange?: (insightId?: string) => void
  onQueryParamsChange?: (params: ParamsDict) => void
  view?: InsightView
  insightTypesGroupedByDate: InsightTypesGroupedByDate
  impactScoreSummary: ImpactScoreSummary[]
  cards: CardSummaryRule[]
  t: TFunction
  language: string
  canWillowUserDeleteInsight?: boolean
  analytics?: Analytics
  history: History
  insightId: string
  ontologyQuery: ReturnType<typeof useOntologyInPlatform>
  modelsOfInterestQuery: ReturnType<typeof useModelsOfInterest>
  ruleId?: string
  isInsightTypeNode?: boolean
  queryParams?: ParamsDict
  filterQueryStatus?: QueryStatus
  lastInsightStatusCountDate?: string
  onInsightCountDateChange?: (currentDate: string) => void
}) {
  const { isScopeSelectorEnabled, location } = useScopeSelector()
  const scopeId = location?.twin?.id
  const [showTotalImpact, setShowTotalImpact] = useState(false)

  const eventBody = {
    impactView,
    // On loading insights page, default value of includedRollups (if empty) :  showTopAsset, showImpactPerYear
    includedRollups: nextIncludedRollups,
    siteId,
    groupBy: groupBy == null ? InsightCardGroups.INSIGHT_TYPE : groupBy,
    ...queryParams,
  }

  // These callbacks are just placeholder for now
  // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/88973
  const onSelectInsight = useCallback((selectedInsight: Insight) => {
    // onInsightIdChange sets insightId query param in url when user clicks on
    // an action icon belongs to an insight row; relevant to Insight Card View
    // since it doesn't display insight modal so we need to set insightId query param
    // for other modals to work properly
    if (onInsightIdChange) {
      onInsightIdChange(selectedInsight?.id)
    } else {
      // onSelectInsight sets selectedInsight in InsightsContext when user clicks on
      // an action icon belongs to an insight row
      dispatch({
        type: InsightsActionType.selectInsight,
        selectedInsight,
      })
    }
  }, [])

  const onSelectInsightIds = useCallback(
    (selectedInsightIds: string[]) =>
      dispatch({
        type: InsightsActionType.selectInsightIds,
        selectedInsightIds,
      }),
    []
  )

  const onSortModelChange = useCallback(
    (sortModel: GridSortModel) =>
      dispatch({
        type: InsightsActionType.updateSortSpecifications,
        sortSpecifications: sortModel,
      }),
    []
  )

  const onResetInsight = useCallback(
    () =>
      dispatch({
        type: InsightsActionType.resetInsight,
      }),
    []
  )

  const onResetInsightIds = useCallback(
    () =>
      dispatch({
        type: InsightsActionType.resetInsightIds,
      }),
    []
  )

  const onUpdateIncludedRollups = useCallback(
    (nextIncludedRollups: InsightTableControls[]) =>
      dispatch({
        type: InsightsActionType.onUpdateIncludedRollups,
        nextIncludedRollups,
      }),
    []
  )

  const onChangeFilter = useCallback(
    (filterName, filterValue) =>
      dispatch({
        type: InsightsActionType.insightCardSummaryFilterChange,
        filterName,
        filterValue,
      }),
    []
  )

  const handleInsightTypeClick = (card: CardSummaryRule) => {
    // reset search and pagination when navigating to new page as the page
    // number and search phrase is not relevant to the new group of insights
    const searchParams = new URLSearchParams(history.location.search)
    searchParams.delete('search')
    searchParams.delete('page')

    const insightRuleId = card.ruleId || 'ungrouped'
    const isAllLocations = isScopeSelectorEnabled
      ? !scopeId
      : siteId === ALL_SITES || siteId == null
    const encodedRuleId = encodeURIComponent(insightRuleId)
    const route = isScopeSelectorEnabled
      ? isAllLocations
        ? routes.insights_rule__ruleId(encodedRuleId)
        : routes.insights_scope__scopeId_rule__ruleId(scopeId, encodedRuleId)
      : isAllLocations
      ? routes.insights_rule__ruleId(encodedRuleId)
      : routes.sites__siteId_insight_rule__ruleId(siteId, encodedRuleId)

    history.push({
      pathname: route,
      search: searchParams.toString(),
    })
  }

  const handleInsightClick = (insight: Insight) => {
    const isAllSiteSelected = siteId === ALL_SITES || siteId == null
    const route = isScopeSelectorEnabled
      ? scopeId
        ? routes.insights_scope__scopeId_insight__insightId(scopeId, insight.id)
        : routes.insights_insight__insightId(insight.id)
      : isAllSiteSelected
      ? routes.insights_insightId(insight.id)
      : routes.sites__siteId_insights__insightId(insight.siteId, insight.id)

    history.push({
      pathname: route,
    })
  }

  const onResetFilters = useCallback(
    (nextParams) => {
      onQueryParamsChange?.({
        ...queryParams,
        ...Object.fromEntries(
          Object.entries(nextParams).map(([key]) => [key, undefined])
        ),
      })
    },
    [onQueryParamsChange, queryParams]
  )

  const viewByOptionsMap = {
    [InsightCardGroups.ALL_INSIGHTS]: {
      view: InsightView.list,
      text: t('plainText.allInsights'),
    },
    [InsightCardGroups.INSIGHT_TYPE]: {
      view: InsightView.card,
      text: t('plainText.skills'),
    },
  }

  const hasAppliedFilter = [
    'selectedStatuses',
    'lastOccurredDate',
    'search',
    'priorities',
    'selectedCategories',
    'selectedPrimaryModelIds',
  ]
    .map((filterVal) => (queryParams?.[filterVal]?.length ?? 0) > 0)
    .includes(true)

  const context = {
    insights,
    totalInsights,
    isUngrouped,
    isWalmartAlert,
    filterSpecifications,
    sortSpecifications,
    sites,
    siteId,
    assetId,
    page,
    pageSize,
    isLoading,
    filterQueryStatus,
    selectedInsight,
    groupBy,
    impactView,
    excludedRollups,
    eventBody,
    rollupControls,
    showTotalImpact,
    setShowTotalImpact,
    onUpdateIncludedRollups,
    onChangeFilter,
    handleInsightTypeClick,
    handleInsightClick,
    onResetFilters,
    onQueryParamsChange,
    onSelectInsight,
    selectedInsightIds,
    onSelectInsightIds,
    onSortModelChange,
    onResetInsight,
    onResetInsightIds,
    view,
    insightTypesGroupedByDate,
    impactScoreSummary,
    cards,
    cardSummaryFilters,
    t,
    language,
    canWillowUserDeleteInsight,
    analytics,
    history,
    ontologyQuery,
    modelsOfInterestQuery,
    insightId,
    ruleId,
    isInsightTypeNode,
    viewByOptionsMap,
    totalCount:
      isInsightTypeNode || groupBy === InsightCardGroups.ALL_INSIGHTS
        ? totalInsights
        : cards.length,
    queryParams,
    lastInsightStatusCountDate,
    onInsightCountDateChange,
    hasAppliedFilter,
  }

  return (
    <InsightsContext.Provider value={context}>
      {children}
    </InsightsContext.Provider>
  )
}
