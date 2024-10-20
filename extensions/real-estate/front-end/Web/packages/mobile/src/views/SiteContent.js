import { useEffect, useMemo, useState } from 'react'
import { Switch, Route, Redirect, useLocation } from 'react-router'
import { TicketStatusesProvider } from '@willow/common'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import {
  NotFound,
  useAnalytics,
  useApi,
  useFeatureFlag,
  useUser,
} from '@willow/mobile-ui'
import { useLayout } from 'providers'
import AnimationTransitionGroup from 'components/AnimationTransitionGroup/AnimationTransitionGroup'
import { native } from '../utils'
import Tickets from './Tickets/Tickets'
import Ticket from './Ticket/Ticket'
import TicketAction from './TicketAction/TicketAction'
import TicketReject from './TicketReject/TicketReject'
import TicketReassign from './TicketReassign/TicketReassign'
import TicketReassignNew from './TicketReassign/TicketReassignNew'
import Floors from './Floors/Floors'
import Inspections from './Inspections/Inspections'
import ScheduledTickets from './ScheduledTickets/ScheduledTickets'
import InspectionsApi from './Inspections/InspectionChecksList/api'
import { IndexedDbStore } from './Inspections/InspectionChecksList/InspectionRecordsDb'
import { InspectionRecordsProvider } from './Inspections/InspectionChecksList/InspectionRecordsContext'

const standardTicketsPath =
  /^\/tickets\/sites\/[\w-]+\/(open|resolved|closed)$/i
const scheduledTicketsPath =
  /^\/sites\/[\w-]+\/scheduled-tickets\/(open|resolved|closed)$/i

const standardTicketsDefinitions = [
  {
    exact: true,
    expression: '/',
    value: 100,
  },
  {
    expression: standardTicketsPath,
    value: 200,
  },
  {
    expression: /^\/tickets\/sites\/[\w-]+\/view\//,
    value: 300,
  },
  {
    expression: /^\/tickets\/sites\/[\w-]+\/action\//,
    value: 400,
  },
  {
    expression: /^\/tickets\/sites\/[\w-]+\/reject\//,
    value: 500,
  },
]
const scheduledTicketsDefinitions = [
  {
    expression: scheduledTicketsPath,
    value: 200,
  },
  {
    expression: /^\/sites\/[\w-]+\/scheduled-tickets\/view\//,
    value: 300,
  },
  {
    expression: /^\/tickets\/sites\/scheduled-tickets\/[\w-]+\/action\//,
    value: 400,
  },
  {
    expression: /^\/tickets\/sites\/scheduled-tickets\/[\w-]+\/reject\//,
    value: 500,
  },
]
const animationDefinitions = [
  ...standardTicketsDefinitions,
  ...scheduledTicketsDefinitions,
]

export default function SiteContent() {
  const { site } = useLayout()
  const location = useLocation()
  const user = useUser()
  const api = useApi()
  const analytics = useAnalytics()

  const transitionKey = useMemo(() => {
    const { pathname } = location

    if (standardTicketsPath.test(pathname)) {
      return `/tickets/sites/${site.id}/${TicketTab.open}`
    }
    if (scheduledTicketsPath.test(pathname)) {
      return `/sites/${site.id}/scheduled-tickets/${TicketTab.open}`
    }

    return pathname
  }, [location.pathname, site])

  const isTicketWorkgroupsEnabled = true

  const TicketReassignComponent = isTicketWorkgroupsEnabled
    ? TicketReassignNew
    : TicketReassign

  const customerId = user.isAuthenticated ? user.customerId : undefined

  useEffect(() => {
    if (user.isAuthenticated && !customerId) {
      analytics.track('CustomerId not present')
    }
  }, [customerId, user.isAuthenticated])

  return (
    <TicketStatusesProvider
      customerId={customerId}
      getTicketStatuses={(cusId) =>
        api.get(`/api/customers/${cusId}/ticketStatuses`)
      }
    >
      <InspectionRecordsProviderWrapper>
        <AnimationTransitionGroup
          definitions={animationDefinitions}
          transitionKey={transitionKey}
          // I don't really know what this does, but the other
          // `AnimationTransitionGroup`s have it, and without it, scrolling does not
          // work properly. For example, on some pages you can scroll down but not
          // up.
          onScroll={(e) => native.scroll(e.target.scrollTop)}
        >
          <Switch>
            <Route path={['/', '/tickets']} exact>
              <Redirect to={`/tickets/sites/${site.id}/${TicketTab.open}`} />
            </Route>
            <Route
              path="/tickets/sites/:siteId/:tab"
              exact
              children={<Tickets />}
            />
            <Route
              path="/tickets/sites/:siteId/view/:ticketId"
              exact
              children={<Ticket />}
            />
            <Route
              path="/tickets/sites/:siteId/action/:ticketId"
              children={<TicketAction />}
            />
            <Route
              path="/tickets/sites/:siteId/reject/:ticketId"
              children={<TicketReject />}
            />
            <Route
              path="/tickets/sites/:siteId/reassign/:ticketId"
              children={<TicketReassignComponent />}
            />
            <Route path="/sites/:siteId/floors" children={<Floors />} />
            <Route
              path="/sites/:siteId/inspectionZones"
              children={<Inspections />}
            />
            <Route
              path="/sites/:siteId/scheduled-tickets"
              children={<ScheduledTickets />}
            />
            <Route>
              <NotFound>Page not found</NotFound>
            </Route>
          </Switch>
        </AnimationTransitionGroup>
      </InspectionRecordsProviderWrapper>
    </TicketStatusesProvider>
  )
}

/**
 * There is some subtle issue with AnimationTransitionGroup whereby the
 * animation into the inspection checks list page does not work when the
 * InspectionRecordsProvider is inside the AnimationTransitionGroup. We have
 * not got to the bottom of this, and so we work around the problem by putting
 * a wrapper provider outside the AnimationTransitionGroup. This wrapper
 * provider does nothing if the inspectionsOfflineMode feature flag is not
 * enabled. The Inspections component knows not to use anything that requires
 * the InspectionRecordsProvider if that feature flag is not enabled.
 */
function InspectionRecordsProviderWrapper({ children }) {
  const featureFlags = useFeatureFlag()

  if (!featureFlags.isLoaded) {
    return null
  }

  if (featureFlags.hasFeatureToggle('inspectionsOfflineMode')) {
    return (
      <InspectionRecordsProviderWrapperInner>
        {children}
      </InspectionRecordsProviderWrapperInner>
    )
  } else {
    return children
  }
}

function InspectionRecordsProviderWrapperInner({ children }) {
  const { site } = useLayout()
  const api = useApi()
  const inspectionsApi = useMemo(() => new InspectionsApi(api), [api])
  const [store, setStore] = useState()

  useEffect(() => {
    async function setup() {
      setStore(await IndexedDbStore.create('inspections'))
    }
    setup()
  }, [])

  if (store == null) {
    return null
  }

  return (
    <InspectionRecordsProvider
      siteId={site.id}
      api={inspectionsApi}
      store={store}
    >
      {children}
    </InspectionRecordsProvider>
  )
}
