import { ProviderRequiredError } from '@willow/common'
import { ParamsDict } from '@willow/common/hooks/useMultipleSearchParams'
import {
  Analytics,
  CardSummaryFilters,
  CardSummaryRule,
  EventBody,
  ImpactScoreSummary,
  Insight,
  InsightCardGroups,
  InsightTableControls,
  InsightTypesGroupedByDate,
  InsightView,
  ViewByOptionsMap,
} from '@willow/common/insights/insights/types'
import { Site } from '@willow/common/site/site/types'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { GridSortModel } from '@willowinc/ui'
import { History } from 'history'
import { createContext, useContext } from 'react'
import { TFunction } from 'react-i18next'
import { QueryStatus } from 'react-query'
import useOntologyInPlatform from '../../../hooks/useOntologyInPlatform'
import { FilterOperator } from '../../../services/Insight/InsightsService'

export type FilterSpecification = {
  field: string
  operator: FilterOperator
  value?: string | string[]
}

export type SortSpecification = {
  field: string
  sort: string
}

export interface InsightsState {
  selectedInsight?: Insight
  selectedInsightIds?: string[]
  nextIncludedRollups?: InsightTableControls[]
  rollupControls: Array<{ text: string; control: InsightTableControls }>
  insightTypesGroupedByDate?: InsightTypesGroupedByDate
  impactScoreSummary?: ImpactScoreSummary[]
  cards?: CardSummaryRule[]
  impactView: string
  sortSpecifications: GridSortModel
  excludedRollups: string | string[]
  showTotalImpact: boolean
  insights?: Insight[]
  assetInsights?: Insight[]
  page?: number
  pageSize?: number
  filterSpecifications: FilterSpecification[]
  onPageSizeChange?: (pageSize: number) => void
  insightStatus?: string[]
  cardSummaryFilters?: CardSummaryFilters
}

export interface InsightsContextType extends InsightsState {
  groupBy?: InsightCardGroups
  isLoading: boolean
  siteId: string
  sites: Site[]
  assetId?: string
  eventBody: EventBody
  totalInsights?: number
  setShowTotalImpact: (showTotalImpact: boolean) => void
  onInsightIdChange?: (insightId?: string) => void
  onQueryParamsChange?: (params: ParamsDict) => void
  onUpdateIncludedRollups?: (
    nextIncludedRollups: InsightTableControls[]
  ) => void
  onChangeFilter: (
    filterName: string,
    filterValue?: string | string[] | null
  ) => void
  handleInsightTypeClick: (card: CardSummaryRule) => void
  handleInsightClick: (insight: Insight) => void
  onResetFilters: (nextQueryParams: ParamsDict) => void
  onSelectInsight: (insight: Insight) => void
  onSelectInsightIds: (selectedInsightIds: string[]) => void
  onSortModelChange: (sortModel: GridSortModel) => void
  cardSummaryFilters?: CardSummaryFilters
  onResetInsight: () => void
  onResetInsightIds: () => void
  view?: InsightView
  t: TFunction
  language: string
  canWillowUserDeleteInsight?: boolean
  analytics?: Analytics
  history: History
  ontologyQuery: ReturnType<typeof useOntologyInPlatform>
  modelsOfInterestQuery: ReturnType<typeof useModelsOfInterest>
  insightId: string
  ruleId?: string
  isInsightTypeNode?: boolean
  isUngrouped?: boolean
  isWalmartAlert?: boolean
  viewByOptionsMap: ViewByOptionsMap
  totalCount?: number
  queryParams: ParamsDict
  filterQueryStatus: QueryStatus
  lastInsightStatusCountDate?: string
  onInsightCountDateChange?: (currentDate: string) => void
  hasAppliedFilter: boolean
}

export const InsightsContext = createContext<InsightsContextType | undefined>(
  undefined
)

export function useInsightsContext() {
  const context = useContext(InsightsContext)
  if (context == null) {
    throw new ProviderRequiredError('InsightsCard')
  }
  return context
}
