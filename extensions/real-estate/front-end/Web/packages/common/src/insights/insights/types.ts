import { Priority as PriorityType } from '@willow/common/Priority'
import { Site } from '@willow/common/site/site/types'
import { useDateTime } from '@willow/ui'
import { IconName } from '@willowinc/ui'
import { AssetDetails } from '../../../../platform/src/services/Assets/useGetSelectedAsset'
import { TicketSimpleDto } from '../../../../platform/src/services/Tickets/TicketsService'

export type InsightPriority = 1 | 2 | 3 | 4

/**
 * this is the legacy insight status
 */
export type InsightStatus = 'open' | 'closed' | 'acknowledged' | 'inProgress'

/**
 * new insight status
 * figma reference: https://www.figma.com/file/dUfwhUC42QG7UkxGTgjv7Q/Insights-to-Action-V2?type=design&node-id=4595-89913&t=H7EAmEmgIlaOs1rD-0
 * confluence: https://willow.atlassian.net/wiki/spaces/MAR/pages/2387935430/Proposed+Insights+Status+Workflow
 */
export type InsightWorkflowStatus =
  | 'open'
  | 'inProgress'
  | 'resolved'
  | 'new'
  | 'archived'
  | 'deleted'
  | 'ignored'
  | 'readyToResolve'

export type InsightState = 'inactive' | 'active' | 'archived'

export enum SortBy {
  asc = 'asc',
  desc = 'desc',
}

export enum SourceType {
  platform = 'platform',
  app = 'app',
  inspection = 'inspection',
  inspections = 'inspections',
}

export enum SourceName {
  walmart = 'Walmart',
  inspection = 'Inspection',
  willowActivate = 'Willow Activate',
}

export type Creator = {
  name?: string
  email?: string
  company?: string
  mobile?: string
}

export type ImpactScore = { name?: string; value: number; unit?: string }

export type ImpactScoreSummary = {
  fieldId: string
  name: string
  value: number
  unit: string
}

type Cost = { unit: string; value: string }

export const insightTypes = [
  'fault',
  'energy',
  'alert',
  'note',
  'goldenStandard',
  'infrastructure',
  'integrityKpi',
  'energyKpi',
  'edgeDevice',
  'dataQuality',
  'commissioning',
  'comfort',
  'wellness',
  'calibration',
  // the following will come from rules engine
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/89416
  'diagnostic',
  'predictiveMaintenance',
  'alarm',
]
export type InsightType = typeof insightTypes[number]

export interface Insight {
  id: string
  siteId: string
  type: InsightType
  asset?: AssetDetails
  priority: InsightPriority
  status: InsightStatus
  lastStatus: InsightWorkflowStatus
  state: InsightState
  sourceType: SourceType
  occurredDate: string
  updatedDate: string
  occurrenceCount: number
  sequenceNumber?: string
  floorCode?: string
  ruleId?: string
  ruleName?: string
  equipmentId?: string
  twinId?: string
  equipmentName?: string
  name?: string
  externalId?: string
  sourceName?: string
  isSourceIdRulingEngineAppId?: boolean
  siteName?: string
  site?: Site
  cost?: Cost
  previouslyIgnored: number
  previouslyResolved: number
  PreviouslyResolvedAndIgnoredCount?: number
  subRowInsightIds?: string[]
  impactScores?: ImpactScore[]
  lastUpdatedOccurredDate?: string
  dailyAvoidableCost?: string
  totalCostToDate?: string
  dailyAvoidableEnergy?: string
  totalEnergyToDate?: string
  ticketCount?: number
  primaryModelId?: string
  createdUser?: Creator
  createdDate?: string
  description?: string
  recommendation?: string
  floorId?: string
  tickets?: TicketSimpleDto[]
  reported?: boolean
  lastResolvedDate?: string
  lastIgnoredDate?: string
}

export type Occurrence = {
  id: string
  insightId: string
  isValid: boolean
  isFaulted: boolean
  started: string
  ended: string
  text: string
}

export enum ActivityType {
  InsightActivity = 'InsightActivity',
  NewTicket = 'NewTicket',
  TicketComment = 'TicketComment',
  TicketModified = 'TicketModified',
  TicketAttachment = 'TicketAttachment',
}

export enum ActivityKey {
  Status = 'Status',
  Priority = 'Priority',
  OccurrenceCount = 'OccurrenceCount',
  PreviouslyIgnored = 'PreviouslyIgnored',
  PreviouslyResolved = 'PreviouslyResolved',
  ImpactScores = 'ImpactScores',
  AssigneeName = 'AssigneeName',
  Reason = 'Reason',
  Comments = 'Comments',
  FileName = 'FileName',
  Description = 'Description',
  DueDate = 'DueDate',
  OccurrenceStarted = 'OccurrenceStarted',
  OccurrenceEnded = 'OccurrenceEnded',
}

export type Activity = {
  key: string
  value: string
}

export type InsightWorkflowActivity = {
  activityDate: string
  sourceType: SourceType
  activities: Activity[]
  ticketId?: string
  activityType?: string
  sourceId?: string
  userId?: string
  fullName?: string
  appName?: string
  ticketSummary?: string
}

/**
 * entityId is the unique identifier as it is the property used
 * in packages\platform\src\components\MiniTimeSeries\Equipments\Point.js
 * to query about live data of the point; for a point that isn't from rules engine,
 * the entityId is the trendId, for a point that is from rules engine, the entityId is the externalId
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/85071
 */
export type PointTwinDto = {
  pointTwinId?: string
  trendId: string
  name?: string
  externalId?: string
  unit?: string
  type: PointType
  entityId?: string
  defaultOn?: boolean
}

export enum PointType {
  InsightPoint = 'InsightPoint',
  ImpactScorePoint = 'ImpactScorePoint',
  DiagnosticPoint = 'DiagnosticPoint',
}

/**
 * this is the data structure returned by /sites/:siteId/insights/:insightId/points
 */
export type InsightPointsDto = {
  insightPoints?: PointTwinDto[]
  impactScorePoints?: PointTwinDto[]
}

/**
 * To be used in InsightWorkflowTimeSeries.tsx which
 * include twinId and twinName of an twin (used to be asset or equipment).
 * impactScorePoints are points returned by rules engine and
 * insightPoints are same points on EquipmentDto
 * returned by "/sites/{siteId}/equipments/{equipmentId}"
 */
export type TimeSeriesTwinInfo = {
  twinName?: string
  twinId?: string
  isInsightPointsLoading: boolean
  insightPoints?: PointTwinDto[]
  impactScorePoints?: PointTwinDto[]
  diagnosticPoints?: Array<DiagnosticOccurrence & PointTwinDto>
  siteInsightId?: string
  diagnosticStart?: string
  diagnosticEnd?: string
  isAnyDiagnosticSelected: boolean
}

/**
 * Insight Tab Values
 */
export enum InsightTab {
  Summary = 'summary',
  Occurrences = 'occurrences',
  Activity = 'activity',
  TimeSeries = 'timeSeries',
  Actions = 'actions',
  Diagnostics = 'diagnostics',
}

export type Filter = {
  days: 7 | 30 | 365 | null
  dateTime: ReturnType<typeof useDateTime>
  priorities: PriorityType[]
  search: string
  selectedPriorities: number[]
  selectedStatuses: string[]
  selectedSources: string[]
  selectedTypes: string[]
  siteId?: string
  sites: Site[]
  statuses: string[]
  sources: string[]
  types: string[]
  modelIds: string[]
  selectedModelId?: string
}

export enum InsightCardGroups {
  ALL_INSIGHTS = 'allInsights',
  INSIGHT_TYPE = 'insightType',
}

export type EventBody = {
  includedRollups?: InsightTableControls[]
  [key: string]: unknown
}

export type Analytics = {
  track: (eventName: string, eventBody: EventBody) => void
}

export enum InsightTableControls {
  impactView = 'impactView',
  showTotalImpactToDate = 'showTotalImpactToDate',
  showSavingsToDate = 'showSavingsToDate',
  showImpactPerYear = 'showImpactPerYear',
  showTopAsset = 'showTopAsset',
  showEstimatedAvoidable = 'showEstimatedAvoidable',
  showEstimatedSavings = 'showEstimatedSavings',
}

export enum InsightView {
  card = 'card',
  list = 'list',
}

export type ViewByOptionsMap = {
  [InsightCardGroups.ALL_INSIGHTS]: {
    view: InsightView
    text: string
  }
  [InsightCardGroups.INSIGHT_TYPE]: {
    view: InsightView
    text: string
  }
}

export type CardSummaryFilters = {
  insightTypes: string[]
  sourceNames: string[]
  detailedStatus?: string[]
  activity?: string[]
  primaryModelIds?: string[]
}

export type CardSummaryRule = {
  ruleId: string
  ruleName: string
  insightType: string
  priority: InsightPriority
  insightCount: number
  lastOccurredDate: string
  impactScores?: ImpactScoreSummary[]
  sourceId: string
  sourceName: string
  recommendation?: string
  primaryModelId?: string
}

export type InsightTypesGroupedByDate = Array<{
  insightTypes: CardSummaryRule[]
  title: string
}>

export type InsightTypesDto = {
  insightTypesGroupedByDate?: InsightTypesGroupedByDate
  cards: CardSummaryRule[]
  filters: CardSummaryFilters
  impactScoreSummary: ImpactScoreSummary[]
}

export type InsightActionIcon = {
  icon: IconName
  tooltipText: string
  enabled: boolean
  onClick?: () => void
  fontSize?: string
  filled?: boolean
  isRed?: boolean
  marginBottom?: string
}

export type Region = 'au' | 'us' | 'eu'

export type ParamsDictionary = { [key: string]: string | string[] | null }

export type DiagnosticOccurrence = {
  id: string
  siteId: string
  sequenceNumber?: string
  twinId?: string
  twinName?: string
  type: Insight['type']
  priority: Insight['priority']
  lastStatus: Insight['lastStatus']
  primaryModelId?: Insight['primaryModelId']
  occurenceCount: Insight['occurrenceCount']
  ruleId?: Insight['ruleId']
  ruleName?: Insight['ruleName']
  parentId: string
  check: boolean
  hierarchy?: string[]
  occurrenceLiveData: {
    pointId: string
    pointEntityId: string
    pointName?: string
    pointType?: string
    unit?: string
    timeSeriesData?: Array<{
      start: string
      end: string
      isFaulty: boolean
    }>
  }
}

/**
 * Walmart can generate their own insights which come with ruleId of 'walmart_alert'
 */
export const WALMART_ALERT = 'walmart_alert'
