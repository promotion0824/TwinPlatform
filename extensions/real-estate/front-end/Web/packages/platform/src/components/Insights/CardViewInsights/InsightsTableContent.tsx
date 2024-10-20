/* eslint-disable max-len */
/* eslint-disable complexity */
import { qs, titleCase } from '@willow/common'
import { ActionIcon } from '@willow/common/insights/component'
import {
  Insight,
  InsightActionIcon,
  InsightCardGroups,
  InsightView,
  InsightWorkflowStatus,
} from '@willow/common/insights/insights/types'
import {
  ALL_SITES,
  caseInsensitiveEquals,
  useDateTime,
  useFeatureFlag,
  useSnackbar as useLegacySnackbar,
} from '@willow/ui'
import {
  Badge,
  Group,
  IconButton,
  Loader,
  Panel,
  PanelContent,
  PanelGroup,
  SegmentedControl,
  useSnackbar,
} from '@willowinc/ui'
import _ from 'lodash'
import { useState } from 'react'
import { UseMutationResult } from 'react-query'
import { Link } from 'react-router-dom'
import styled, { useTheme } from 'styled-components'
import 'twin.macro'
import useGetInsightSnackbarsStatus from '../../../hooks/Insight/useGetInsightSnackbarsStatus'
import useUpdateInsightsStatuses from '../../../hooks/Insight/useUpdateInsightsStatuses'
import {
  InsightSnackbarsStatus,
  statusMap,
} from '../../../services/Insight/InsightsService'
import AssetDetailsModal from '../../AssetDetailsModal/AssetDetailsModal'
import { InsightActions } from '../ui/ActionsViewControl'
import InsightTypeCards from './InsightTypes/InsightTypeCards'
import { useInsightsContext } from './InsightsContext'
import AllInsightsDataGrid from './TableContent/AllInsightsDataGrid'
import InsightTypeDataGrid from './TableContent/InsightTypeDataGrid'

const InsightsTableContent = () => {
  const theme = useTheme()
  const featureFlag = useFeatureFlag()
  const legacySnackbar = useLegacySnackbar()
  const snackbar = useSnackbar()
  const dateTime = useDateTime()
  const {
    siteId,
    insights = [],
    insightTypesGroupedByDate = [],
    view,
    t,
    language,
    groupBy,
    isLoading,
    selectedInsightIds = [],
    canWillowUserDeleteInsight,
    onSelectInsightIds,
    onResetInsightIds,
    impactView,
    totalCount = 0,
    onInsightCountDateChange,
    lastInsightStatusCountDate,
  } = useInsightsContext()
  const [selectedAction, setSelectedAction] = useState<string | undefined>(
    undefined
  )
  const isInsightType = groupBy === InsightCardGroups.INSIGHT_TYPE
  const isCardView = isInsightType && view === InsightView.card

  // exclude insights that are not actionable
  // e.g. when user selects insights on "open" tab, and then switches to "resolved" tab,
  // insights with status "Open" will not be displayed and hence cannot be actioned
  const selectedInsightsCanBeActioned = selectedInsightIds
    .map((id) => insights.find((insight) => insight.id === id))
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

  const isSingleSiteSelected = siteId !== ALL_SITES
  const isActionEnabled =
    isSingleSiteSelected &&
    selectedInsightIds.length > 0 &&
    groupBy === InsightCardGroups.ALL_INSIGHTS
  const isDeleteEnabled = canWillowUserDeleteInsight && isActionEnabled
  const getTooltip = (text: string, nextSiteId?: string) =>
    nextSiteId === ALL_SITES
      ? t('plainText.bulkActionsAreDisabled')
      : _.startCase(text)
  const isMultiInsightsIgnoreEnabled =
    isActionEnabled && insightIdsCanBeIgnored.length > 0
  const isMultiInsightsSetToNewEnabled =
    isActionEnabled && insightIdsCanBeSetToNew.length > 0

  useGetInsightSnackbarsStatus(lastInsightStatusCountDate, {
    // Calling the API only for first time navigation page and upcoming days
    enabled:
      featureFlag.hasFeatureToggle('autoResolveSnackbars') &&
      dateTime.now().differenceInDays(lastInsightStatusCountDate) > 0,
    onSuccess: (response: InsightSnackbarsStatus[]) => {
      // Displays a snackbar with insight information based on the selected status.
      const showInsightSnackbar = (
        data: InsightSnackbarsStatus[],
        selectedStatus: InsightWorkflowStatus
      ) => {
        // Retrieve the count and id of the selected status from the data array
        const { count = 0, id } =
          data.find(({ status: dataStatus }) =>
            caseInsensitiveEquals(dataStatus, selectedStatus)
          ) || {}

        // Display the snackbar only if the count is greater than 0
        if (count > 0) {
          const isReadyToResolveStatus = selectedStatus === 'readyToResolve'
          const url = qs.createUrl(
            `/insights`,
            {
              groupBy: 'allInsights',
              sourceType: 'app',
              updatedDate: lastInsightStatusCountDate,
              status: isReadyToResolveStatus
                ? statusMap.default
                : statusMap.inactive,
              selectedStatuses: isReadyToResolveStatus
                ? statusMap.readyToResolve
                : statusMap.resolved,
            },
            { indices: false }
          )
          snackbar.show({
            id: selectedStatus,
            title: isReadyToResolveStatus
              ? `${t('plainText.pendingActions')}:`
              : t('interpolation.resolvedDescriptionText', {
                  count,
                }),
            description: isReadyToResolveStatus
              ? t('interpolation.readyToResolveDescriptionText', {
                  count,
                })
              : '',
            intent: isReadyToResolveStatus ? 'primary' : 'positive',
            autoClose: isReadyToResolveStatus ? false : 4000,
            actions:
              count > 1 ? (
                // Navigating to the all insights table with relevant insight rows based on selected status.
                <StyledLink
                  to={url}
                  onClick={() => snackbar.hide(selectedStatus)}
                >
                  {t('plainText.view')}
                </StyledLink>
              ) : (
                // Render a link to navigate to insight node page
                <StyledLink
                  to={
                    isReadyToResolveStatus
                      ? `/insights/insight/${id}?insightTab=activity`
                      : `/insights/insight/${id}`
                  }
                  onClick={() => snackbar.hide(selectedStatus)}
                >
                  {t('plainText.view')}
                </StyledLink>
              ),
          })
        }
      }
      showInsightSnackbar(response, 'readyToResolve')
      showInsightSnackbar(response, 'resolved')
      // Setting current date once the API call is successful
      onInsightCountDateChange?.(dateTime.now().format('dateLocal'))
    },
  })

  const ignoreInsightsMutation = useUpdateInsightsStatuses(
    {
      siteId,
      insightIds: insightIdsCanBeIgnored,
      newStatus: 'ignored',
    },
    {
      enabled: isSingleSiteSelected,
    }
  )

  const setInsightsStatusesToNewMutation = useUpdateInsightsStatuses(
    {
      siteId,
      insightIds: insightIdsCanBeSetToNew,
      newStatus: 'new',
    },
    {
      enabled: isSingleSiteSelected,
    }
  )

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
        legacySnackbar.show(t('plainText.errorOccurred'), {
          ...snackbarOptions,
          isError: true,
        })
      },
      onSuccess: () => {
        onSelectInsightIds([])

        legacySnackbar.show(
          _.upperFirst(
            t('interpolation.insightsActioned', {
              count: insightIdsCanBeSetToNew.length,
              action: t('plainText.setToNew'),
            })
          ),
          snackbarOptions
        )

        if (
          selectedInsightsCanBeActioned.length -
            insightIdsCanBeSetToNew.length >
          0
        ) {
          const newInsights = selectedInsightsCanBeActioned.filter(
            (i) => i.lastStatus === 'new'
          )

          const inProgressInsights = selectedInsightsCanBeActioned.filter(
            (i) => i.lastStatus === 'inProgress'
          )

          for (const collection of [newInsights, inProgressInsights]) {
            if (collection.length > 0) {
              legacySnackbar.show(
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
        legacySnackbar.show(t('plainText.errorOccurred'), {
          ...snackbarOptions,
          isError: true,
        })
      },
      onSuccess: () => {
        onSelectInsightIds([])
        const insightCountCannotBeIgnored =
          selectedInsightsCanBeActioned.length - insightIdsCanBeIgnored.length
        legacySnackbar.show(
          _.capitalize(
            t('interpolation.insightsActioned', {
              count: insightIdsCanBeIgnored.length,
              action: t('headers.ignored'),
            })
          ),
          snackbarOptions
        )
        if (insightCountCannotBeIgnored > 0) {
          legacySnackbar.show(
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

  const actionIcons: InsightActionIcon[] = [
    {
      icon: 'undo',
      tooltipText: getTooltip(t('plainText.setToNew'), siteId),
      enabled: isMultiInsightsSetToNewEnabled,
      onClick: isMultiInsightsSetToNewEnabled ? handleSetToNewClick : undefined,
      fontSize: '26px',
      marginBottom: '2px',
    },
    {
      icon: 'do_not_disturb_on',
      tooltipText: getTooltip(t('plainText.ignore'), siteId),
      enabled: isMultiInsightsIgnoreEnabled,
      onClick: isMultiInsightsIgnoreEnabled
        ? handleIgnoreInsightsClick
        : undefined,
      filled: false,
      fontSize: theme.font.heading.xl2.fontSize,
    },
    {
      icon: 'feedback',
      tooltipText: getTooltip(t('plainText.report'), siteId),
      enabled: isActionEnabled,
      onClick: isActionEnabled
        ? () => setSelectedAction(InsightActions.report)
        : undefined,
      filled: true,
      fontSize: theme.font.heading.xl2.fontSize,
    },
    {
      icon: 'delete',
      tooltipText: getTooltip(t('plainText.delete'), siteId),
      isRed: true,
      onClick: isDeleteEnabled
        ? () => setSelectedAction(InsightActions.delete)
        : undefined,
      enabled: !!isDeleteEnabled,
      filled: false,
      fontSize: theme.font.display.lg.medium.fontSize,
    },
  ]

  return (
    <ContentLayout
      header={titleCase({
        text:
          groupBy === InsightCardGroups.ALL_INSIGHTS
            ? t('headers.insights')
            : t('plainText.skills'),
        language,
      })}
      headerCount={totalCount}
      bulkActions={actionIcons}
      ignoreInsightsMutation={ignoreInsightsMutation}
      setInsightsStatusesToNewMutation={setInsightsStatusesToNewMutation}
    >
      {isCardView ? (
        <InsightTypeCards
          noData={totalCount === 0}
          insightTypesGroupedByDate={insightTypesGroupedByDate}
          t={t}
          language={language}
          isLoading={isLoading}
          impactView={impactView}
        />
      ) : groupBy === InsightCardGroups.ALL_INSIGHTS ? (
        <AllInsightsDataGrid />
      ) : (
        <InsightTypeDataGrid noData={totalCount === 0} />
      )}
      {/*
        this modal is used for confirming deletion or report one or multiple insights
      */}
      {selectedInsightIds.length > 0 &&
        isSingleSiteSelected &&
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
            onActionChange={setSelectedAction as (action?: string) => void}
            canDeleteInsight={siteId !== ALL_SITES}
            selectedInsightIds={selectedInsightIds}
            onClearSelectedInsightIds={onResetInsightIds}
          />
        )}
    </ContentLayout>
  )
}

export default InsightsTableContent

/**
 * A panel with a header which contains 2 sections:
 * 1. a title text with a count badge
 * 2. a list of bulk actions and a view toggle
 */
const ContentLayout = ({
  header,
  headerCount,
  bulkActions,
  children,
  ignoreInsightsMutation,
  setInsightsStatusesToNewMutation,
}: {
  header: string
  headerCount: number
  bulkActions?: InsightActionIcon[]
  children: React.ReactNode
  ignoreInsightsMutation: UseMutationResult<Insight>
  setInsightsStatusesToNewMutation: UseMutationResult<Insight>
}) => {
  const {
    view,
    onQueryParamsChange,
    language,
    groupBy,
    selectedInsightIds = [],
  } = useInsightsContext()

  return (
    <PanelGroup>
      <Panel
        id="insightTablePanel"
        title={
          <Group gap={0}>
            {titleCase({
              language,
              text: header,
            })}
            <Badge variant="bold" size="xs" color="gray" tw="ml-[6px]">
              {headerCount}
            </Badge>
          </Group>
        }
        headerControls={
          <div tw="flex">
            {ignoreInsightsMutation.status === 'loading' ||
            setInsightsStatusesToNewMutation.status === 'loading' ? (
              <StyledLoader intent="secondary" />
            ) : (
              groupBy === InsightCardGroups.ALL_INSIGHTS &&
              selectedInsightIds.length > 0 &&
              bulkActions?.map(
                ({
                  icon,
                  tooltipText,
                  isRed = false,
                  onClick = () => _.noop,
                  enabled = false,
                  filled = true,
                  fontSize,
                }) => (
                  <IconButton
                    kind="secondary"
                    tw="outline-none p-[0px]"
                    key={icon}
                  >
                    <StyledActionIcon
                      icon={icon}
                      data-tooltip={tooltipText}
                      data-tooltip-position="top"
                      $isRed={isRed}
                      onClick={onClick}
                      data-testid={`insights-action-${icon}`}
                      $enabled={enabled}
                      filled={filled}
                      fontSize={fontSize}
                    />
                  </IconButton>
                )
              )
            )}
            {/**
             * Displaying card and table view options only for insight type
             * For all insights, we will show only table view
             */}
            {groupBy === InsightCardGroups.INSIGHT_TYPE && (
              <StyledSegmentedControl
                data-testid="insightSegmentedControl"
                data={[
                  {
                    iconName: 'view_column',
                    iconOnly: true,
                    label: 'Card',
                    value: InsightView.card,
                  },
                  {
                    iconName: 'view_list',
                    iconOnly: true,
                    label: 'Table',
                    value: InsightView.list,
                  },
                ]}
                onChange={(newView) => onQueryParamsChange?.({ view: newView })}
                value={view}
              />
            )}
          </div>
        }
      >
        <PanelContent tw="h-full">{children}</PanelContent>
      </Panel>
    </PanelGroup>
  )
}

const StyledActionIcon = styled(ActionIcon)(({ theme }) => ({
  lineHeight: theme.font.heading.xl2.lineHeight,
  margin: `0 ${theme.spacing.s8}`,
}))

const StyledLoader = styled(Loader)(({ theme }) => ({
  marginRight: theme.spacing.s20,
  marginTop: theme.spacing.s6,
}))

const StyledSegmentedControl = styled(SegmentedControl)(({ theme }) => ({
  height: '28px',
  alignItems: 'center',
  gap: theme.spacing.s8,
}))

const StyledLink = styled(Link)(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.intent.primary.fg.default,
}))
