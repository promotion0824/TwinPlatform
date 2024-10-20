import type { Meta, StoryObj } from '@storybook/react'

import { FlexDecorator } from '../../../storybookUtils'
import { storybookAutoSourceParameters } from '../../utils/constant'
import { Button } from '../Button'
import { ButtonGroup } from './'

const meta: Meta<typeof ButtonGroup> = {
  title: 'ButtonGroup',
  component: ButtonGroup,
  decorators: [FlexDecorator],
}

export default meta

type Story = StoryObj<typeof ButtonGroup>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    children: [
      <Button key="submit">Submit</Button>,
      <Button key="cancel">Cancel</Button>,
    ],
  },
}
