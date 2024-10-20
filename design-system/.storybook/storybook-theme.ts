// cannot use path alias because this object is imported by /libs/ui/.storybook/manager.ts
// which has issue: https://github.com/storybookjs/storybook/issues/22078
import { create } from '@storybook/theming/create'
import { darkTheme, lightTheme, ThemeName } from '../libs/theme/src'

// The below list is available in part from https://storybook.js.org/docs/react/configure/theming
// But this full list was created by console logging the themes variable and inspecting the
// properties, overriding them when their functionality was identified

export const createStorybookTheme = (themeName: ThemeName) => {
  const theme = themeName === 'dark' ? darkTheme : lightTheme
  return create({
    base: themeName,
    appBorderColor: theme.color.neutral.border.default,
    barSelectedColor: theme.color.neutral.fg.default,
    barTextColor: theme.color.neutral.fg.muted,
    colorPrimary: theme.color.intent.primary.fg.default,
    colorSecondary: theme.color.intent.secondary.fg.default,
    fontBase: theme.font.body.md.regular.fontFamily,
    fontCode: 'monospace',
    inputBg: theme.color.neutral.bg.base.default,
    inputBorder: theme.color.neutral.border.default,
    inputTextColor: theme.color.neutral.fg.default,
    textColor: theme.color.neutral.fg.default,
    textMutedColor: theme.color.neutral.fg.muted,
    appBg: theme.color.neutral.bg.base.default,
    appContentBg: theme.color.neutral.bg.panel.default,
    barBg: theme.color.neutral.bg.accent.default,
  })
}
