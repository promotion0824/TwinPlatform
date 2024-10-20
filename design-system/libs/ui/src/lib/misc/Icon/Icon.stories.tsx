import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { Icon } from './'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Icon> = {
  title: 'Icon',
  component: Icon,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof Icon>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    icon: 'add',
  },
}

/** Icon defaults to inheriting the current color. */
export const InheritingColor: Story = {
  render: () => (
    <div css={{ color: 'blue' }}>
      <Icon icon="search" />
    </div>
  ),
}

/** If you need a custom color then use the normal methods: */
export const CustomizedColor: Story = {
  render: () => <Icon icon="add" css={{ color: 'red' }} />,
}

export const DifferentSizes: Story = {
  render: () => (
    <div>
      <Icon icon="star" size={16} />
      <Icon icon="star" />
      <Icon icon="star" size={24} />
    </div>
  ),
}

// Future possible APIs:

// <Icon>{ClownSVG}</Icon>
// <Icon outlined>add</Icon>
