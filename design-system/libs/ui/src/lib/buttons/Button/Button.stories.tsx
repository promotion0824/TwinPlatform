import type { Meta, StoryObj } from '@storybook/react'
import { Button, IconButton } from '.'
import { FlexDecorator } from '../../../storybookUtils'
import { storybookAutoSourceParameters } from '../../utils/constant'
import { Icon } from '../../misc/Icon'
import { Group } from '../../layout/Group'
import { Stack } from '../../layout/Stack'

const meta: Meta<typeof Button> = {
  title: 'Button',
  component: Button,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof Button>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    children: 'Button',
  },
}

// Docs Stories

/**
 * - **Primary** \- used occasionally for important actions such as
 * "create new", "save", "submit"
 * - **Negative** \- used occasionally for actions that will destroy data
 * or cause a user to lose progress, such as "delete"
 * - **Secondary** \- most common, used for all other actions
 */
export const Kind: Story = {
  render: () => (
    <>
      <Button kind="primary">Primary</Button>
      <Button kind="secondary">Secondary</Button>
      <Button kind="negative">Negative</Button>
    </>
  ),
}

export const PrefixAndSuffix: Story = {
  render: () => (
    <>
      <Button prefix={<Icon icon="info" />}>Prefix</Button>
      <Button suffix={<Icon icon="info" />}>Suffix</Button>
      <Button prefix={<Icon icon="info" />} suffix={<Icon icon="info" />}>
        Prefix and Suffix
      </Button>
    </>
  ),
}

export const Transparent: Story = {
  render: () => (
    <>
      <Button
        prefix={<Icon icon="info" />}
        kind="primary"
        background="transparent"
      >
        Primary Transparent
      </Button>
      <Button
        prefix={<Icon icon="info" />}
        kind="secondary"
        background="transparent"
      >
        Secondary Transparent
      </Button>
      <Button
        prefix={<Icon icon="info" />}
        kind="negative"
        background="transparent"
      >
        Negative Transparent
      </Button>
    </>
  ),
}

/**
 * `Button` with `background="none"` is basically
 * a text button without background color or paddings.
 */
export const NoBackground: Story = {
  render: () => (
    <>
      <Button prefix={<Icon icon="info" />} kind="primary" background="none">
        Primary
      </Button>
      <Button prefix={<Icon icon="info" />} kind="secondary" background="none">
        Secondary
      </Button>
      <Button prefix={<Icon icon="info" />} kind="negative" background="none">
        Negative
      </Button>
    </>
  ),
}

export const Disabled: Story = {
  render: () => (
    <Stack gap="s8">
      <Group>
        <Button kind="primary" disabled>
          Primary Disabled
        </Button>
        <Button kind="secondary" disabled>
          Secondary Disabled
        </Button>
        <Button kind="negative" disabled>
          Negative Disabled
        </Button>
      </Group>
      <Group>
        <Button kind="primary" disabled background="transparent">
          Primary Transparent Disabled
        </Button>
        <Button kind="secondary" disabled background="transparent">
          Secondary Transparent Disabled
        </Button>
        <Button kind="negative" disabled background="transparent">
          Negative Transparent Disabled
        </Button>
      </Group>

      <Group>
        <Button kind="primary" disabled background="none">
          Primary No Background Disabled
        </Button>
        <Button kind="secondary" disabled background="none">
          Secondary No Background Disabled
        </Button>
        <Button kind="negative" disabled background="none">
          Negative No Background Disabled
        </Button>
      </Group>
    </Stack>
  ),
}

/**
 * - `medium` \- default size used for the majority of interface actions
 * - `large` \- can be used for visual emphasis, an example being form actions
 *  like "submit" buttons
 */
export const Size: Story = {
  render: () => (
    <>
      <Button size="medium">Medium</Button>
      <Button size="large">Large</Button>
    </>
  ),
}

export const IconOnly: Story = {
  render: () => (
    <Group>
      <IconButton kind="primary">
        <Icon icon="info" />
      </IconButton>
      <IconButton kind="secondary">
        <Icon icon="info" />
      </IconButton>
      {/* `IconButton` provides a shortcut for rendering any material symbol using the `icon` prop. */}
      <IconButton kind="negative" icon="info" />

      <IconButton kind="primary" background="transparent">
        <Icon icon="info" />
      </IconButton>
      <IconButton kind="secondary" background="transparent">
        <Icon icon="info" />
      </IconButton>
      {/* `IconButton` provides a shortcut for rendering any material symbol using the `icon` prop. */}
      <IconButton kind="negative" icon="info" background="transparent" />

      <IconButton kind="primary" background="none">
        <Icon icon="info" />
      </IconButton>
      <IconButton kind="secondary" background="none">
        <Icon icon="info" />
      </IconButton>
      {/* `IconButton` provides a shortcut for rendering any material symbol using the `icon` prop. */}
      <IconButton kind="negative" icon="info" background="none" />
    </Group>
  ),
}

export const ButtonAsLink: Story = {
  render: () => (
    <Button href="#" target="_blank">
      Link Button
    </Button>
  ),
}

export const CustomCSS: Story = {
  render: () => (
    <Button css={{ width: '150px' }}>
      Search Now
      <Icon icon="search" />
    </Button>
  ),
}

export const Loading: Story = {
  render: () => (
    <Group>
      <Button kind="primary" loading>
        Click Me
      </Button>
      <Button kind="secondary" loading>
        Click Me
      </Button>
      <Button kind="negative" loading>
        Click Me
      </Button>

      <Button kind="primary" loading background="transparent">
        Click Me
      </Button>
      <Button kind="secondary" loading background="transparent">
        Click Me
      </Button>
      <Button kind="negative" loading background="transparent">
        Click Me
      </Button>

      <Button kind="primary" loading background="none">
        Click Me
      </Button>
      <Button kind="secondary" loading background="none">
        Click Me
      </Button>
      <Button kind="negative" loading background="none">
        Click Me
      </Button>
    </Group>
  ),
}
