import type { Meta, StoryObj } from '@storybook/react'
import { Site } from '../../../../../../../common/src/site/site/types'
import ScopeSelectorDecorator from '../../../../../storybook-decorators/ScopeSelectorDecorator'
import SiteDecorator from '../../../../../storybook-decorators/SiteDecorator'
import TicketsWidget from './TicketsWidget'

const site: Partial<Site> = {
  ticketStats: {
    highCount: 14,
    lowCount: 14,
    mediumCount: 14,
    openCount: 14,
    overdueCount: 14,
    urgentCount: 14,
  },
  ticketStatsByStatus: {
    closedCount: 14,
    openCount: 14,
    resolvedCount: 14,
  },
}

const meta: Meta<typeof TicketsWidget> = {
  component: TicketsWidget,
  decorators: [SiteDecorator, ScopeSelectorDecorator],
  parameters: { site },
}

export default meta

type Story = StoryObj<typeof TicketsWidget>

export const Playground: Story = {}
