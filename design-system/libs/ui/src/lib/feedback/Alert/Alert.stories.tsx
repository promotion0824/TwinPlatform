import type { Meta, StoryObj } from '@storybook/react'

import { Alert } from '.'
import { Stack } from '../../layout/Stack'
import { Intent } from '../../common'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Alert> = {
  title: 'Alert',
  component: Alert,
  args: {
    title: 'Alert title',
    children: 'Alert description',
  },
}
export default meta

type Story = StoryObj<typeof Alert>

const intents: Intent[] = [
  'primary',
  'secondary',
  'positive',
  'negative',
  'notice',
]

export const Playground: Story = {
  ...storybookAutoSourceParameters,
}

export const Variants: Story = {
  render: () => (
    <Stack>
      {intents.map((intent) => (
        <Alert key={intent} title="Alert title" intent={intent} hasIcon>
          Alert description
        </Alert>
      ))}
    </Stack>
  ),
}

export const WithCloseButton: Story = {
  ...storybookAutoSourceParameters,
  args: {
    withCloseButton: true,
  },
}
