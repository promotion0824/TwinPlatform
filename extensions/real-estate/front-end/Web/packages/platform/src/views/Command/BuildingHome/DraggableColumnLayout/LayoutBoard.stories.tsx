import type { Meta, StoryObj } from '@storybook/react'
import { Button, Stack } from '@willowinc/ui'
import { fromPairs, range } from 'lodash'
import React, { forwardRef, useState } from 'react'
import WidgetCard from '../../../../components/LocationHome/WidgetCard/WidgetCard'
import LayoutBoard from './LayoutBoard'
import { DraggableContent } from './types'

const meta: Meta<typeof LayoutBoard> = {
  component: LayoutBoard,
}

export default meta

type Story = StoryObj<typeof LayoutBoard>

// at least 80px otherwise not enough remained for the content
const getWidgetHeight = (id: string | number) => Number(id) * 70 + 80

const placeholderWidget = (id: string | number): DraggableContent =>
  forwardRef(({ canDrag, ...props }, ref) => (
    <WidgetCard
      {...props}
      isEditingMode={canDrag}
      title={`Widget-${id}`}
      navigationButtonContent="Go to link"
      ref={ref}
    >
      <div
        css={{
          height:
            getWidgetHeight(id) -
            66 /* space for wrapper content in WidgetCard */,
        }}
      >
        Tickets content placeholder
      </div>
    </WidgetCard>
  ))

const componentMap = fromPairs(
  range(10).map((id) => [
    id,
    {
      defaultHeight: getWidgetHeight(id),
      component: placeholderWidget(id),
      id,
    },
  ])
)

export const Example: Story = {
  render: () => {
    const [editingMode, setEditingMode] = useState(false)
    const [data, setData] = useState([
      [
        { id: '0' },
        { id: '1' },
        { id: '2' },
        { id: '3' },
        { id: '5' },
        { id: '7' },
      ],
      [{ id: '9' }, { id: '4' }, { id: '6' }, { id: '8' }],
    ])

    return (
      <Stack p="s8">
        <Button onClick={() => setEditingMode((prev) => !prev)}>
          editing mode: {editingMode ? 'on' : 'off'}
        </Button>
        <LayoutBoard
          cols={{
            1800: 2,
            1200: 1,
            900: 1,
            600: 1,
          }}
          isEditingMode={editingMode}
          // will come from server or local storage
          data={data}
          setData={setData}
          // will be configured in a separate file
          componentMap={componentMap}
        />
      </Stack>
    )
  },
}
