import type { StoryObj } from '@storybook/react'

import { TextInput } from '.'

const defaultStory = {
  component: TextInput,
  title: 'TextInput',
}

export default defaultStory

type Story = StoryObj<typeof TextInput>

export const ResponsiveWithLongLabelAndErrorMessage: Story = {
  render: () => (
    <div css={{ width: '30%' }}>
      <TextInput
        name="responsiveWithLongLabelAndErrorMessage"
        defaultValue="Input Value"
        required
        label="Long long long long long long long long long long long long long long long long long long long long long Label"
        error="Failed long long long long long long long long long long long long validation message"
      />
    </div>
  ),
}

// We can only test auto fill style manually at the moment
// before Playwright support in the future.
// https://github.com/microsoft/playwright/issues/26831
export const AutoCompleteStyle: Story = {
  args: {
    autoComplete: 'email',
    name: 'email',
    type: 'email',
  },
}
