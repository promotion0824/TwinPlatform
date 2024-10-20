import type { Meta, StoryObj } from '@storybook/react'
import { StoryFlexContainer } from '../../../storybookUtils'

import { Indicator } from '.'
import { Avatar } from '../Avatar'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Indicator> = {
  title: 'Indicator',
  component: Indicator,
  decorators: [
    (Story) => (
      <StoryFlexContainer css={{ gap: 32, padding: '16px 32px' }}>
        <Story />
      </StoryFlexContainer>
    ),
  ],
}
export default meta

type Story = StoryObj<typeof Indicator>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const Intents: Story = {
  render: () => (
    <>
      <Indicator intent="primary">
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="secondary">
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="positive">
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="negative">
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="notice">
        <Avatar shape="rectangle" />
      </Indicator>
    </>
  ),
}

export const Label: Story = {
  render: () => (
    <>
      <Indicator intent="primary" label="Label">
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="secondary" label="Label">
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="positive" label="Label">
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="negative" label="Label">
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="notice" label="Label">
        <Avatar shape="rectangle" />
      </Indicator>
    </>
  ),
}

export const HasBorder: Story = {
  render: () => (
    <>
      <Indicator intent="primary" hasBorder>
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="secondary" hasBorder>
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="positive" hasBorder>
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="negative" hasBorder>
        <Avatar shape="rectangle" />
      </Indicator>
      <Indicator intent="notice" hasBorder>
        <Avatar shape="rectangle" />
      </Indicator>
    </>
  ),
}

export const Position: Story = {
  render: () => (
    <div css={{ display: 'flex', gap: 16, flexDirection: 'column' }}>
      <div css={{ display: 'flex', gap: 16 }}>
        <Indicator position="top-start">
          <Avatar shape="rectangle" />
        </Indicator>
        <Indicator position="top-center">
          <Avatar shape="rectangle" />
        </Indicator>
        <Indicator position="top-end">
          <Avatar shape="rectangle" />
        </Indicator>
      </div>
      <div css={{ display: 'flex', gap: 16 }}>
        <Indicator position="middle-start">
          <Avatar shape="rectangle" />
        </Indicator>
        <Indicator position="middle-center">
          <Avatar shape="rectangle" />
        </Indicator>
        <Indicator position="middle-end">
          <Avatar shape="rectangle" />
        </Indicator>
      </div>
      <div css={{ display: 'flex', gap: 16 }}>
        <Indicator position="bottom-start">
          <Avatar shape="rectangle" />
        </Indicator>
        <Indicator position="bottom-center">
          <Avatar shape="rectangle" />
        </Indicator>
        <Indicator position="bottom-end">
          <Avatar shape="rectangle" />
        </Indicator>
      </div>
    </div>
  ),
}
