export enum NotificationStatus {
  New = 'new',
  Open = 'cleared',
}

export enum DateProximity {
  Today = 'Today',
  Yesterday = 'Yesterday',
  Older = 'Older',
}

export enum NotificationSource {
  Insight = 'insight',
  Ticket = 'ticket',
}

export const noMoreNotificationsId = 'no-more-notifications'

export type Notification = {
  id: string
  source: NotificationSource
  sourceId?: string
  title?: string
  propertyBagJson?: string
  properties: {
    id: string
    twinId?: string
    twinName?: string
    twinModelId?: string
    category?: string
    priority: number
    modelId?: string
  }
  userId: string
  state: NotificationStatus
  createdDateTime: string
}

export type NotificationResponse = {
  after: number
  before: number
  total: number
  items: Notification[]
}

export enum NotificationFilterOperator {
  Contains = 'contains',
  EqualsLiteral = 'equals',
  ContainedIn = 'containedIn',
  GreaterThan = '>',
}
