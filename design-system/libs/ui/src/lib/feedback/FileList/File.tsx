import { forwardRef } from 'react'
import styled, { css } from 'styled-components'

import { Button, ButtonProps } from '../../buttons/Button'
import { CloseButton } from '../../common'
import { Group } from '../../layout/Group'
import { Stack } from '../../layout/Stack'
import { Box, BoxProps } from '../../misc/Box'
import { Icon } from '../../misc/Icon'
import { rem, WillowStyleProps } from '../../utils'
import { Loader } from '../Loader'

export interface FileProps
  extends WillowStyleProps,
    Omit<BoxProps, 'hiddenFrom' | 'renderRoot'> {
  /** File name. */
  title?: string
  /** File size or other information such as failure message. */
  children?: React.ReactNode

  /** Determines whether the component should be disabled. */
  disabled?: boolean
  /** Determines whether the component should be in a loading. */
  loading?: boolean
  /** Determines whether the component should be in a failed state. */
  failed?: boolean
  /** Called when the close button is clicked. */
  onClose?: () => void
  /** Props for the close button. */
  closeButtonProps?: ButtonProps
  /** Called when the retry button is clicked. */
  onRetry?: () => void
  /** Props for the retry button. */
  retryButtonProps?: ButtonProps
}

/**
 * `File` is a component that displays information and status for a file.
 */
export const File = forwardRef<HTMLDivElement, FileProps>(
  (
    {
      title,
      children,
      disabled = false,
      loading = false,
      failed = false,

      onRetry,
      retryButtonProps,
      onClose,
      closeButtonProps,
      ...restProps
    },
    ref
  ) => {
    const icon = loading ? (
      <Loader />
    ) : failed ? (
      <Icon icon="report" c="intent.negative.fg.default" />
    ) : (
      <Icon
        icon="check_circle"
        c={disabled ? 'state.disabled.fg' : 'intent.positive.fg.default'}
      />
    )

    return (
      <Container
        role="status"
        aria-live="polite"
        $failed={failed}
        ref={ref}
        {...restProps}
      >
        <Group align="flex-start">
          {icon}
          <Stack gap={0} flex={1} miw={0}>
            <Title $disabled={disabled}>{title}</Title>
            <Description $disabled={disabled} $failed={failed}>
              {children}
            </Description>
          </Stack>

          <Group>
            {failed && (
              // TODO: use Button with background = 'none' when available
              <Button
                prefix={<Icon icon="autorenew" />}
                kind="primary"
                background="transparent"
                onClick={onRetry}
                p={0}
                css={{
                  background: 'none !important',
                }}
                {...retryButtonProps}
              >
                {retryButtonProps?.children ?? 'Retry'}
              </Button>
            )}
            {!disabled && (
              // TODO: use updated CloseButton without style override when naked button is available
              <CloseButton
                onClick={onClose}
                c="neutral.fg.muted"
                p={0}
                css={{
                  background: 'none !important',
                }}
                {...closeButtonProps}
              />
            )}
          </Group>
        </Group>
      </Container>
    )
  }
)

const Container = styled(Box<'div'>)<{
  $failed: boolean
}>(
  ({ theme, $failed }) => css`
    border: 1px solid
      ${$failed
        ? theme.color.intent.negative.border.default
        : theme.color.neutral.border.default};
    border-radius: ${theme.radius.r4};
    box-shadow: ${theme.shadow.s2};
    background-color: ${theme.color.neutral.bg.accent.default};
    padding-left: ${theme.spacing.s16};
    padding-right: ${theme.spacing.s16};
    padding-top: ${rem(11)}; // offset border width
    padding-bottom: ${rem(11)}; // offset border width
    ${theme.font.body.md.regular}
  `
)

const Title = styled.div<{
  $disabled: boolean
}>(
  ({ theme, $disabled }) => css`
    text-overflow: ellipsis;
    overflow: hidden;
    white-space: nowrap;

    color: ${$disabled
      ? theme.color.state.disabled.fg
      : theme.color.neutral.fg.default};
  `
)

const Description = styled.div<{
  $disabled: boolean
  $failed: boolean
}>(
  ({ theme, $disabled, $failed }) => css`
    word-break: break-word;
    color: ${$disabled
      ? theme.color.state.disabled.fg
      : $failed
      ? theme.color.intent.negative.fg.default
      : theme.color.neutral.fg.muted};
  `
)
