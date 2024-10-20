/* eslint-disable complexity */
import {
  getTotalImpactScore,
  InsightCostImpactPropNames,
  titleCase,
} from '@willow/common'
import AssetLink from '@willow/common/components/AssetLink'
import { ActionIcon } from '@willow/common/insights/component'
import { InsightMetric } from '@willow/common/insights/costImpacts/types'
import {
  Insight,
  InsightActionIcon,
  InsightTableControls,
} from '@willow/common/insights/insights/types'
import {
  Blocker,
  InsightGroups,
  InsightWorkflowTabName,
  Option,
  Select,
  useAnalytics,
  useLanguage,
  useSnackbar,
} from '@willow/ui'
import {
  IconButton,
  Loader,
  Panel,
  PanelContent,
  PanelGroup,
  Tabs,
} from '@willowinc/ui'
import _ from 'lodash'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useIsMutating } from 'react-query'
import styled, { css, useTheme } from 'styled-components'
import useUpdateInsightsStatuses from '../../hooks/Insight/useUpdateInsightsStatuses'
import routes from '../../routes'
import AssetDetailsModal from '../AssetDetailsModal/AssetDetailsModal'
import { useInsights } from './InsightsContext'
import InsightsTable from './InsightsTable'
import { InsightActions } from './ui/ActionsViewControl'
import RollupSummary from './ui/RollupSummary'

export default function InsightsContent({
  canWillowUserDeleteInsight,
  onInsightIdChange,
}: {
  canWillowUserDeleteInsight: boolean
  onInsightIdChange: (insightId?: string) => void
}) {
  const theme = useTheme()
  const analytics = useAnalytics()
  const insights = useInsights()
  const { t } = useTranslation()
  const { language } = useLanguage()
  const snackbar = useSnackbar()
  // useIsMutating returns the number of mutations matches queryKey that are currently running
  const isMutating = useIsMutating(['insightsStatuses']) > 0

  const {
    tab,
    siteId,
    groupBy,
    isLoading,
    tableControls,
    filteredInsights,
    selectedInsight,
    selectedInsightIds,
    onTabChange,
    setSelectedInsightIds,
    onTableControlChange,
    onGroupByOptionClick,
    clearSelectedInsightIds,
    eventBody,
    showTotalImpact,
    isSavings,
  } = insights

  // exclude insights that are not actionable
  // e.g. when user selects insights on "open" tab, and then switches to "resolved" tab,
  // insights with status "Open" will not be displayed and hence cannot be actioned
  const selectedInsightsCanBeActioned = selectedInsightIds
    .map((id) => (filteredInsights ?? []).find((insight) => insight.id === id))
    .filter((insight): insight is Insight => insight != null)

  // exclude insights with status of "inProgress" as they cannot be ignored
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/84169
  const insightIdsCanBeIgnored = selectedInsightsCanBeActioned
    .filter((i) => i.lastStatus !== 'inProgress')
    .map((i) => i.id)

  // only insights with status of "open" can be "setToNew"
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80914
  const insightIdsCanBeSetToNew = selectedInsightsCanBeActioned
    .filter((i) => i.lastStatus === 'open')
    .map((i) => i.id)

  const mutationSiteId =
    selectedInsight?.siteId ??
    siteId ??
    selectedInsightsCanBeActioned[0]?.siteId

  const ignoreInsightsMutation = useUpdateInsightsStatuses({
    siteId: mutationSiteId,
    insightIds: insightIdsCanBeIgnored,
    newStatus: 'ignored',
  })

  const setInsightsStatusesToNewMutation = useUpdateInsightsStatuses({
    siteId: mutationSiteId,
    insightIds: insightIdsCanBeSetToNew,
    newStatus: 'new',
  })

  const handleSetToNewClick = () => {
    const snackbarOptions = {
      isToast: true,
      closeButtonLabel: t('plainText.dismiss'),
    }

    if (setInsightsStatusesToNewMutation.status === 'loading') {
      return
    }

    setInsightsStatusesToNewMutation.mutate(undefined, {
      onError: () => {
        snackbar.show(t('plainText.errorOccurred'), {
          ...snackbarOptions,
          isError: true,
        })
      },
      onSuccess: () => {
        setSelectedInsightIds([])
        const insigtCountCannotBeSetToNew =
          selectedInsightsCanBeActioned.length - insightIdsCanBeSetToNew.length

        snackbar.show(
          _.upperFirst(
            t('interpolation.insightsActioned', {
              count: insightIdsCanBeSetToNew.length,
              action: t('plainText.setToNew'),
            })
          ),
          snackbarOptions
        )

        if (insigtCountCannotBeSetToNew > 0) {
          const newInsights = selectedInsightsCanBeActioned.filter(
            (i) => i.lastStatus === 'new'
          )

          const inProgressInsights = selectedInsightsCanBeActioned.filter(
            (i) => i.lastStatus === 'inProgress'
          )

          for (const collection of [newInsights, inProgressInsights]) {
            if (collection.length > 0) {
              snackbar.show(
                t('interpolation.insightCountCannotBeSetToNew', {
                  count: collection.length,
                  lastStatus:
                    collection[0].lastStatus === 'inProgress'
                      ? _.startCase(t('plainText.inProgress'))
                      : t('beamer.new'),
                }),
                { ...snackbarOptions, isError: true, height: '84px' }
              )
            }
          }
        }
      },
    })
  }

  const handleIgnoreInsightsClick = () => {
    const snackbarOptions = {
      isToast: true,
      closeButtonLabel: t('plainText.dismiss'),
    }

    if (ignoreInsightsMutation.status === 'loading') {
      return
    }
    ignoreInsightsMutation.mutate(undefined, {
      onError: () => {
        snackbar.show(t('plainText.errorOccurred'), {
          ...snackbarOptions,
          isError: true,
        })
      },
      onSuccess: () => {
        setSelectedInsightIds([])
        const insightCountCannotBeIgnored =
          selectedInsightsCanBeActioned.length - insightIdsCanBeIgnored.length
        snackbar.show(
          _.capitalize(
            t('interpolation.insightsActioned', {
              count: insightIdsCanBeIgnored.length,
              action: t('headers.ignored'),
            })
          ),
          snackbarOptions
        )
        if (insightCountCannotBeIgnored > 0) {
          snackbar.show(
            _.capitalize(
              t('interpolation.insightsCannotBeIgnored', {
                count: insightCountCannotBeIgnored,
              })
            ),
            { ...snackbarOptions, isError: true, height: '84px' }
          )
        }
      },
    })
  }

  const [selectedAction, setSelectedAction] = useState<string | undefined>(
    undefined
  )

  const { impactView = InsightMetric.cost, excludedRollups = [] } =
    tableControls || {}

  const noRollup =
    !showTotalImpact &&
    [
      InsightTableControls.showImpactPerYear,
      InsightTableControls.showTopAsset,
    ].every((controlName) => excludedRollups.includes(controlName))
  const showRollupSummary = onTableControlChange != null && !noRollup

  const isEnergyImpactView = impactView === InsightMetric.energy
  const impact = isEnergyImpactView
    ? t('plainText.energy')
    : t('plainText.cost')

  /**
   * group filtered insights by equipmentId and return an object in form
   * of following for each group of insights having same equipmentId:
   * {
   *   equipmentName?: string,
   *   siteId?: string,
   *   twinId?: string,
   *   aggregatedCost?: number,
   *   aggregatedEnergy?: number
   * }
   * finally, return one object with largest aggregatedCost/aggregatedEnergy depending on impact view
   */
  const topContributorAsset = useMemo(
    () =>
      _.maxBy(
        Object.entries(
          _.groupBy(filteredInsights, (i) => i?.equipmentId ?? '')
        ).map(([_equipmentId, groupInsights]) => ({
          equipmentName: groupInsights.find((i) => i?.equipmentName != null)
            ?.equipmentName,
          siteId: groupInsights.find((i) => i.siteId != null)?.siteId,
          twinId: groupInsights.find((i) => i?.twinId != null)?.twinId,
          aggregatedCost: getTotalImpactScore({
            groupInsights,
            scoreName: InsightCostImpactPropNames.totalCostToDate,
            language,
            decimalPlaces: 0,
          }).value,
          aggregatedEnergy: getTotalImpactScore({
            groupInsights,
            scoreName: InsightCostImpactPropNames.totalEnergyToDate,
            language,
            decimalPlaces: 0,
          }).value,
        })),
        (o) => (isEnergyImpactView ? o.aggregatedEnergy : o.aggregatedCost)
      ),
    [filteredInsights, isEnergyImpactView, language]
  )

  const impactPerYearHeader = `${titleCase({
    text: isSavings
      ? t('plainText.totalSavingsPerYear')
      : t('interpolation.totalAvoidableExpensePerYear', {
          expense: impact,
        }),
    language,
  })} *`
  const impactPerYearTooltipText = isSavings
    ? t('plainText.totalSavingsPerYearTooltip')
    : _.capitalize(
        t('interpolation.totalAvoidableExpensePerYearRollup', {
          expense: impact,
        })
      )

  const totalImpactToDateHeader = `${titleCase({
    text: isSavings
      ? t('plainText.totalSavingsToDate')
      : t('interpolation.totalExpenseToDate', {
          expense: impact,
        }),
    language,
  })} *`
  const totalImpactToDateTooltip = isSavings
    ? t('plainText.totalSavingsToDateTooltip')
    : _.capitalize(
        t('interpolation.totalAvoidableExpenseToDateRollup', {
          expense: impact,
        })
      )

  const rollupData = [
    {
      value: getTotalImpactScore({
        groupInsights: filteredInsights ?? [],
        scoreName: isEnergyImpactView
          ? InsightCostImpactPropNames.dailyAvoidableEnergy
          : InsightCostImpactPropNames.dailyAvoidableCost,
        multiplier: 365,
        language,
        isRollUpTotal: true,
        decimalPlaces: 0,
      }).totalImpactScore,
      header: impactPerYearHeader,
      isVisible: !excludedRollups.includes(
        InsightTableControls.showImpactPerYear
      ),
      tooltipText: impactPerYearTooltipText,
      tooltipWidth: '260px',
    },
    {
      value: getTotalImpactScore({
        groupInsights: filteredInsights ?? [],
        scoreName: isEnergyImpactView
          ? InsightCostImpactPropNames.totalEnergyToDate
          : InsightCostImpactPropNames.totalCostToDate,
        language,
        isRollUpTotal: true,
        decimalPlaces: 0,
      }).totalImpactScore,
      header: totalImpactToDateHeader,
      isVisible: showTotalImpact,
      tooltipText: totalImpactToDateTooltip,
      tooltipWidth: '242px',
    },
    {
      header: titleCase({ text: t('plainText.topContributorAsset'), language }),
      children: (
        <div tw="w-full flex justify-center">
          <AssetLink
            // Navigating to Twin explorer page with Asset history tab and Insights table displayed by default
            path={`${routes.portfolio_twins_view__siteId__twinId(
              topContributorAsset?.siteId,
              topContributorAsset?.twinId
            )}?tab=assetHistory&type=insight`}
            siteId={topContributorAsset?.siteId}
            twinId={topContributorAsset?.twinId}
            assetName={topContributorAsset?.equipmentName}
            css={css({
              ...theme.font.display.sm.light,
            })}
          />
        </div>
      ),
      isVisible: !excludedRollups.includes(InsightTableControls.showTopAsset),
      tooltipText: _.capitalize(t('plainText.topContributionRollup')),
      tooltipWidth: '242px',
    },
  ]

  const isActionEnabled = siteId != null && selectedInsightIds.length > 0
  const isDeleteEnabled = canWillowUserDeleteInsight && isActionEnabled
  const getTooltip = (text: string, nextSiteId?: string) =>
    nextSiteId == null ? t('plainText.bulkActionsAreDisabled') : text
  const isMultiInsightsIgnoreEnabled =
    isActionEnabled && insightIdsCanBeIgnored.length > 0
  const isMultiInsightsSetToNewEnabled =
    isActionEnabled && insightIdsCanBeSetToNew.length > 0

  const actionIcons: InsightActionIcon[] = [
    {
      icon: 'undo',
      tooltipText: getTooltip('plainText.setToNew', siteId),
      enabled: isMultiInsightsSetToNewEnabled,
      onClick: isMultiInsightsSetToNewEnabled ? handleSetToNewClick : undefined,
      fontSize: '26px',
      marginBottom: '2px',
    },
    {
      icon: 'do_not_disturb_on',
      tooltipText: getTooltip('plainText.ignore', siteId),
      enabled: isMultiInsightsIgnoreEnabled,
      onClick: isMultiInsightsIgnoreEnabled
        ? handleIgnoreInsightsClick
        : undefined,
      filled: false,
      fontSize: theme.font.heading.xl2.fontSize,
    },
    {
      icon: 'feedback',
      tooltipText: getTooltip('plainText.report', siteId),
      enabled: isActionEnabled,
      onClick: isActionEnabled
        ? () => setSelectedAction(InsightActions.report)
        : undefined,
      filled: true,
      fontSize: theme.font.heading.xl2.fontSize,
    },
    {
      icon: 'delete',
      tooltipText: getTooltip('plainText.delete', siteId),
      isRed: true,
      onClick: isDeleteEnabled
        ? () => setSelectedAction(InsightActions.delete)
        : undefined,
      enabled: isDeleteEnabled,
      filled: false,
      fontSize: theme.font.display.lg.medium.fontSize,
    },
  ]

  return (
    <>
      {isMutating && <Blocker className="overlay" />}
      <StyledPanelGroup direction="vertical">
        {showRollupSummary ? (
          <Panel defaultSize={135}>
            <RollupSummary rollupData={rollupData} isLoading={isLoading} />
          </Panel>
        ) : (
          // to satisfy PanelGroup's child prop type
          <></>
        )}
        <Panel
          tw="border-t-0"
          tabs={
            <Tabs
              defaultValue="open"
              onTabChange={onTabChange}
              value={
                (
                  InsightWorkflowTabName.find(
                    (insightTab) => insightTab.value === tab
                  ) ?? InsightWorkflowTabName[0]
                ).value
              }
            >
              <Tabs.List>
                {InsightWorkflowTabName.map(({ label, value }) => (
                  <Tabs.Tab
                    key={value}
                    value={value}
                    onClick={() => onTabChange(value)}
                    data-testid="insights-tab"
                  >
                    {_.capitalize(t(`headers.${label}`))}
                  </Tabs.Tab>
                ))}

                <Container>
                  {insights.tab === 'open' && (
                    <>
                      {ignoreInsightsMutation.status === 'loading' ||
                      setInsightsStatusesToNewMutation.status === 'loading' ? (
                        <StyledLoader intent="secondary" />
                      ) : (
                        // Display action icons only when the checkbox of at least one insight is checked.
                        // For more details, refer to: https://dev.azure.com/willowdev/Unified/_workitems/edit/82971
                        selectedInsightsCanBeActioned.length > 0 &&
                        actionIcons.map(
                          ({
                            icon,
                            tooltipText,
                            isRed = false,
                            onClick = () => {},
                            enabled = false,
                            filled = true,
                            fontSize,
                            marginBottom,
                          }) => (
                            <IconButton
                              kind="secondary"
                              tw="outline-none"
                              key={icon + tooltipText}
                            >
                              <ActionIcon
                                key={icon}
                                icon={icon}
                                data-tooltip={_.startCase(t(tooltipText))}
                                data-tooltip-position="top"
                                $isRed={isRed}
                                onClick={onClick}
                                data-testid={`insights-action-${icon}`}
                                $enabled={enabled}
                                filled={filled}
                                fontSize={fontSize}
                                marginBottom={marginBottom}
                              />
                            </IconButton>
                          )
                        )
                      )}
                    </>
                  )}

                  {onGroupByOptionClick != null && (
                    <StyledSelect
                      value={
                        groupBy === InsightGroups.RULE
                          ? titleCase({
                              text: t('plainText.groupByRule'),
                              language,
                            })
                          : groupBy === InsightGroups.ASSET_TYPE
                          ? titleCase({
                              text: t('interpolation.groupByItem', {
                                item: _.capitalize(t(`plainText.assetType`)),
                                // to unescape the string literal to preserve
                                // the apostrophe in French
                                interpolation: { escapeValue: false },
                              }),
                              language,
                            })
                          : t('plainText.groupByNone')
                      }
                    >
                      {Object.values(InsightGroups).map((option) => (
                        <Option
                          data-testid={`group-by-${option}`}
                          key={option}
                          value={option}
                          onClick={() => {
                            const nextGroupBySelected =
                              option === InsightGroups.NONE ? undefined : option

                            analytics.track('Insight_Page_Group_By_Changed', {
                              ...eventBody,
                              groupBy: option,
                              impactView,
                            })
                            onGroupByOptionClick({
                              groupBy: nextGroupBySelected,
                              expandedGroupId: undefined,
                            })
                          }}
                        >
                          {titleCase({
                            text: t(`plainText.${_.camelCase(option)}`),
                            language,
                          })}
                        </Option>
                      ))}
                    </StyledSelect>
                  )}
                </Container>
              </Tabs.List>

              <StyledPanelContent>
                <InsightsTable onInsightIdChange={onInsightIdChange} />
              </StyledPanelContent>
            </Tabs>
          }
        />
      </StyledPanelGroup>
      {/*
        this modal is used for confirming deletion or report one or multiple insights
      */}
      {selectedInsight == null &&
        siteId != null &&
        (selectedAction === InsightActions.delete ||
          selectedAction === InsightActions.report) && (
          <AssetDetailsModal
            siteId={siteId}
            item={{
              modalType:
                selectedAction === InsightActions.report
                  ? 'report'
                  : 'deleteInsightsConfirmation',
            }}
            onClose={() => {
              setSelectedAction(undefined)
            }}
            dataSegmentPropPage={`Insights ${
              selectedAction === InsightActions.report ? 'Report' : 'Deletion'
            } Confirmation`}
            // confirmation modal does not need navigation
            navigationButtonProps={{
              items: [],
              selectedItem: undefined,
              setSelectedItem: _.noop,
            }}
            selectedInsightIds={selectedInsightIds}
            onClearSelectedInsightIds={clearSelectedInsightIds}
          />
        )}
    </>
  )
}

const Container = styled.div({
  marginLeft: 'auto',
  display: 'flex',
  alignItems: 'center',
})

const StyledSelect = styled(Select)({
  minWidth: '168px',
  height: '28px',
  marginRight: '16px',
})

const StyledLoader = styled(Loader)(({ theme }) => ({
  marginRight: theme.spacing.s20,
}))

const StyledPanelContent = styled(PanelContent)({
  height: '100%',
})

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  paddingLeft: theme.spacing.s4,
  '& > *:not(:first-child)': {
    marginTop: 0,
  },
}))
