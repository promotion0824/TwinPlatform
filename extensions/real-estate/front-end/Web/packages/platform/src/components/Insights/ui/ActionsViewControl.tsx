import _ from 'lodash'
import { styled } from 'twin.macro'
import { useTranslation } from 'react-i18next'
import { Fragment, ReactNode, useEffect } from 'react'
import {
  Icon,
  Loader,
  IconName,
  Menu,
  Indicator,
  MenuProps,
} from '@willowinc/ui'
import { isWillowUser, titleCase } from '@willow/common'
import { Text, useUser, useFeatureFlag, useSnackbar } from '@willow/ui'
import { useHistory } from 'react-router'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import {
  Insight,
  InsightWorkflowStatus,
} from '@willow/common/insights/insights/types'
import { useIsMutating } from 'react-query'
import useUpdateInsightsStatuses from '../../../hooks/Insight/useUpdateInsightsStatuses'
import { useSite } from '../../../providers'

export enum InsightActions {
  investigate = 'investigate',
  newTicket = 'newTicket',
  readyToResolve = 'readyToResolve',
  resolve = 'resolve',
  setToNew = 'setToNew',
  ignore = 'ignore',
  report = 'report',
  delete = 'delete',
  ticket = 'ticket',
}

const KEY_NAME_ESC = 'Escape'
const KEY_EVENT_TYPE = 'keyup'
const WHEEL = 'wheel'

/**
 * A widget to be shown when user click on the three vertical dots at insights row level and
 * insight drawer. It contains various actions like investigate, new ticket,
 * resolve, ignore, report and delete which can be performed on insight
 * Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/79584
 */
function ActionsViewControl({
  selectedInsight,
  siteId,
  lastStatus,
  assetId,
  floorId,
  className,
  onCreateTicketClick,
  onDeleteClick,
  onReportClick,
  onResolveClick,
  onModalClose,
  children,
  canDeleteInsight = false,
  onToggleActionsView,
  opened,
  ...restProps
}: {
  children: ReactNode
  selectedInsight: Insight
  siteId: string
  lastStatus: InsightWorkflowStatus
  assetId?: string
  floorId?: string
  className?: string
  onCreateTicketClick: () => void
  onReportClick?: () => void
  onDeleteClick?: () => void
  onResolveClick?: () => void
  onModalClose?: () => void
  canDeleteInsight?: boolean
  opened: boolean
  onToggleActionsView: (toggle: boolean) => void
} & Partial<MenuProps>) {
  const site = useSite()
  const user = useUser()
  const [{ tab: tableTab }] = useMultipleSearchParams(['tab'])
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const history = useHistory()
  const featureFlags = useFeatureFlag()
  const snackbar = useSnackbar()
  const investigateEnabled =
    assetId &&
    floorId &&
    featureFlags.hasFeatureToggle('investigateAssetDisabled') === false
  const snackbarOption = {
    isToast: true,
    closeButtonLabel: t('plainText.dismiss'),
  }
  const isMutating = useIsMutating(['insightsStatuses']) > 0

  const setStatusToNewMutation = useUpdateInsightsStatuses({
    siteId,
    insightIds: [selectedInsight.id],
    newStatus: 'new',
  })
  const handleClickSetToNew = () => {
    let errorMessage: string | undefined
    let height: string | undefined

    /**
     * If an Insight occurs again and if it is either in open or resolved state, we do not allow the insight to be set to New,
     * provided the user action is performed from Ignored Tab in Insights Table.
     */
    const isSetInsightStatusToNewDisabled =
      tableTab === 'acknowledged' && selectedInsight.previouslyIgnored > 0

    if (isSetInsightStatusToNewDisabled && lastStatus === 'open') {
      errorMessage = t('plainText.thisInsightGeneratedAgainInActiveTab')
      height = '98px'
    }

    if (isSetInsightStatusToNewDisabled && lastStatus === 'resolved') {
      errorMessage = t('plainText.thisInsightNoLongerActive')
      height = '84px'
    }

    if (errorMessage != null) {
      snackbar.show(
        <>
          <p>{t('plainText.canNotSetStatusNew')}.</p>
          <p>{errorMessage}</p>
        </>,
        {
          isToast: true,
          isError: true,
          height,
          closeButtonLabel: t('plainText.dismiss'),
        }
      )
      return
    }

    setStatusToNewMutation.mutate(undefined, {
      onError: () => {
        snackbar.show(t('plainText.errorOccurred'))
      },
      onSuccess: () => {
        snackbar.show(t('plainText.insightStatusSetToNew'), snackbarOption)
      },
    })
  }

  const ignoreInsightMutation = useUpdateInsightsStatuses({
    siteId,
    insightIds: [selectedInsight.id],
    newStatus: 'ignored',
  })

  const handleClickIgnore = () => {
    ignoreInsightMutation.mutate(undefined, {
      onError: () => {
        snackbar.show(t('plainText.errorOccurred'))
      },
      onSuccess: () => {
        snackbar.show(
          t('interpolation.insightsActioned', {
            count: 1,
            action: t('headers.ignored'),
          }),
          snackbarOption
        )
        onModalClose?.()
      },
    })
  }

  /**
   * This action list is just a placeholder
   * All the corresponding actions will be added as part of different story
   */
  const actionLists: Array<{
    id: InsightActions
    icon: IconName
    text: string
    isDelete?: boolean
    borderBottom?: boolean
    isDisabled?: boolean
    filled?: boolean
    onClick?: () => void
    handleClick?: () => void
  }> = [
    ...(investigateEnabled
      ? [
          {
            id: InsightActions.investigate,
            icon: 'open_in_new' as const,
            text: 'plainText.viewInViewer',
            isDisabled: floorId == null && assetId == null,
            onClick: () =>
              history.push({
                pathname: `/sites/${siteId}/floors/${floorId}`,
                search: new URLSearchParams({
                  assetId,
                  tab: 'details',
                  isInsightStatsLayerOn: '1',
                }).toString(),
              }),
          },
        ]
      : []),
    {
      id: InsightActions.newTicket,
      icon: 'assignment' as const,
      text: 'plainText.newTicket',
      onClick: onCreateTicketClick,
      isDisabled:
        ['resolved', 'ignored'].includes(lastStatus) ||
        site.features.isTicketingDisabled,
    },
    {
      id: InsightActions.resolve,
      icon: 'check',
      text: 'plainText.resolve',
      // https://dev.azure.com/willowdev/Unified/_workitems/edit/80180
      // an insight can only be resolved if it is in progress;
      isDisabled: !['inProgress', 'readyToResolve'].includes(lastStatus),
      borderBottom: true,
      onClick: onResolveClick,
    },
    {
      id: InsightActions.setToNew,
      icon: 'undo' as const,
      text: 'plainText.setToNew',
      isDisabled: !['open', 'ignored'].includes(lastStatus), // enabled only for open && ignored Insights....
      handleClick: handleClickSetToNew,
    },
    {
      id: InsightActions.ignore,
      icon: 'do_not_disturb_on' as const,
      text: 'plainText.ignore',
      isDisabled: lastStatus !== 'open' && lastStatus !== 'new',
      handleClick: handleClickIgnore,
      filled: false,
    },
    {
      id: InsightActions.report,
      icon: 'feedback' as const,
      text: 'plainText.report',
      isDisabled: false,
      onClick: onReportClick,
      filled: true,
    },
    // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/78758
    // only available for willow users
    ...(isWillowUser(user?.email) && canDeleteInsight
      ? [
          {
            id: InsightActions.delete,
            icon: 'delete' as const,
            text: 'plainText.delete',
            isDelete: true,
            onClick: onDeleteClick,
            filled: false,
          },
        ]
      : []),
  ]

  // keystroke of "Esc", click outside of table view action controls, or any wheel movement
  // will close table view controls, but not when mutation is in progress as it contains
  // loading indicator
  useEffect(() => {
    const handleEscKey = (event) => {
      if (event.key === KEY_NAME_ESC || isMutating) {
        onToggleActionsView(false)
      }
    }
    const handleWheel = () => {
      if (isMutating) {
        onToggleActionsView(false)
      }
    }
    document.addEventListener(KEY_EVENT_TYPE, handleEscKey)
    document.addEventListener(WHEEL, handleWheel)

    return () => {
      document.removeEventListener(KEY_EVENT_TYPE, handleEscKey)
      document.removeEventListener(WHEEL, handleWheel)
    }
  }, [isMutating])

  const isReadyToResolve =
    featureFlags.hasFeatureToggle('readyToResolve') &&
    lastStatus === InsightActions.readyToResolve

  return (
    <Menu
      opened={opened}
      onChange={(isOpen) => {
        onToggleActionsView?.(isOpen)
      }}
      withinPortal={false}
      {...restProps}
    >
      <Menu.Target>{children}</Menu.Target>
      {/* Design wants to have the right edge of dropdown align with edge of the button */}
      <Menu.Dropdown
        css={`
          transform: translateX(-11px);
        `}
      >
        <ActionsViewContainer className={className}>
          {isReadyToResolve && (
            <Menu.Item disabled>
              <IconTextContainer>
                <Indicator position="middle-start" tw="ml-[10px]">
                  <StyledText tw="ml-[15px]" $isMuted>
                    {t('plainText.insightIsReadyToResolve')}
                  </StyledText>
                </Indicator>
              </IconTextContainer>
            </Menu.Item>
          )}
          {isReadyToResolve && <Menu.Divider />}
          {actionLists.map(
            ({
              id,
              icon,
              text,
              isDelete = false,
              borderBottom = false,
              isDisabled = false,
              filled = true,
              onClick,
              handleClick,
            }) => {
              // show loading icon if mutation status is loading and buttonId is setToNew
              const isSetToNewLoading =
                setStatusToNewMutation.status === 'loading' &&
                id === InsightActions.setToNew
              const isIgnoreLoading =
                ignoreInsightMutation.status === 'loading' &&
                id === InsightActions.ignore

              const isHighlighted = featureFlags.hasFeatureToggle(
                'readyToResolve'
              )
                ? icon === 'check' &&
                  lastStatus === InsightActions.readyToResolve
                : false

              return (
                <Fragment key={id}>
                  <Menu.Item
                    disabled={isDisabled}
                    prefix={
                      isSetToNewLoading || isIgnoreLoading ? (
                        <Loader intent="secondary" />
                      ) : (
                        <StyledIcon
                          icon={icon}
                          $isDelete={isDelete}
                          $isHighlighted={isHighlighted}
                          $isDisabled={isDisabled}
                          filled={filled}
                        />
                      )
                    }
                    id={id}
                    onClick={(event) => {
                      event.stopPropagation()
                      if (isDisabled || isSetToNewLoading || isIgnoreLoading) {
                        return
                      }

                      if (handleClick) {
                        handleClick()
                      } else {
                        onClick?.()
                      }
                    }}
                  >
                    {!isSetToNewLoading && !isIgnoreLoading && (
                      <StyledText
                        $isDelete={isDelete}
                        $isDisabled={isDisabled}
                        $isHighlighted={isHighlighted}
                      >
                        {titleCase({
                          text: t(text),
                          language,
                        }).replace('3d', '3D')}
                      </StyledText>
                    )}
                  </Menu.Item>
                  {borderBottom && <Menu.Divider />}
                </Fragment>
              )
            }
          )}
        </ActionsViewContainer>
      </Menu.Dropdown>
    </Menu>
  )
}

export default ActionsViewControl

const IconTextContainer = styled.div(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  color: theme.color.neutral.fg.default,

  '&:hover': {
    backgroundColor: theme.color.neutral.bg.accent.hovered,
  },

  // overwrite Maintine's default margin: auto style
  '&&& .mantine-Button-inner': {
    margin: '0px',
  },
}))

const StyledIcon = styled(Icon)<{
  $isDelete?: boolean
  $isDisabled?: boolean
  $isHighlighted?: boolean
}>(
  ({
    theme,
    $isDisabled = false,
    $isHighlighted = false,
    $isDelete = false,
  }) => ({
    '&&&': {
      color: $isDelete
        ? theme.color.intent.negative.fg.default
        : theme.color.neutral.fg.default,

      ...($isDisabled && {
        color: theme.color.state.disabled.fg,
      }),

      ...($isHighlighted && {
        color: theme.color.intent.primary.fg.default,
      }),
    },
  })
)

const StyledText = styled(Text)<{
  $isDelete?: boolean
  $isDisabled?: boolean
  $isHighlighted?: boolean
  $isMuted?: boolean
}>(
  ({
    theme,
    $isDelete = false,
    $isDisabled = false,
    $isHighlighted = false,
    $isMuted = false,
  }) => ({
    color: $isDelete
      ? theme.color.intent.negative.fg.default
      : theme.color.neutral.fg.default,

    ...($isDisabled && {
      color: theme.color.state.disabled.fg,
    }),

    ...($isHighlighted && {
      color: theme.color.intent.primary.fg.default,
    }),

    ...($isMuted && {
      color: theme.color.neutral.fg.muted,
    }),
  })
)

const ActionsViewContainer = styled.div(({ theme }) => ({
  width: 'max-content',
  display: 'flex',
  flexDirection: 'column',
  zIndex: 0,
}))
