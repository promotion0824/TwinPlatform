/* eslint-disable complexity */
import { useMemo, useCallback } from 'react'
import tw from 'twin.macro'
import {
  Progress,
  Message,
  NotFound,
  InsightGroups,
  useLanguage,
  useScopeSelector,
} from '@willow/ui'
import {
  Insight,
  InsightTableControls,
} from '@willow/common/insights/insights/types'
import { useTranslation, TFunction } from 'react-i18next'
import { useHistory } from 'react-router'
import { InsightMetric } from '@willow/common'
import { useInsights } from './InsightsContext'
import ImpactInsightsGroupedTable from './InsightGroupTable/ImpactInsightsGroupedTable/ImpactInsightsGroupedTable'
import routes from '../../routes'
import { filterInsights } from './InsightsProvider'
import ImpactInsightsUngroupedTable from './InsightsTableContent/ImpactInsightsUngroupedTable/ImpactInsightsUngroupedTable'

/**
 * This is a container component retrieves insights data for a site
 * when assetId does not exist, or insights data for an asset
 * otherwise, and pass insights data to its children
 */
export default function InsightsTable({
  onSelectInsight,
  selectedInsight: controlledSelectedInsight,
  onInsightIdChange,
}: {
  onSelectInsight?: (insight?: Insight) => void
  selectedInsight?: Insight
  onInsightIdChange?: (insightId?: string) => void
}) {
  const {
    assetId,
    filters,
    assetInsightsQuery,
    insightsQuery,
    siteId,
    selectedInsight,
    groupBy,
    selectedInsightIds,
    selectAllGroupedInsights,
    selectGroupedInsights,
    isLoading,
    isError,
    noInsights,
    selectedModelId,
    dataSegmentPropPage,
    tableControls,
    onExpandGroup,
    setSelectedInsightIds,
    toggleSelectedInsightId,
    groupByName,
    expandedGroupId,
    groupById,
    getTranslatedModelName,
    paginationEnabled,
    pageSize,
    onPageSizeChange,
    initialPageIndex,
    insightTab,
    onInsightTabChange,
    hideStatusColumn,
    dateColumn,
    isSavings,
    tab,
    clearSelectedInsightIds,
  } = useInsights()
  const history = useHistory()
  const { t } = useTranslation()
  const { language } = useLanguage()
  const { isScopeSelectorEnabled, location } = useScopeSelector()
  const showGroupedInsightTable = groupBy && groupBy !== InsightGroups.NONE
  /**
   * this handler will enable user to click on
   * asset name on Insight Detail Drawer and navigate to
   * twin explorer of that asset, once user click on "back"
   * button, user will come back to Insights page with
   * same Insight Detail Drawer opened.
   *
   * Note: this behavior is only relevant to
   * - packages\platform\src\components\Insights\Insights.js (main Insight Page)
   * - packages\platform\src\components\TimeSeries\PointSelector\AssetModal\Content\Insights.js
   * - packages\platform\src\views\Command\Dashboard\FloorViewer\Floor\SidePanel\Content\Insights.js
   *
   */
  const handleSelectInsight = useCallback(
    (insight?: Insight) => {
      const isScopeDefined = isScopeSelectorEnabled && location?.twin?.id
      const cardViewInsightNodePathname =
        insight != null
          ? isScopeDefined
            ? routes.insights_scope__scopeId_insight__insightId(
                location.twin.id,
                insight.id
              )
            : routes.sites__siteId_insights__insightId(
                insight.siteId,
                insight.id
              )
          : isScopeDefined
          ? routes.insights_scope__scopeId(location.twin.id)
          : siteId != null
          ? routes.sites__siteId_insights(siteId)
          : routes.insights

      const isTimeSeries = history.location?.pathname?.includes('time-series')
      const isClassicExplorer = history.location?.pathname?.includes('/floors/')

      // if user is at time series or classic explorer page
      // while insight isn't defined;
      // we update query string param "insightId" to insight?.id whether it's defined or not
      if (isTimeSeries || isClassicExplorer) {
        if (insight == null) {
          onInsightIdChange?.(undefined)
          return
        }
      }

      history.push({
        pathname: cardViewInsightNodePathname,
        search: new URLSearchParams(history.location.search).toString(),
      })
    },
    [
      history,
      isScopeSelectorEnabled,
      location?.twin?.id,
      onInsightIdChange,
      siteId,
    ]
  )
  const metricFromQueryString = tableControls?.[InsightTableControls.impactView]
  const insightMetric = isString(metricFromQueryString)
    ? metricFromQueryString
    : InsightMetric.cost

  // memorize the following values and handlers helps to prevent unnecessary re-rendering
  // which can improve performance drastically
  const memorizedSelectedSources = useMemo(
    () =>
      filters.selectedSources.filter((source) =>
        filters.sources.includes(source)
      ),
    [filters.selectedSources, filters.sources]
  )
  const memorizedSelectedTypes = useMemo(
    () => filters.selectedTypes.filter((type) => filters.types.includes(type)),
    [filters.selectedTypes, filters.types]
  )

  const memorizedSelectedStatuses = useMemo(
    () =>
      filters.selectedStatuses.filter((status) =>
        filters.statuses.includes(status)
      ),
    [filters.selectedStatuses, filters.statuses]
  )

  const memorizedSites = useMemo(() => filters.sites ?? [], [filters.sites])
  const memorizedDateTime = useMemo(() => filters.dateTime, [filters.dateTime])
  const memorizedOnToggleSelectedInsightId = useCallback(
    (insightId: string) => toggleSelectedInsightId(insightId),
    []
  )
  const memorizedIsInsightIdSelected = useCallback(
    (insightId: string) => selectedInsightIds.includes(insightId),
    [selectedInsightIds]
  )
  const memorizedOnPageSizeChange = useCallback(
    (size: number) => onPageSizeChange?.(size),
    []
  )
  const memorizedTFunction = useCallback<TFunction>(
    (translationKey: string, options) => t(translationKey, options),
    []
  )

  const memorizedFilteredInsights = useMemo(
    () =>
      filterInsights({
        insights: insightsQuery.data ?? [],
        sites: memorizedSites,
        filters,
        selectedSources: memorizedSelectedSources,
        selectedStatuses: memorizedSelectedStatuses,
        selectedTypes: memorizedSelectedTypes,
        selectedModelId,
        dateTime: memorizedDateTime,
        language,
      }),
    [
      insightsQuery.data,
      memorizedSites,
      filters,
      memorizedSelectedSources,
      memorizedSelectedStatuses,
      memorizedSelectedTypes,
      selectedModelId,
      memorizedDateTime,
      language,
    ]
  )

  const memorizedOnSelectAllGroupedInsights = useCallback(
    (
      allGroupedInsights: Insight[],
      isAllDisplayedInsightsSelected: boolean
    ) => {
      selectAllGroupedInsights?.(
        allGroupedInsights,
        isAllDisplayedInsightsSelected
      )
    },
    []
  )
  const memorizedOnSelectGroupedInsights = useCallback(
    (isEveryInsightChecked: boolean, insight: Insight) => {
      selectGroupedInsights?.(isEveryInsightChecked, insight)
    },
    [selectedInsightIds]
  )

  const memorizedClearSelectedInsightIds = useCallback(
    () => clearSelectedInsightIds?.(),
    []
  )

  return isError ? (
    <Message tw="h-full" icon="error">
      {t('plainText.errorOccurred')}
    </Message>
  ) : isLoading ? (
    <Progress />
  ) : noInsights ? (
    <NotFound>{t('plainText.noInsightsFound')}</NotFound>
  ) : showGroupedInsightTable ? (
    <ImpactInsightsGroupedTable
      language={language}
      t={memorizedTFunction}
      dataSegmentPropPage={dataSegmentPropPage}
      filteredInsights={
        assetId == null
          ? memorizedFilteredInsights
          : assetInsightsQuery.data ?? []
      }
      selectedInsightIds={selectedInsightIds}
      isInsightIdSelected={memorizedIsInsightIdSelected}
      onSelectedInsightIds={setSelectedInsightIds}
      onToggleSelectedInsightId={memorizedOnToggleSelectedInsightId}
      onExpandGroup={onExpandGroup}
      onSelectAllGroupedInsights={memorizedOnSelectAllGroupedInsights}
      onSelectGroupedInsights={memorizedOnSelectGroupedInsights}
      siteId={siteId}
      selectedInsight={
        onSelectInsight ? controlledSelectedInsight : selectedInsight
      }
      onSelectInsight={onSelectInsight ?? handleSelectInsight}
      insightMetric={insightMetric}
      groupBy={groupBy}
      groupByName={groupByName}
      expandedGroupId={expandedGroupId}
      groupById={groupById}
      getTranslatedModelName={getTranslatedModelName}
      paginationEnabled={paginationEnabled}
      pageSize={pageSize}
      onPageSizeChange={memorizedOnPageSizeChange}
      initialPageIndex={initialPageIndex}
      insightTab={insightTab}
      onInsightTabChange={onInsightTabChange}
      hideStatusColumn={hideStatusColumn}
      dateColumn={dateColumn}
      isSavings={isSavings}
      tab={tab}
      clearSelectedInsightIds={memorizedClearSelectedInsightIds}
    />
  ) : (
    <ImpactInsightsUngroupedTable
      selectedInsight={selectedInsight}
      filteredInsights={
        assetId == null
          ? memorizedFilteredInsights
          : assetInsightsQuery.data ?? []
      }
      selectedInsightIds={selectedInsightIds}
      onToggleSelectedInsightId={memorizedOnToggleSelectedInsightId}
      isInsightIdSelected={memorizedIsInsightIdSelected}
      onSelectedInsightIds={setSelectedInsightIds}
      onSelectInsight={onSelectInsight ?? handleSelectInsight}
      siteId={siteId}
      dataSegmentPropPage={dataSegmentPropPage}
      insightMetric={insightMetric}
      paginationEnabled={paginationEnabled}
      pageSize={pageSize}
      initialPageIndex={initialPageIndex}
      onPageSizeChange={memorizedOnPageSizeChange}
      language={language}
      t={memorizedTFunction}
      insightTab={insightTab}
      onInsightTabChange={onInsightTabChange}
      hideStatusColumn={hideStatusColumn}
      dateColumn={dateColumn}
      isSavings={isSavings}
      tab={tab}
      onInsightIdChange={onInsightIdChange}
      clearSelectedInsightIds={memorizedClearSelectedInsightIds}
    />
  )
}

export const isString = (s: string | string[] | undefined): s is string =>
  typeof s === 'string'
