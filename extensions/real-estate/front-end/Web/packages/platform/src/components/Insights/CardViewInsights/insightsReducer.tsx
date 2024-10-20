/* eslint-disable complexity */
import { InsightMetric } from '@willow/common'
import {
  CardSummaryFilters,
  CardSummaryRule,
  ImpactScoreSummary,
  Insight,
  InsightTableControls,
  InsightTypesGroupedByDate,
} from '@willow/common/insights/insights/types'
import { GridSortDirection, GridSortModel } from '@willowinc/ui'
import _ from 'lodash'
import { FilterOperator } from '../../../services/Insight/InsightsService'
import { InsightsState } from './InsightsContext'

export const makeDefaultInsightState = ({
  rollupControls,
  status,
}: {
  siteId?: string
  rollupControls: Array<{ text: string; control: InsightTableControls }>
  status: string[]
  lastOccurredDate?: string | string[]
  ruleId?: string | string[]
}) => ({
  filterSpecifications: [],
  sortSpecifications: [{ field: 'LastOccurredDate', sort: desc }],
  status,
  showTotalImpact: false,
  rollupControls,
  selectedInsightIds: [],
  nextIncludedRollups: [],
  impactView: InsightMetric.cost,
  cards: [],
  insightTypesGroupedByDate: [],
  excludedRollups: [],
  page: 1,
  pageSize: 10,
  impactScoreSummary: [],
  onPageSizeChange: (pageSize: number) => {},
})

/**
 * This is the action type which user can perform in Insight Card view table and filter section
 */
export enum InsightsActionType {
  'onLoadedInsights',
  'updateFilterSpecifications',
  'onLoadedInsightTypes',
  'onLoadedAssetInsights',
  'updateSortSpecifications',
  'insightCardSummaryFilterChange',
  'updateInsight',
  'selectInsight',
  'selectInsightIds',
  'resetInsight',
  'resetInsightIds',
  'onUpdateTotalImpact',
  'onUpdateIncludedRollups',
  'onLoadedInsightFilters',
}

const sortMap = new Map<string | undefined, string>([
  ['lastStatus', 'status'],
  ['name', 'ruleName'],
  ['occurredDate', 'LastOccurredDate'],
])

export type InsightsAction =
  | {
      type: InsightsActionType.onLoadedInsights
      insights: Insight[]
      impactScoreSummary?: ImpactScoreSummary[]
    }
  | {
      type: InsightsActionType.onLoadedInsightFilters
      filters: CardSummaryFilters
    }
  | {
      type: InsightsActionType.updateFilterSpecifications
      filterSpecUpdates?: Array<{
        specName: string
        specOperator?: FilterOperator
        specValue?: string | string[]
      }>
    }
  | {
      type: InsightsActionType.updateSortSpecifications
      sortSpecifications: GridSortModel
    }
  | {
      type: InsightsActionType.onLoadedInsightTypes
      insightTypesGroupedByDate?: InsightTypesGroupedByDate
      cards?: CardSummaryRule[]
      impactScoreSummary?: ImpactScoreSummary[]
    }
  | {
      type: InsightsActionType.insightCardSummaryFilterChange
      filterName: string
      filterValue: string
    }
  | {
      type: InsightsActionType.onLoadedAssetInsights
      assetInsights: Insight[]
    }
  | {
      type: InsightsActionType.selectInsight
      selectedInsight: Insight
    }
  | {
      type: InsightsActionType.selectInsightIds
      selectedInsightIds: string[]
    }
  | {
      type: InsightsActionType.resetInsight
    }
  | {
      type: InsightsActionType.resetInsightIds
    }
  | {
      type: InsightsActionType.onUpdateTotalImpact
      showTotalImpact: boolean
    }
  | {
      type: InsightsActionType.onUpdateIncludedRollups
      nextIncludedRollups: InsightTableControls[]
    }

const insightsReducer = (
  state: InsightsState,
  action: InsightsAction
): InsightsState => {
  switch (action.type) {
    case InsightsActionType.onLoadedInsights:
      return {
        ...state,
        insights: action.insights,
        impactScoreSummary: action.impactScoreSummary,
      }
    case InsightsActionType.updateFilterSpecifications: {
      const updatedSpecs = (action.filterSpecUpdates ?? [])
        .map(({ specName, specValue, specOperator }) => ({
          field: specName,
          operator: specOperator,
          value: specValue,
        }))
        .filter(
          (
            s
          ): s is {
            field: string
            operator: FilterOperator
            value: string | string[]
          } =>
            (s != null && s.operator === FilterOperator.isNull) ||
            (s != null && s.value != null && s.value.length > 0)
        )

      return {
        ...state,
        filterSpecifications: _.sortBy(updatedSpecs, 'field'),
      }
    }
    case InsightsActionType.updateSortSpecifications: {
      return {
        ...state,
        /**
         * Toggle the sorting for priority column to display correct sorting icons in the table header
         * since we have the logic to convert the insight priority to low, medium, high or critical
         */
        sortSpecifications: action.sortSpecifications.map((spec) => ({
          sort:
            spec.field === 'priority'
              ? spec.sort === 'asc'
                ? 'desc'
                : 'asc'
              : spec.sort,
          field: sortMap.get(spec.field) ?? spec.field,
        })),
      }
    }
    case InsightsActionType.onLoadedAssetInsights:
      return {
        ...state,
        assetInsights: state.assetInsights,
      }
    case InsightsActionType.onLoadedInsightTypes:
      return {
        ...state,
        insightTypesGroupedByDate: action.insightTypesGroupedByDate,
        cards: action.cards,
        impactScoreSummary: action.impactScoreSummary,
      }
    case InsightsActionType.insightCardSummaryFilterChange:
      return {
        ...state,
        filterSpecifications:
          action.filterName === 'status'
            ? state.filterSpecifications.map((spec) =>
                spec.field === 'status'
                  ? { ...spec, value: action.filterValue }
                  : spec
              )
            : state.filterSpecifications,
      }
    case InsightsActionType.selectInsight:
      return {
        ...state,
        selectedInsight: action.selectedInsight,
      }
    case InsightsActionType.resetInsight:
      return {
        ...state,
        selectedInsight: undefined,
      }
    case InsightsActionType.onLoadedInsightFilters:
      return {
        ...state,
        cardSummaryFilters: action.filters,
      }
    case InsightsActionType.resetInsightIds:
      return {
        ...state,
        selectedInsightIds: [],
      }
    case InsightsActionType.selectInsightIds:
      return {
        ...state,
        selectedInsightIds: action.selectedInsightIds,
      }
    case InsightsActionType.onUpdateTotalImpact: {
      /**
       *  If Rollup Widgets are checked, they are added to nextIncludedRollups.
       *  nextIncludedRollups is used for analytics tracking of TableViewControls.
       */
      const { excludedRollups = [], rollupControls } = state
      const nextExcludedRollups = action.showTotalImpact
        ? [...excludedRollups]
        : [...excludedRollups, InsightTableControls.showTotalImpactToDate]
      return {
        ...state,
        showTotalImpact: action.showTotalImpact,
        nextIncludedRollups: rollupControls
          .filter(({ control }) => !nextExcludedRollups.includes(control))
          .map(({ control }) => control),
      }
    }
    case InsightsActionType.onUpdateIncludedRollups:
      return {
        ...state,
        nextIncludedRollups: action.nextIncludedRollups,
      }
    default:
      return state
  }
}

export default insightsReducer

const asc: GridSortDirection = 'asc'
const desc: GridSortDirection = 'desc'
