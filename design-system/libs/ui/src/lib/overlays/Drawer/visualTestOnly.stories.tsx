import type { StoryObj } from '@storybook/react'
import { css } from 'styled-components'
import { Drawer, DrawerProps } from '.'
import { Button } from '../../buttons/Button'
import { useDisclosure } from '../../hooks'

const defaultStory = {
  component: Drawer,
  title: 'Drawer',
}

export default defaultStory

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

const SizeableDrawerStroy = ({ size }: Pick<DrawerProps, 'size'>) => {
  const [opened, { open, close }] = useDisclosure(false)

  return (
    <>
      <Drawer
        opened={opened}
        onClose={close}
        header="Drawer Header"
        footer={<Footer />}
        size={size}
      >
        <Content />
      </Drawer>

      <Button onClick={open}>Open Drawer</Button>
    </>
  )
}

export const MdSize: Story = {
  render: () => <SizeableDrawerStroy size="md" />,
}

export const LgSize: Story = {
  render: () => <SizeableDrawerStroy size="lg" />,
}
