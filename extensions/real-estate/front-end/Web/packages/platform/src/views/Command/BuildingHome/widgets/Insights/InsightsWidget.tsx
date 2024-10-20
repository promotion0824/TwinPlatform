import { titleCase } from '@willow/common'
import { getModelDisplayName } from '@willow/common/twins/view/models'
import { getContainmentHelper, invariant, useScopeSelector } from '@willow/ui'
import { BarChart, Group, Stack } from '@willowinc/ui'
import { forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import styled from 'styled-components'
import { ChartTile } from '../../../../../components/LocationHome/ChartTile/ChartTile'
import CountsTile from '../../../../../components/LocationHome/CountsTile/CountsTile'
import {
  useGetActiveInsightCountsByTwinModel,
  useOntology,
} from '../../../../../hooks'
import { useSite } from '../../../../../providers'
import routes from '../../../../../routes'
import { WidgetId } from '../../../../../store/buildingHomeSlice'
import { DraggableContent } from '../../DraggableColumnLayout'
import BuildingHomeWidgetCard from '../BuildingHomeWidgetCard'
import useCancelableEditModal from '../useCancelableEditModal'
import ImpactScoresTile from './ImpactScoresTile'
import InsightsWidgetEditModal from './InsightsWidgetEditModal'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

// TODO: Replace with data from backend when available.
// https://dev.azure.com/willowdev/Unified/_workitems/edit/134871
const dailyInsightOccurrences = [
  { date: '2024-02-08', count: 45 },
  { date: '2024-02-09', count: 48 },
  { date: '2024-02-10', count: 47 },
  { date: '2024-02-11', count: 41 },
  { date: '2024-02-12', count: 44 },
  { date: '2024-02-13', count: 8 },
  { date: '2024-02-14', count: 5 },
  { date: '2024-02-15', count: 45 },
  { date: '2024-02-16', count: 48 },
  { date: '2024-02-17', count: 47 },
  { date: '2024-02-18', count: 41 },
  { date: '2024-02-19', count: 44 },
  { date: '2024-02-20', count: 8 },
  { date: '2024-02-21', count: 5 },
  { date: '2024-02-22', count: 45 },
  { date: '2024-02-23', count: 48 },
  { date: '2024-02-24', count: 47 },
  { date: '2024-02-25', count: 41 },
  { date: '2024-02-26', count: 44 },
  { date: '2024-02-27', count: 8 },
  { date: '2024-02-28', count: 5 },
  { date: '2024-02-29', count: 45 },
  { date: '2024-03-01', count: 48 },
  { date: '2024-03-02', count: 47 },
  { date: '2024-03-03', count: 41 },
  { date: '2024-03-04', count: 44 },
  { date: '2024-03-05', count: 8 },
  { date: '2024-03-06', count: 5 },
]

const Container = styled.div(({ theme }) => {
  const containerQuery = getContainerQuery(
    `max-width: ${theme.breakpoints.mobile}`
  )

  return {
    '.hide-on-mobile': { display: 'inherit' },
    '.show-on-mobile': { display: 'none' },

    [containerQuery]: {
      '.hide-on-mobile': { display: 'none' },
      '.show-on-mobile': { display: 'inherit' },
    },
  }
})

const InsightsWidget: DraggableContent<WidgetId> = forwardRef(
  ({ canDrag, id = WidgetId.Insights, ...props }, ref) => {
    const history = useHistory()
    const ontologyRequest = useOntology()
    const { scopeId } = useScopeSelector()
    const site = useSite()
    const translation = useTranslation()
    const {
      i18n: { language },
      t,
    } = translation

    invariant(scopeId, 'No scope ID was found.')

    const {
      editModalOpened,
      onCancelEdit,
      onCloseEditModal,
      onOpenEditModal,
      onSaveEdit,
      setWidgetConfig,
      widgetConfig,
    } = useCancelableEditModal(WidgetId.Insights)

    const activeInsightCountsQuery = useGetActiveInsightCountsByTwinModel({
      twinId: scopeId,
    })

    const activeInsightCounts =
      ontologyRequest.isSuccess && activeInsightCountsQuery.isSuccess
        ? activeInsightCountsQuery.data.map(({ count, modelId }) => {
            const model = ontologyRequest.data.getModelById(modelId)
            const name = getModelDisplayName(model, translation)
            return {
              count,
              name,
            }
          })
        : []

    invariant(
      site.insightsStatsByStatus,
      'No insights stats were returned for this site.'
    )

    const countsTile = (
      <CountsTile
        breakpoint={0}
        data={[
          {
            icon: { name: 'release_alert' },
            intent: 'negative',
            label: titleCase({
              language,
              text: t('plainText.critical'),
            }),
            onClick: () =>
              history.push(
                `${routes.insights_scope__scopeId(scopeId)}?priorities=1`
              ),
            value: site.insightsStats.urgentCount,
          },
          {
            icon: { filled: false, name: 'circle' },
            intent: 'secondary',
            label: titleCase({ language, text: t('plainText.open') }),
            onClick: () =>
              history.push(
                `${routes.insights_scope__scopeId(
                  scopeId
                )}?selectedStatuses=Open`
              ),
            value: site.insightsStats.openCount,
          },
          {
            icon: { name: 'clock_loader_40' },
            intent: 'primary',
            label: titleCase({
              language,
              text: t('plainText.inProgress'),
            }),
            onClick: () =>
              history.push(
                `${routes.insights_scope__scopeId(
                  scopeId
                )}?selectedStatuses=InProgress`
              ),
            value: site.insightsStatsByStatus.inProgressCount,
          },
        ]}
      />
    )

    const dailyInsightOccurrencesChartTile = (
      <ChartTile
        chart={
          <BarChart
            dataset={[
              {
                data: dailyInsightOccurrences.map(({ count }) => count),
                name: titleCase({
                  language,
                  text: t('headers.dailyInsightOccurrences'),
                }),
              },
            ]}
            labels={dailyInsightOccurrences.map(({ date }) => date)}
            labelsType="time"
            orientation="vertical"
          />
        }
        title={titleCase({
          language,
          text: t('headers.dailyInsightOccurrences'),
        })}
      />
    )

    const topActiveInsightTwinTypesTile = activeInsightCounts.length > 0 && (
      <ChartTile
        chart={
          <BarChart
            dataset={[
              {
                data: activeInsightCounts.map(({ count }) => count),
                name: titleCase({
                  language,
                  text: t('headers.top5ActiveInsightsByTwinType'),
                }),
              },
            ]}
            labels={activeInsightCounts.map(({ name }) => name)}
          />
        }
        title={titleCase({
          language,
          text: t('headers.top5ActiveInsightsByTwinType'),
        })}
      />
    )

    const impactScoresTile = (
      <ImpactScoresTile
        showActiveAvoidableCost={widgetConfig.showActiveAvoidableCost}
        showActiveAvoidableEnergy={widgetConfig.showActiveAvoidableEnergy}
        showAverageDuration={widgetConfig.showAverageDuration}
      />
    )

    return (
      <>
        <BuildingHomeWidgetCard
          {...props}
          id={id}
          isDraggingMode={canDrag}
          navigationButtonContent={t('interpolation.goTo', {
            value: t('headers.insights'),
          })}
          navigationButtonLink={routes.insights_scope__scopeId(scopeId)}
          onWidgetEdit={onOpenEditModal}
          ref={ref}
          title={t('headers.insights')}
        >
          <ContainmentWrapper style={{ containerType: 'inline-size' }}>
            <Container>
              <Group align="flex-start" gap="s12" wrap="nowrap">
                <Stack gap="s12" w="100%">
                  {countsTile}

                  <div className="show-on-mobile">{impactScoresTile}</div>

                  {dailyInsightOccurrencesChartTile}

                  <div className="show-on-mobile">
                    {topActiveInsightTwinTypesTile}
                  </div>
                </Stack>
                <Stack className="hide-on-mobile" gap="s12" w="100%">
                  {impactScoresTile}
                  {topActiveInsightTwinTypesTile}
                </Stack>
              </Group>
            </Container>
          </ContainmentWrapper>
        </BuildingHomeWidgetCard>

        <InsightsWidgetEditModal
          onCancel={onCancelEdit}
          onClose={onCloseEditModal}
          onSave={onSaveEdit}
          opened={editModalOpened}
          setWidgetConfig={setWidgetConfig}
          widgetConfig={widgetConfig}
        />
      </>
    )
  }
)

export default InsightsWidget
