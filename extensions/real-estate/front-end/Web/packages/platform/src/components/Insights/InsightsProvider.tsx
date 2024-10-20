/* eslint-disable complexity */
import { useState, useMemo, useEffect, ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import _ from 'lodash'
import { ALL_SITES, InsightGroups, useDateTime, useUser } from '@willow/ui'
import {
  Filter,
  Insight,
  InsightStatus,
  InsightTableControls,
  InsightWorkflowStatus,
  SourceType,
} from '@willow/common/insights/insights/types'
import { InsightMetric } from '@willow/common/insights/costImpacts/types'
import { Site } from '@willow/common/site/site/types'
import { priorities, formatDateTime } from '@willow/common'
import selectInsights from '@willow/common/utils/insightUtils'
import { useSites } from '../../providers/sites/SitesContext'
import { InsightsContext } from './InsightsContext'
import {
  FilterOperator,
  InsightsResponse,
  statusMap,
} from '../../services/Insight/InsightsService'
import useGetInsights from '../../hooks/Insight/useGetInsights'
import useGetAssetInsights from '../../hooks/Insight/useGetAssetInsights'

// Setting default occurred date to last 30 days to improve Insight fetching performance
// Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/88112
const DEFAULT_OCCURRED_DATE = 30

export default function InsightsProvider({
  siteId,
  selectedInsightId,
  showSite,
  assetId,
  sourceType,
  tab,
  onTabChange,
  groupBy,
  expandedGroupId,
  onGroupByOptionClick,
  onModelNameOptionClick,
  primaryModelId,
  onExpandGroup,
  tableControls,
  onTableControlChange,
  dataSegmentPropPage = 'Insights Page',
  paginationEnabled = false,
  pageSize = 10,
  initialPageIndex = 0,
  onPageSizeChange,
  children,
  getTranslatedModelName,
  insightTab,
  onInsightTabChange,
  days,
  onDaysChange,
  twinId,
  onTwinIdChange,
}: {
  siteId: string
  selectedInsightId?: string
  showSite: boolean
  assetId: string
  sourceType: SourceType
  tab: InsightStatus | InsightWorkflowStatus
  onTabChange: (tab: InsightStatus | null) => void
  groupBy: InsightGroups
  onModelNameOptionClick?: (obj: { primaryModelId?: string }) => void
  primaryModelId?: string
  expandedGroupId?: string
  onGroupByOptionClick?: (obj: {
    groupBy?: string
    expandedGroupId?: string
  }) => void
  onExpandGroup?: (obj: { expandedGroupId?: string }) => void
  tableControls?: { [key: string]: string | string[] }
  onTableControlChange?: (obj: { [key: string]: string | string[] }) => void
  dataSegmentPropPage: string
  paginationEnabled?: boolean
  pageSize?: number
  onPageSizeChange?: (pageSize: number) => void
  initialPageIndex: number
  children: ReactNode
  getTranslatedModelName?: (modelId: string) => void
  insightTab: string
  onInsightTabChange: (tab?: string) => void
  days?: number
  onDaysChange?: (nextDays?: { days?: number }) => void
  twinId?: string
  onTwinIdChange?: (twin?: { twinId?: number }) => void
}) {
  const user = useUser()
  const sites = useSites()
  const dateTime = useDateTime()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const nextSite = sites.find((site) => site.id === siteId)
  const nextSiteId = nextSite?.id
  const selectedSiteId: string = nextSiteId ?? ALL_SITES

  const groupByName =
    groupBy === InsightGroups.ASSET_TYPE ? 'modelName' : 'ruleName'

  const groupById =
    groupBy === InsightGroups.ASSET_TYPE ? 'primaryModelId' : 'ruleId'

  const isResolved = _.isEqual(statusMap.resolved, statusMap[tab])
  const isIgnored = _.isEqual(statusMap.ignored, statusMap[tab])
  /**
   * Retrieve user's filter option preferences from user.options at site level
   */
  const defaultInsightFilterSettings =
    user.options?.insightFilterSettings?.[selectedSiteId]
  /**
   *  Checking if the user has previously selected a date in which case
   *  defaultInsightFilterSettings will have a "days" property
   *  If not, set the default occurred date to 30 days
   *  Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/88112
   */
  const defaultDays =
    days ??
    (Object.keys(defaultInsightFilterSettings ?? {}).includes('days')
      ? defaultInsightFilterSettings?.days
      : DEFAULT_OCCURRED_DATE)

  const [filters, setFilters] = useState<Filter>({
    sites,
    dateTime,
    priorities,
    statuses: [],
    sources: [],
    types: [],
    modelIds: [],
    siteId: defaultInsightFilterSettings?.siteId || nextSiteId,
    search: twinId || defaultInsightFilterSettings?.search || '',
    selectedStatuses: defaultInsightFilterSettings?.selectedStatuses || [],
    selectedPriorities: defaultInsightFilterSettings?.selectedPriorities || [],
    selectedSources: defaultInsightFilterSettings?.selectedSources || [],
    selectedTypes: defaultInsightFilterSettings?.selectedTypes || [],
    selectedModelId:
      defaultInsightFilterSettings?.selectedModelId || primaryModelId,
    days: defaultDays,
  })

  const insightsQuery = useGetInsights(
    {
      filterSpecifications: [
        ...(siteId != null
          ? [
              {
                field: 'siteId',
                operator: FilterOperator.equalsLiteral,
                value: siteId,
              },
            ]
          : []),
        ...(twinId != null
          ? [
              {
                field: 'twinId',
                operator: FilterOperator.equalsLiteral,
                value: twinId,
              },
            ]
          : []),
        // Calculating the date in yyyy-mm-dd format based on last occurred date selected by user
        // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/88112
        ...(defaultDays
          ? [
              {
                ...lastOccurredDateSpecification,
                value: dateTime.now().addDays(-defaultDays).format('dateLocal'),
              },
            ]
          : []),
        // according to design doc listed below, to display insights that are on "Resolved"/"Ignored" tab
        // means that we want to to go to "StatusLogs" database and check if an insight has its status
        // changed to "Resolved"/"Ignored" at some point in time
        // https://willow.atlassian.net/wiki/spaces/PE/pages/2482831361/Insights+to+Action+V2+Feature+Overview?focusedCommentId=2523430930
        ...(isResolved ? [resolvedSpecification] : []),
        ...(isIgnored ? [ignoredSpecification] : []),
        ...(!isResolved && !isIgnored
          ? [
              {
                field: 'Status',
                operator: FilterOperator.containedIn,
                value: statusMap[tab] ?? statusMap.default,
              },
            ]
          : []),
      ],
    },
    {
      enabled: assetId == null,
      select: (data) => selectInsights(data, sites),
    }
  )

  const assetInsightsQuery = useGetAssetInsights(
    {
      filterSpecifications: [
        {
          field: 'siteId',
          operator: FilterOperator.equalsLiteral,
          value: siteId,
        },
        {
          field: 'equipmentId',
          operator: FilterOperator.equalsLiteral,
          value: assetId,
        },
        {
          field: 'status',
          operator: FilterOperator.containedIn,
          value: statusMap.default,
        },
      ],
    },
    {
      enabled: nextSiteId != null && assetId != null,
      select: (data) => {
        const insights = selectInsights(data, sites)
        return insights.map((insight) => ({
          ...insight,
          occurredDate: formatDateTime({
            value: insight.occurredDate,
            language,
            timeZone: nextSite?.timeZone,
          }),
        }))
      },
    }
  )

  const isLoading = insightsQuery.isLoading || assetInsightsQuery.isLoading
  const isError = insightsQuery.isError || assetInsightsQuery.isError
  const noInsights =
    (insightsQuery.isSuccess && insightsQuery.data?.length === 0) ||
    (assetInsightsQuery.isSuccess && assetInsightsQuery.data?.length === 0)

  const filterOptions = useMemo(() => {
    const insights = insightsQuery.data ?? assetInsightsQuery.data ?? []

    const nextStatus = _(insights)
      .map((insight) => insight.lastStatus ?? '')
      .uniq()
      .orderBy((lastStatus) => lastStatus.toLowerCase())
      .value()

    const nextSources = _(insights)
      .map((insight) => insight.sourceName ?? '')
      .uniq()
      .orderBy((source) => source.toLowerCase())
      .value()

    const nextPrimaryModelIds = _(insights)
      .map((insight) => insight.primaryModelId ?? '')
      ?.uniq()
      ?.value()
      .filter((item) => item !== '')

    const nextTypes = _(insights)
      .map((insight) => insight.type)
      .uniq()
      .orderBy((type) => type.toLowerCase())
      .value()

    return {
      statusOptions: nextStatus,
      sourceOptions: nextSources,
      typeOptions: nextTypes,
      primaryModelIdOptions: nextPrimaryModelIds,
    }
  }, [insightsQuery.data, assetInsightsQuery.data])

  // When query is in loading state, sourceOptions and typeOptions will be empty,
  // so we do not want to update filters options, when either one of them has
  // length larger than 0, we update filters options
  useEffect(() => {
    const { statusOptions, sourceOptions, typeOptions, primaryModelIdOptions } =
      filterOptions
    if (
      sourceOptions.length > 0 ||
      typeOptions.length > 0 ||
      primaryModelIdOptions.length > 0 ||
      statusOptions.length > 0 ||
      // when response is empty, update filters options.
      insightsQuery.data?.length === 0 ||
      assetInsightsQuery.data?.length === 0
    ) {
      setFilters((prevFilters) => ({
        ...prevFilters,
        statuses: statusOptions,
        sources: sourceOptions,
        types: typeOptions,
        modelIds: primaryModelIdOptions as string[],
      }))
    }
  }, [
    filterOptions,
    insightsQuery.data?.length,
    assetInsightsQuery.data?.length,
  ])

  const selectedInsight = (
    assetId == null ? insightsQuery.data : assetInsightsQuery.data
  )?.find((insight) => insight.id === selectedInsightId)

  /**
   * Saving selected insights filters in insightFilterSettings user preference.
   * The functionality is similar to filter section in Tickets page
   */
  useEffect(() => {
    const existingFilterSettings =
      user.options?.insightFilterSettings?.[selectedSiteId]
    user.saveOptions('insightFilterSettings', {
      ...user.options?.insightFilterSettings,
      [selectedSiteId]: {
        ...existingFilterSettings,
        siteId: filters.siteId,
        search: filters.search,
        selectedPriorities: filters.selectedPriorities,
        selectedStatuses: filters.selectedStatuses,
        selectedSources: filters.selectedSources,
        selectedTypes: filters.selectedTypes,
        selectedModelId: filters.selectedModelId,
      },
    })
  }, [
    filters.selectedStatuses,
    filters.siteId,
    filters.search,
    filters.selectedPriorities,
    filters.selectedSources,
    filters.selectedTypes,
    filters.selectedModelId,
  ])

  // to ensure the group containing selectedInsight is
  // always expanded
  useEffect(() => {
    if (groupBy && groupBy !== InsightGroups.NONE && selectedInsight) {
      const groupByIdForSelectedInsight = selectedInsight?.[groupById]
      onExpandGroup?.({ expandedGroupId: groupByIdForSelectedInsight })
    }
  }, [selectedInsight, groupBy, expandedGroupId, groupById, onExpandGroup])

  const [selectedInsightIds, setSelectedInsightIds] = useState<string[]>([])
  const [selectedRuleIds, setSelectedRuleIds] = useState<string[]>([])

  const showOccurrences =
    showSite || !nextSite?.features.isHideOccurrencesEnabled

  // Exclude selectedSources and selectedTypes that are not in insights list.
  // - We stored all selected filters from all the tabs in insights page, so applied filters will persist
  //   when switching tabs.
  const selectedSources = filters.selectedSources.filter((source) =>
    filters.sources.includes(source)
  )
  const selectedTypes = filters.selectedTypes.filter((type) =>
    filters.types.includes(type)
  )

  const selectedStatuses = filters.selectedStatuses.filter((status) =>
    filters.statuses.includes(status)
  )

  // legacy business logic to not filter insights for asset
  const filteredInsights =
    assetId == null
      ? filterInsights({
          insights: insightsQuery.data ?? [],
          sites,
          filters,
          selectedSources,
          selectedStatuses,
          selectedTypes,
          selectedModelId: primaryModelId,
          dateTime,
          language,
        })
      : assetInsightsQuery.data

  // when viewing insights on resolved tab, the impact column would be "savings" instead of cost or energy
  // https://dev.azure.com/willowdev/Unified/_workitems/edit/84465
  const isSavings = tab === 'resolved' || tab === 'closed'
  const rollupControls = [
    // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/84465
    ...(isSavings
      ? [
          {
            text: t('plainText.totalSavingsToDate'),
            control: InsightTableControls.showSavingsToDate,
          },
        ]
      : [
          {
            text: t('interpolation.totalExpenseToDate', {
              expense: t('plainText.impact'),
            }),
            control: InsightTableControls.showTotalImpactToDate,
          },
        ]),
    {
      text: isSavings
        ? t('plainText.totalSavingsPerYear')
        : t('interpolation.avoidableExpensePerYear', {
            expense: t('plainText.impact'),
          }),
      control: InsightTableControls.showImpactPerYear,
    },
    {
      text: t('plainText.topContributorAsset'),
      control: InsightTableControls.showTopAsset,
    },
  ]

  const [showTotalImpact, setShowTotalImpact] = useState(false)

  /**
   *  If Rollup Widgets are checked, they are added to nextIncludedRollups.
   *  nextIncludedRollups is used for analytics tracking of TableViewControls.
   */

  const nextExcludedRollups = showTotalImpact
    ? [...(tableControls?.excludedRollups ?? [])]
    : [
        ...(tableControls?.excludedRollups ?? []),
        InsightTableControls.showTotalImpactToDate,
      ]

  const [nextIncludedRollups, setNextIncludedRollups] = useState<
    InsightTableControls[]
  >(
    rollupControls
      .filter(({ control }) => !nextExcludedRollups.includes(control))
      .map(({ control }) => control)
  )

  const eventBody = {
    impactView: tableControls?.impactView ?? InsightMetric.cost,
    // On loading insights page, default value of includedRollups (if empty) :  showTopAsset, showImpactPerYear
    includedRollups: nextIncludedRollups,
    siteId: filters.siteId,
    selectedSources: filters.selectedSources,
    selectedPriorities: filters.selectedPriorities,
    selectedStatuses: filters.selectedStatuses,
    selectedTypes: filters.selectedTypes,
    selectedModelId: filters.selectedModelId,
    lastOccurredDate: filters.days,
    groupBy: groupBy === undefined ? InsightGroups.NONE : groupBy,
  }

  const context = {
    insightsQuery,
    assetInsightsQuery,
    siteId: nextSiteId,
    showSite,
    showOccurrences,
    assetId,
    sourceType,
    dataSegmentPropPage,
    filteredInsights,
    isLoading,
    isError,
    noInsights,
    eventBody,
    setNextIncludedRollups,
    showTotalImpact,
    setShowTotalImpact,
    rollupControls,
    isSavings,

    tab,
    insightTab,
    groupBy,
    groupByName,
    expandedGroupId,
    groupById,
    tableControls,
    filters,
    selectedInsightIds,
    selectedRuleIds,
    selectedInsight,
    dateColumn: dateColumnMap[tab] ?? defaultDateColumn,
    days: defaultDays,

    setFilters,
    onInsightTabChange,
    onExpandGroup,
    setSelectedInsightIds,
    setSelectedRuleIds,
    onTabChange,
    onGroupByOptionClick,
    onModelNameOptionClick,
    onTableControlChange,
    getTranslatedModelName,
    onPageSizeChange,
    onDaysChange,
    onTwinIdChange,
    paginationEnabled,
    pageSize,
    initialPageIndex,
    selectedModelId: filters.selectedModelId,
    // hide status columns on resolved and ignored tab; since we had "resolved" mapped to "closed"
    // and "ignored" mapped to "acknowledged", we hide status column for those as well
    // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/84467
    // https://willow.atlassian.net/wiki/spaces/PE/pages/2498625742/Tech+Notes+LastStatus+for+Insight+Workflow+associated+Dtos
    hideStatusColumn: [
      'acknowledged',
      'ignored',
      'resolved',
      'closed',
    ].includes(tab),

    clearFilters() {
      setFilters((prevFilters) => ({
        ...prevFilters,
        search: '',
        siteId: showSite ? undefined : prevFilters.siteId,
        selectedPriorities: [],
        selectedSources: [],
        selectedStatuses: [],
        selectedTypes: [],
        selectedModelId: '',
        date: null,
      }))

      if (onModelNameOptionClick) {
        onModelNameOptionClick({
          primaryModelId: undefined,
        })
      }

      onTwinIdChange?.({ twinId: undefined })
    },

    hasFiltersChanged() {
      return !_.isEqual(
        {
          search: filters.search,
          siteId: filters.siteId,
          selectedPriorities: filters.selectedPriorities,
          // Exclude selectedSources and selectedTypes that are not in insights response.
          // - We stored all selected filters from all the tabs in insights page, so applied filters will persist
          //   when switching tab
          selectedSources,
          selectedTypes,
          selectedStatuses,
          selectedModelId: filters.selectedModelId,
          days: defaultDays,
          twinId,
        },
        {
          search: '',
          siteId: showSite ? undefined : filters.siteId,
          selectedPriorities: [],
          selectedSources: [],
          selectedTypes: [],
          selectedStatuses: [],
          selectedModelId: '',
          days: null,
          twinId: undefined,
        }
      )
    },

    isInsightIdSelected(insightId: string) {
      return selectedInsightIds.includes(insightId)
    },

    toggleSelectedInsightId(insightId: string) {
      setSelectedInsightIds((prevSelectedInsightIds) =>
        _.xor(prevSelectedInsightIds, [insightId])
      )
    },

    clearSelectedInsightIds() {
      setSelectedInsightIds([])
    },

    /**
     * Here we are checking if entire table is selected then it will reset the
     * rule IDs and insight IDs state with empty array else it will update all
     * the rule IDs and insight IDs present in table
     */
    selectAllGroupedInsights(
      allGroupedInsights: Insight[],
      isAllDisplayedInsightsSelected: boolean
    ) {
      if (isAllDisplayedInsightsSelected) {
        setSelectedRuleIds([])
        setSelectedInsightIds([])
      } else {
        let allInsightIds: string[] = []
        const allGroupIds = allGroupedInsights.map((insight) => {
          allInsightIds = allInsightIds.concat(insight.subRowInsightIds ?? [])
          return insight.id
        })
        setSelectedInsightIds(allInsightIds)
        setSelectedRuleIds(allGroupIds)
      }
    },

    /**
     * Here we are checking if all the insights belonging to a grouped rule is selected(i.e. grouped rule is selected)
     * then it will toggle their insight IDs and rule ID from their states
     * else it will add all the insight IDs and selected grouped rule ID
     */
    selectGroupedInsights(isEveryInsightChecked: boolean, insight: Insight) {
      const { subRowInsightIds, id } = insight
      if (isEveryInsightChecked) {
        setSelectedInsightIds(_.xor(selectedInsightIds, subRowInsightIds))
        setSelectedRuleIds(_.xor(selectedRuleIds, [id]))
      } else {
        setSelectedInsightIds(
          _.uniq(_.concat(selectedInsightIds, subRowInsightIds ?? []))
        )
        setSelectedRuleIds(_.concat(selectedRuleIds, [id]))
      }
    },
  }

  return (
    <InsightsContext.Provider value={context}>
      {children}
    </InsightsContext.Provider>
  )
}

export const filterInsights = ({
  insights,
  sites,
  filters,
  selectedSources,
  selectedTypes,
  selectedStatuses = [],
  selectedModelId,
  dateTime,
  language,
}: {
  insights: InsightsResponse
  sites: Site[]
  filters: Filter
  selectedSources: string[]
  selectedTypes: string[]
  selectedModelId?: string
  selectedStatuses?: string[]
  dateTime: ReturnType<typeof useDateTime>
  language: string
}) =>
  insights
    ?.map((insight) => {
      const site = sites?.find((s) => s.id === insight.siteId)
      return {
        ...insight,
        site,
        siteName: site?.name ?? '-',
        occurredDate: formatDateTime({
          value: insight.occurredDate,
          language,
          timeZone: site?.timeZone,
        }),
        lastResolvedDate: formatDateTime({
          value: insight.lastResolvedDate,
          language,
          timeZone: site?.timeZone,
        }),
        lastIgnoredDate: formatDateTime({
          value: insight.lastIgnoredDate,
          language,
          timeZone: site?.timeZone,
        }),
        cost:
          insight.impactScores?.find((score) => score.name === 'Cost') ?? {},
      } as Insight
    })
    .filter(
      (insight) => filters?.siteId == null || filters?.siteId === insight.siteId
    )
    .filter(
      (insight) =>
        insight?.sequenceNumber
          ?.toLowerCase()
          .includes(filters?.search.toLowerCase()) ||
        insight?.name?.toLowerCase().includes(filters?.search.toLowerCase()) ||
        insight?.twinId?.toLowerCase().includes(filters?.search.toLowerCase())
    )
    .filter(
      (insight) =>
        filters?.selectedPriorities.length === 0 ||
        filters?.selectedPriorities.includes(insight.priority)
    )
    .filter(
      (insight) =>
        selectedSources.length === 0 ||
        (insight.sourceName != null &&
          selectedSources.includes(insight.sourceName))
    )
    .filter(
      (insight) =>
        selectedTypes.length === 0 || selectedTypes.includes(insight.type)
    )
    .filter(
      (insight) =>
        selectedStatuses.length === 0 ||
        selectedStatuses?.includes(insight.lastStatus)
    )
    .filter(
      (insight) =>
        selectedModelId == null ||
        (insight.primaryModelId &&
          insight.primaryModelId === selectedModelId) ||
        // if selectedModelId is not in the list of available modelIds
        // to filter by, then this filter is a pass through
        !filters.modelIds.includes(selectedModelId)
    )
    .filter((insight) => {
      if (filters?.days === 7) {
        return (
          dateTime.now().differenceInDays(dateTime(insight.occurredDate)) < 7
        )
      }

      if (filters?.days === 30) {
        return (
          dateTime.now().differenceInDays(dateTime(insight.occurredDate)) < 30
        )
      }

      if (filters?.days === 365) {
        return (
          dateTime.now().differenceInYears(dateTime(insight.occurredDate)) < 1
        )
      }

      return true
    })

/**
 * key is possible insight statuses when querying insights and columnText
 * is the expected column text for "Last Date" column, and accessor is
 * the property name of the insight; e.g., when viewing ignored insights,
 * the column text should be "Last Ignored Date", and property name should
 * be "lastIgnoredDate"
 */
const dateColumnMap = {
  ignored: {
    columnText: 'plainText.lastIgnoredDate',
    accessor: 'lastIgnoredDate',
  },
  acknowledged: {
    columnText: 'plainText.lastIgnoredDate',
    accessor: 'lastIgnoredDate',
  },
  resolved: {
    columnText: 'plainText.lastResolvedDate',
    accessor: 'lastResolvedDate',
  },
  closed: {
    columnText: 'plainText.lastResolvedDate',
    accessor: 'lastResolvedDate',
  },
  open: {
    columnText: 'plainText.lastOccurredDate',
    accessor: 'occurredDate',
  },
}

const defaultDateColumn = {
  columnText: 'plainText.lastOccurredDate',
  accessor: 'occurredDate',
}

const resolvedSpecification = {
  field: 'StatusLogs[Status]',
  operator: FilterOperator.equalsLiteral,
  value: statusMap.resolved[0],
}
const ignoredSpecification = {
  field: 'StatusLogs[Status]',
  operator: FilterOperator.equalsLiteral,
  value: statusMap.ignored[0],
}
const lastOccurredDateSpecification = {
  field: 'LastOccurredDate',
  operator: FilterOperator.greaterThanOrEqual,
}
