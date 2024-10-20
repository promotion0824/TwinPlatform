import type { Meta, StoryObj } from '@storybook/react'

import { ForecastTile } from './ForecastTile'

const meta: Meta<typeof ForecastTile> = {
  args: {
    forecast: [
      {
        code: 800,
        icon: 'c01d',
        temperature: 13.3,
      },
      {
        code: 700,
        icon: 'a01d',
        temperature: 8.3,
      },
      {
        code: 800,
        icon: 'c01d',
        temperature: 13.3,
      },
      {
        code: 300,
        icon: 'd01d',
        temperature: 12.2,
      },
      {
        code: 802,
        icon: 'c02d',
        temperature: 8.3,
      },
    ],
    temperatureUnit: 'celsius',
  },
  component: ForecastTile,
}

export default meta

type Story = StoryObj<typeof ForecastTile>

export const Playground: Story = {}

export const Fahrenheit: Story = {
  args: {
    temperatureUnit: 'fahrenheit',
  },
}
