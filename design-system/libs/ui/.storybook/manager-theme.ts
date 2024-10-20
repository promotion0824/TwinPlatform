import { create } from '@storybook/theming/create'
// cannot use path alias because of https://github.com/storybookjs/storybook/issues/22078
// eslint-disable-next-line @nx/enforce-module-boundaries
import { createStorybookTheme } from '../../../.storybook/storybook-theme'

export const ManagerTheme = {
  ...create({
    ...createStorybookTheme('dark'),
    // Brand
    brandTarget: '_self',
    brandTitle: 'Willow Design System',
    brandUrl: '/',
    brandImage: './willow-logo-white.png',
  }),
}
