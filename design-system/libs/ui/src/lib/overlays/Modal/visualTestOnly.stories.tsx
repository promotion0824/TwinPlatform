import type { StoryObj } from '@storybook/react'

import { css } from 'styled-components'
import { Modal } from '.'
import { Button } from '../../buttons/Button'
import { useDisclosure } from '../../hooks'

const defaultStory = {
  component: Modal,
  title: 'Modal',
}

export default defaultStory

type Story = StoryObj<typeof Modal>

export const SizeMd: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close} header="Modal Header" size="md">
          Modal Content
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const SizeLg: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close} header="Modal Header" size="lg">
          Modal Content
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const OnlyCloseButton: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close}>
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}
