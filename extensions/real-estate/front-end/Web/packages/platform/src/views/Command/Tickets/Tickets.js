/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import { capitalize } from 'lodash'
import { useSite, useSites } from 'providers'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Redirect, Route, Switch, useLocation, useParams } from 'react-router'
import { Link } from 'react-router-dom'
import { styled } from 'twin.macro'

import { FullSizeLoader, siteAdminUserRole, titleCase } from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import { NavTab, NavTabs, DocumentTitle, useScopeSelector } from '@willow/ui'
import { PageTitle, PageTitleItem } from '@willowinc/ui'

import TicketsNew from 'components/TicketsNew/Tickets'
import { TicketsControls } from 'components/TicketsNew/TicketsTable'
import HeaderWithTabs from 'views/Layout/Layout/HeaderWithTabs.tsx'
import DisabledWarning from '../../../components/DisabledWarning/DisabledWarning.tsx'
import routes from '../../../routes'
import useCommandAnalytics from '../useCommandAnalytics.ts'
import Schedules, { SchedulesControls } from './Schedules/Schedules'

const defaultTab = TicketTab.open

/**
 * Wrapped Tickets table for Standard Tickets and Scheduled Tickets
 * that display the tabs "Open", "Resolved" and "Closed" and the
 * list of tickets in the selected tab.
 */
function TicketsTable({ tab, onTabChange, isScheduled = false }) {
  const params = useParams()

  const commandAnalytics = useCommandAnalytics(params.siteId)
  useEffect(() => {
    // Landing page analytics
    commandAnalytics.pageTickets(isScheduled ? 'scheduled' : 'standard')
  }, [commandAnalytics, isScheduled])

  useEffect(() => {
    // Page analytics for the selected tab
    commandAnalytics.pageTickets(isScheduled ? 'scheduled' : 'standard', tab)
  }, [commandAnalytics, tab, isScheduled])

  return (
    <TicketsNew
      siteId={params.siteId}
      showSite={false}
      showHeader={!isScheduled}
      isScheduled={isScheduled}
      tab={tab}
      onTabChange={onTabChange}
    />
  )
}

// eslint-disable-next-line complexity
export default function TicketsComponent() {
  const {
    descendantSiteIds,
    isScopeSelectorEnabled,
    location,
    scopeLookup,
    isScopeUsedAsBuilding,
    twinQuery,
    locationName,
  } = useScopeSelector()
  const scopeId = location?.twin?.id
  const { siteId, ticketId } = useParams()
  const { pathname: path } = useLocation()
  const site = useSite()
  const allSites = useSites()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const isScopeBuildingTwin = isScopeUsedAsBuilding(location)
  // there exists cases where site scope returned by scope selector endpoint
  // that isn't returned by the sites endpoint, so we need to filter out those
  // to be defensive
  const sitesBasedOnScope = isScopeBuildingTwin
    ? allSites.filter((s) => s.id === location?.twin?.siteId)
    : (descendantSiteIds ?? [])
        .map((id) => allSites.find((s) => s.id === id))
        .filter((s) => s)
  // when scope selector is on, we check whether the current scope is a building twin
  // and if so, return array of sites that match the building twin's siteId, otherwise
  // include all sites that are descendants of the current scope
  const sites = isScopeSelectorEnabled
    ? sitesBasedOnScope
    : allSites.find((s) => s.id === siteId)
    ? [site]
    : allSites
  // to be used for redirecting to the correct scope
  const scopeIdBasedOnSiteId = isScopeSelectorEnabled
    ? scopeLookup[siteId]?.twin?.id
    : undefined

  const isScheduledTicketsEnabled = sites.some(
    (s) =>
      s.features.isScheduledTicketsEnabled && !s.features.isTicketingDisabled
  )
  const [{ tab = defaultTab }, setSearchParams] = useMultipleSearchParams([
    'tab',
  ])

  const handleTabChange = (nextTab) => {
    if (nextTab !== tab) {
      setSearchParams({ tab: nextTab })
    }
  }

  if (twinQuery.isLoading) {
    return <FullSizeLoader />
  }

  if (sites.every((s) => s.features.isTicketingDisabled)) {
    return <DisabledWarning title={t('plainText.ticketsDisabled')} />
  }

  // as per legacy business logic, we only enabled schedules for individual site/building twin
  const isSchedulesDisabled =
    (isScopeSelectorEnabled ? !isScopeBuildingTwin : siteId == null) ||
    !isScheduledTicketsEnabled

  const isScopeDefined = isScopeSelectorEnabled && !!scopeId

  const tabsText = {
    [standard]: {
      label: t('plainText.standardTickets'),
      pageTitle: t('plainText.standard'),
    },
    [scheduledTickets]: {
      label: t('headers.scheduledTickets'),
      pageTitle: t('plainText.scheduled'),
    },
    [schedules]: {
      label: t('headers.schedules'),
      pageTitle: t('headers.schedules'),
    },
  }
  const currentTab = path.includes(scheduledTickets)
    ? scheduledTickets
    : path.includes(schedules)
    ? schedules
    : standard

  return (
    <>
      <DocumentTitle
        scopes={[
          titleCase({ text: tabsText[currentTab].pageTitle, language }),
          t('headers.tickets'),
          locationName,
        ]}
      />

      <HeaderWithTabs
        titleRow={[
          <PageTitle key="pageTitle">
            {[
              {
                text: capitalize(t('headers.tickets')),
                to: window.location.pathname,
              },
            ].map(({ text, to }) => (
              <PageTitleItem key={text}>
                {to ? <Link to={to}>{text}</Link> : text}
              </PageTitleItem>
            ))}
          </PageTitle>,
        ]}
        tabs={
          <NavTabs
            value={currentTab}
            tabs={[
              <NavTab
                data-testid="tickets-sub-menu-standard-tickets"
                to={
                  isScopeDefined
                    ? routes.tickets_scope__scopeId(scopeId)
                    : siteId
                    ? routes.sites__siteId_tickets(siteId)
                    : routes.tickets
                }
                value={standard}
              >
                {tabsText[standard].label}
              </NavTab>,

              <NavTab
                data-testid="tickets-sub-menu-scheduled-tickets"
                to={
                  isScopeDefined
                    ? routes.tickets_scope__scopeId_scheduled(scopeId)
                    : siteId
                    ? routes.sites__siteId_tickets_scheduled(siteId)
                    : routes.tickets_scheduled
                }
                value={scheduledTickets}
                disabled={!isScheduledTicketsEnabled}
              >
                {tabsText[scheduledTickets].label}
              </NavTab>,
              // Never show this tab for non admin
              ...(site.userRole === siteAdminUserRole
                ? [
                    <NavTab
                      data-testid="tickets-sub-menu-schedules"
                      to={
                        isScopeDefined
                          ? routes.tickets_scope__scopeId_schedules(scopeId)
                          : siteId
                          ? routes.sites__siteId_tickets_schedules(siteId)
                          : routes.tickets_schedules
                      }
                      value={schedules}
                      disabled={isSchedulesDisabled}
                    >
                      {tabsText[schedules].label}
                    </NavTab>,
                  ]
                : []),
            ]}
          />
        }
        controlsOnTabs={
          <ButtonsContainer>
            {path.includes(schedules) ? (
              <SchedulesControls
                siteId={
                  isScopeSelectorEnabled && isScopeBuildingTwin
                    ? location?.twin?.siteId
                    : siteId
                }
              />
            ) : path.includes(scheduledTickets) ? (
              <TicketsControls isScheduled />
            ) : (
              <TicketsControls isScheduled={false} />
            )}
          </ButtonsContainer>
        }
      />
      <Switch>
        {isScheduledTicketsEnabled && (
          <Route
            // scheduled tickets for one site with ticket modal opened
            path={[
              routes.sites__siteId_tickets_scheduled__ticketId(),
              routes.tickets_scope__scopeId_scheduled__ticketId(),
            ]}
            children={
              <ScheduledTicketsTable
                tab={tab}
                onTabChange={handleTabChange}
                redirect={
                  scopeIdBasedOnSiteId
                    ? routes.tickets_scope__scopeId_scheduled__ticketId(
                        scopeIdBasedOnSiteId,
                        ticketId
                      )
                    : null
                }
              />
            }
          />
        )}
        {isScheduledTicketsEnabled && (
          <Route
            // scheduled tickets for one site
            path={[
              routes.sites__siteId_tickets_scheduled(),
              routes.tickets_scope__scopeId_scheduled(),
            ]}
            children={
              <ScheduledTicketsTable
                tab={tab}
                onTabChange={handleTabChange}
                redirect={
                  scopeIdBasedOnSiteId
                    ? routes.tickets_scope__scopeId_scheduled(
                        scopeIdBasedOnSiteId
                      )
                    : null
                }
              />
            }
          />
        )}
        {isScheduledTicketsEnabled && site.userRole === siteAdminUserRole && (
          <Route
            // ticket schedules for one site
            path={[
              routes.sites__siteId_tickets_schedules(),
              routes.tickets_scope__scopeId_schedules(),
            ]}
            children={
              <Schedules
                redirect={
                  scopeIdBasedOnSiteId
                    ? routes.tickets_scope__scopeId_schedules(
                        scopeIdBasedOnSiteId
                      )
                    : null
                }
              />
            }
          />
        )}
        <Route
          // standard tickets for one site
          path={[
            routes.sites__siteId_tickets(),
            routes.tickets_scope__scopeId(),
          ]}
          exact
          children={
            <StandardTicketsTable
              tab={tab}
              onTabChange={handleTabChange}
              redirect={
                scopeIdBasedOnSiteId
                  ? routes.tickets_scope__scopeId(scopeIdBasedOnSiteId)
                  : null
              }
            />
          }
        />

        {isScheduledTicketsEnabled && (
          <Route
            // scheduled tickets for all sites with ticket modal opened
            path={routes.tickets_scheduled__ticketId()}
            children={
              <ScheduledTicketsTable tab={tab} onTabChange={handleTabChange} />
            }
          />
        )}
        {isScheduledTicketsEnabled && (
          // scheduled tickets for all sites
          <Route
            path={routes.tickets_scheduled}
            children={
              <ScheduledTicketsTable tab={tab} onTabChange={handleTabChange} />
            }
          />
        )}
        {isScheduledTicketsEnabled && site.userRole === siteAdminUserRole && (
          <Route
            path={[
              // ticket schedules for all sites
              routes.tickets_schedules,
              // will never reach below route, as it's capture by above route
              routes.tickets_scheduled__ticketId(),
            ]}
            children={<Schedules />}
          />
        )}
        <Route
          // standard tickets for all sites with/without ticket modal opened
          path={[
            routes.tickets,
            routes.tickets_ticketId(),
            routes.tickets_ticket__ticketId(),
          ]}
          exact
          children={
            <StandardTicketsTable tab={tab} onTabChange={handleTabChange} />
          }
        />

        <Route
          // standard tickets for one site with ticket modal opened
          path={[
            routes.sites__siteId_tickets__ticketId(),
            routes.tickets_scope__scopeId_ticket__ticketId(),
          ]}
          children={
            <StandardTicketsTable
              tab={tab}
              onTabChange={handleTabChange}
              redirect={
                scopeIdBasedOnSiteId
                  ? routes.tickets_scope__scopeId_ticket__ticketId(
                      scopeIdBasedOnSiteId,
                      ticketId
                    )
                  : null
              }
            />
          }
        />
      </Switch>
    </>
  )
}

const ButtonsContainer = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s8,
}))

const scheduledTickets = 'scheduled-tickets'
const schedules = 'schedules'
const standard = 'standard'

const StandardTicketsTable = ({ tab, onTabChange, redirect }) => {
  if (redirect) {
    return <Redirect to={redirect} />
  }
  return (
    <TicketsTable key="standardTickets" tab={tab} onTabChange={onTabChange} />
  )
}

const ScheduledTicketsTable = ({ tab, onTabChange, redirect }) => {
  if (redirect) {
    return <Redirect to={redirect} />
  }
  return (
    <TicketsTable
      key="scheduledTickets"
      isScheduled
      tab={tab}
      onTabChange={onTabChange}
    />
  )
}
