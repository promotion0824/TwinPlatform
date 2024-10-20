import {
  Badge as MantineBadge,
  BadgeProps as MantineBadgeProps,
} from '@mantine/core'
import { ForwardedRef, forwardRef } from 'react'
import styled, { css } from 'styled-components'
import { Colors } from '../../common'
import { rem } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { BadgeSizes, BadgeVariants, getColors } from './getColors'

export interface BadgeProps
  extends WillowStyleProps,
    Omit<
      MantineBadgeProps,
      | keyof WillowStyleProps
      | 'size'
      | 'color'
      | 'variant'
      | 'leftSection'
      | 'rightSection'
    > {
  children?: MantineBadgeProps['children']
  /** @default 'bold' */
  variant?: BadgeVariants
  /** @default 'gray' */
  color?: Colors
  /** @default 'xs */
  size?: BadgeSizes
  prefix?: MantineBadgeProps['leftSection']
  suffix?: MantineBadgeProps['rightSection']
}

/**
 * `Badge` is generally used to display a count, status, or a notification.
 */
export const Badge = forwardRef<HTMLDivElement, BadgeProps>(
  (
    {
      variant = 'bold',
      color = 'gray',
      size = 'xs',
      prefix,
      suffix,
      ...restProps
    },
    ref
  ) => {
    return (
      <StyledBadge
        ref={ref}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        $color={color}
        $variant={variant}
        $size={size}
        variant={getMantineVariant(variant)}
        leftSection={prefix}
        rightSection={suffix}
        data-testid="badge"
      />
    )
  }
)

const StyledBadge = styled(MantineBadge)<
  MantineBadgeProps & { ref: ForwardedRef<HTMLDivElement> } & {
    $color: Exclude<BadgeProps['color'], undefined>
    $variant: Exclude<BadgeProps['variant'], undefined>
    $size: Exclude<BadgeProps['size'], undefined>
  }
>(({ theme, $color, $variant, $size }) => {
  const colors = getColors($color, $variant, theme)

  const paddingMap = {
    xs: `0 ${theme.spacing.s6}`,
    sm: `0 ${theme.spacing.s6}`,
    md: `${theme.spacing.s2} ${theme.spacing.s8}`,
    lg: `${theme.spacing.s4} ${theme.spacing.s12}`,
  } as const

  return css`
    ${theme.font.body.md.regular};
    text-transform: initial;
    background-color: ${colors.bgColor};
    color: ${colors.textColor};
    border-color: ${colors.borderColor};
    border-radius: 100px;
    height: ${getSize($size)};
    padding: ${paddingMap[$size]};

    &::before {
      background-color: ${colors.indicatorColor};
      flex-shrink: 0;
      height: ${rem(10)};
      width: ${rem(10)};
      margin-right: ${theme.spacing.s8};
    }

    .mantine-Badge-section {
      /* make the content vertically centered */
      display: flex;
    }
    .mantine-Badge-section[data-position='left'] {
      margin-right: ${theme.spacing.s8};
    }

    .mantine-Badge-section[data-position='right'] {
      margin-left: ${theme.spacing.s8};
    }
  `
})

const getMantineVariant = (
  variant: Exclude<BadgeProps['variant'], undefined>
) => {
  const variantMap = {
    dot: 'dot',
    bold: 'filled',
    muted: 'filled',
    subtle: 'filled',
    outline: 'outline',
  } as const

  return variantMap[variant]
}

const getSize = (size: Exclude<BadgeProps['size'], undefined>) => {
  const sizeMap = {
    xs: rem(16),
    sm: rem(20),
    md: rem(24),
    lg: rem(28),
  } as const

  return sizeMap[size]
}
