import type { Meta, StoryObj } from '@storybook/react'

import { PageTitle } from '.'
import { PageTitleItem } from '.'
import { FlexDecorator } from '../../../storybookUtils'

const meta: Meta<typeof PageTitle> = {
  title: 'PageTitle',
  component: PageTitle,
  decorators: [FlexDecorator],
}
export default meta

type Story = StoryObj<typeof PageTitle>

export const OverflowMenu: Story = {
  render: () => (
    <div style={{ height: '150px' }}>
      <PageTitle>
        <PageTitleItem href="#">Page 1</PageTitleItem>
        <PageTitleItem href="#">Page 2</PageTitleItem>
        <PageTitleItem href="#">Page 3</PageTitleItem>
        <PageTitleItem href="#">Page 4</PageTitleItem>
        <PageTitleItem href="#">Page 5</PageTitleItem>
      </PageTitle>
    </div>
  ),
}
