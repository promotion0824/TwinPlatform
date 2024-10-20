import { ReactElement } from 'react'
import { Item } from '../AssetDetailsModal'

export default function TicketModal(props: {
  siteId: string
  ticketId: string
  ticket?: Item
  onClose: () => void
  showNavigationButtons: boolean
  onPreviousItem: () => void
  onNextItem: () => void
  dataSegmentPropPage: string
  isTicketUpdated?: boolean
  insightId?: string
  insightName?: string
  insightRuleName?: string
  selectedInsight?: {
    id: Insight['id']
    name: Insight['name']
    ruleName: Insight['ruleName']
  }
}): ReactElement
