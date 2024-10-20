import type { StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { CheckboxGroup } from '.'
import { Checkbox } from '../Checkbox'

const defaultStory = {
  component: CheckboxGroup,
  title: 'CheckboxGroup',
  decorators: [FlexDecorator],
}

export default defaultStory

type Story = StoryObj<typeof CheckboxGroup>

export const RequiredInlineCheckboxGroupWithErrorMessage: Story = {
  render: () => (
    <CheckboxGroup label="Legend" inline required error="Error Message">
      <Checkbox label="Label" value="value1" />
      <Checkbox label="Label" value="value2" />
      <Checkbox label="Label" value="value3" />
    </CheckboxGroup>
  ),
}
