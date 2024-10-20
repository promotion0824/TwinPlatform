import { ButtonProps as MantineButtonProps } from '@mantine/core'
import { Theme } from '../../theme'

export type ButtonBackground = 'solid' | 'transparent' | 'none'
export type ButtonKind = 'primary' | 'secondary' | 'negative'
export type ButtonSize = 'medium' | 'large'

export const getButtonStyles = ({
  kind,
  background,
  theme,
}: {
  kind: ButtonKind
  background: ButtonBackground
  theme: Theme
}): {
  defaultBackground: string
  activatedBackground: string
  hoveredBackground: string
  disabledBackground: string

  defaultFontColor: string
  activatedFontColor: string
  hoveredFontColor: string

  outlineStyle?: {
    outline: string
    outlineOffset: string
  }
} => {
  const baseColorObject = theme.color.intent[kind]

  const defaultTransparentStyles = {
    defaultBackground: 'transparent',
    activatedBackground: 'transparent',
    hoveredBackground: 'transparent',
    disabledBackground: theme.color.state.disabled.bg,

    defaultFontColor: theme.color.neutral.fg.default,
    activatedFontColor: theme.color.neutral.fg.default,
    hoveredFontColor: theme.color.neutral.fg.default,
  }

  const defaultSolidStyles = {
    defaultBackground: baseColorObject.bg.bold.default,
    activatedBackground: baseColorObject.bg.bold.activated,
    hoveredBackground: baseColorObject.bg.bold.hovered,
    disabledBackground: theme.color.state.disabled.bg,

    defaultFontColor: theme.color.neutral.fg.highlight,
    activatedFontColor: theme.color.neutral.fg.highlight,
    hoveredFontColor: theme.color.neutral.fg.highlight,
  }

  const panelBackgroundColors = {
    activatedBackground: theme.color.neutral.bg.panel.activated,
    hoveredBackground: theme.color.neutral.bg.panel.hovered,
    disabledBackground: theme.color.neutral.bg.panel.default,
  }

  if (background === 'none') {
    return {
      ...defaultTransparentStyles,
      disabledBackground: 'transparent',

      defaultFontColor: baseColorObject.fg.default,
      activatedFontColor: baseColorObject.fg.activated,
      hoveredFontColor: baseColorObject.fg.hovered,
    }
  }

  if (kind === 'secondary') {
    if (background === 'solid') {
      return {
        ...defaultSolidStyles,
        defaultBackground: panelBackgroundColors.disabledBackground,
        hoveredBackground: panelBackgroundColors.hoveredBackground,
        activatedBackground: panelBackgroundColors.activatedBackground,
        activatedFontColor: theme.color.neutral.fg.default,
        defaultFontColor: theme.color.neutral.fg.default,
        hoveredFontColor: theme.color.neutral.fg.default,

        // only secondary + solid has border
        outlineStyle: {
          // use outline to mock border, so that the button height will not be impacted
          outline: `1px solid ${theme.color.neutral.border.default}`,
          outlineOffset: '-1px',
        },
      }
    }

    return {
      ...defaultTransparentStyles,
      ...panelBackgroundColors,
    }
  }

  if (background === 'transparent') {
    return {
      ...defaultTransparentStyles,
      ...panelBackgroundColors,
      defaultFontColor: baseColorObject.fg.default,
      activatedFontColor: baseColorObject.fg.default,
      hoveredFontColor: baseColorObject.fg.default,
    }
  }

  return defaultSolidStyles
}

export const getPadding = ({
  size,
  prefix,
  suffix,
  theme,
}: {
  size: ButtonSize
  prefix: MantineButtonProps['leftSection']
  suffix: MantineButtonProps['rightSection']
  theme: Theme
}) => {
  return size === 'large'
    ? {
        paddingTop: theme.spacing.s6,
        paddingBottom: theme.spacing.s6,
        paddingLeft: prefix ? theme.spacing.s8 : theme.spacing.s12,
        paddingRight: suffix ? theme.spacing.s8 : theme.spacing.s12,
      }
    : {
        // size === 'medium'
        paddingTop: theme.spacing.s4,
        paddingBottom: theme.spacing.s4,
        paddingLeft: prefix ? theme.spacing.s4 : theme.spacing.s8,
        paddingRight: suffix ? theme.spacing.s4 : theme.spacing.s8,
      }
}

export const getLoaderColor = ({
  theme,
  kind,
  background,
}: {
  theme: Theme
  kind: ButtonKind
  background: ButtonBackground
}) => {
  if (kind === 'secondary') return theme.color.neutral.fg.default
  if (background === 'transparent' || background === 'none') {
    return theme.color.intent[kind].fg.default
  }

  return theme.color.neutral.fg.highlight
}
