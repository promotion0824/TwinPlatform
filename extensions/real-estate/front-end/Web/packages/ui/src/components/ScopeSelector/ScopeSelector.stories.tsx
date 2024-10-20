import React from 'react'
import { uniqueId } from 'lodash'
import type { Meta, StoryObj } from '@storybook/react'
import ScopeSelector, { LocationNode } from './ScopeSelector'

const meta: Meta<typeof ScopeSelector> = {
  component: ScopeSelector,
  render: () => <ScopeSelector locations={locations} />,
}

const locations: LocationNode[] = [
  {
    children: [
      {
        children: [
          {
            children: [],
            twin: {
              id: uniqueId(),
              metadata: { modelId: 'dtmi:com:willowinc:Building;1' },
              name: '135 King Street',
              siteId: uniqueId(),
              userId: '',
            },
          },
          {
            children: [],
            twin: {
              id: uniqueId(),
              metadata: { modelId: 'dtmi:com:willowinc:Building;1' },
              name: '151 Clarence Street',
              siteId: uniqueId(),
              userId: '',
            },
          },
          {
            children: [],
            twin: {
              id: uniqueId(),
              metadata: { modelId: 'dtmi:com:willowinc:Building;1' },
              name: '60 Martin Place',
              siteId: uniqueId(),
              userId: '',
            },
          },
        ],
        twin: {
          id: 'NSW',
          metadata: { modelId: 'dtmi:com:willowinc:Land;1' },
          name: 'NSW',
          siteId: uniqueId(),
          userId: '',
        },
      },
      {
        children: [
          {
            children: [],
            twin: {
              id: uniqueId(),
              metadata: { modelId: 'dtmi:com:willowinc:Building;1' },
              name: '567 Collins Street',
              siteId: uniqueId(),
              userId: '',
            },
          },
          {
            children: [],
            twin: {
              id: uniqueId(),
              metadata: { modelId: 'dtmi:com:willowinc:Building;1' },
              name: 'Sofia',
              siteId: uniqueId(),
              userId: '',
            },
          },
        ],
        twin: {
          id: 'VIC',
          metadata: { modelId: 'dtmi:com:willowinc:Land;1' },
          name: 'VIC',
          siteId: uniqueId(),
          userId: '',
        },
      },
    ],
    twin: {
      id: 'Australia',
      metadata: { modelId: 'dtmi:com:willowinc:Region;1' },
      name: 'Australia',
      siteId: uniqueId(),
      userId: '',
    },
  },
]

export default meta
type Story = StoryObj<typeof ScopeSelector>

export const Basic: Story = {}
