import { Fragment } from 'react'
import { Switch, Route, useParams } from 'react-router'
/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import ScheduledTickets from './ScheduledTickets/ScheduledTickets'
import ScheduledTicket from './ScheduledTicket/ScheduledTicket'
import ScheduledTicketAction from './Actions/ScheduledTicketAction/ScheduledTicketAction'
import ScheduledTicketReject from './Actions/ScheduledTicketReject/ScheduledTicketReject'
import ScheduledTicketReassignNew from './Actions/ScheduledTicketReassign/ScheduledTicketReassignNew'

export default function Inspections() {
  const { siteId } = useParams()

  const ScheduledTicketReassignComponent = ScheduledTicketReassignNew

  return (
    <Fragment key={siteId}>
      <Switch>
        <Route
          path="/sites/:siteId/scheduled-tickets/:tab"
          exact
          children={<ScheduledTickets />}
        />
        <Route
          path="/sites/:siteId/scheduled-tickets/view/:ticketId"
          exact
          children={<ScheduledTicket />}
        />
        <Route
          path="/sites/:siteId/scheduled-tickets/action/:ticketId"
          children={<ScheduledTicketAction />}
        />
        <Route
          path="/sites/:siteId/scheduled-tickets/reject/:ticketId"
          children={<ScheduledTicketReject />}
        />
        <Route
          path="/sites/:siteId/scheduled-tickets/reassign/:ticketId"
          children={<ScheduledTicketReassignComponent />}
        />
      </Switch>
    </Fragment>
  )
}
