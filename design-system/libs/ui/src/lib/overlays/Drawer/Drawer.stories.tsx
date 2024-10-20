import type { Meta, StoryObj } from '@storybook/react'
import { css } from 'styled-components'

import { Drawer } from '.'
import { Button } from '../../buttons/Button'
import { useDisclosure } from '../../hooks'

const meta: Meta<typeof Drawer> = {
  title: 'Drawer',
  component: Drawer,
}
export default meta

type Story = StoryObj<typeof Drawer>

const Content = () => (
  <div
    css={css(({ theme }) => ({
      padding: theme.spacing.s16,
    }))}
  >
    Drawer Content
  </div>
)

const Footer = () => (
  <div
    css={{
      display: 'flex',
      width: '100%',
      justifyContent: 'flex-end',
    }}
  >
    <Button>Submit</Button>
  </div>
)

export const Playground: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Drawer
          opened={opened}
          onClose={close}
          header="Drawer Header"
          footer={<Footer />}
        >
          <Content />
        </Drawer>

        <Button onClick={open}>Open Drawer</Button>
      </>
    )
  },
}

export const SizeExtraLarge: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Drawer
          opened={opened}
          onClose={close}
          header="Drawer Header"
          footer={<Footer />}
          size="xl"
        >
          <Content />
        </Drawer>

        <Button onClick={open}>Open Drawer</Button>
      </>
    )
  },
}

export const FullScreen: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Drawer
          opened={opened}
          onClose={close}
          header="Drawer Header"
          footer={<Footer />}
          size="fullScreen"
        >
          <Content />
        </Drawer>

        <Button onClick={open}>Open Drawer</Button>
      </>
    )
  },
}

export const OpenFromLeft: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Drawer
          opened={opened}
          onClose={close}
          header="Drawer Header"
          footer={<Footer />}
          position="left"
        >
          <Content />
        </Drawer>

        <Button onClick={open}>Open Drawer</Button>
      </>
    )
  },
}

export const WithoutHeaderSection: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Drawer
          opened={opened}
          onClose={close}
          withCloseButton={false}
          footer={<Footer />}
        >
          <Content />
        </Drawer>

        <Button onClick={open}>Open Drawer</Button>
      </>
    )
  },
}

export const WithoutFooter: Story = {
  render: () => {
    const [opened, { open, close }] = useDisclosure(false)

    return (
      <>
        <Drawer opened={opened} onClose={close} header="Drawer Header">
          <Content />
        </Drawer>

        <Button onClick={open}>Open Drawer</Button>
      </>
    )
  },
}
