import type { Meta, StoryObj } from '@storybook/react'
import MemoryRouterDecorator from '../../../storybook-decorators/MemoryRouterDecorator'
import ScopeSelectorDecorator from '../../../storybook-decorators/ScopeSelectorDecorator'
import { PerformanceTile } from './PerformanceTile'

const meta: Meta<typeof PerformanceTile> = {
  args: {
    averageScore: 95,
    performanceScores: [
      { label: 'Feb 08', value: 100 },
      { label: 'Feb 09', value: 70 },
      { label: 'Feb 10', value: 45 },
      { label: 'Feb 11', value: 90 },
      { label: 'Feb 12', value: 95 },
      { label: 'Feb 13', value: 100 },
      { label: 'Feb 14', value: 65 },
      { label: 'Feb 15', value: 90 },
      { label: 'Feb 16', value: 85 },
      { label: 'Feb 17', value: 35 },
      { label: 'Feb 18', value: 100 },
      { label: 'Feb 19', value: 95 },
      { label: 'Feb 20', value: 90 },
      { label: 'Feb 21', value: 85 },
      { label: 'Feb 22', value: 95 },
      { label: 'Feb 23', value: 100 },
      { label: 'Feb 24', value: 68 },
      { label: 'Feb 25', value: 62 },
      { label: 'Feb 26', value: 60 },
      { label: 'Feb 27', value: 56 },
      { label: 'Feb 28', value: 100 },
      { label: 'Mar 01', value: 95 },
      { label: 'Mar 02', value: 90 },
      { label: 'Mar 03', value: 85 },
      { label: 'Mar 04', value: 95 },
      { label: 'Mar 05', value: 48 },
      { label: 'Mar 06', value: 44 },
      { label: 'Mar 07', value: 72 },
    ],
  },
  component: PerformanceTile,
  decorators: [MemoryRouterDecorator, ScopeSelectorDecorator],
}

export default meta

type Story = StoryObj<typeof PerformanceTile>

export const Playground: Story = {}
