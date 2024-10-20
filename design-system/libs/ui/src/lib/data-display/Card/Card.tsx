import { Box, BoxProps } from '@mantine/core'
import { ReactNode, forwardRef } from 'react'
import styled, { css } from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export type Background = 'base' | 'panel' | 'accent'
export type Radius = 'r2' | 'r4'
export type Shadow = 's1' | 's2' | 's3'

export interface CardProps
  extends WillowStyleProps,
    Omit<BoxProps, keyof WillowStyleProps> {
  /**
   * Recommend to have a Group or Stack container with a list
   * of children to be displayed in card.
   */
  children?: ReactNode
  /**
   * The background color of the card.
   * @default 'base'
   */
  background?: Background
  /**
   * The border outside the card.
   * @default true
   */
  withBorder?: boolean
  /**
   * The radius of the card. Default to be square.
   * @default undefined
   */
  radius?: Radius
  /**
   * The shadow of the card. Default to be no shadow.
   * @default undefined
   */
  shadow?: Shadow
}

/**
 * `Card` is a container component with a set of style options.
 * It does not contain any layout style so it's recommended to
 * have a `Group` or `Stack` component as top level children when using it.
 */
export const Card = forwardRef<HTMLDivElement, CardProps>(
  (
    { background = 'base', withBorder = true, radius, shadow, ...restProps },
    ref
  ) => (
    <StyledCard
      $background={background}
      $withBorder={withBorder}
      $radius={radius}
      $shadow={shadow}
      {...restProps}
      {...useWillowStyleProps(restProps)}
      ref={ref}
    />
  )
)

const StyledCard = styled(Box<'div'>)<{
  $background: Background
  $withBorder: boolean
  $radius?: Radius
  $shadow?: Shadow
}>(
  ({ theme, $background, $withBorder, $radius, $shadow }) => css`
    background: ${theme.color.neutral.bg[$background].default};
    border: ${$withBorder
      ? `1px solid ${theme.color.neutral.border.default}`
      : 'none'};
    border-radius: ${$radius ? theme.radius[$radius] : '0px'};
    box-shadow: ${$shadow ? theme.shadow[$shadow] : 'none'};

    width: 100%;
    height: 100%;
  `
)
