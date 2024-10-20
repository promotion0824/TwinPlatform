import type { StoryObj, Meta } from '@storybook/react'
import { StoryFlexContainer } from '../../../storybookUtils'

import { Field } from '.'
import { TextInput } from '../TextInput'

const defaultStory: Meta<typeof Field> = {
  component: Field,
  title: 'Field',
  decorators: [
    (Story) => (
      <StoryFlexContainer
        css={{
          width: '100%',
        }}
      >
        <Story />
      </StoryFlexContainer>
    ),
  ],
}

export default defaultStory

type Story = StoryObj<typeof Field>

export const WithLabel: Story = {
  render: () => (
    <Field label="Label">
      <TextInput />
    </Field>
  ),
}

export const WithError: Story = {
  render: () => (
    <Field error="Error message">
      <TextInput error />
    </Field>
  ),
}

export const RequiredWithError: Story = {
  render: () => (
    <Field label="Label" error="Error message" required>
      <TextInput error />
    </Field>
  ),
}
