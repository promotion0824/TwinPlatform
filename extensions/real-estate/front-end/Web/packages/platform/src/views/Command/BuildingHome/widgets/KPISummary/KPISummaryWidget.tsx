import { noop } from 'lodash'
import { forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

import { getContainmentHelper } from '@willow/ui'
import { EmptyState } from '@willowinc/ui'
import {
  NumberTile,
  NumberTileProps,
} from '../../../../../components/LocationHome/NumberTile/NumberTile'
import {
  KpiSummarySettings,
  WidgetId,
} from '../../../../../store/buildingHomeSlice'
import { DraggableContent } from '../../DraggableColumnLayout'
import BuildingHomeWidgetCard from '../BuildingHomeWidgetCard'
import useCancelableEditModal from '../useCancelableEditModal'
import KPISummaryWidgetEditModal from './KPISummaryWidgetEditModal'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

type KPISummaryWidgetData = NumberTileProps & {
  fieldId: string
}

const Container = styled(ContainmentWrapper)`
  container-type: inline-size;
  min-height: 84px;
  overflow-y: hidden;
`
const StyledGrid = styled.div(({ theme }) => {
  const containerQuery = getContainerQuery(
    `max-width: ${theme.breakpoints.mobile}`
  )
  return {
    display: 'grid',
    gridTemplateColumns: `repeat(3, 1fr)`,
    gap: theme.spacing.s12,

    [containerQuery]: {
      gridTemplateColumns: `repeat(1, 1fr)`,

      // Style for mobile view.
      '& .number-tile-body': {
        display: 'flex',

        '& .value': {
          ...theme.font.display.sm.light,
          fontWeight: theme.font.display.lg.medium.fontWeight,
        },

        '& .unit': {
          lineHeight: '22px',
        },
      },

      '& .number-tile-sparkline': {
        display: 'none',
      },
    },
  }
})

/**
 * Draggable Widget that displays a summary of KPIs.
 * It shows a list of NumberTile components with a trend badge.
 * Maximum count of items is 6.
 */
const KPISummaryWidget: DraggableContent<WidgetId> = forwardRef(
  ({ canDrag, id = WidgetId.KpiSummary, ...props }, ref) => {
    const { t } = useTranslation()

    const {
      widgetConfig: kpiWidgetFeatures,
      setWidgetConfig,
      editModalOpened,
      onOpenEditModal,
      onCloseEditModal,
      onCancelEdit,
      onSaveEdit,
    } = useCancelableEditModal(WidgetId.KpiSummary)

    const setKpiSummaryFeatures = (newConfig: KpiSummarySettings) => {
      setWidgetConfig({
        ...kpiWidgetFeatures,
        ...newConfig,
      })
    }

    // This is a temporary KPI data from the backend.
    const getData = (): KPISummaryWidgetData[] => {
      const configToLabels = {
        comfort: 'Comfort',
        energy: 'Energy',
        estimatedAvoidableCost: 'Estimated_Avoidable Cost',
        markDownLossRisk: 'Mark Down Loss Risk',
        priority: 'Priority',
        duration: 'Duration',
      }

      return Object.keys(kpiWidgetFeatures)
        .filter((key) => kpiWidgetFeatures[key] && configToLabels[key])
        .map((key) => ({
          description:
            'The estimated yearly cost that could be avoided if the unresolved insights were resolved.',
          label: configToLabels[key],
          fieldId: key.toLowerCase(),
          unit: 'USD',
          value: '327K',
          trendingInfo: {
            sentiment: 'positive',
            trend: 'upwards',
            value: '5%',
          },
          sparkline: {
            labels: [
              '2024-01-01',
              '2024-01-02',
              '2024-01-03',
              '2024-01-04',
              '2024-01-05',
              '2024-01-06',
              '2024-01-07',
              '2024-01-08',
              '2024-01-09',
              '2024-01-10',
            ],
            dataset: [
              {
                name: 'Building 1',
                data: [54, 85, 1, 90, 34, 57, 71, 58, 80, 60],
              },
            ],
          },
          onClick: noop,
        }))
    }

    // This is a temporary solution to show only comfort and energy kpis.
    const kpiDataByWidgetOptions: KPISummaryWidgetData[] = getData() ?? []

    return (
      <>
        <BuildingHomeWidgetCard
          {...props}
          ref={ref}
          id={id}
          isDraggingMode={canDrag}
          title={t('headers.kpiSummary')}
          navigationButtonContent={t('interpolation.goTo', {
            value: t('labels.link').toLowerCase(),
          })}
          onWidgetEdit={onOpenEditModal}
        >
          <Container>
            {kpiDataByWidgetOptions.length > 0 ? (
              <StyledGrid>
                {kpiDataByWidgetOptions?.map(
                  ({
                    trendingInfo,
                    sparkline,
                    fieldId,
                    ...numberTileProps
                  }) => (
                    <NumberTile
                      key={fieldId}
                      {...numberTileProps}
                      size="large"
                      trendingInfo={
                        kpiWidgetFeatures?.showTrend ? trendingInfo : undefined
                      }
                      sparkline={
                        kpiWidgetFeatures?.showSparkline ? sparkline : undefined
                      }
                    />
                  )
                )}
              </StyledGrid>
            ) : (
              <EmptyState title={t('plainText.noKpiSelected')} />
            )}
          </Container>
        </BuildingHomeWidgetCard>
        <KPISummaryWidgetEditModal
          opened={editModalOpened}
          onClose={onCloseEditModal}
          onCancel={onCancelEdit}
          onSave={onSaveEdit}
          options={kpiWidgetFeatures}
          onSaveOptions={setKpiSummaryFeatures}
        />
      </>
    )
  }
)

export default KPISummaryWidget
