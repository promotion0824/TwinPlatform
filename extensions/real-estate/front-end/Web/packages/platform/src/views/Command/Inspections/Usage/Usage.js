import { Fetch } from '@willow/ui'
import { Panel, PanelContent, PanelGroup, Select } from '@willowinc/ui'
import { useEffect, useState } from 'react'
import styled, { css } from 'styled-components'

import { useSite } from 'providers'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import useCommandAnalytics from '../../useCommandAnalytics.ts'
import { useInspections } from '../InspectionsProvider'
import { getInspectionsPageTitle } from '../getPageTitles'
import Graph from './Graph/Graph'
import styles from './Usage.css'
import UsageGraphPeriods from './UsageGraphPeriods'

export default function Usage() {
  const site = useSite()
  const { t } = useTranslation()
  const params = useParams()
  const commandAnalytics = useCommandAnalytics(site.id)

  const [days, setDays] = useState(UsageGraphPeriods.MONTH)
  const [data, setData] = useState([])
  const { setPageTitles } = useInspections()

  useEffect(() => {
    setPageTitles([
      getInspectionsPageTitle({
        siteId: params.siteId,
        title: t('headers.inspections'),
      }),
    ])
  }, [params.siteId, setPageTitles, t])

  const handleDataResponse = (response) => {
    const nextData = Array.from(response.xAxis).map((xAxisName, i) => ({
      name: xAxisName,
      segments: Array.from(response.data[i]).map((userValue, j) => ({
        value: userValue,
        name: response.userName[j] ?? '',
      })),
    }))

    setData(nextData)
  }

  useEffect(() => {
    commandAnalytics.pageInspections('usage')
  }, [commandAnalytics])

  return (
    <StyledPanelGroup>
      <Panel
        title={t('plainText.checksCompleted')}
        headerControls={
          <Select
            css={{
              width: 115 /* same as previous */,

              // this is a temporary fix before task BUG 96964
              // https://dev.azure.com/willowdev/Unified/_workitems/edit/96964
              boxSizing: 'border-box',
            }}
            value={days}
            onChange={(nextDays) => {
              commandAnalytics.trackInspectionUsageDropdown(nextDays)

              setDays(nextDays)
            }}
            data={[
              {
                label: t('plainText.last7Days'),
                value: UsageGraphPeriods.WEEK,
              },
              {
                label: t('plainText.last30Days'),
                value: UsageGraphPeriods.MONTH,
              },
              {
                label: t('plainText.lastQuarter'),
                value: UsageGraphPeriods.QUARTER,
              },
              {
                label: t('plainText.lastYear'),
                value: UsageGraphPeriods.YEAR,
              },
            ]}
          />
        }
      >
        <GraphPanelContent>
          <Fetch
            url={`/api/sites/${site.id}/inspectionUsage`}
            params={{
              period: days,
            }}
            onResponse={handleDataResponse}
          >
            {() => (
              <Graph
                key={data}
                data={data}
                days={days}
                className={styles.graph}
              />
            )}
          </Fetch>
        </GraphPanelContent>
      </Panel>
    </StyledPanelGroup>
  )
}

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))

const GraphPanelContent = styled(PanelContent)(
  ({ theme }) => css`
    height: 100%;
    display: flex;
    padding: ${theme.spacing.s16};
    justify-content: center;
  `
)
