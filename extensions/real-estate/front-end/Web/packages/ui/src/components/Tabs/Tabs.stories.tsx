import type { Meta, StoryObj } from '@storybook/react'
import React from 'react'
import Tab from './Tab'
import Tabs from './Tabs'
import TabsHeader from './TabsHeader'
import Button from '../Button/Button'
import tw from 'twin.macro'

const meta: Meta<typeof Tabs> = {
  component: Tabs,
  argTypes: {
    numTabs: {
      description: 'Number of sample tabs to be displayed',
      defaultValue: 4,
      control: { type: 'number', min: 1 },
    },
    includeQueryStringForSelectedTab: {
      defaultValue: false,
      control: 'boolean',
    },
    tabType: {
      control: 'select',
      description: 'Tab type',
      defaultValue: '',
      options: ['', 'modal'],
    },
    showTabsHeader: {
      defaultValue: false,
      control: 'boolean',
    },
  },
  render: (args) => {
    const { numTabs, tabType, showTabsHeader, ...tabsProps } = args
    return (
      <div tw="width[400px] height[300px]">
        <Tabs {...tabsProps}>
          {Array(numTabs)
            .fill('')
            .map((_, index) => {
              const header = `Tab ${index + 1}`
              return (
                <Tab
                  // eslint-disable-next-line react/no-array-index-key
                  key={index}
                  header={header}
                  type={tabType}
                >
                  <h2>{`---- Content for ${header} -----`}</h2>
                  <p>{getRandomContent()}</p>
                  <p>{getRandomContent()}</p>
                  <p>{getRandomContent()}</p>
                </Tab>
              )
            })}
          {showTabsHeader && (
            <TabsHeader>
              <Button color="purple">Click me</Button>
            </TabsHeader>
          )}
        </Tabs>
      </div>
    )
  },
}

export default meta
type Story = StoryObj<typeof Tabs>

const sampleTabChildren = [
  'Maecenas eu laoreet turpis, ut scelerisque sem. Vivamus in eros dolor. Praesent nulla augue, consectetur ut hendrerit id, tincidunt vel nulla.',
  'Nullam mattis, enim et pretium volutpat, enim ante aliquet diam, ut imperdiet velit mauris eu augue. In hac habitasse platea dictumst. Proin tempor eu enim ut consectetur. Aliquam eleifend vulputate nunc, quis sagittis nulla elementum at. Praesent et sapien sagittis, dictum ante eu, efficitur justo. Fusce ultrices suscipit enim, vitae facilisis dolor tristique a. Etiam at dignissim justo. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Nulla aliquam et velit eu tincidunt. Aliquam erat volutpat. Cras ultricies vel sem a ornare. Aenean ac ligula malesuada, dignissim augue in, ullamcorper tellus.',
  'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris sagittis placerat ullamcorper. Cras volutpat velit quis nisi luctus iaculis. Integer quis ligula consequat, blandit urna ac, luctus elit. Proin et ex a sem blandit consectetur. Nullam sit amet neque sed ligula sodales pellentesque ut non ex.',
  'Morbi ut dui condimentum, ornare mi vitae, tempor enim. Integer sit amet luctus quam. Proin sed lacus at enim vulputate convallis et varius massa. Pellentesque non leo aliquam, scelerisque eros sit amet, tincidunt est. In posuere molestie fermentum. Ut facilisis ipsum a laoreet tempus. Maecenas vitae diam eget ligula sodales ultricies. Proin ullamcorper et eros id hendrerit. Nunc pretium et arcu non tincidunt.',
]

const getRandomContent = () =>
  sampleTabChildren[Math.floor(Math.random() * sampleTabChildren.length)]

export const BasicTabs: Story = {}

export const BasicTabsWithHeader: Story = {
  args: {
    showTabsHeader: true,
  },
}
export const SingleTab: Story = {
  args: {
    numTabs: 1,
    showTabsHeader: true,
  },
}
export const ManyTabs: Story = {
  args: {
    numTabs: 10,
  },
}
export const ManyTabsWithHeader: Story = {
  args: {
    numTabs: 10,
    showTabsHeader: true,
  },
}
