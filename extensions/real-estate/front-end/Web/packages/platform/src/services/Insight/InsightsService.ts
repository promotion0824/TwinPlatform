import axios from 'axios'
import { v4 as uuidv4 } from 'uuid'
import { getUrl } from '@willow/ui'
import {
  CardSummaryFilters,
  ImpactScoreSummary,
  Insight,
  InsightTypesDto,
  InsightWorkflowStatus,
} from '@willow/common/insights/insights/types'
import { GridSortModel } from '@willowinc/ui'

export type InsightsResponse = Insight[]

export type InsightsWithTotalCount = {
  total: number
  items: Insight[]
}

export type AllInsightsResponse = {
  insights: {
    before: number
    after: number
    items: Insight[]
    total: number
  }
  filters: CardSummaryFilters
  impactScoreSummary: ImpactScoreSummary[]
}

export type Specifications = {
  sortSpecifications?: GridSortModel
  filterSpecifications: Array<{
    field: string
    operator: FilterOperator
    value?: string | string[]
  }>
  page?: number
  pageSize?: number
}

/**
 * reference: https://willow.atlassian.net/wiki/spaces/PE/pages/2498298113/Tech+Notes+Move+towards+Server+Side+pagination+sorting+filtering+on+fetching+Insights
 */
export enum FilterOperator {
  containedIn = 'containedIn',
  contains = 'contains',
  startsWith = 'startsWith',
  endsWith = 'endsWith',
  isEmpty = 'isEmpty',
  isNotEmpty = 'isNotEmpty',
  equalsLiteral = 'equals',
  like = 'like',
  equalsShort = '=',
  notEquals = '!=',
  greaterThan = '>',
  greaterThanOrEqual = '>=',
  lessThan = '<',
  lessThanOrEqual = '<=',
  is = 'is',
  isNull = 'isNull',
}

export type InsightSnackbarsStatus = {
  status: InsightWorkflowStatus
  count: number
  id?: string
  sourceName?: string
  sourceType?: string
}

/**
 * to be used as a single filter specification for status;
 * e.g.
 * {
 *   field: 'status',
 *   operator: FilterOperator.containedIn,
 *   value: statusMap.default,
 * }
 */
export const statusMap = {
  closed: ['Resolved'],
  acknowledged: ['Ignored'],
  default: ['Open', 'InProgress', 'New', 'ReadyToResolve'],
  resolved: ['Resolved'],
  ignored: ['Ignored'],
  inactive: ['Resolved', 'Ignored'],
  readyToResolve: ['ReadyToResolve'],
}

export function fetchInsights({
  specifications,
}: {
  specifications: Specifications
}): Promise<InsightsResponse> {
  const postInsightsUrl = getUrl(`/api/insights`)

  return axios
    .post(postInsightsUrl, specifications)
    .then(({ data }) => data.items)
}

export function fetchInsightsWithCount({
  specifications,
}: {
  specifications: Specifications
}): Promise<InsightsWithTotalCount> {
  const postInsightsUrl = getUrl(`/api/insights`)

  return axios.post(postInsightsUrl, specifications).then(({ data }) => data)
}

export interface AssetInsightsHistoryParams {
  tab: 'all' | 'open' | 'acknowledged' | 'closed'
  'api-version'?: string
}

export interface InsightStatusParams {
  statusList: string[]
}

export function fetchAssetInsights({
  params,
}: {
  params?: Specifications
}): Promise<InsightsResponse> {
  const postInsightsUrl = getUrl(`/api/insights`)

  return axios.post(postInsightsUrl, params).then(({ data }) => data.items)
}

export function fetchAllInsights({
  params,
}: {
  params?: Specifications
}): Promise<AllInsightsResponse> {
  const postAllInsightsUrl = getUrl(`/api/insights/all`)

  return axios.post(postAllInsightsUrl, params).then(({ data }) => data)
}

export function fetchInsightTypes({
  params,
}: {
  params?: Specifications
}): Promise<InsightTypesDto> {
  const postInsightTypesUrl = getUrl('/api/insights/cards')

  // if ruleName empty, we are making impactScores empty array
  // Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/92289
  return axios.post(postInsightTypesUrl, params).then(({ data }) => {
    const items = data.cards.items.map((item) => ({
      ...item,
      id: uuidv4(),
      impactScores: item.ruleName ? item.impactScores : [],
      ruleName: item.ruleName ?? 'Ungrouped Insights',
      insightType: item.insightType ?? 'multiple',
    }))
    return { ...data, cards: items ?? [] }
  })
}
