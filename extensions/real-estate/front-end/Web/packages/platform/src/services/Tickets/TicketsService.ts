import axios from 'axios'
import { getUrl } from '@willow/ui'
import { Tab as TicketTab } from '@willow/common/ticketStatus'

export type TicketAssigneeType = 'noAssignee' | 'customerUser' | 'workGroup'

export type IssueType = 'noIssue' | 'equipment' | 'asset'

export type TicketTaskType = 'numberic' | 'checkbox'

export type TicketTaskDto = {
  id: string
  taskName?: string
  type: TicketTaskType
  isComplete: boolean
  numberValue?: number
  decimalPlaces?: number
  minValue?: number
  maxValue?: number
  unit?: string
}

export interface TicketSimpleDto {
  assigneeType: TicketAssigneeType
  assigneeId?: string
  assigneeName?: string
  id: string
  siteId: string
  floorCode?: string
  sequenceNumber?: string
  priority: number
  statusCode: number
  issueType: IssueType
  issueId?: string
  issueName?: string
  insightId?: string
  insightName?: string
  summary?: string
  description?: string
  reporterName?: string
  assignedTo?: string
  dueDate?: string
  createdDate?: string
  updatedDate?: string
  resolvedDate?: string
  closedDate?: string
  categoryId?: string
  category?: string
  sourceId?: string
  sourceName?: string
  externalId?: string
  scheduledDate?: string
  tasks?: TicketTaskDto[]
  groupTotal: number
  twinId?: string
  sourceTye?: TicketSourceType
}

export enum TicketSourceType {
  willow = 'platform',
  app = 'app',
  dynamics = 'dynamics',
  mapped = 'mapped',
}

export interface ScheduleTicket extends TicketSimpleDto {
  assets?: Array<{ id: string; assetName: string; assetId: string }>
  assignee?: { id: string; name: string; type: TicketAssigneeType }
  overDueThreshold?: { unitOfMeasure: string; units: number }
  recurrence?: {
    dayOccurrences: string[]
    days: number[]
    endDate: string
    interval: number
    maxOccurrences: number
    occurs: string
    startDate: string
    timezone: string
  }
  reporterCompany: string
  reporterEmail: string
  reporterId: string
  reporterName: string
  reporterPhone: string
  sequenceNumber: string
  siteId: string
  sourceType: string
  status: string
  summary: string
  updatedDate: string
}

export type TicketsResponse = TicketSimpleDto[]

export type TicketCategoriesResponse = {
  ticketStatus: string[]
  ticketSubStatus: string[]
  priorities: string[]
  assigneeTypes: string[]
  jobTypes: string[]
  requestTypes: string[]
}

type Tab = 'all' | TicketTab
export interface TicketsParams {
  siteId: string
  assetId: string
  tab: Tab
  scheduled: boolean
  isClosed: boolean
  orderBy: string
  page: number
  pageSize: number
  'api-version': string
}

export interface AssetsTicketsHistoryParams {
  siteId: string
  assetId: string
  tab: Tab
  scheduled?: boolean
  'api-version'?: string
}

export type DependentDiagnostic = {
  id: string
  name: string
  ruleName: string
  check: boolean
  started: string
  ended: string
  diagnostics?: Array<DependentDiagnostic>
}

export type InsightDiagnosticResponse = {
  id: string
  name: string
  ruleName: string
  started: string
  ended: string
  diagnostics: Array<DependentDiagnostic>
}

/**
 * Fetch tickets.
 * When assetId is provided, fetch tickets based on asset.
 * When siteId is provided, fetch tickets for that site. Otherwise, fetch all tickets.
 */
export function getTickets(params: Partial<TicketsParams>) {
  const { siteId, assetId } = params
  const getTicketsUrl = getUrl(
    assetId && siteId
      ? `/api/sites/${siteId}/assets/${assetId}/tickets`
      : siteId
      ? `/api/sites/${siteId}/tickets`
      : `/api/tickets`
  )
  return axios.get(getTicketsUrl, { params }).then(({ data }) => data)
}

/**
 * Fetch tickets based on asset used in twin view's asset history tab.
 */
export function getAssetTicketsHistory(
  params: AssetsTicketsHistoryParams
): Promise<TicketsResponse> {
  const { siteId, assetId } = params
  const getAssetTicketsHistoryUrl = getUrl(
    `/api/sites/${siteId}/assets/${assetId}/tickets/history`
  )

  return axios
    .get(getAssetTicketsHistoryUrl, { params })
    .then(({ data }) => data)
}

/**
 * Fetch ticket categories data for creating or updating existing ticket.
 */
export function getTicketCategories(): Promise<TicketCategoriesResponse> {
  const getCategoriesUrl = getUrl(`/api/tickets/ticketCategoricalData`)

  return axios.get(getCategoriesUrl).then(({ data }) => data)
}
