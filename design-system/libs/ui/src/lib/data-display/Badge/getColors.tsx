import { Theme } from '@willowinc/theme'

import { Colors, Sizes, Variants } from '../../common'

export type BadgeSizes = Extract<Sizes, 'xs' | 'sm' | 'md' | 'lg'>
export type BadgeVariants = 'dot' | Variants

export const getColors = (
  color: Colors,
  variant: BadgeVariants,
  theme: Theme
) => {
  return {
    bgColor: getBackgroundColor(color, variant, theme),
    textColor: getTextColor(color, variant, theme),
    borderColor: getBorderColor(color, variant, theme),
    indicatorColor: getIndicatorColor(color, variant, theme),
  }
}

const getForegroundColor = (color: Colors, theme: Theme) => {
  if (color === 'gray') {
    return theme.color.intent.secondary.fg.default
  }
  if (color === 'red') {
    return theme.color.intent.negative.fg.hovered
  }
  return theme.color.core[color].fg.default
}

const getBackgroundColor = (
  color: Colors,
  variant: BadgeVariants,
  theme: Theme
) => {
  if (variant === 'outline' || variant === 'dot') {
    return 'transparent'
  }

  if (color === 'gray') {
    return theme.color.intent.secondary.bg[variant].default
  }

  return theme.color.core[color].bg[variant].default
}

const getTextColor = (color: Colors, variant: BadgeVariants, theme: Theme) => {
  const textColorMap = {
    bold: theme.color.neutral.fg.highlight,
    muted: theme.color.neutral.fg.default,
    subtle: theme.color.core[color].fg.default,
    outline: getForegroundColor(color, theme),
    dot: theme.color.neutral.fg.default,
  } as const

  return textColorMap[variant]
}

const getBorderColor = (
  color: Colors,
  variant: BadgeVariants,
  theme: Theme
) => {
  if (variant === 'dot') {
    return theme.color.neutral.border.default
  }

  if (variant === 'outline') {
    if (color === 'gray') {
      return theme.color.intent.secondary.border.default
    }

    return theme.color.core[color].border.default
  }

  return 'transparent'
}

const getIndicatorColor = (
  color: Colors,
  variant: BadgeVariants,
  theme: Theme
) => {
  if (variant === 'dot') {
    return getForegroundColor(color, theme)
  }

  return undefined
}
