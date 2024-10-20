import type { Meta, StoryObj } from '@storybook/react'
import { Site } from '../../../../../../../common/src/site/site/types'
import ScopeSelectorDecorator from '../../../../../storybook-decorators/ScopeSelectorDecorator'
import SiteDecorator from '../../../../../storybook-decorators/SiteDecorator'
import UserDecorator from '../../../../../storybook-decorators/UserDecorator'
import LocationWidget from './LocationWidget'

const sampleSite: Partial<Site> = {
  suburb: 'New York',
  state: 'NY',
  logoUrl: '',
  timeZoneId: 'America/New_York',
  area: '21,000,000 sqft',
  type: 'Office',
  status: 'Operations',
  timeZone: 'utc',
  weather: {
    code: 800,
    icon: 'c01d',
    temperature: 13.3,
  },
}

const meta: Meta<typeof LocationWidget> = {
  component: LocationWidget,
  decorators: [SiteDecorator, ScopeSelectorDecorator, UserDecorator],
}

export default meta

type Story = StoryObj<typeof LocationWidget>

export const Operations: Story = {
  parameters: { site: sampleSite },
}

export const NonOperations: Story = {
  parameters: {
    site: {
      ...sampleSite,
      status: 'Construction',
    },
  },
}
