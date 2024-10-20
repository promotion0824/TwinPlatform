import type { Meta, StoryObj } from '@storybook/react'
import { rest } from 'msw'
import { Site } from '../../../../../../../common/src/site/site/types'
import AppDecorator from '../../../../../storybook-decorators/AppDecorator'
import ScopeSelectorDecorator from '../../../../../storybook-decorators/ScopeSelectorDecorator'
import SiteDecorator from '../../../../../storybook-decorators/SiteDecorator'
import InsightsWidget from './InsightsWidget'

const site: Partial<Site> = {
  insightsStats: {
    highCount: 14,
    lowCount: 14,
    mediumCount: 14,
    openCount: 14,
    urgentCount: 14,
  },
  insightsStatsByStatus: {
    ignoredCount: 14,
    inProgressCount: 14,
    newCount: 14,
    openCount: 14,
    resolvedCount: 14,
  },
}

const meta: Meta<typeof InsightsWidget> = {
  component: InsightsWidget,
  decorators: [AppDecorator, SiteDecorator, ScopeSelectorDecorator],
  parameters: { site },
}

export default meta

type Story = StoryObj<typeof InsightsWidget>

export const Playground: Story = {
  parameters: {
    msw: {
      handlers: [
        rest.post('/api/insights/cards', (_, res, ctx) => {
          return res(
            ctx.json({
              cards: {
                items: [],
              },
              impactScoreSummary: [
                {
                  fieldId: 'daily_avoidable_cost',
                  name: 'Daily Avoidable Cost',
                  value: 943.3087775000008,
                  unit: 'USD',
                },
                {
                  fieldId: 'daily_avoidable_energy',
                  name: 'Daily Avoidable Energy',
                  value: 4956.699999999999,
                  unit: 'kWh',
                },
              ],
            })
          )
        }),
      ],
    },
  },
}
