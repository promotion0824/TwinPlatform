import type { Meta, StoryObj } from '@storybook/react'
import { Tracker } from '.'
import { storyContainerTestId } from '../../../storybookUtils/StoryContainers'
import { storybookAutoSourceParameters } from '../../utils/constant'

const TrackerDecorator = (Story: React.ComponentType) => (
  <div data-testid={storyContainerTestId} style={{ width: '400px' }}>
    <Story />
  </div>
)

const meta: Meta<typeof Tracker> = {
  title: 'Tracker',
  component: Tracker,
  decorators: [TrackerDecorator],
}
export default meta

type Story = StoryObj<typeof Tracker>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    data: [
      100, 50, 80, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100,
      87, 92, 88, 100, 100,
    ],
  },
}

export const LabelAndDescription: Story = {
  ...storybookAutoSourceParameters,
  args: {
    data: [
      100, 50, 80, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100,
      87, 92, 88, 100, 100,
    ],
    description: 'Previous Month',
    label: 'Building Performance',
  },
}

export const TooltipLabels: Story = {
  render: () => (
    <Tracker
      data={[
        { label: 'Nov 1', value: 100 },
        { label: 'Nov 2', value: 50 },
        { label: 'Nov 3', value: 80 },
        { label: 'Nov 4', value: 100 },
        { label: 'Nov 5', value: 100 },
        { label: 'Nov 6', value: 100 },
        { label: 'Nov 7', value: 100 },
        { label: 'Nov 8', value: 100 },
        { label: 'Nov 9', value: 100 },
        { label: 'Nov 10', value: 100 },
        { label: 'Nov 11', value: 100 },
        { label: 'Nov 12', value: 100 },
        { label: 'Nov 13', value: 100 },
        { label: 'Nov 14', value: 100 },
        { label: 'Nov 15', value: 100 },
        { label: 'Nov 16', value: 87 },
        { label: 'Nov 17', value: 92 },
        { label: 'Nov 18', value: 88 },
        { label: 'Nov 19', value: 100 },
        { label: 'Nov 20', value: 100 },
      ]}
      data-testid="tooltip-labels-tracker"
    />
  ),
}

export const DisableTooltips: Story = {
  ...storybookAutoSourceParameters,
  args: {
    data: [
      100, 50, 80, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100,
      87, 92, 88, 100, 100,
    ],
    disableTooltips: true,
  },
}

export const Height: Story = {
  ...storybookAutoSourceParameters,
  args: {
    data: [
      100, 50, 80, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100,
      87, 92, 88, 100, 100,
    ],
    height: 24,
  },
}

export const IntentThresholds: Story = {
  ...storybookAutoSourceParameters,
  args: {
    data: [
      100, 50, 80, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100,
      87, 92, 88, 100, 100,
    ],
    intentThresholds: {
      noticeThreshold: 50,
      positiveThreshold: 85,
    },
  },
}

export const StatusVariant: Story = {
  ...storybookAutoSourceParameters,
  args: {
    data: [
      true,
      true,
      true,
      true,
      true,
      false,
      false,
      false,
      true,
      true,
      true,
      true,
      true,
      true,
      true,
      true,
      true,
      true,
      true,
      true,
    ],
  },
}
