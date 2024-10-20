import type { Meta, StoryObj } from '@storybook/react'
import { Link as ReactRouterLink } from 'react-router-dom'
import { Link } from '.'
import { FlexDecorator, MemoryRouterDecorator } from '../../../storybookUtils'
import { Stack } from '../../layout/Stack'
import { Icon } from '../../misc/Icon'
import { LinkSize } from './Link'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Link> = {
  title: 'Link',
  component: Link,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof Link>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    children: 'Willow Design System',
    href: 'https://storybook.willowinc.com/',
  },
}

const linkSizes: LinkSize[] = ['xs', 'sm', 'md', 'lg']

export const Sizes: Story = {
  render: () => (
    <Stack>
      {linkSizes.map((size) => (
        <Link
          data-testid={`link-size-${size}`}
          href="https://storybook.willowinc.com/"
          key={size}
          size={size}
        >
          Willow Design System
        </Link>
      ))}
    </Stack>
  ),
}

export const Prefix: Story = {
  render: () => (
    <Link
      data-testid="link-prefix"
      href="https://storybook.willowinc.com/"
      prefix={<Icon icon="add" size={16} />}
    >
      Willow Design System
    </Link>
  ),
}

export const Suffix: Story = {
  render: () => (
    <Link
      data-testid="link-suffix"
      href="https://storybook.willowinc.com/"
      suffix={<Icon icon="open_in_new" size={16} />}
    >
      Willow Design System
    </Link>
  ),
}

/** By default `Link` uses `a` as its root element, but you can provide any other element or component to the `component` prop. */
export const Component: Story = {
  decorators: [MemoryRouterDecorator],
  render: () => (
    <Stack>
      <Link component={ReactRouterLink} to="https://storybook.willowinc.com/">
        Willow Design System
      </Link>
      <Link
        component={ReactRouterLink}
        prefix={<Icon icon="add" size={16} />}
        suffix={<Icon icon="open_in_new" size={16} />}
        to="https://storybook.willowinc.com/"
      >
        Willow Design System
      </Link>
    </Stack>
  ),
}
