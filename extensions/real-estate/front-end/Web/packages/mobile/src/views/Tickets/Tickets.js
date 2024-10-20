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

import TicketFilter from './TicketFilter'
import TicketItem from './TicketItem'
import EmptyTicket from './EmptyTicket'
import styles from './Tickets.css'
import { useTickets } from '../../providers/TicketsProvider'

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

export default function Tickets() {
  const { siteId, tab } = useParams()
  const { isExiting } = useAnimationTransition()
  const { setShowBackButton } = useLayout()
  const [filteredStatusCode, setFilteredStatusCode] = useState(undefined)
  const { getTickets } = useTickets()
  const { data } = getTickets(siteId, tab)

  const hasFilter = TicketTab[tab.toLowerCase()] === TicketTab.open

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
                  const targetTab = site.id === siteId ? tab : TicketTab.open
                  return `/tickets/sites/${site.id}/${targetTab}`
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
            to={`/tickets/sites/${siteId}/${TicketTab.open}`}
            className={styles.tab}
          />
          <Tab
            header="Completed"
            to={`/tickets/sites/${siteId}/${TicketTab.resolved}`}
            className={styles.tab}
          />
          <Tab
            header="Closed"
            to={`/tickets/sites/${siteId}/${TicketTab.closed}`}
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
