import axios from 'axios'
import { getUrl, api } from '@willow/ui'

export type Attachment = {
  id: string
  type: 'image' | 'file'
  fileName?: string
  createdDate: string
  previewUrl?: string
  url?: string
}

export type CheckRecord = {
  id: string
  inspectionId: string
  checkId: string
  checkType?: string
  inspectionRecordId: string
  status: CheckRecordStatus
  submittedUserId?: string
  submittedDate?: string
  submittedSiteLocalDate?: string
  enteredBy?: string
  numberValue?: number
  typeValue?: string
  stringValue?: string
  dateValue?: string
  effectiveDate: string
  notes?: string
  attachments?: Attachment[]
}

type InspectionCheckStatistics = {
  checkRecordCount: number
  lastCheckSubmittedEntry?: string
  lastCheckSubmittedUserId?: string
  lastCheckSubmittedDate?: string
  workableCheckStatus: CheckRecordStatus
  nextCheckRecordDueTime?: string
  lastCheckSubmittedUserName?: string
}

export type Check = {
  id: string
  inspectionId: string
  name?: string
  type?: string
  typeValue?: string
  decimalPlaces: number
  minValue?: number
  maxValue?: number
  dependencyId?: string
  dependencyValue?: string
  pauseStartDate?: string
  pauseEndDate?: string
  isArchived: boolean
  isPaused: boolean
  canGenerateInsight: boolean
  lastSubmittedRecord?: CheckRecord
  statistics: InspectionCheckStatistics
}

export enum CheckRecordStatus {
  Due = 'due',
  Overdue = 'overdue',
  Completed = 'completed',
  Missed = 'missed',
  NotRequired = 'notRequired',
}

export interface Inspection {
  id: string
  name?: string
  siteId: string
  zoneId: string
  floorCode?: string
  assetId: string
  assignedWorkgroupId: string
  frequency: number
  unit: string
  nextEffectiveDate?: string
  startDate?: string
  endDate?: string
  sortOrder: number
  checks?: Check[]
  checkRecordCount: number
  workableCheckCount: number
  completedCheckCount: number
  nextCheckRecordDueTime?: string
  assignedWorkgroupName?: string
  zoneName?: string
  assetName?: string
  checkRecordSummaryStatus: CheckRecordStatus
  isSiteAdmin?: boolean
  status?: string
}

export type InspectionZone = {
  id: string
  siteId: string
  checkCount?: number
  lastUpdated?: string
  statistics: InspectionCheckStatistics
  inspections?: Inspection[]
  inspectionCount?: number
}

export type InspectionsResponse = Inspection[]

export function getInspections(siteId: string): Promise<InspectionsResponse> {
  const getInspectionsUrl = getUrl(`/api/sites/${siteId}/inspections`)
  return axios.get(getInspectionsUrl).then(({ data }) => data)
}

export function getScopedInspections(
  scopeId?: string
): Promise<InspectionsResponse> {
  return api
    .get(`/inspections${scopeId ? `?scopeId=${scopeId}` : ''}`)
    .then(({ data }) => data)
}
