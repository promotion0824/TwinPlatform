import type { Meta, StoryObj } from '@storybook/react'
import { TwinViewContent } from './TwinView'

const meta: Meta<typeof TwinViewContent> = {
  component: TwinViewContent,
}

export default meta
type Story = StoryObj<typeof TwinViewContent>

export const Default: Story = {}

function withArgs(component: () => any, args: any) {
  const bound = component.bind({})
  bound.args = args
  return bound
}

export const LoadedState = withArgs(TwinViewContent, {
  status: 'loaded',
  twin: {
    'External ID': 'CRAC-31-1',
    'Geometry Spatial Reference': 'CRAC-31-1',
    'Model Number': 'CRAC-31-1',
    'Serial Number': 'Y18D6S0',
    'Site ID': '4b737046-748b-4b87-b433-7cd477188f06',
    'Empty property 1': '',
    'Empty property 2': null,
    'Group Label': {
      'Custom Group Value 01': 'Example Value 01',
      'Custom Group Value 02': 'Example Value 02',
      'Custom Group Value 03': 'Example Value 03',
      'Empty property 1': '',
      'Empty property 2': null,
      nested: { name: 'example', value: 'none' },
    },
  },
})

export const LoadingState = withArgs(TwinViewContent, {
  status: 'loading',
})

export const ErrorState = withArgs(TwinViewContent, {
  status: 'error',
})

export const TwinNotFoundState = withArgs(TwinViewContent, {
  status: 'not_found',
})
