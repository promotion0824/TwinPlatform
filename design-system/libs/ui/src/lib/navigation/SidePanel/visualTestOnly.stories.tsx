import type { Meta, StoryObj } from '@storybook/react'
import { css } from 'styled-components'

import { SidePanel } from '.'
import { StoryFlexContainer } from '../../../../src/storybookUtils'

const defaultStory: Meta<typeof SidePanel> = {
  component: SidePanel,
  title: 'SidePanel',
  decorators: [
    (Story) => (
      <StoryFlexContainer
        css={css(({ theme }) => ({
          height: 200,
          background: theme.color.neutral.bg.base.default,
          color: theme.color.neutral.fg.default,
        }))}
      >
        <Story />
      </StoryFlexContainer>
    ),
  ],
}

export default defaultStory

type Story = StoryObj<typeof SidePanel>

export const DefaultSidePanel: Story = {
  render: () => <SidePanel title="Side Panel Title">Dummy content</SidePanel>,
}

export const OverflowContent: Story = {
  render: () => (
    <SidePanel title="Side Panel Title">
      <div>Dummy content</div>
      <div>Dummy content</div>
      <div>Dummy content</div>
      <div>Dummy content</div>
      <div>Dummy content</div>
      <div>Dummy content</div>
      <div>Dummy content</div>
      <div>Dummy content</div>
      <div>Dummy content</div>
      <div>Last Dummy content</div>
    </SidePanel>
  ),
}

export const CustomizedWidth: Story = {
  render: () => (
    <SidePanel css={{ width: 150 }} title="Side Panel Title">
      Dummy content
    </SidePanel>
  ),
}
