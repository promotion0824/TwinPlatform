import {
  Alert as MantineAlert,
  AlertProps as MantineAlertProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import styled from 'styled-components'

import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { Intent } from '../../common'
import { Icon } from '../../misc/Icon'

interface BaseProps {
  /**
   * The intent of the alert.
   * @default primary
   */
  intent?: Intent
  /**
   * The alert has an icon if true.
   * @default false
   */
  hasIcon?: boolean
  /**
   * Determines whether close button should be displayed.
   * @default false
   */
  withCloseButton?: boolean
}

/**
 * Storybook ArgTypes is not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
interface PropsForDocuments {
  /** Alert title */
  title?: MantineAlertProps['title']
  /** Called when the close button is clicked */
  onClose?: MantineAlertProps['onClose']
}

export interface AlertProps
  extends BaseProps,
    WillowStyleProps,
    Omit<
      MantineAlertProps,
      keyof WillowStyleProps | 'withCloseButton' | 'icon' | 'color'
    > {}

const StyledAlert = styled(MantineAlert)<{ $intent: Intent }>(
  ({ $intent, theme }) => ({
    padding: theme.spacing.s12,
    background: theme.color.intent[$intent].bg.subtle.default,
    borderColor: theme.color.intent[$intent].border.default,

    '.mantine-Alert-body': {
      gap: theme.spacing.s4,
    },

    '.mantine-Alert-icon': {
      color: theme.color.intent[$intent].fg.default,
      marginTop: 0,
      marginRight: theme.spacing.s8,
    },

    '.mantine-Alert-closeButton': {
      color: theme.color.neutral.fg.highlight,
    },

    '.mantine-Alert-title': {
      ...theme.font.body.md.semibold,
      color: theme.color.intent[$intent].fg.default,
    },

    '.mantine-Alert-message': {
      ...theme.font.body.md.regular,
      color: theme.color.neutral.fg.default,
    },
  })
)

/**
 * The Alert component is used to display an important static message to the user.
 */
export const Alert = forwardRef<HTMLDivElement, AlertProps>(
  (
    {
      intent = 'primary',
      hasIcon = false,
      withCloseButton = false,
      children,
      ...restProps
    },
    ref
  ) => {
    const getIconName = (intent: Intent) => {
      switch (intent) {
        case 'positive':
          return 'check'
        case 'negative':
          return 'report'
        case 'notice':
          return 'warning'
        default:
          return 'info'
      }
    }

    return (
      <StyledAlert
        $intent={intent}
        withCloseButton={withCloseButton}
        icon={hasIcon ? <Icon icon={getIconName(intent)} /> : null}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        ref={ref}
        data-testid="alert"
      >
        {children}
      </StyledAlert>
    )
  }
)

export const DivForDocumentation = forwardRef<
  HTMLDivElement,
  PropsForDocuments & BaseProps
>(() => <div />)
