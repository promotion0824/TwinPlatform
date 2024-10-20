import type { Meta, StoryObj } from '@storybook/react'
import AppDecorator from '../../../../../storybook-decorators/AppDecorator'
import KPISummaryWidget from './KPISummaryWidget'

const meta: Meta<typeof KPISummaryWidget> = {
  component: KPISummaryWidget,
  decorators: [AppDecorator],
}

export default meta

type Story = StoryObj<typeof KPISummaryWidget>

export const Playground: Story = {}
