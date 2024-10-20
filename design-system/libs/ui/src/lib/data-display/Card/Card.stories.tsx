import type { Meta, StoryObj } from '@storybook/react'
import styled, { css } from 'styled-components'
import { Card } from '.'
import { IconButton } from '../../buttons/Button'
import { Group } from '../../layout/Group'
import { Stack } from '../../layout/Stack'

const meta: Meta<typeof Card> = {
  title: 'Card',
  component: Card,
}
export default meta

type Story = StoryObj<typeof Card>

const Header = styled.h2(
  ({ theme }) => css`
    ${theme.font.body.md}
    color: ${theme.color.neutral.fg.default};
    margin: 0;
  `
)

const ContentWrapper = styled.div(
  ({ theme }) => css`
    padding: ${theme.spacing.s16};
  `
)

export const Playground: Story = {
  render: (args) => <Card {...args} css={{ width: 320, height: 160 }} />,
}

export const Shadows: Story = {
  render: (args) => (
    <Stack css={{ width: 320, height: 160 }}>
      <Card {...args}>
        <ContentWrapper>No Shadow</ContentWrapper>
      </Card>
      <Card {...args} shadow="s1">
        <ContentWrapper>Shadow s1</ContentWrapper>
      </Card>
      <Card {...args} shadow="s2">
        <ContentWrapper>Shadow s2</ContentWrapper>
      </Card>
      <Card {...args} shadow="s3">
        <ContentWrapper>Shadow s3</ContentWrapper>
      </Card>
    </Stack>
  ),
}

export const Backgrounds: Story = {
  render: (args) => (
    <Stack css={{ width: 320, height: 160 }}>
      <Card {...args}>
        <ContentWrapper>Base</ContentWrapper>
      </Card>
      <Card {...args} background="panel">
        <ContentWrapper>Panel</ContentWrapper>
      </Card>
      <Card {...args} background="accent">
        <ContentWrapper>Accent</ContentWrapper>
      </Card>
    </Stack>
  ),
}

export const Radius: Story = {
  render: (args) => (
    <Stack css={{ width: 320, height: 160 }}>
      <Card {...args}>
        <ContentWrapper>Default</ContentWrapper>
      </Card>
      <Card {...args} radius="r2">
        <ContentWrapper>Radius r2</ContentWrapper>
      </Card>
      <Card {...args} radius="r4">
        <ContentWrapper>Radius r4</ContentWrapper>
      </Card>
    </Stack>
  ),
}

export const VerticalLayoutCardExample: Story = {
  render: (args) => (
    <Card
      {...args}
      css={{
        width: 320,
      }}
    >
      <Stack>
        <Group
          css={css(
            ({ theme }) => css`
              padding: ${theme.spacing.s8} ${theme.spacing.s16};
              border-bottom: 1px solid ${theme.color.neutral.border.default};
            `
          )}
          gap="s8"
        >
          <Header css={{ flexGrow: 1 }}>Title</Header>
          <IconButton
            icon="more_vert"
            background="transparent"
            kind="secondary"
            size="large"
          />
        </Group>
        <div
          css={css(
            ({ theme }) => css`
              padding: ${theme.spacing.s16};
            `
          )}
        >
          Card content
        </div>
      </Stack>
    </Card>
  ),
}

export const HorizontalLayoutCardExample: Story = {
  render: (args) => (
    <Card
      {...args}
      css={{
        width: 320,
      }}
    >
      <Group
        css={css(
          ({ theme }) => css`
            padding: ${theme.spacing.s16};
          `
        )}
        gap="s16"
        align="flex-start"
      >
        <Stack gap="s8" css={{ flexGrow: 1 }}>
          <Header>Title</Header>
          <div>Card content</div>
        </Stack>

        <IconButton
          icon="more_vert"
          background="transparent"
          kind="secondary"
          size="large"
        />
      </Group>
    </Card>
  ),
}
