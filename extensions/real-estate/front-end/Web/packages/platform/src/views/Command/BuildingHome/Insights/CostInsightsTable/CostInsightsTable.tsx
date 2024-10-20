import _ from 'lodash'
import { useMemo } from 'react'
import { TFunction } from 'react-i18next'
import styled from 'styled-components'

import {
  InsightCostImpactPropNames,
  InsightMetric,
  Priority,
  getImpactScore,
  titleCase,
} from '@willow/common'
import { DataGrid, GridColDef, Group } from '@willowinc/ui'
import { PriorityBadge } from '@willow/common/insights/component'
import { calculatePriority } from '@willow/common/insights/costImpacts/getInsightPriority'
import {
  formatDateTime,
  getPriorityByRange,
} from '@willow/common/insights/costImpacts/utils'
import { Insight } from '@willow/common/insights/insights/types'
import { NotFound, Pill, Progress, dateComparator } from '@willow/ui'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import { AvoidableExpPerYearHeader } from '../../../../../components/Insights/InsightGroupTable/ImpactInsightsGroupedTable/useWorkflowGroupedColumns'

export interface FormattedInsight extends Insight {
  priorityScore?: number
  impactPriority: Priority | null
}

/**
 * There are 2 types of priorities in insight, one is impactScores,
 * another is the legacy priority, this formatInsight will calculate its priority
 * based on impactScores if provided, otherwise based on the legacy priority.
 */
export function formatInsight(
  insights: Insight[],
  language: Language = 'en'
): FormattedInsight[] {
  return insights.map((insight) => {
    const priorityScore = calculatePriority({
      impactScores: insight.impactScores,
      language,
      insightPriority: insight.priority,
    })

    return {
      ...insight,
      // the priority value within range [0, 100] based on impactScores and legacy priority
      priorityScore,
      // the priority object of type Priority
      impactPriority: getPriorityByRange(priorityScore),
    }
  })
}

export default function CostInsightsTable({
  insights,
  metric,
  analytics,
  language,
  t,
  onSelectedInsightChange,
  fetchNextPage,
  total,
  hideFooter = false,
}: {
  insights: FormattedInsight[]
  metric: InsightMetric
  analytics: { track: (name: string, value: { [k: string]: string }) => void }
  language: Language
  t: TFunction
  onSelectedInsightChange: (insightId?: string) => void
  fetchNextPage: () => void
  total: number
  hideFooter?: boolean
}) {
  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'name',
        headerName: t('headers.insight'),
        valueGetter: ({ row }) => row.ruleName || row.name,
        width: 140,
      },
      {
        field: 'impactScores',
        renderHeader: () => (
          <AvoidableExpPerYearHeader
            insightMetric={metric}
            t={t}
            language={language}
          />
        ),
        valueGetter: ({ row }) =>
          getImpactScore({
            impactScores: row.impactScores,
            scoreName:
              metric === InsightMetric.cost
                ? InsightCostImpactPropNames.dailyAvoidableCost
                : InsightCostImpactPropNames.dailyAvoidableEnergy,
            multiplier: 365,
            language,
            decimalPlaces: 0,
          }),
        width: 200,
      },
      {
        field: 'floorCode',
        headerName: _.startCase(t('labels.floor')),
        renderCell: ({ row }) => <Pill>{row?.floorCode ?? ''}</Pill>,
        width: 64,
      },
      {
        field: 'priority',
        headerName: t('labels.priority'),
        renderCell: ({ row }) => (
          <PriorityBadge priority={row.impactPriority} />
        ),
        width: 110,
      },
      {
        field: 'occurredDate',
        headerName: titleCase({
          text: t('plainText.lastOccurrence'),
          language,
        }),
        valueGetter: ({ row }) =>
          formatDateTime({
            value: row.occurredDate,
            language,
            timeZone: row.site?.timeZone,
          }),
        sortComparator: dateComparator,
        width: 170,
      },
    ],
    [metric, t, language]
  )

  // This is used when user selects an insight in the insights table and redirected to detail page.
  const handleSelectInsightClick = (insight) => {
    analytics.track('Insight Viewed', {
      ...insight,
      page: 'Building Dashboard',
    })
    onSelectedInsightChange(insight.id)
  }

  return (
    <StyledDataGrid
      rows={insights}
      columns={columns}
      onRowClick={({ row }) => handleSelectInsightClick(row)}
      initialState={{
        sorting: {
          sortModel: [{ field: 'occurredDate', sort: 'desc' }],
        },
      }}
      slots={{
        loadingOverlay: Progress, // Custom loading to override with MUI loading icon
        noRowsOverlay: () => (
          <NotFound>{t('plainText.noInsightsFound')}</NotFound>
        ),
        // This table will be removed once new Home page is fully implemented, so no translation needed
        footerRowCount: () => <Group pr="s16">{`Total Rows: ${total}`}</Group>,
      }}
      autosizeOptions={{
        includeHeaders: true,
      }}
      onRowsScrollEnd={fetchNextPage}
      hideFooter={hideFooter}
    />
  )
}

const StyledDataGrid = styled(DataGrid)({
  '&&&': {
    border: '0px',
    height: '100%',
  },

  // Overriding the default sorting icon only for priority column because
  // priority value of 1 is the highest priority (called Critical)
  // so descending sorted priority would be 1, 2, 3, 4, so we want to
  // rotate the icon 180deg to indicate the descending order
  '&&& [data-field="priority"] [title="Sort"]': {
    transform: 'rotate(180deg)',
  },
})
