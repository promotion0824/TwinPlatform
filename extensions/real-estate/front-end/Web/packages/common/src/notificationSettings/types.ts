import { InsightType } from '../insights/insights/types'
import { PriorityIds } from '../Priority'

export enum NotificationType {
  settings = 'settings',
  addNotification = 'addNotification',
}

export type Workgroup = {
  id: string
  name: string
  selected?: boolean
  siteId: string
  memberIds?: string[]
}

export type MyWorkgroup = Pick<Workgroup, 'id' | 'name'>

export enum ModalType {
  workgroup = 'workgroup',
  location = 'location',
  categories = 'categories',
  skill = 'skill',
  twin = 'twin',
  twinCategory = 'twinCategory',
  locationReport = 'locationReport',
}

export enum Focus {
  skillCategory = 'skillCategory',
  skill = 'skill',
  twin = 'twin',
  twinCategory = 'twinCategory',
}

export enum Segment {
  personal = 'personal',
  workgroup = 'workgroup',
}

export const ALL_CATEGORIES = 'All Categories'

export type Skill = { id: string; name: string; category: InsightType }

export type Twin = {
  id: string
  name: string
  modelId: string
}

export type CreateNotification = {
  id?: string
  type: Segment
  source: string
  focus: Focus
  locations?: string[]
  isEnabled: boolean
  createdBy: string
  canUserDisableNotification: boolean
  workGroupIds?: string[]
  skillCategories?: number[]
  twinCategoryIds?: string[]
  twins?: Array<{ twinId: string; twinName: string; modelId: string }>
  skillIds?: string[]
  priorityIds?: PriorityIds[]
  channels?: string[]
}

export type NotificationTrigger = {
  id: string
  type: Segment
  source: string
  focus: Focus
  isEnabled: boolean
  isMuted: boolean
  isEnabledForUser: boolean
  canUserDisableNotification: boolean
  subscriberId: string
  createdBy: string
  createdDate: string
  updatedBy: string
  updatedDate: string
  channels?: string[]
  priorities: string[]
  IsDefault: boolean
  derrivedFrom: string
  categories?: InsightType[]
  locations?: string[]
  skills?: string[]
  twins?: Array<{ twinId: string; twinName: string; modelId: string }>
  skillIds?: string[]
  skillCategoryIds?: number[]
  workgroupIds?: string[]
  priorityIds?: PriorityIds[]
  twinCategoryIds?: string[]
}

export enum NotificationActions {
  edit = 'edit',
  delete = 'delete',
  view = 'view',
}
