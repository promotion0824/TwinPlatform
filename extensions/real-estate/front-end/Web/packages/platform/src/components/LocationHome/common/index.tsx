import { isTouchDevice } from '@willow/common'
import {
  Card,
  CardProps,
  Icon,
  IconProps,
  Stack,
  UnstyledButton,
  UnstyledButtonProps,
} from '@willowinc/ui'
import classNames from 'classnames'
import { forwardRef, ReactNode } from 'react'
import styled, { css } from 'styled-components'
import StatusPlaceholder, {
  TileStatusPlaceholderProps,
} from './StatusPlaceholder'

export interface InteractiveTileProps
  extends UnstyledButtonProps,
    TileStatusPlaceholderProps {
  children: ReactNode
  onClick?: () => void
}

export const InteractiveCard = styled(Card)<{ $isInteractive: boolean }>(
  ({ $isInteractive, theme }) => {
    const isTouch = isTouchDevice()

    return {
      borderRadius: theme.radius.r4,

      ...($isInteractive && !isTouch // in case of Desktop device & having click Event
        ? {
            '*': {
              cursor: 'pointer',
            },

            '.arrow-icon': {
              display: 'none',
            },

            '&:focus-visible': {
              border: `1px solid ${theme.color.state.focus.border}`,
            },

            '&:hover': {
              backgroundColor: theme.color.neutral.bg.accent.hovered,

              '.arrow-icon': {
                display: 'block',
              },
            },
          }
        : {
            cursor: 'default',
          }),
    }
  }
)

export const MutedIcon = styled(Icon)(({ theme }) => ({
  color: theme.color.neutral.fg.muted,
}))
/**
 * used in a tile with click event in mobile & desktop
 * Action:
 * - show on hover on desktop
 * - all times on mobile
 */
export const ArrowIcon = ({
  className,
  ...restProps
}: Omit<IconProps, 'icon'>) => (
  <MutedIcon
    className={classNames(className, 'arrow-icon')}
    icon="arrow_forward"
    {...restProps}
  />
)

export const InteractiveTile = forwardRef<HTMLDivElement, InteractiveTileProps>(
  (
    {
      children,
      onClick,
      error,
      loading,
      empty,
      defaultHeight,
      title,
      ...restProps
    },
    ref
  ) =>
    error || empty || loading ? (
      <StatusPlaceholder
        error={error}
        loading={loading}
        empty={empty}
        defaultHeight={defaultHeight}
        title={title}
      />
    ) : (
      <UnstyledButton
        onClick={onClick}
        renderRoot={(props) => (
          <InteractiveCard
            background="accent"
            $isInteractive={!!onClick}
            ref={ref}
            {...props}
          />
        )}
        tabIndex={onClick ? 0 : undefined}
        {...restProps}
      >
        <Stack gap="s4" h="100%" px="s12" py="s8">
          {children}
        </Stack>
      </UnstyledButton>
    )
)

export interface TileProps extends CardProps, TileStatusPlaceholderProps {}

export const Tile = forwardRef<HTMLDivElement, TileProps>(
  (
    { children, error, loading, empty, defaultHeight, title, ...restProps },
    ref
  ) =>
    error || empty || loading ? (
      <StatusPlaceholder
        error={error}
        loading={loading}
        empty={empty}
        defaultHeight={defaultHeight}
        title={title}
      />
    ) : (
      <Card
        background="accent"
        css={css(({ theme }) => ({ borderRadius: theme.radius.r4 }))}
        ref={ref}
        {...restProps}
      >
        <Stack gap="s4" h="100%" px="s12" py="s8">
          {children}
        </Stack>
      </Card>
    )
)
