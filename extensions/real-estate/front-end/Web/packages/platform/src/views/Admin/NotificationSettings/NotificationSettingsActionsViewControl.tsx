import { NotificationActions, titleCase } from '@willow/common'
import { Text } from '@willow/ui'
import { Icon, IconName, Menu, MenuProps } from '@willowinc/ui'
import { TFunction, useTranslation } from 'react-i18next'
import styled from 'styled-components'

const makeNotificationSettingsActionLists = ({
  isCustomerAdmin,
  t,
  language,
  onClick,
}: {
  isCustomerAdmin: boolean
  t: TFunction
  language: string
  onClick: { [key in NotificationActions]?: () => void }
}) => {
  const actionLists: Array<{
    id: NotificationActions
    text: string
    icon: IconName
    isDelete?: boolean
    filled?: boolean
    isDisabled?: boolean
    isHidden?: boolean
  }> = [
    // TODO: View notifications will be completed as part of below story
    // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/135913
    {
      id: NotificationActions.view,
      icon: 'preview' as const,
      text: 'plainText.viewNotification',
      filled: false,
      isHidden: isCustomerAdmin,
    },
    // TODO: Edit notifications will be completed as part of below story
    // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/135550
    {
      id: NotificationActions.edit,
      icon: 'edit' as const,
      text: 'plainText.editNotification',
      isDisabled: !isCustomerAdmin,
    },
    {
      id: NotificationActions.delete,
      icon: 'delete' as const,
      text: 'plainText.deleteNotification',
      filled: false,
      isDelete: true,
      isDisabled: !isCustomerAdmin,
    },
  ]

  return actionLists.map(
    ({
      id,
      icon,
      text,
      isDelete = false,
      filled = true,
      isDisabled = false,
      isHidden = false,
    }) => (
      <>
        {!isHidden && (
          <>
            <Menu.Item
              disabled={isDisabled}
              prefix={
                <StyledIcon
                  icon={icon}
                  $isDelete={isDelete}
                  $isDisabled={isDisabled}
                  filled={filled}
                />
              }
              id={id}
              onClick={onClick[id]}
            >
              <StyledText $isDelete={isDelete} $isDisabled={isDisabled}>
                {titleCase({ language, text: t(text) })}
              </StyledText>
            </Menu.Item>
          </>
        )}
      </>
    )
  )
}

/**
 * A widget to be shown when user click on the three vertical dots at notification setting row level .
 * It contains various actions like edit, delete, view notifications.
 */
function NotificationSettingsActionsViewControl({
  className,
  children,
  onToggleActionsView,
  opened,
  isCustomerAdmin,
  onDeleteNotificationSettings,
  onEditNotificationSettings,
  ...restProps
}: {
  className?: string
  opened: boolean
  isCustomerAdmin: boolean
  onToggleActionsView: (toggle: boolean) => void
  onEditNotificationSettings: () => void
  onDeleteNotificationSettings: () => void
} & Partial<MenuProps>) {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const notificationActionHandlers = {
    [NotificationActions.delete]: onDeleteNotificationSettings,
    [NotificationActions.edit]: onEditNotificationSettings,
  }

  return (
    <Menu
      opened={opened}
      onChange={(isOpen) => {
        onToggleActionsView?.(isOpen)
      }}
      {...restProps}
    >
      <Menu.Target>{children}</Menu.Target>
      <Menu.Dropdown
        css={`
          transform: translateX(-11px);
        `}
      >
        <ActionsViewContainer className={className}>
          {makeNotificationSettingsActionLists({
            isCustomerAdmin,
            t,
            language,
            onClick: notificationActionHandlers,
          })}
        </ActionsViewContainer>
      </Menu.Dropdown>
    </Menu>
  )
}

export default NotificationSettingsActionsViewControl

const ActionsViewContainer = styled.div({
  width: 'max-content',
  display: 'flex',
  flexDirection: 'column',
  zIndex: 0,
})

const StyledIcon = styled(Icon)<{
  $isDelete?: boolean
  $isDisabled?: boolean
}>(({ theme, $isDelete = false, $isDisabled = false }) => ({
  '&&&': {
    color: $isDelete
      ? theme.color.intent.negative.fg.default
      : theme.color.neutral.fg.default,

    ...($isDisabled && {
      color: theme.color.state.disabled.fg,
    }),
  },
}))

const StyledText = styled(Text)<{
  $isDelete?: boolean
  $isDisabled?: boolean
}>(({ theme, $isDelete = false, $isDisabled = false }) => ({
  color: $isDelete
    ? theme.color.intent.negative.fg.default
    : theme.color.neutral.fg.default,

  ...($isDisabled && {
    color: theme.color.state.disabled.fg,
  }),
}))
