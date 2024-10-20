import type { Meta, StoryObj } from '@storybook/react'

import { css } from 'styled-components'
import { Modal } from '.'
import { Button } from '../../buttons/Button'
import { useDisclosure } from '../../hooks'

const meta: Meta<typeof Modal> = {
  title: 'Modal',
  component: Modal,
}
export default meta

type Story = StoryObj<typeof Modal>

export const Playground: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close} header="Modal Header">
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const WithoutHeaderArea: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close} withCloseButton={false}>
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const WithoutCloseButton: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal
          opened={opened}
          onClose={close}
          header="Modal Header"
          withCloseButton={false}
        >
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const VerticallyCentered: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close} header="Modal Header" centered>
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const SmallSize: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close} header="Modal Header" size="sm">
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const ExtraLargeSize: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close} header="Modal Header" size="xl">
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const PercentageSize: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal opened={opened} onClose={close} header="Modal Header" size="90%">
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const AutoSize: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal
          opened={opened}
          onClose={close}
          header="Modal Header"
          size="auto"
        >
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const FullScreen: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Modal
          opened={opened}
          onClose={close}
          header="Modal Header"
          size="fullScreen"
        >
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            Modal Content
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const AutoScrollable: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)
    const content = Array(100)
      .fill(0)
      .map((_, index) => <p key={index}>Modal content</p>)
    return (
      <>
        <Modal opened={opened} onClose={close} header="Modal Header" size="lg">
          <div css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}>
            {content}
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}

export const ScrollableInsideBody: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)
    const content1 = Array(100)
      .fill(0)
      .map((_, index) => <p key={index}>Modal content</p>)

    const content2 = Array(50)
      .fill(0)
      .map((_, index) => <p key={index}>Modal content</p>)
    return (
      <>
        <Modal
          opened={opened}
          onClose={close}
          header="Modal Header"
          size="lg"
          scrollInBody
        >
          <div
            css={{
              display: 'flex',
              flexDirection: 'row',
              gap: '40px',
              width: '100%',
              // configure height here if you don't want take full available height
            }}
          >
            <div
              css={{
                overflowY:
                  'scroll' /** this is essential to scroll the content */,
                width: '30%',
              }}
            >
              {content1}
            </div>
            <div
              css={{
                overflowY:
                  'scroll' /** this is essential to scroll the content */,
                width: '70%',
              }}
            >
              {content2}
            </div>
          </div>
        </Modal>

        <Button onClick={open}>Open Modal</Button>
      </>
    )
  },
}
