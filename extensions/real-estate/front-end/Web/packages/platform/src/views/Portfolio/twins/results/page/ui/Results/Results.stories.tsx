import type { Meta, StoryObj } from '@storybook/react'
import Results from './Results'
import { WithTwins as ResultsListWithTwins } from './ResultsList.stories'
import { WithTwins as ResultsTableWithTwins } from './ResultsTable.stories'

const meta: Meta<typeof Results> = {
  component: Results,
  parameters: { layout: 'fullscreen' },
  render: (args) => (
    <Results
      ResultsList={ResultsListWithTwins.bind({}, ResultsListWithTwins.args)}
      ResultsTable={ResultsTableWithTwins.bind({}, ResultsTableWithTwins.args)}
      useSearchResults={() => ({
        t: (_) => _,
        ...args,
      })}
    />
  ),
}

export default meta
type Story = StoryObj<typeof Results>

export const Default: Story = {
  args: {
    display: 'list',
    isLoading: false,
    twins: ['stuff'],
  },
}

export const IsLoadingInitial: Story = {
  args: {
    isLoading: true,
  },
}
export const IsError: Story = {
  args: {
    isError: true,
  },
}
export const NoTwins: Story = {
  args: {
    twins: [],
  },
}
export const NoTwinsAfterInterestRegistered: Story = {
  args: {
    twins: [],
    hasRegisteredInterest: true,
  },
}
