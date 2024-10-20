import type { Meta, StoryObj } from '@storybook/react'
import { Sparkline } from '.'
import { storyContainerTestId } from '../../../storybookUtils/StoryContainers'
import { Box } from '../../misc/Box'
import { storybookAutoSourceParameters } from '../../utils/constant'

const SparklineContainer = (Story: React.ComponentType) => (
  <Box data-testid={storyContainerTestId} h={24} w={500}>
    <Story />
  </Box>
)

const meta: Meta<typeof Sparkline> = {
  args: {
    dataset: [
      {
        name: 'Building 1',
        data: [
          54, 85, 1, 90, 34, 57, 71, 58, 80, 60, 69, 65, 22, 40, 78, 59, 81, 73,
          75, 91, 79, 12, 25, 46, 37, 16, 33, 99, 21, 61, 20, 72, 18, 2, 6, 52,
          49, 17, 86, 35, 26, 8, 3, 38, 87, 32, 97, 62, 30, 95, 14, 45, 39, 94,
          76, 82, 74, 89, 36, 51, 67, 24, 93, 64, 44, 98, 13, 84, 43, 48, 7, 42,
          100, 27, 77, 4, 47, 55, 31, 15, 41, 96, 9, 83, 70, 19, 63, 66, 53, 28,
          68, 23, 50, 29, 56, 5, 11, 10, 92, 88,
        ],
      },
    ],
    labels: [
      '2024-01-01',
      '2024-01-02',
      '2024-01-03',
      '2024-01-04',
      '2024-01-05',
      '2024-01-06',
      '2024-01-07',
      '2024-01-08',
      '2024-01-09',
      '2024-01-10',
      '2024-01-11',
      '2024-01-12',
      '2024-01-13',
      '2024-01-14',
      '2024-01-15',
      '2024-01-16',
      '2024-01-17',
      '2024-01-18',
      '2024-01-19',
      '2024-01-20',
      '2024-01-21',
      '2024-01-22',
      '2024-01-23',
      '2024-01-24',
      '2024-01-25',
      '2024-01-26',
      '2024-01-27',
      '2024-01-28',
      '2024-01-29',
      '2024-01-30',
      '2024-01-31',
      '2024-02-01',
      '2024-02-02',
      '2024-02-03',
      '2024-02-04',
      '2024-02-05',
      '2024-02-06',
      '2024-02-07',
      '2024-02-08',
      '2024-02-09',
      '2024-02-10',
      '2024-02-11',
      '2024-02-12',
      '2024-02-13',
      '2024-02-14',
      '2024-02-15',
      '2024-02-16',
      '2024-02-17',
      '2024-02-18',
      '2024-02-19',
      '2024-02-20',
      '2024-02-21',
      '2024-02-22',
      '2024-02-23',
      '2024-02-24',
      '2024-02-25',
      '2024-02-26',
      '2024-02-27',
      '2024-02-28',
      '2024-02-29',
      '2024-03-01',
      '2024-03-02',
      '2024-03-03',
      '2024-03-04',
      '2024-03-05',
      '2024-03-06',
      '2024-03-07',
      '2024-03-08',
      '2024-03-09',
      '2024-03-10',
      '2024-03-11',
      '2024-03-12',
      '2024-03-13',
      '2024-03-14',
      '2024-03-15',
      '2024-03-16',
      '2024-03-17',
      '2024-03-18',
      '2024-03-19',
      '2024-03-20',
      '2024-03-21',
      '2024-03-22',
      '2024-03-23',
      '2024-03-24',
      '2024-03-25',
      '2024-03-26',
      '2024-03-27',
      '2024-03-28',
      '2024-03-29',
      '2024-03-30',
      '2024-03-31',
      '2024-04-01',
      '2024-04-02',
      '2024-04-03',
      '2024-04-04',
      '2024-04-05',
      '2024-04-06',
      '2024-04-07',
      '2024-04-08',
      '2024-04-09',
    ],
  },
  component: Sparkline,
  decorators: [SparklineContainer],
  title: 'Sparkline',
  ...storybookAutoSourceParameters,
}
export default meta

type Story = StoryObj<typeof Sparkline>

export const Playground: Story = {}

export const Fill: Story = {
  args: {
    fill: true,
  },
}

export const NoTooltip: Story = {
  args: {
    tooltipEnabled: false,
  },
}
