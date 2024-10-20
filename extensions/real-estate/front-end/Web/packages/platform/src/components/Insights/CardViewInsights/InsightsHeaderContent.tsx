/* eslint-disable complexity */
import { InsightMetric, titleCase, DebouncedSearchInput } from '@willow/common'
import { InsightCardGroups } from '@willow/common/insights/insights/types'
import {
  DocumentTitle,
  caseInsensitiveEquals,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import {
  Button,
  Drawer,
  Group,
  Icon,
  Indicator,
  PageTitle,
  PageTitleItem,
  Select,
  useDisclosure,
} from '@willowinc/ui'
import _ from 'lodash'
import { useLocation, useParams } from 'react-router'
import { Link } from 'react-router-dom'
import { styled } from 'twin.macro'
import routes from '../../../routes'
import { statusMap } from '../../../services/Insight/InsightsService'
import GenericHeader from '../../../views/Layout/Layout/GenericHeader'
import TableViewControl from '../ui/TableViewControl'
import { useInsightsContext } from './InsightsContext'
import InsightsFilters from './InsightsFilters'

export default function InsightsHeaderContent({
  showDrawer,
}: {
  showDrawer?: boolean
}) {
  const {
    queryParams,
    hasAppliedFilter,
    insights,
    t,
    language,
    onQueryParamsChange,
    impactView = InsightMetric.cost,
    excludedRollups = [],
    rollupControls,
    showTotalImpact,
    setShowTotalImpact,
    eventBody,
    groupBy,
    onUpdateIncludedRollups,
    analytics,
    isInsightTypeNode,
    viewByOptionsMap,
    onResetFilters,
  } = useInsightsContext()
  const user = useUser()
  const location = useLocation()
  const {
    isScopeSelectorEnabled,
    location: scope,
    locationName,
  } = useScopeSelector()
  const [drawerOpened, { close: closeDrawer, open: openDrawer }] =
    useDisclosure(false)

  const handleTableControlChange = (params) => {
    onQueryParamsChange?.(params)
  }

  const { ruleId, siteId } = useParams<{ ruleId: string; siteId?: string }>()
  const ruleName = insights?.find(
    (insight) => insight.ruleId === ruleId
  )?.ruleName

  const legacyRoute = siteId
    ? routes.sites__siteId_insights(siteId)
    : routes.insights
  const scopedRoute = scope?.twin?.id
    ? routes.insights_scope__scopeId(scope.twin.id)
    : routes.insights

  const nextUrlSearchParams = new URLSearchParams(location.search)
  if (user?.localOptions?.insightsGroupBy) {
    nextUrlSearchParams.set('groupBy', user.localOptions.insightsGroupBy)
  }

  const ruleNameForPageTitle =
    ruleName ||
    (caseInsensitiveEquals(ruleId, 'ungrouped') ? _.capitalize(ruleId) : '')

  return (
    <>
      <DocumentTitle
        scopes={[ruleNameForPageTitle, t('headers.insights'), locationName]}
      />

      {showDrawer && (
        <Drawer
          {...(hasAppliedFilter
            ? {
                footer: (
                  <Group justify="flex-end" w="100%">
                    <Button
                      disabled={false}
                      kind="secondary"
                      onClick={() =>
                        // group by is not a filter, so the corresponding query param should not be removed
                        onResetFilters(_.omit(queryParams, ['groupBy']))
                      }
                    >
                      {titleCase({
                        language,
                        text: t('labels.resetFilters'),
                      })}
                    </Button>
                  </Group>
                ),
              }
            : {})}
          header={t('headers.filters')}
          opened={drawerOpened}
          onClose={closeDrawer}
          size="xs"
        >
          <InsightsFilters />
        </Drawer>
      )}

      <GenericHeader
        topLeft={
          <PageTitle key="pageTitle">
            {[
              {
                text: t('headers.insights'),
                to: `${isScopeSelectorEnabled ? scopedRoute : legacyRoute}${
                  nextUrlSearchParams.toString()
                    ? `?${nextUrlSearchParams}`
                    : ''
                }`,
              },
              ...(ruleNameForPageTitle
                ? [
                    {
                      text: ruleNameForPageTitle,
                    },
                  ]
                : []),
            ].map(({ text, to }) => (
              <PageTitleItem key={text}>
                {to ? <Link to={to}>{text}</Link> : text}
              </PageTitleItem>
            ))}
          </PageTitle>
        }
        topRight={
          <div tw="flex items-center" key="additionalControls">
            {/* Hiding the view by dropdown for Insight type node page */}
            {!isInsightTypeNode && (
              <>
                {!showDrawer &&
                  `${titleCase({
                    text: t('plainText.viewBy'),
                    language,
                  })} :`}
                <StyledSelect
                  value={groupBy}
                  onChange={(nextOption: InsightCardGroups) => {
                    if (nextOption === groupBy) {
                      return
                    }
                    analytics?.track('Insight_Page_Group_By_Changed', {
                      ...eventBody,
                      groupBy: nextOption,
                      impactView,
                    })

                    onQueryParamsChange?.({
                      groupBy: nextOption,
                    })
                    user?.saveLocalOptions('insightsGroupBy', nextOption)
                  }}
                  data={Object.values(InsightCardGroups).map((option) => ({
                    label: titleCase({
                      text: `${
                        showDrawer
                          ? `${titleCase({
                              text: t('plainText.view'),
                              language,
                            })} `
                          : ''
                      }${viewByOptionsMap[option].text}`,
                      language,
                    }),
                    value: option,
                  }))}
                />
              </>
            )}
            <TableViewControl
              controls={{ impactView, excludedRollups }}
              rollupControls={isInsightTypeNode ? [] : rollupControls}
              onChange={handleTableControlChange}
              showTotalImpact={showTotalImpact}
              onShowTotalImpact={() => {
                setShowTotalImpact(!showTotalImpact)
              }}
              selectedImpactView={(impactView || InsightMetric.cost) as string}
              analytics={analytics}
              eventBody={eventBody}
              nextIncludedRollups={eventBody.includedRollups ?? []}
              setNextIncludedRollups={onUpdateIncludedRollups}
              isCardView
            />
          </div>
        }
        bottomLeft={
          showDrawer && (
            <Group>
              <DebouncedSearchInput
                key={(queryParams.search ?? '').toString()}
                onDebouncedSearchChange={onQueryParamsChange}
                value={queryParams.search?.toString()}
              />
              <Select
                w="100px"
                data-testid="last-occurred-date-select"
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
                onChange={(nextOption: string) => {
                  onQueryParamsChange?.({
                    status: statusMap[nextOption],
                    // following fields are reset when status is changed
                    selectedCategories: undefined,
                    selectedPrimaryModelIds: undefined,
                    selectedStatuses: undefined,
                    page: undefined,
                  })
                }}
                value={
                  _.isEqual(queryParams?.status, statusMap.inactive)
                    ? 'inactive'
                    : 'default'
                }
              />
            </Group>
          )
        }
        bottomRight={
          showDrawer && (
            <Indicator disabled={!hasAppliedFilter}>
              <Button
                kind="secondary"
                onClick={openDrawer}
                prefix={<Icon icon="filter_list" />}
              >
                {t('headers.filters')}
              </Button>
            </Indicator>
          )
        }
      />
    </>
  )
}

const StyledSelect = styled(Select)(({ theme }) => ({
  width: '130px',
  margin: `0 ${theme.spacing.s12}`,
}))
