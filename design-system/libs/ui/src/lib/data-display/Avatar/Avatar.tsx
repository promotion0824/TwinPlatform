import {
  Avatar as MantineAvatar,
  AvatarProps as MantineAvatarProps,
} from '@mantine/core'
import { ForwardedRef, ReactNode, forwardRef } from 'react'
import styled, { css } from 'styled-components'

import { Theme, useTheme } from '@willowinc/theme'
import { Colors, Sizes, Variants } from '../../common'
import { Icon } from '../../misc/Icon'
import { rem } from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { Tooltip } from '../../overlays/Tooltip'

type AvatarPredefinedSizes = Extract<Sizes, 'sm' | 'md' | 'lg'>
export interface AvatarProps
  extends WillowStyleProps,
    Omit<
      MantineAvatarProps,
      keyof WillowStyleProps | 'color' | 'variant' | 'size'
    > {
  src?: MantineAvatarProps['src']
  alt?: MantineAvatarProps['alt']
  children?: MantineAvatarProps['children']
  /**
   * Style variants
   * @default 'bold'
   */
  variant?: Variants
  /** @default 'gray' */
  color?: Colors
  /**
   * @type 'sm' | 'md' | 'lg' | 'string' | 'number'
   * @default 'md'
   */
  size?: AvatarPredefinedSizes | number | string
  /** @default 'circle' */
  shape?: 'circle' | 'rectangle'
  /**
   * Tooltip to display when hovering over the avatar.
   */
  tooltip?: ReactNode
}

/**
 * `Avatar` display user profile image, initials or any icon.
 *
 * @see TODO: add link to storybook
 */
export const Avatar = forwardRef<HTMLDivElement, AvatarProps>(
  (
    {
      color = 'gray',
      variant = 'bold',
      size = 'md',
      shape = 'circle',
      tooltip,
      children,
      ...restProps
    },
    ref
  ) => {
    const theme = useTheme()

    return (
      <Tooltip label={tooltip} withArrow withinPortal disabled={!tooltip}>
        <StyledAvatar
          ref={ref}
          {...restProps}
          {...useWillowStyleProps(restProps)}
          $color={color}
          $variant={variant}
          $size={size}
          size={getDimension(size, theme)}
          variant={hasBorder(variant) ? 'outline' : 'filled'}
          radius={shape === 'circle' ? 'xl' : theme.radius.r2}
          data-testid="avatar"
        >
          {children ?? (
            // Those default children will be used as the placeholder avatar.
            <Icon icon="person" size={size === 'lg' ? 24 : 20} />
          )}
        </StyledAvatar>
      </Tooltip>
    )
  }
)

const StyledAvatar = styled(MantineAvatar)<
  MantineAvatarProps & { ref: ForwardedRef<HTMLDivElement> } & {
    $color: Exclude<AvatarProps['color'], undefined>
    $variant: Exclude<AvatarProps['variant'], undefined>
    $size: Exclude<AvatarProps['size'], undefined>
  }
>(({ theme, $color, $variant, $size }) => {
  const { bgColor, borderColor, textColor } = getColors($color, $variant, theme)
  return css`
    .mantine-Avatar-placeholder {
      ${$size in theme.font.body
        ? theme.font.body[$size as keyof typeof theme.font.body].semibold
        : {
            // if not our predefined size, use the default font style from Mantine,
            // and only replace the font family
            fontFamily: theme.font.body.md.regular.fontFamily,
          }};

      background-color: ${bgColor};
      color: ${textColor};
      border-color: ${borderColor};

      /* This is required to fix a bug for MantineAvatar: BUG 81840
      https://dev.azure.com/willowdev/Unified/_workitems/edit/81840 */
      box-sizing: border-box;
    }
  `
})

const hasBorder = (variant: Exclude<AvatarProps['variant'], undefined>) =>
  variant === 'outline'

const getColors = (
  color: Exclude<AvatarProps['color'], undefined>,
  variant: Exclude<AvatarProps['variant'], undefined>,
  theme: Theme
) => {
  const variantMap = {
    bold: 'bold',
    muted: 'muted',
    subtle: 'subtle',
    outline: 'border',
  } as const
  const textColorMap = {
    bold: theme.color.neutral.fg.highlight,
    muted: theme.color.neutral.fg.default,
    subtle: theme.color.core[color].fg.default,
    outline: theme.color.core[color].fg.default,
  } as const

  const themeVariant = variantMap[variant]
  const [bgColor, borderColor] =
    themeVariant === 'border'
      ? [undefined, theme.color.core[color].border.default]
      : [theme.color.core[color].bg[themeVariant].default, undefined]

  return { bgColor, borderColor, textColor: textColorMap[variant] }
}

/** @returns predefined size or customise size if not predefined size. */
const getDimension = (
  size: Exclude<AvatarProps['size'], undefined>,
  theme: Theme
) => {
  const dimensionMap = {
    sm: theme.spacing.s20,
    md: rem(28),
    lg: rem(36),
  } as const

  const isDefinedSize = (
    size: Exclude<AvatarProps['size'], undefined>
  ): size is AvatarPredefinedSizes =>
    Object.keys(dimensionMap).some((key) => key === size)

  return isDefinedSize(size) ? dimensionMap[size] : size
}
