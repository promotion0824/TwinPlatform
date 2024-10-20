import { addons } from '@storybook/manager-api'
import { ManagerTheme } from './manager-theme'

addons.setConfig({
  theme: ManagerTheme,
  sidebar: {
    showRoots: false,
  },
})
