import { ReactNode, forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'

import { isTouchDevice } from '@willow/common'
import {
  Button,
  ButtonProps,
  Card,
  CardProps,
  Group,
  Icon,
  IconButton,
  Loader,
  Stack,
  useDisclosure,
} from '@willowinc/ui'
import {
  MoreButtonDropdown,
  MoreButtonDropdownOption,
  MoreButtonDropdownOptionDivider,
} from '../../../../../ui/src/components/MoreButtonDropdown/MoreButtonDropdown'
import WarningModal from '../WarningModal/WarningModal'
import EditingModeOverlay from './EditingModeOverlay'

export interface WidgetCardProps extends CardProps {
  title: string
  children: ReactNode
  navigationButtonContent?: string
  navigationButtonLink?: string
  navigationButtonProps?: Partial<ButtonProps>
  isEditingMode?: boolean
  isDraggingMode?: boolean
  isLoading?: boolean
  /**
   * @example
   * ```
   * <MoreButtonDropdownOption
   *   onClick={() => {}}
   *   prefix={<Icon />}
   * >
   *   Button
   * </MoreButtonDropdownOption>
   * ```
   */
  actions?: ReactNode
  /** Ref for the draggable handle when it's editing mode */
  draggableRef: React.RefObject<HTMLButtonElement>
  /**
   * Callback function will be invoked when Edit Widget button is clicked.
   * There are two edit buttons, one in the dropdown and another when `isEditingMode = true`.
   * Widget will not have Edit Widget button if this callback is not provided.
   */
  onWidgetEdit?: () => void
  /**
   * Callback function will be invoked when Delete Widget button is clicked.
   * There are two delete buttons, one in the dropdown and another when `isEditingMode = true`.
   * Widget will not have Delete Widget button if this callback is not provided.
   */
  onWidgetDelete?: () => void
}

const WidgetCard = forwardRef<HTMLDivElement, WidgetCardProps>(
  // eslint-disable-next-line complexity
  (
    {
      title,
      navigationButtonContent,
      navigationButtonLink,
      navigationButtonProps,
      actions,
      children,
      isEditingMode = false,
      isDraggingMode = false,
      isLoading = false,
      draggableRef,
      onWidgetEdit,
      onWidgetDelete,
      ...rest
    },
    ref
  ) => {
    const [opened, { open: openDeletionModal, close }] = useDisclosure(false)
    const { t } = useTranslation()

    return (
      <>
        <Card
          h="fit-content"
          w="100%"
          background="panel"
          css={css(({ theme }) => ({
            '&:hover': {
              [`.${NAVIGATION_BUTTON_CLASSNAME}`]: {
                visibility: 'visible',
              },
            },

            borderRadius: theme.radius.r4,
          }))}
          ref={ref}
          {...rest}
        >
          <Stack
            pt="s12"
            pb="s12"
            pl="s16"
            pr="s16"
            gap="s12"
            h="100%"
            w="100%"
          >
            <Group gap="s8" w="100%">
              {isDraggingMode && (
                <IconButton
                  icon="drag_indicator"
                  kind="secondary"
                  background="transparent"
                  css={{
                    cursor: 'grab',
                    display: isEditingMode ? 'block' : 'none', // to keep the ref for the draggable handle
                  }}
                  ref={draggableRef}
                />
              )}

              {title && <Title>{title}</Title>}
              {!isLoading && !isEditingMode && navigationButtonContent && (
                <Button
                  kind="secondary"
                  suffix={<Icon icon="arrow_forward" />}
                  className={NAVIGATION_BUTTON_CLASSNAME}
                  href={navigationButtonLink}
                  target="_blank"
                  css={{
                    // always visible for touch device
                    visibility: isTouchDevice() ? 'visible' : 'hidden',
                  }}
                  {...navigationButtonProps}
                >
                  {navigationButtonContent}
                </Button>
              )}
              {!isLoading &&
                (isEditingMode ? (
                  <>
                    {!!onWidgetEdit && (
                      <IconButton
                        icon="edit"
                        kind="secondary"
                        background="transparent"
                        onClick={onWidgetEdit}
                      />
                    )}
                    {!!onWidgetDelete && (
                      <IconButton
                        icon="delete"
                        kind="negative"
                        background="transparent"
                        onClick={openDeletionModal}
                      />
                    )}
                  </>
                ) : (
                  // if none of the buttons are provided, it will not render the dropdown
                  (onWidgetEdit || onWidgetDelete || actions) && (
                    <MoreActionsDropdown
                      actions={actions}
                      onEdit={onWidgetEdit}
                      // if deletable, it will open the deletion modal, otherwise
                      // it will pass undefine which will hide the deletion button in the dropdown
                      onDelete={onWidgetDelete ? openDeletionModal : undefined}
                    />
                  )
                ))}
            </Group>
            {isLoading ? (
              <Group p="s32" justify="center">
                <Loader intent="secondary" />
              </Group>
            ) : (
              <EditingModeOverlay $isEditingMode={isEditingMode}>
                {children}
              </EditingModeOverlay>
            )}
          </Stack>
        </Card>
        {!!onWidgetDelete && (
          <WarningModal
            opened={opened}
            onClose={close}
            onWarningConfirm={onWidgetDelete}
            confirmationButtonLabel={t('plainText.remove')}
          >
            {t('interpolation.confirmToRemoveWidget', { title })}?
          </WarningModal>
        )}
      </>
    )
  }
)

/**
 * The dropdown to toggle Edit and Delete Widget,
 * and any customized actions.
 */
const MoreActionsDropdown = ({
  actions,
  onEdit,
  onDelete,
}: Pick<WidgetCardProps, 'actions'> & {
  onEdit?: () => void
  onDelete?: () => void
}) => {
  const { t } = useTranslation()

  return (
    <MoreButtonDropdown
      targetButtonProps={{
        background: 'transparent',
      }}
    >
      {actions && (
        <>
          {actions}
          <MoreButtonDropdownOptionDivider />
        </>
      )}
      {onEdit && (
        <MoreButtonDropdownOption
          onClick={onEdit}
          prefix={<Icon icon="edit" />}
          css={{
            textTransform: 'capitalize',
          }}
        >
          {t('labels.editWidget')}
        </MoreButtonDropdownOption>
      )}
      {onDelete && (
        <MoreButtonDropdownOption
          onClick={onDelete}
          prefix={<Icon icon="delete" />}
          intent="negative"
          css={{
            textTransform: 'capitalize',
          }}
        >
          {t('labels.deleteWidget')}
        </MoreButtonDropdownOption>
      )}
    </MoreButtonDropdown>
  )
}

const Title = styled.h3(
  ({ theme }) => css`
    ${theme.font.body.lg.semibold};
    color: ${theme.color.neutral.fg.default};
    flex-grow: 1;
    margin: 0;
  `
)

const NAVIGATION_BUTTON_CLASSNAME = 'navigation_button'

export default WidgetCard
