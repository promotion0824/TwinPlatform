import { BadgeProps } from '@willowinc/ui'

export enum Status {
  open = 'Open',
  inProgress = 'InProgress',
  limitedAvailability = 'LimitedAvailability',
  onHold = 'OnHold',
  resolved = 'Resolved',
  closed = 'Closed',
  reassign = 'Reassign',
}

export enum Tab {
  open = 'Open',
  closed = 'Closed',
  resolved = 'Resolved',
}

export type TicketStatus = {
  customerId: string
  status: Status
  tab: Tab
  color: BadgeProps['color']
  statusCode: number
}

export enum SyncStatus {
  Failed = 'Failed',
  InProgress = 'InProgress',
  Completed = 'Completed',
}
