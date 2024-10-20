import {
  ResizeObserverContainer,
  isWillowUser,
  titleCase,
  DebouncedSearchInput,
} from '@willow/common'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  DocumentTitle,
  FILTER_PANEL_BREAKPOINT,
  useAnalytics,
  useDateTime,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { Box, Button, Panel, PanelGroup, SegmentedControl } from '@willowinc/ui'
import _ from 'lodash'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory, useParams } from 'react-router'
import { styled } from 'twin.macro'
import useOntologyInPlatform from '../../../hooks/useOntologyInPlatform'
import { useSites } from '../../../providers'
import { statusMap } from '../../../services/Insight/InsightsService'
import { useInsightsContext } from './InsightsContext'
import InsightsFilters from './InsightsFilters'
import InsightsHeaderContent from './InsightsHeaderContent'
import InsightsRollUpContent from './InsightsRollUpContent'
import InsightsTableContent from './InsightsTableContent'
import InsightsView from './InsightsView'

const CardViewInsights = () => {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const user = useUser()
  const dateTime = useDateTime()
  const sites = useSites()
  const { siteId } = useParams<{ siteId: string }>()
  const analytics = useAnalytics()
  const history = useHistory()
  const ontologyQuery = useOntologyInPlatform()
  const modelsOfInterestQuery = useModelsOfInterest()
  const canWillowUserDeleteInsight = isWillowUser(user?.email)
  const { location, locationName } = useScopeSelector()
  const [currentPageWidth, setCurrentPageWidth] = useState(Infinity)
  const showFiltersPanel = currentPageWidth > FILTER_PANEL_BREAKPOINT

  return (
    <InsightsView
      t={t}
      language={language}
      insightFilterSettings={user.options?.insightFilterSettings ?? []}
      impactView={user?.options?.insightsImpactView}
      lastInsightStatusCountDate={
        user?.options?.lastInsightStatusCountDate ??
        dateTime.now().addDays(-1).format('dateLocal')
      }
      onInsightCountDateChange={(currentDate) =>
        user.saveOptions('lastInsightStatusCountDate', currentDate)
      }
      dateTime={dateTime}
      sites={sites}
      siteId={siteId}
      analytics={analytics}
      history={history}
      ontologyQuery={ontologyQuery}
      modelsOfInterestQuery={modelsOfInterestQuery}
      canWillowUserDeleteInsight={canWillowUserDeleteInsight}
      scopeId={location?.twin?.id}
    >
      <DocumentTitle scopes={[t('headers.insights'), locationName]} />

      <ResizeObserverContainer onContainerWidthChange={setCurrentPageWidth}>
        <PanelGroup direction="vertical">
          <InsightsHeaderContent
            showDrawer={!showFiltersPanel}
            key={showFiltersPanel.toString()}
          />
          <StyledPanelGroup>
            {showFiltersPanel ? <InsightFiltersPanel /> : <></>}
            <PanelGroup direction="vertical">
              <InsightsRollUpContent />
              <InsightsTableContent />
            </PanelGroup>
          </StyledPanelGroup>
        </PanelGroup>
      </ResizeObserverContainer>
    </InsightsView>
  )
}

export default CardViewInsights

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))

const InsightFiltersPanel = () => {
  const { onResetFilters, queryParams, hasAppliedFilter, onQueryParamsChange } =
    useInsightsContext()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <Panel
      id="insightFilterPanel"
      data-testid="insightFilterPanel"
      defaultSize="292px"
      collapsible
      title={t('headers.filters')}
      footer={
        hasAppliedFilter ? (
          <Button
            background="transparent"
            kind="secondary"
            onClick={() => {
              onResetFilters(_.omit(queryParams, ['groupBy']))
            }}
          >
            <Box component="span" c="neutral.fg.default">
              {titleCase({ text: t('labels.resetFilters'), language })}
            </Box>
          </Button>
        ) : null
      }
    >
      <InsightsFilters
        additionalFilters={
          <>
            <SegmentedControl
              w="100%"
              orientation="vertical"
              data={[
                {
                  value: 'default',
                  label: _.capitalize(t('headers.active')),
                },
                {
                  value: 'inactive',
                  label: _.capitalize(t('headers.inactive')),
                },
              ]}
              onChange={(nextOption) => {
                onQueryParamsChange?.({
                  status: statusMap[nextOption],
                  // When user switches between active and inactive status filters,
                  // resets the following filters as they might not be applicable
                  // for insights with different status
                  selectedCategories: undefined,
                  selectedPrimaryModelIds: undefined,
                  selectedStatuses: undefined,
                  selectedSourceNames: undefined,
                  page: undefined,
                })
              }}
              value={
                _.isEqual(queryParams?.status, statusMap.inactive)
                  ? 'inactive'
                  : 'default'
              }
            />
            <DebouncedSearchInput
              key={(queryParams.search ?? '').toString()}
              onDebouncedSearchChange={onQueryParamsChange}
              value={queryParams.search?.toString()}
              mt="s12"
            />
          </>
        }
      />
    </Panel>
  )
}
