import { FullSizeContainer, FullSizeLoader } from '@willow/common'
import { Message, api } from '@willow/ui'
import { Panel, PanelContent, PanelGroup, Select } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery } from 'react-query'
import styled, { css } from 'styled-components'

import Graph from './Graph/Graph'
import styles from './Usage.css'
import UsageGraphPeriods from './UsageGraphPeriods'

/**
 * This is the Usage component incorporating the scope selector feature
 * and will display the usage graph for a scope.
 *
 * Note: Usage is only available for building twins.
 *
 * TODO: remove packages\platform\src\views\Command\Inspections\Usage\Usage.js
 */
export default function ScopedUsage({ siteId }: { siteId?: string }) {
  const { t } = useTranslation()

  const [days, setDays] = useState(UsageGraphPeriods.MONTH)

  const usageQuery = useQuery(
    ['usage', siteId, days],
    async () => {
      const response = await api.get(`/sites/${siteId}/inspectionUsage`, {
        params: {
          period: days,
        },
      })

      return response.data
    },
    {
      enabled: !!siteId,
      select: (usageData: {
        xAxis: string[]
        data: number[][]
        userName: string[]
      }) =>
        usageData.xAxis.map((xAxisName, i) => ({
          name: xAxisName,
          segments: usageData.data[i].map((userValue, j) => ({
            value: userValue,
            name: usageData.userName[j] ?? '',
          })),
        })),
    }
  )

  return (
    <StyledPanelGroup>
      <Panel
        title={t('plainText.checksCompleted')}
        headerControls={
          <Select
            css={{
              width: 115,
            }}
            value={days}
            onChange={(nextDays: string) => {
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
        {usageQuery.isError ? (
          <FullSizeContainer>
            <Message icon="error">{t('plainText.errorOccurred')}</Message>
          </FullSizeContainer>
        ) : usageQuery.isLoading ? (
          <FullSizeLoader />
        ) : (
          <GraphPanelContent>
            <Graph
              data={usageQuery.data || []}
              days={days}
              className={styles.graph}
            />
          </GraphPanelContent>
        )}
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
