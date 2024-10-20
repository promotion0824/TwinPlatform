import { useParams } from 'react-router'
import TicketsTable from '../../../../../../../components/TicketsNew/TicketsTable'
import TicketsProvider from '../../../../../../../components/TicketsNew/TicketsProvider'

export default function InProgress({ assetId, ticketId, onTicketIdChange }) {
  const params = useParams()

  return (
    <TicketsProvider
      siteId={params.siteId}
      assetId={assetId}
      selectedTicketId={ticketId}
      dataSegmentPropsPage="Asset Details"
    >
      <TicketsTable onSelectedTicketIdChange={onTicketIdChange} />
    </TicketsProvider>
  )
}
