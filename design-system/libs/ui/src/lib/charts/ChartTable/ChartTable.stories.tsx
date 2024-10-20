import type { Meta, StoryObj } from '@storybook/react'
import { ChartTable, linkColumnType, progressColumnType } from '.'
import { MemoryRouterDecorator } from '../../../storybookUtils'

const meta: Meta<typeof ChartTable> = {
  title: 'ChartTable',
  component: ChartTable,
  args: {
    getRowId: (row) => row.floor,
    rows: [
      {
        floor: 'Level 1',
        comfortScore: 50,
      },
      {
        floor: 'Level 2',
        comfortScore: 55,
      },
      {
        floor: 'Level 3',
        comfortScore: 70,
      },
      {
        floor: 'Level 4',
        comfortScore: 85,
      },
      {
        floor: 'Level 5',
        comfortScore: 40,
      },
      {
        floor: 'Level 6',
        comfortScore: 90,
      },
      {
        floor: 'Level 7',
        comfortScore: 65,
      },
      {
        floor: 'Level 8',
        comfortScore: 70,
      },
    ],
  },
}
export default meta

type Story = StoryObj<typeof ChartTable>

// The code for these stories is set manually because the automated version
// doesn't match the actual properties that are provided to the component,
// and thus isn't useful for this documentation.
const rowsCodeString = `rows={[
    {
      comfortScore: 50,
      floor: 'Level 1',
    },
    {
      comfortScore: 55,
      floor: 'Level 2',
    },
    {
      comfortScore: 70,
      floor: 'Level 3',
    },
    {
      comfortScore: 85,
      floor: 'Level 4',
    },
    {
      comfortScore: 40,
      floor: 'Level 5',
    },
    {
      comfortScore: 90,
      floor: 'Level 6',
    },
    {
      comfortScore: 65,
      floor: 'Level 7',
    },
    {
      comfortScore: 70,
      floor: 'Level 8',
    }
  ]}`

export const Playground: Story = {
  args: {
    columns: [
      {
        field: 'floor',
        headerName: 'Floor',
      },
      {
        field: 'comfortScore',
        flex: 1,
        headerName: 'Comfort Score',
      },
    ],
  },
  parameters: {
    docs: {
      source: {
        code: `<ChartTable
  columns={[
    {
      field: 'floor',
      headerName: 'Floor'
    },
    {
      field: 'comfortScore',
      flex: 1,
      headerName: 'Comfort Score'
    }
  ]}
  getRowId: (row) => row.floor,
  ${rowsCodeString} 
/>`,
      },
    },
  },
}

export const ProgressColumn: Story = {
  args: {
    columns: [
      {
        field: 'floor',
        headerName: 'Floor',
      },
      {
        ...progressColumnType({
          intentThresholds: {
            noticeThreshold: 50,
            positiveThreshold: 80,
          },
        }),
        field: 'comfortScore',
        headerName: 'Comfort Score',
      },
    ],
  },
  parameters: {
    docs: {
      description: {
        story: `The rendering of a column type can be configured by simply importing the column type you want to use, and passing this option to the column configuration along with any required parameters.

The Progress column type optionally takes parameters to define the thresholds where the intents change.`,
      },
      source: {
        code: `<ChartTable
  columns={[
    {
      field: 'floor',
      headerName: 'Floor',
    },
    {
      ...progressColumnType({
        intentThresholds: {
          noticeThreshold: 50,
          positiveThreshold: 80,
        },
      }),
      field: 'comfortScore',
      headerName: 'Comfort Score',
    },
  ]}
  getRowId: (row) => row.floor,
  ${rowsCodeString} 
/>`,
      },
    },
  },
}

export const LinkColumn: Story = {
  args: {
    columns: [
      {
        ...linkColumnType({ urlPropertyName: 'url' }),
        field: 'floor',
        headerName: 'Floor',
      },
      {
        ...progressColumnType({
          intentThresholds: {
            noticeThreshold: 50,
            positiveThreshold: 80,
          },
        }),
        field: 'comfortScore',
        headerName: 'Comfort Score',
      },
    ],
    rows: [
      {
        floor: 'Level 1',
        comfortScore: 50,
        url: '/level-1',
      },
      {
        floor: 'Level 2',
        comfortScore: 55,
        url: '/level-2',
      },
      {
        floor: 'Level 3',
        comfortScore: 70,
        url: '/level-3',
      },
      {
        floor: 'Level 4',
        comfortScore: 85,
        url: '/level-4',
      },
      {
        floor: 'Level 5',
        comfortScore: 40,
        url: '/level-5',
      },
      {
        floor: 'Level 6',
        comfortScore: 90,
        url: '/level-6',
      },
      {
        floor: 'Level 7',
        comfortScore: 65,
        url: '/level-7',
      },
      {
        floor: 'Level 8',
        comfortScore: 70,
        url: '/level-8',
      },
    ],
  },
  decorators: [MemoryRouterDecorator],
  parameters: {
    docs: {
      description: {
        story:
          'The Link column type takes a parameter to specify the property that contains the URL to link to.',
      },
      source: {
        code: `<ChartTable
  columns={[
    {
      ...linkColumnType({ urlPropertyName: 'url' }),
      field: 'floor',
      headerName: 'Floor',
    },
    {
      ...progressColumnType({
        intentThresholds: {
          noticeThreshold: 50,
          positiveThreshold: 80,
        },
      }),
      field: 'comfortScore',
      headerName: 'Comfort Score',
    },
  ]}
  getRowId: (row) => row.floor,
  rows={[
    {
      comfortScore: 50,
      floor: 'Level 1',
      url: '/level-1',
    },
    {
      comfortScore: 55,
      floor: 'Level 2',
      url: '/level-2',
    },
    {
      comfortScore: 70,
      floor: 'Level 3',
      url: '/level-3',
    },
    {
      comfortScore: 85,
      floor: 'Level 4',
      url: '/level-4',
    },
    {
      comfortScore: 40,
      floor: 'Level 5',
      url: '/level-5',
    },
    {
      comfortScore: 90,
      floor: 'Level 6',
      url: '/level-6',
    },
    {
      comfortScore: 65,
      floor: 'Level 7',
      url: '/level-7',
    },
    {
      comfortScore: 70,
      floor: 'Level 8',
      url: '/level-8',
    }
  ]}
/>`,
      },
    },
  },
}
