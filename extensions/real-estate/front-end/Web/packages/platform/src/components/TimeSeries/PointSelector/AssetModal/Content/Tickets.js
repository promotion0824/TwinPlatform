import TicketsTable from '../../../../TicketsNew/TicketsTable'
import TicketsProvider from '../../../../TicketsNew/TicketsProvider'

export default function Tickets({
  assetId,
  siteId,
  selectedTicketId,
  onSelectedTicketIdChange,
}) {
  return (
    <TicketsProvider
      siteId={siteId}
      assetId={assetId}
      selectedTicketId={selectedTicketId}
      dataSegmentPropsPage="Time Series Page"
    >
      <TicketsTable onSelectedTicketIdChange={onSelectedTicketIdChange} />
    </TicketsProvider>
  )
}
