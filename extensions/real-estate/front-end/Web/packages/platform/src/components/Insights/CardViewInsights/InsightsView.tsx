/* eslint-disable complexity */
import {
  ALL_SITES,
  Message,
  useDateTime,
  reduceQueryStatuses,
  useScopeSelector,
  caseInsensitiveEquals,
} from '@willow/ui'
import { TFunction } from 'react-i18next'
import { useHistory } from 'react-router'
import { ReactNode, Reducer, useEffect, useReducer } from 'react'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { Site } from '@willow/common/site/site/types'
import selectInsights, {
  filterImpactScoreSummary,
  selectInsightTypes,
} from '@willow/common/utils/insightUtils'
import {
  Analytics,
  InsightCardGroups,
  InsightTableControls,
  InsightView,
  InsightTypesDto,
  SourceType,
  SourceName,
} from '@willow/common/insights/insights/types'
import _ from 'lodash'
import { FullSizeLoader, InsightMetric } from '@willow/common'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import useOntologyInPlatform from '../../../hooks/useOntologyInPlatform'
import useGetInsightTypes from '../../../hooks/Insight/useGetInsightTypes'
import useGetAllInsights from '../../../hooks/Insight/useGetAllInsights'
import useGetInsightFilters from '../../../hooks/Insight/useGetInsightFilters'
import {
  FilterOperator,
  statusMap,
} from '../../../services/Insight/InsightsService'
import InsightsContextProvider from './InsightsContextProvider'
import DisabledWarning from '../../DisabledWarning/DisabledWarning'
import insightsReducer, {
  InsightsAction,
  InsightsActionType,
  makeDefaultInsightState,
} from './insightsReducer'
import { InsightsState } from './InsightsContext'

export default function InsightsView({
  t,
  impactView: defaultImpactView,
  insightFilterSettings,
  dateTime,
  sites,
  siteId,
  assetId,
  children,
  lastInsightStatusCountDate,
  onInsightCountDateChange,
  onInsightIdChange,
  language,
  analytics,
  canWillowUserDeleteInsight = false,
  history,
  ontologyQuery,
  modelsOfInterestQuery,
  defaultRuleId,
  isInsightTypeNode = false,
  isUngrouped = false,
  isWalmartAlert = false,
  scopeId,
}: {
  t: TFunction
  siteId: string
  sites: Site[]
  children: ReactNode
  impactView?: string
  insightFilterSettings?: []
  dateTime: ReturnType<typeof useDateTime>
  assetId?: string
  lastInsightStatusCountDate?: string
  onInsightCountDateChange?: (currentDate: string) => void
  onInsightIdChange?: (insightId?: string) => void
  language: string
  canWillowUserDeleteInsight?: boolean
  analytics?: Analytics
  history: ReturnType<typeof useHistory>
  ontologyQuery: ReturnType<typeof useOntologyInPlatform>
  modelsOfInterestQuery: ReturnType<typeof useModelsOfInterest>
  defaultRuleId?: string
  isInsightTypeNode?: boolean
  isUngrouped?: boolean
  isWalmartAlert?: boolean
  scopeId?: string
}) {
  const {
    isScopeSelectorEnabled,
    location,
    descendantSiteIds,
    isScopeUsedAsBuilding,
    twinQuery,
  } = useScopeSelector()
  // for backward compatibility, siteId and site info are still needed for
  // bulk actions, so we find the site info based on the location which represents
  // the current scope
  const isBuildingScope = isScopeUsedAsBuilding(location)
  const nextSite = isScopeSelectorEnabled
    ? sites.find(
        (site) => site.id === location?.twin?.siteId && isBuildingScope
      )
    : sites.find((site) => site.id === siteId)
  const selectedSiteId = nextSite?.id ?? ALL_SITES

  const isDisabled = nextSite
    ? nextSite.features?.isInsightsDisabled
    : sites.every((s) => s.features.isInsightsDisabled)
  const isInsightDisabledForScope =
    isScopeSelectorEnabled &&
    (isBuildingScope
      ? sites.find((site) => site.id === location?.twin?.siteId)?.features
          ?.isInsightsDisabled
      : descendantSiteIds?.every(
          (descendantSiteId) =>
            sites.find((site) => site.id === descendantSiteId)?.features
              ?.isInsightsDisabled
        ))

  /**
   * Retrieve user's filter option preferences from user.options at site level
   */
  const defaultInsightFilterSettings = insightFilterSettings?.[selectedSiteId]

  const [queryParams, setSearchParams] = useMultipleSearchParams([
    'search',
    'groupBy',
    'pageSize',
    'page',
    { name: 'impactView', type: 'string' },
    { name: 'excludedRollups', type: 'array' },
    { name: 'status', type: 'array' },
    { name: 'selectedActivity', type: 'array' },
    { name: 'selectedPrimaryModelIds', type: 'array' },
    { name: 'selectedCategories', type: 'array' },
    { name: 'selectedSourceNames', type: 'array' },
    { name: 'selectedStatuses', type: 'array' },
    { name: 'lastOccurredDate', type: 'string' },
    { name: 'priorities', type: 'array' },
    { name: 'sources', type: 'array' },
    { name: 'types', type: 'array' },
    'days',
    'view',
    'insightId',
    'ruleId',
    'sourceType',
    'updatedDate',
  ])
  const {
    excludedRollups = [],
    pageSize = '10',
    page = '1',
    impactView = defaultImpactView ?? InsightMetric.cost,
    groupBy = InsightCardGroups.INSIGHT_TYPE,
    search = '',
    status = defaultInsightFilterSettings?.selectedStatuses,
    selectedActivity = '',
    selectedPrimaryModelIds = [],
    priorities = [],
    selectedCategories = [],
    selectedSourceNames = [],
    selectedStatuses = [],
    lastOccurredDate,
    view = InsightView.card,
    insightId,
    ruleId = defaultRuleId,
    sourceType,
    updatedDate,
  } = queryParams

  const rollupControls = [
    {
      text: _.startCase(
        t('interpolation.estimatedAvoidable', {
          expense: t('plainText.impact'),
        })
      ),
      control: InsightTableControls.showEstimatedAvoidable,
    },
    {
      text: _.startCase(t('plainText.estimatedSavings')),
      control: InsightTableControls.showEstimatedSavings,
    },
  ]

  const onQueryParamsChange = (params) => {
    setSearchParams({
      ...params,
      sourceType: params?.sourceType,
      updatedDate: params?.updatedDate,
    })
  }

  const [insightState, dispatch] = useReducer<
    Reducer<InsightsState, InsightsAction>
  >(insightsReducer, {
    ...makeDefaultInsightState({
      rollupControls,
      status: status.length > 0 ? status : statusMap.default,
    }),
  })

  const statusDependency =
    status.length > 0 ? status.join(',') : statusMap.default.join(',')

  const detailedStatusDependency =
    Array.isArray(selectedStatuses) && selectedStatuses.length > 0
      ? selectedStatuses.join(',')
      : undefined

  const formattedDate = lastOccurredDate
    ? dateTime.now().addDays(-lastOccurredDate).format('dateLocal')
    : ''

  const categoryDependency =
    Array.isArray(selectedCategories) && selectedCategories.length > 0
      ? selectedCategories.join(',')
      : undefined

  const primaryModelDependency =
    Array.isArray(selectedPrimaryModelIds) && selectedPrimaryModelIds.length > 0
      ? selectedPrimaryModelIds.join(',')
      : undefined

  const ticketsDependency =
    Array.isArray(selectedActivity) && selectedActivity.length > 0
      ? selectedActivity.join(',')
      : undefined

  const prioritiesDependency =
    Array.isArray(priorities) && priorities.length > 0
      ? priorities.join(',')
      : undefined

  const sourcesDependency =
    Array.isArray(selectedSourceNames) && selectedSourceNames.length > 0
      ? selectedSourceNames.join(',')
      : undefined

  const statusFilterSpec = (insightState.filterSpecifications ?? []).find(
    (spec) => spec.field === 'status'
  )
  const insightFiltersQuery = useGetInsightFilters(
    {
      ...(siteId ? { siteIds: [siteId] } : {}),
      scopeId,
      statusList: statusFilterSpec?.value,
    },
    {
      enabled:
        !!statusFilterSpec?.value?.length &&
        !isDisabled &&
        !isInsightDisabledForScope,
      onSuccess: ({ filters }) =>
        dispatch({
          type: InsightsActionType.onLoadedInsightFilters,
          filters,
        }),
    }
  )

  const getSourceFilterSpecs = () => {
    const sourceIds = [] as string[]
    const sourceNames = [] as string[]
    let walmartId

    const sources =
      insightFiltersQuery?.data?.filters?.sourceNames.map((item) =>
        JSON.parse(item)
      ) ?? []

    sources.forEach((item) => {
      if (item.sourceName === SourceName.walmart) {
        walmartId = item.sourceId
      }
      return item
    })

    if (selectedSourceNames instanceof Array) {
      selectedSourceNames?.forEach((item) => {
        const [id, name] = item.split('/')
        sourceIds.push(id)
        sourceNames.push(name || id)
        return id
      })
    }

    // Only Inspection is selected as Source.
    if (
      sourceIds.length === 1 &&
      sourceIds.some((element) =>
        caseInsensitiveEquals(element, SourceType.inspection)
      )
    ) {
      return [
        {
          specName: 'sourceType',
          specOperator: FilterOperator.equalsShort,
          specValue: SourceType.inspection,
        },
      ]
    }

    // Only Willow Activate is Selected.
    if (
      sourceNames.length === 1 &&
      sourceNames.includes(SourceName.willowActivate)
    ) {
      return [
        {
          specName: 'sourceType',
          specOperator: FilterOperator.equalsShort,
          specValue: SourceType.app,
        },
        {
          specName: 'sourceId',
          specOperator: FilterOperator.notEquals,
          specValue: walmartId,
        },
      ]
    }

    // Only Walmart is Selected.
    if (sourceNames.length === 1 && sourceNames.includes(SourceName.walmart)) {
      return [
        {
          specName: 'sourceId',
          specOperator: FilterOperator.equalsShort,
          specValue: walmartId,
        },
      ]
    }

    // if Inspection and Willow Activate are selected
    if (
      [SourceName.inspection, SourceName.willowActivate].every((element) =>
        sourceNames.includes(element)
      )
    ) {
      return [
        {
          specName: 'sourceId',
          specOperator: FilterOperator.notEquals,
          specValue: walmartId,
        },
      ]
    }

    // if Walmart and Inspection are selected, send Mapped
    // TODO: This is a temporary solution, API needs to be fixed to handle OR.
    if (
      [SourceName.inspection, SourceName.walmart].every((element) =>
        sourceNames.includes(element)
      )
    ) {
      return [
        {
          specName: 'sourceId',
          specOperator: FilterOperator.equalsShort,
          specValue: walmartId,
        },
      ]
    }

    // If all are sources are selected or none
    // TODO: This is a temporary solution, API needs to be fixed to handle OR.
    return []
  }

  useEffect(() => {
    dispatch({
      type: InsightsActionType.updateFilterSpecifications,
      filterSpecUpdates: [
        {
          specName: 'LastOccurredDate',
          specOperator: FilterOperator.greaterThanOrEqual,
          specValue: formattedDate,
        },
        {
          specName: 'status',
          specOperator: FilterOperator.containedIn,
          specValue: status.length > 0 ? status : statusMap.default,
        },
        {
          specName: 'type',
          specOperator: FilterOperator.containedIn,
          specValue: selectedCategories,
        },
        {
          specName: 'primarymodelId',
          specOperator: FilterOperator.containedIn,
          specValue: selectedPrimaryModelIds,
        },
        // when scope id is defined, it should take precedence over site id
        // when querying for insights;
        scopeId != null
          ? {
              specName: 'scopeId',
              specOperator: FilterOperator.equalsLiteral,
              specValue: scopeId,
            }
          : {
              specName: 'siteId',
              specOperator: FilterOperator.equalsLiteral,
              specValue: siteId,
            },
        {
          specName: 'ruleName',
          specOperator: FilterOperator.like,
          specValue: search,
        },
        {
          specName: 'detailedStatus',
          specOperator: FilterOperator.containedIn,
          specValue: selectedStatuses,
        },
        {
          specName: 'Activity',
          specOperator: FilterOperator.containedIn,
          specValue: selectedActivity,
        },
        {
          specName: 'equipmentId',
          specOperator: FilterOperator.equalsLiteral,
          specValue: assetId,
        },
        {
          specName: 'ruleId',
          specOperator: isUngrouped
            ? FilterOperator.isNull
            : FilterOperator.equalsLiteral,
          specValue: isUngrouped ? '' : ruleId,
        },
        {
          specName: 'priority',
          specOperator: FilterOperator.containedIn,
          specValue: priorities,
        },
        {
          specName: 'updatedDate',
          specOperator: FilterOperator.greaterThan,
          specValue: updatedDate,
        },
        ...getSourceFilterSpecs(),
      ],
    })
  }, [
    formattedDate,
    ticketsDependency,
    statusDependency,
    detailedStatusDependency,
    categoryDependency,
    sourcesDependency,
    primaryModelDependency,
    siteId,
    assetId,
    isUngrouped,
    isWalmartAlert,
    ruleId,
    search,
    scopeId,
    prioritiesDependency,
    updatedDate,
    sourceType,
  ])

  const isInsightType =
    isInsightTypeNode || groupBy === InsightCardGroups.INSIGHT_TYPE

  const isAllInsights =
    isInsightTypeNode || groupBy === InsightCardGroups.ALL_INSIGHTS

  const isFilterSpecification = insightState.filterSpecifications.length > 0

  const insightTypesQuery = useGetInsightTypes(
    {
      filterSpecifications: insightState.filterSpecifications.map((spec) => ({
        ...spec,
        value:
          spec.field === 'status' && selectedStatuses.length
            ? selectedStatuses
            : spec.value,
      })),
    },
    {
      enabled: isInsightType && isFilterSpecification,
      select: (data) =>
        selectInsightTypes(
          data.cards,
          data.filters,
          filterImpactScoreSummary(data.cards),
          dateTime,
          t
        ),
      onSuccess: (data: InsightTypesDto) =>
        dispatch({
          type: InsightsActionType.onLoadedInsightTypes,
          insightTypesGroupedByDate: data?.insightTypesGroupedByDate,
          impactScoreSummary: data?.impactScoreSummary,
          cards: data?.cards,
        }),
    }
  )

  const insightsQuery = useGetAllInsights(
    {
      sortSpecifications: insightState.sortSpecifications,
      filterSpecifications: insightState.filterSpecifications,
      page: Number(page),
      pageSize: Number(pageSize),
    },
    {
      enabled: isAllInsights && isFilterSpecification,
      select: (data) => ({
        ...data,
        insights: {
          ...data.insights,
          items: selectInsights(data.insights.items ?? [], sites),
        },
      }),
      onSuccess: ({ insights: { items = [] }, impactScoreSummary = [] }) =>
        dispatch({
          type: InsightsActionType.onLoadedInsights,
          insights: items,
          impactScoreSummary,
        }),
    }
  )

  const notIdleStatuses = [
    insightsQuery.status,
    insightTypesQuery.status,
  ].filter((s) => s !== 'idle')
  const reducedQueryStatus = reduceQueryStatuses(
    notIdleStatuses.length > 0 ? notIdleStatuses : ['loading']
  )

  const isLoading = reducedQueryStatus === 'loading'
  const isError = reducedQueryStatus === 'error'
  const insightsData = isAllInsights
    ? insightsQuery.data?.insights?.items ?? []
    : []

  const totalInsights = isAllInsights ? insightsQuery.data?.insights?.total : 0

  return twinQuery.isLoading ? (
    <FullSizeLoader />
  ) : isDisabled || isInsightDisabledForScope ? (
    <DisabledWarning title={t('plainText.insightsNotEnabled')} />
  ) : isError ? (
    <Message tw="h-full" icon="error">
      {t('plainText.errorOccurred')}
    </Message>
  ) : (
    <InsightsContextProvider
      {...insightState}
      isUngrouped={isUngrouped}
      isWalmartAlert={isWalmartAlert}
      insights={insightsData}
      totalInsights={totalInsights}
      siteId={selectedSiteId}
      sites={sites}
      assetId={assetId}
      dispatch={dispatch}
      isLoading={isLoading}
      canWillowUserDeleteInsight={canWillowUserDeleteInsight}
      insightId={insightId as string}
      groupBy={groupBy as InsightCardGroups}
      impactView={impactView as string}
      excludedRollups={excludedRollups}
      onInsightIdChange={onInsightIdChange}
      onQueryParamsChange={onQueryParamsChange}
      view={view as InsightView}
      insightTypesGroupedByDate={
        insightTypesQuery.data?.insightTypesGroupedByDate ?? []
      }
      cards={insightTypesQuery.data?.cards ?? []}
      cardSummaryFilters={insightFiltersQuery.data?.filters}
      impactScoreSummary={
        groupBy === InsightCardGroups.INSIGHT_TYPE
          ? insightTypesQuery.data?.impactScoreSummary ?? []
          : insightsQuery.data?.impactScoreSummary ?? []
      }
      page={Number(page)}
      pageSize={Number(pageSize)}
      t={t}
      language={language}
      analytics={analytics}
      history={history}
      ontologyQuery={ontologyQuery}
      modelsOfInterestQuery={modelsOfInterestQuery}
      ruleId={ruleId as string}
      isInsightTypeNode={isInsightTypeNode}
      queryParams={queryParams}
      filterQueryStatus={insightFiltersQuery.status}
      onInsightCountDateChange={onInsightCountDateChange}
      lastInsightStatusCountDate={lastInsightStatusCountDate}
    >
      {children}
    </InsightsContextProvider>
  )
}
