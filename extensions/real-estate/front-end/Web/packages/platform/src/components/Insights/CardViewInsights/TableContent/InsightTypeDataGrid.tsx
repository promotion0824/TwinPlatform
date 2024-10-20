import { DataGrid, GridColDef } from '@willowinc/ui'
import { dateComparator } from '@willow/ui'
import { styled } from 'twin.macro'
import { useMemo } from 'react'
import _ from 'lodash'
import {
  InsightTypeBadge,
  PriorityName,
} from '@willow/common/insights/component'
import {
  InsightCostImpactPropNames,
  InsightMetric,
  formatDateTime,
  getImpactScore,
  titleCase,
  FullSizeLoader,
} from '@willow/common'
import { useInsightsContext } from '../InsightsContext'
import NotFound from '../../ui/NotFound'

export default function InsightTypeDataGrid({ noData }: { noData: boolean }) {
  const {
    t,
    language,
    impactView,
    isLoading,
    cards = [],
    handleInsightTypeClick,
  } = useInsightsContext()

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'name',
        minWidth: 300,
        flex: 3.2,
        headerName: titleCase({
          text: t('plainText.skill'),
          language,
        }),
        // business requirement to display ruleName if it's defined and not an empty string,
        // display insight name (also called summary) otherwise
        // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/78451
        valueGetter: ({ row, value }) => row.ruleName || value,
      },
      {
        field: 'insightType',
        minWidth: 120,
        flex: 0.6,
        headerName: t('labels.category'),
        renderCell: ({ value }) => (
          <InsightTypeBadge type={value} badgeSize="md" />
        ),
      },
      {
        field: 'impactScore',
        minWidth: 204,
        flex: 1.2,
        headerName: titleCase({
          text: t('interpolation.avoidableExpensePerYear', {
            expense: t(`plainText.${impactView}`),
          }),
          language,
        }),
        headerAlign: 'right',
        align: 'right',
        renderCell: ({ row }) => (
          <span tw="pr-[8px]">
            {getImpactScore({
              impactScores: row.impactScores,
              scoreName:
                impactView === InsightMetric.cost
                  ? InsightCostImpactPropNames.dailyAvoidableCost
                  : InsightCostImpactPropNames.dailyAvoidableEnergy,
              language,
              multiplier: 365,
              decimalPlaces: 0,
            })}
          </span>
        ),
      },
      {
        field: 'priority',
        minWidth: 100,
        flex: 0.4,
        headerName: t('labels.priority'),
        renderCell: ({ value }) => <PriorityName insightPriority={value} />,
      },
      {
        field: 'insightCount',
        minWidth: 104,
        flex: 0.4,
        headerName: t('headers.insights'),
        align: 'center',
        valueGetter: ({ value }) => value,
      },
      {
        field: 'lastOccurredDate',
        minWidth: 180,
        flex: 1,
        headerName: titleCase({
          text: t('plainText.lastOccurrence'),
          language,
        }),
        valueGetter: ({ row, value }) =>
          formatDateTime({
            value,
            language,
            timeZone: row.site?.timeZone,
          }),
        sortComparator: dateComparator,
      },
    ],
    [impactView, language, t]
  )

  return isLoading ? (
    // loader is brought in separately, so as to perform autoSizing after data is loaded.
    <FullSizeLoader />
  ) : noData ? (
    <NotFound
      message={titleCase({
        language,
        text: t('plainText.noSkillsFound'),
      })}
    />
  ) : (
    <StyledDataGrid
      data-segment="Insight Type Selected"
      data-testid="insight-type-results"
      rows={cards}
      columns={columns}
      onRowClick={({ row }) => handleInsightTypeClick(row)}
      // to avoid issue like:
      // https://github.com/mui/mui-x/issues/2714
      isRowSelectable={(data) => data.row != null && data.row.id != null}
      disableColumnResize={false} // Disabling default behavior of column resizing
    />
  )
}

const StyledDataGrid = styled(DataGrid)(({ theme }) => ({
  '&&&': {
    border: '0px',

    '& > div': {
      cursor: 'pointer',
    },
  },

  // Overriding the existing styling to show the resize icon in the table header
  '.MuiDataGrid-columnSeparator': {
    display: 'block',
    color: theme.color.neutral.fg.default,

    '&:hover': {
      color: theme.color.neutral.fg.highlight,
    },

    '> svg': {
      height: '56px',
      display: 'block',
    },
  },
}))
