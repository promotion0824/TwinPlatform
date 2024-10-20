import type { Meta, StoryObj } from '@storybook/react'
import { Box, Modal, useDisclosure } from '@willowinc/ui'
import React, { useRef } from 'react'
import WidgetCard from './WidgetCard.tsx'

const meta: Meta<typeof WidgetCard> = {
  component: WidgetCard,
}

export default meta
type Story = StoryObj<typeof WidgetCard>

export const Widget: Story = {
  render: () => {
    const ref = useRef(null)
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <WidgetCard
          title="Widget card"
          navigationButtonContent="go to product"
          draggableRef={ref}
          onWidgetDelete={() => {
            window.alert('Will delete widget')
          }}
          onWidgetEdit={open}
        >
          children
        </WidgetCard>
        <Modal opened={opened} onClose={close} header="Edit Widget" centered>
          <Box p="s16"> Edit widget content</Box>
        </Modal>
      </>
    )
  },
}

export const IsLoading: Story = {
  render: () => {
    const ref = useRef(null)

    return (
      <WidgetCard
        title="Widget card"
        navigationButtonContent="go to product"
        isLoading
        draggableRef={ref}
      >
        children
      </WidgetCard>
    )
  },
}

export const DraggingMode: Story = {
  render: () => {
    const ref = useRef(null)
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <WidgetCard
          title="Widget card"
          navigationButtonContent="go to product"
          isEditingMode
          draggableRef={ref}
          onWidgetDelete={() => {
            window.alert('Will delete widget')
          }}
          onWidgetEdit={open}
        >
          children
        </WidgetCard>
        <Modal opened={opened} onClose={close} header="Edit Widget" centered>
          <Box p="s16"> Edit widget content</Box>
        </Modal>
      </>
    )
  },
}
