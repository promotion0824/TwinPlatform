import React, { useRef } from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import { Ontology } from '../../../../../../../../../common/src/twins/view/models'
import ResultsList from './ResultsList'

const meta: Meta<typeof ResultsList> = {
  component: ResultsList,
  parameters: { layout: 'fullscreen' },
  render: (args) => (
    <ResultsList
      endOfPageRef={useRef()}
      useSearchResultsList={() => ({
        t: (_) => _,
        ...args,
      })}
    />
  ),
}

export default meta
type Story = StoryObj<typeof ResultsList>

export const WithTwins: Story = {
  args: {
    sites: [{ id: 123, name: 'My site' }],
    ontology: new Ontology({
      'dtmi:com:willowinc:Asset;1': {
        '@id': 'dtmi:com:willowinc:Asset;1',
        contents: [],
      },
    }),

    twins: [
      {
        id: 123123,
        name: `Twin 1`,
        modelId: 'dtmi:com:willowinc:Asset;1',
        siteId: '123',
      },
      {
        id: 124124,
        name: `Twin 2`,
        modelId: 'dtmi:com:willowinc:Asset;1',
        siteId: '123',
      },
    ],
    isLoading: false,
  },
}

export const IsLoadingMore: Story = {
  args: {
    ...WithTwins.args,
    isLoadingNextPage: true,
  },
}

export const WithHundredsOfTwins: Story = {
  args: {
    ...WithTwins.args,
    twins: [...Array(100)].reduce(
      (twins) => twins.concat(WithTwins.args.twins),
      []
    ),
  },
}
