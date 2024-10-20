import type { Meta, StoryObj } from '@storybook/react'
import styled from 'styled-components'

import { Flex } from '.'
import { Button } from '../../buttons/Button'
import { FlexDecorator } from '../../../storybookUtils'

const meta: Meta<typeof Flex> = {
  title: 'Flex',
  component: Flex,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof Flex>

const StyledButton = styled(Button)`
  display: block;
`

export const Playground: Story = {
  render: () => (
    <Flex gap="s8">
      <StyledButton kind="primary">First</StyledButton>
      <StyledButton kind="primary">Second</StyledButton>
      <StyledButton kind="primary">Third</StyledButton>
    </Flex>
  ),
}

export const Gap: Story = {
  render: () => (
    <Flex gap="s32" wrap="wrap" w={260}>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
    </Flex>
  ),
}

export const ColumnGap: Story = {
  render: () => (
    <Flex rowGap="s8" columnGap="s32" wrap="wrap" w={260}>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
    </Flex>
  ),
}

export const RowGap: Story = {
  render: () => (
    <Flex rowGap="s32" columnGap="s8" wrap="wrap" w={260}>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
    </Flex>
  ),
}

export const Align: Story = {
  render: () => (
    <Flex gap="s8" align="self-end" h={100}>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
    </Flex>
  ),
}

export const Justify: Story = {
  render: () => (
    <Flex gap="s8" justify="flex-end">
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
    </Flex>
  ),
}

export const Direction: Story = {
  render: () => (
    <Flex gap="s8" direction="column">
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
    </Flex>
  ),
}

export const Wrap: Story = {
  render: () => (
    <Flex gap="s8" w={260} wrap="wrap">
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
      <StyledButton kind="primary">Button</StyledButton>
    </Flex>
  ),
}

export const PolymorphicAndDifferentChildrenWidth: Story = {
  render: () => (
    <Flex gap="s8" component="section">
      <StyledButton kind="primary">First</StyledButton>
      <StyledButton kind="primary">Second</StyledButton>
      <StyledButton kind="primary">Third</StyledButton>
    </Flex>
  ),
}
