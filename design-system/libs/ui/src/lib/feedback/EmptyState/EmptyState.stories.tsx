import type { Meta, StoryObj } from '@storybook/react'

import { EmptyState } from '.'
import { Button } from '../../buttons/Button'
import { Group } from '../../layout/Group'
import { Icon } from '../../misc/Icon'
import { Link } from '../../navigation/Link'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof EmptyState> = {
  title: 'EmptyState',
  component: EmptyState,
  args: {
    title: 'State title',
  },
}
export default meta

type Story = StoryObj<typeof EmptyState>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: { title: 'State title' },
}

export const TitleSize: Story = {
  render: (args) => (
    <Group>
      <EmptyState {...args} titleSize="lg" />
      <EmptyState {...args} titleSize="md" />
      <EmptyState {...args} titleSize="sm" />
    </Group>
  ),
}

export const Description: Story = {
  ...storybookAutoSourceParameters,
  args: {
    description: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
  },
}

export const WithIcon: Story = {
  render: (args) => (
    <Group>
      <EmptyState
        {...args}
        title="With smaller icon"
        icon="info"
        iconProps={{ size: 20 }}
      />
      <EmptyState {...args} title="With icon" icon="info" />
      <EmptyState
        {...args}
        title="With customized icon color"
        icon="info"
        css={{
          color: 'red',
        }}
      />
    </Group>
  ),
}

export const WithIllustration: Story = {
  ...storybookAutoSourceParameters,
  args: {
    title: 'No data found',
    illustration: 'no-data',
    description: 'There is no data available',
  },
}

export const WithCustomizedGraphic: Story = {
  render: (args) => (
    <Group>
      <EmptyState
        {...args}
        title="With customized icon"
        graphic={<Icon icon="settings" />}
      />
      <EmptyState
        {...args}
        title="With customized image"
        graphic={
          <img src="./example-avatar.jpeg" alt="puppy" style={{ width: 120 }} />
        }
      />
    </Group>
  ),
}

export const WithActionsOrLink: Story = {
  render: (args) => (
    <Group>
      <EmptyState
        {...args}
        title="With actions"
        primaryActions={
          <>
            <Button kind="secondary">Cancel</Button>
            <Button kind="primary">Submit</Button>
          </>
        }
      />
      <EmptyState
        {...args}
        title="With link"
        primaryActions={
          <Link href="#" target="_blank">
            Link
          </Link>
        }
      />
      <EmptyState
        {...args}
        title="With customized link"
        primaryActions={
          <Button href="#" target="_blank">
            Link Button
          </Button>
        }
      />
      <EmptyState
        {...args}
        title="With actions and link"
        primaryActions={
          <>
            <Button kind="secondary">Cancel</Button>
            <Button kind="primary">Submit</Button>
          </>
        }
        secondaryActions={
          <Link href="#" target="_blank">
            Link
          </Link>
        }
      />
    </Group>
  ),
}
