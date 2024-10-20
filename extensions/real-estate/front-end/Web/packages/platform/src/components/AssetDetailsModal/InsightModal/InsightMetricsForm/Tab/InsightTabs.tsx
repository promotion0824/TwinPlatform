import { useState } from 'react'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useUser, Tabs, Tab, useDateTime } from '@willow/ui'
import tw, { styled } from 'twin.macro'
import {
  Insight,
  Occurrence,
  InsightWorkflowActivity,
  SortBy,
  TimeSeriesTwinInfo,
} from '@willow/common/insights/insights/types'
import { useSites } from '../../../../../providers'
import { TicketSimpleDto } from '../../../../../services/Tickets/TicketsService'
import Summary from './Summary'
import Occurrences from './Occurrences'
import Activities from './Activities'
import InsightWorkflowTimeSeries from './InsightWorkflowTimeSeries'
import { SelectedPointsProvider } from '../../../../MiniTimeSeries/index'

const InsightTabs = ({
  insight,
  setIsTicketUpdated,
  occurrences,
  activities = [],
  insightTab,
  onInsightTabChange,
  twinInfo,
}: {
  insight: InsightWithTickets
  setIsTicketUpdated: (isUpdated: boolean) => void
  activities?: InsightWorkflowActivity[]
  occurrences: Occurrence[]
  insightTab?: string
  onInsightTabChange?: (tab: string) => void
  twinInfo: TimeSeriesTwinInfo
}) => {
  const { t } = useTranslation()
  const user = useUser()

  const sites = useSites()
  const site = sites.find((s) => s.id === insight.siteId)

  const isInsightTabControlled = !!onInsightTabChange
  const [localInsightTab, setLocalInsightTab] = useState('summary')
  const dateTime = useDateTime()

  const [sortBy, setSortByChange] = useState(
    user?.options?.sortBy ?? SortBy.desc
  )

  const handleSortByChange = (option) => {
    user.saveOptions('sortBy', option)
    setSortByChange(option)
  }

  // occurrences that are either invalid or faulted are considered abnormal
  const abnormalOccurrences = occurrences.filter(
    (occurrence: Occurrence) => !occurrence.isValid || occurrence.isFaulted
  )
  const shadedDurations = _.uniqBy(
    abnormalOccurrences.map(({ started, ended, isValid }) => ({
      start: started,
      end: ended,
      color: isValid ? 'red' : 'orange',
    })),
    ({ start, end, color }) => `${start}-${end}-${color}`
  )
  const earliestAbnormalOccurrence = _.minBy(abnormalOccurrences, 'started')
  const latestAbnormalOccurrence = _.maxBy(abnormalOccurrences, 'ended')

  const tabsMap = {
    summary: {
      header: 'labels.summary',
      children: (
        <Summary insight={insight} setIsTicketUpdated={setIsTicketUpdated} />
      ),
    },
    activity: {
      header: 'plainText.activity',
      children: (
        <Activities
          activities={activities}
          sortBy={sortBy}
          onSortByChange={handleSortByChange}
          timeZone={site?.timeZone}
        />
      ),
    },
    occurrences: {
      header: 'plainText.occurrences',
      children: (
        <Occurrences
          occurrences={occurrences}
          sortBy={sortBy}
          onSortByChange={handleSortByChange}
          timeZone={site?.timeZone}
        />
      ),
    },
    timeSeries: {
      header: 'headers.timeSeries',
      children: (
        <SelectedPointsProvider>
          <InsightWorkflowTimeSeries
            insight={insight}
            // without passing a key, the points are never removed on time series graph
            // if user switches between different insights
            key={insight.id}
            start={
              earliestAbnormalOccurrence?.started ??
              new Date(insight.occurredDate).toISOString()
            }
            end={
              latestAbnormalOccurrence?.ended ??
              dateTime(insight.occurredDate).addHours(48).format()
            }
            shadedDurations={shadedDurations}
            twinInfo={twinInfo}
          />
        </SelectedPointsProvider>
      ),
    },
  }

  return (
    <StyledTabs>
      {Object.keys(tabsMap).map((tab) => {
        const { header, children } = tabsMap[tab]
        return (
          <StyledTab
            key={tab}
            header={_.capitalize(t(header))}
            type="modal"
            selected={
              isInsightTabControlled
                ? insightTab === tab
                : localInsightTab === tab
            }
            onClick={() =>
              isInsightTabControlled
                ? onInsightTabChange(tab)
                : setLocalInsightTab(tab)
            }
          >
            {children}
          </StyledTab>
        )
      })}
    </StyledTabs>
  )
}

const StyledTab = styled(Tab)({
  borderRight: '0px',
})

const StyledTabs = styled(Tabs)(({ theme }) => ({
  border: 'none',
  borderTop: `1px solid ${theme.color.neutral.border.default}`,
  color: theme.color.neutral.fg.default,
}))

export default InsightTabs

export type InsightWithTickets = Insight & { tickets: TicketSimpleDto[] }
