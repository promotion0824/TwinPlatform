import type { StorybookConfig } from '@storybook/react-webpack5'
// have to use relative path to pass build for storybook somehow
// eslint-disable-next-line @nx/enforce-module-boundaries
import { rootMain } from '../../../.storybook/main'
import { hiddenPrefix } from '../src/storybookUtils'

const groups = [
  { directory: 'layout', name: 'Layout' },
  { directory: 'inputs', name: 'Inputs' },
  { directory: 'buttons', name: 'Buttons' },
  { directory: 'navigation', name: 'Navigation' },
  { directory: 'feedback', name: 'Feedback' },
  { directory: 'overlays', name: 'Overlays' },
  { directory: 'data-display', name: 'Data Display' },
  { directory: 'misc', name: 'Misc' },
  { directory: 'dates', name: 'Dates' },
  { directory: 'charts', name: 'Charts' },
  { directory: 'ui-chrome', name: 'UI Chrome' },
]

const config: StorybookConfig = {
  ...rootMain,
  stories: [
    ...rootMain.stories,
    // this order of file will impact the default order of stories in the sidebar
    '../src/docs/**/*.mdx',
    '../src/lib/**/*.mdx',
    {
      directory: '../src/lib',
      // The titlePrefix field will generate automatic titles for your stories
      // for example for the visual test only story with title "Button" will be rendered
      // with title "${hiddenPrefix}-button", which is under structure "${hiddenPrefix}/Button"
      titlePrefix: hiddenPrefix,
      // Only the visualTestOnly stories
      files: '**/**/visualTestOnly.stories.@(js|jsx|ts|tsx)',
    },
    ...groups.map(({ directory, name }) => ({
      files: '**/!(visualTestOnly).stories.@(js|jsx|ts|tsx)',
      directory: `../src/lib/${directory}`,
      titlePrefix: `Components/${name}`,
    })),
  ],
}

export default config
