import type { Meta, StoryObj } from '@storybook/react'
import { FlexDecorator } from '../../../storybookUtils'

import { PageTitle, PageTitleItem } from '.'
import { Badge } from '../../data-display/Badge'
import { Icon } from '../../misc/Icon'

const meta: Meta<typeof PageTitle> = {
  title: 'PageTitle',
  component: PageTitle,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof PageTitle>

export const Playground: Story = {
  render: () => (
    <PageTitle>
      <PageTitleItem href="#">Page</PageTitleItem>
    </PageTitle>
  ),
}

export const TwoItems: Story = {
  render: () => (
    <PageTitle>
      <PageTitleItem href="#">Page 1</PageTitleItem>
      <PageTitleItem href="#">Page 2</PageTitleItem>
    </PageTitle>
  ),
}

export const ThreeItems: Story = {
  render: () => (
    <PageTitle>
      <PageTitleItem href="#">Page 1</PageTitleItem>
      <PageTitleItem href="#">Page 2</PageTitleItem>
      <PageTitleItem href="#">Page 3</PageTitleItem>
    </PageTitle>
  ),
}

export const OverflowMenu: Story = {
  render: () => (
    <PageTitle>
      <PageTitleItem href="#">Page 1</PageTitleItem>
      <PageTitleItem href="#">Page 2</PageTitleItem>
      <PageTitleItem href="#">Page 3</PageTitleItem>
      <PageTitleItem href="#">Page 4</PageTitleItem>
      <PageTitleItem href="#">Page 5</PageTitleItem>
    </PageTitle>
  ),
}

export const MaxItems: Story = {
  render: () => (
    <PageTitle maxItems={5}>
      <PageTitleItem href="#">Page 1</PageTitleItem>
      <PageTitleItem href="#">Page 2</PageTitleItem>
      <PageTitleItem href="#">Page 3</PageTitleItem>
      <PageTitleItem href="#">Page 4</PageTitleItem>
      <PageTitleItem href="#">Page 5</PageTitleItem>
      <PageTitleItem href="#">Page 6</PageTitleItem>
    </PageTitle>
  ),
}

export const SuffixAndPrefix: Story = {
  render: () => (
    <PageTitle>
      <PageTitleItem
        href="#"
        prefix={<Icon icon="info" size={24} />}
        suffix={<Badge size="lg">Badge label</Badge>}
      >
        Page 1
      </PageTitleItem>
      <PageTitleItem
        href="#"
        prefix={<Icon icon="info" size={24} />}
        suffix={<Badge size="lg">Badge label</Badge>}
      >
        Page 2
      </PageTitleItem>
    </PageTitle>
  ),
}
