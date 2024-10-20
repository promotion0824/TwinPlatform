import { Parameters } from '@storybook/react'

export enum GroupNames {
  Buttons = 'buttons',
  Charts = 'charts',
  DataDisplay = 'data-display',
  Dates = 'dates',
  Feedback = 'feedback',
  Inputs = 'inputs',
  Layout = 'layout',
  Misc = 'misc',
  Navigation = 'navigation',
  Overlays = 'overlays',
  UiChrome = 'ui-chrome',
}

export const storybookAutoSourceParameters: Partial<Parameters> = {
  parameters: {
    docs: {
      source: { type: 'auto' },
    },
  },
}
