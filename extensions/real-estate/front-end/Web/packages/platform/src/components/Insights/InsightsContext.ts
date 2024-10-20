import { ProviderRequiredError } from '@willow/common'
import {
  EventBody,
  Filter,
  Insight,
  InsightStatus,
  InsightTableControls,
  InsightWorkflowStatus,
  SourceType,
} from '@willow/common/insights/insights/types'
import { InsightGroups } from '@willow/ui/constants'
import { createContext, Dispatch, SetStateAction, useContext } from 'react'
import { UseQueryResult } from 'react-query'
import { InsightsResponse } from '../../services/Insight/InsightsService'

export interface InsightsContextType {
  insightsQuery: UseQueryResult<InsightsResponse, unknown>
  assetInsightsQuery: UseQueryResult<InsightsResponse, unknown>
  assetId: string
  clearFilters: () => void
  clearSelectedInsightIds: () => void
  dataSegmentPropPage: string
  filteredInsights?: Insight[]
  filters: Filter
  groupBy: InsightGroups
  eventBody: EventBody
  setNextIncludedRollups?: (nextIncludedRollups: InsightTableControls[]) => void
  showTotalImpact: boolean
  setShowTotalImpact: (showTotalImpact: boolean) => void
  rollupControls: Array<{ text: string; control: InsightTableControls }>
  /**
   * the property of insight used to display as the name of each group
   * e.g.:
   * - when table is grouped by asset, use insight.equipmentName
   * - when table is grouped by rule, use insight.ruleName
   */
  groupByName: string
  expandedGroupId?: string
  /**
   * the property of insight used to group insights
   * e.g.:
   * - when table is grouped by asset, use insight.equipmentId
   * - when table is grouped by rule, use insight.ruleId
   */
  groupById: string
  hasFiltersChanged: () => boolean
  isInsightIdSelected: (insightId: string) => boolean
  onModelNameOptionClick?: (obj: { primaryModelId?: string }) => void
  onGroupByOptionClick?: (obj: {
    groupBy?: string
    expandedGroupId?: string
  }) => void
  tableControls?: { [key: string]: string | string[] }
  onTableControlChange?: (obj: { [key: string]: string | string[] }) => void
  onTabChange: (tab: InsightStatus | null) => void
  selectedInsightIds: string[]
  selectedRuleIds: string[]
  selectedInsight?: Insight
  selectedModelId?: string
  setSelectedInsightIds: (selectedInsightIds: string[]) => void
  setSelectedRuleIds: (selectedRuleIds: string[]) => void
  onExpandGroup?: (obj: { expandedGroupId?: string }) => void
  onDaysChange?: (nextDays?: { days?: number }) => void
  onTwinIdChange?: (twin?: { twinId?: number }) => void
  days?: number
  selectAllGroupedInsights: (
    allGroupedInsights: Insight[],
    isAllDisplayedInsightsSelected: boolean
  ) => void
  selectGroupedInsights: (
    isEveryInsightChecked: boolean,
    rowData: Insight
  ) => void
  setFilters: Dispatch<SetStateAction<Filter>>
  showOccurrences: boolean
  sourceType: SourceType
  showSite: boolean
  siteId?: string
  tab: InsightStatus | InsightWorkflowStatus
  toggleSelectedInsightId: (insightId: string) => void
  isLoading: boolean
  isError?: boolean
  noInsights?: boolean
  getTranslatedModelName?: (primaryModelId: string) => void
  paginationEnabled: boolean
  onPageSizeChange?: (pageSize: number) => void
  pageSize: number
  initialPageIndex: number
  insightTab: string
  onInsightTabChange: (tab?: string) => void
  hideStatusColumn: boolean
  dateColumn: {
    columnText: string
    accessor: string
  }
  isSavings: boolean
}

export const InsightsContext = createContext<InsightsContextType | undefined>(
  undefined
)

export function useInsights() {
  const context = useContext(InsightsContext)
  if (context == null) {
    throw new ProviderRequiredError('Insights')
  }
  return context
}
