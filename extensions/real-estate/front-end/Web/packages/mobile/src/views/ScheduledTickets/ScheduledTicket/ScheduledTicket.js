import { useState, useEffect } from 'react'
import { useParams } from 'react-router'
import { Loader } from '@willow/mobile-ui'
import Ticket from './Ticket'
import { useTickets } from '../../../providers/TicketsProvider'

export default function ScheduledTicket() {
  const { siteId, ticketId } = useParams()
  const { getScheduledTicket } = useTickets()
  const { data, updateCache, isFetching } = getScheduledTicket(siteId, ticketId)
  const [ticket, setTicket] = useState(data)

  useEffect(() => {
    setTicket(data)
  }, [data])

  const updateTicket = (nextTicket) => {
    setTicket(nextTicket)
    updateCache(nextTicket)
  }

  if (isFetching || !ticket) {
    return <Loader size="extraLarge" />
  }

  return <Ticket ticket={ticket} updateTicket={updateTicket} />
}
