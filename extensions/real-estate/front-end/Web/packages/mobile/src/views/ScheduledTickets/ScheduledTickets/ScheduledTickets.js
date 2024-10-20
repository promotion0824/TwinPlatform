import { Fragment, useState } from 'react'
import { useParams } from 'react-router'
import cx from 'classnames'
import { native } from 'utils'
import { Tabs, Tab, Spacing } from '@willow/mobile-ui'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import { useLayout, useAnimationTransition } from 'providers'
import AnimationTransitionGroup from 'components/AnimationTransitionGroup/AnimationTransitionGroup'
import List from 'components/List/List'
import SitesSelect from 'components/SitesSelect/SitesSelect'
import { LayoutHeader } from 'views/Layout/Layout'

import TicketFilter from '../../Tickets/TicketFilter'
import TicketItem from './ScheduledTicketItem'
import EmptyTicket from './EmptyTicket'
import styles from './ScheduledTickets.css'
import { useTickets } from '../../../providers/TicketsProvider'

const animationDefinitions = [
  {
    expression: 'open',
    value: 100,
  },
  {
    expression: 'resolved',
    value: 200,
  },
  {
    expression: 'closed',
    value: 300,
  },
]

export default function ScheduledTickets() {
  const { siteId, tab } = useParams()
  const { isExiting } = useAnimationTransition()
  const { setShowBackButton } = useLayout()
  // Selected filter
  const [filteredStatusCode, setFilteredStatusCode] = useState(undefined)

  const { getScheduledTickets } = useTickets()
  const { data } = getScheduledTickets(siteId, tab)

  const hasFilter = tab === TicketTab.open

  const tickets =
    hasFilter && filteredStatusCode
      ? data?.filter((item) => item.statusCode === filteredStatusCode)
      : data

  const cxHeaderClassName = cx(styles.header, {
    [styles.hasFilter]: hasFilter,
  })

  setShowBackButton(false)

  return (
    <Fragment key={siteId}>
      {!isExiting && (
        <LayoutHeader className={styles.headerRoot} type="content" width="100%">
          <div className={cxHeaderClassName}>
            <div className={styles.siteWrap}>
              <SitesSelect
                to={(site) => {
                  const { isScheduledTicketsEnabled } = site.features
                  const targetTab = site.id === siteId ? tab : TicketTab.open
                  return isScheduledTicketsEnabled
                    ? `/sites/${site.id}/scheduled-tickets/${targetTab}`
                    : `/tickets/sites/${site.id}/${TicketTab.open}`
                }}
              />
            </div>
            {hasFilter && (
              <TicketFilter
                className={styles.filter}
                value={filteredStatusCode}
                onTicketStatusChanged={setFilteredStatusCode}
              />
            )}
          </div>
        </LayoutHeader>
      )}
      <Spacing type="content">
        <Tabs type="mobile">
          <Tab
            header="Open"
            to={`/sites/${siteId}/scheduled-tickets/${TicketTab.open}`}
            className={styles.tab}
          />
          <Tab
            header="Resolved"
            to={`/sites/${siteId}/scheduled-tickets/${TicketTab.resolved}`}
            className={styles.tab}
          />
          <Tab
            header="Closed"
            to={`/sites/${siteId}/scheduled-tickets/${TicketTab.closed}`}
            className={styles.tab}
          />
        </Tabs>
        <AnimationTransitionGroup
          definitions={animationDefinitions}
          transitionKey={tab}
          onScroll={(e) => native.scroll(e.target.scrollTop)}
        >
          <List
            horizontal
            stretchColumn
            activeIndex={-1}
            data={tickets}
            itemsPreColumn={3}
            ListItem={TicketItem}
            Placeholder={EmptyTicket}
          />
        </AnimationTransitionGroup>
      </Spacing>
    </Fragment>
  )
}
