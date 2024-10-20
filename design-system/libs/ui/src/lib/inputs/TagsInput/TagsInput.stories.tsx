import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'

import { TagsInput } from '.'
import { SelectItem, SelectOptionsFilter } from '../Select'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof TagsInput> = {
  title: 'TagsInput',
  component: TagsInput,
}
export default meta

type Story = StoryObj<typeof TagsInput>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const WithLabelAndErrorMessage: Story = {
  ...storybookAutoSourceParameters,
  args: {
    label: 'Input label',
    error: 'Error message',
  },
}

export const HorizontalLayout: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'Input label',
    error: 'Error message',
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'Input label',
    labelWidth: 300,
    error: 'Error message',
  },
}

export const Controlled: Story = {
  render: () => {
    const [value, setValue] = useState<string[]>([])

    return <TagsInput data={[]} value={value} onChange={setValue} />
  },
}

export const Clearable: Story = {
  render: () => (
    <TagsInput
      label="Press Enter to submit a tag"
      placeholder="Enter tag"
      defaultValue={['React']}
      clearable
    />
  ),
}

export const WithSuggestions: Story = {
  render: () => (
    <TagsInput
      label="Press Enter to submit a tag"
      placeholder="Pick tag from list"
      data={['React', 'Angular', 'Svelte']}
    />
  ),
}

export const ControlledSearchValue: Story = {
  render: () => {
    const [searchValue, setSearchValue] = useState('r')

    return (
      <TagsInput
        data={['React', 'Angular', 'Svelte']}
        searchValue={searchValue}
        onSearchChange={setSearchValue}
      />
    )
  },
}

export const MaxTags: Story = {
  render: () => (
    <TagsInput
      label="Press Enter to submit a tag"
      description="Add up to 2 tags"
      placeholder="Enter tag"
      defaultValue={['first']}
      maxTags={2}
    />
  ),
}

/**
 * By default, TagsInput splits values by comma (,), you can change this
 * behavior by setting splitChars prop to an array of strings. All values
 * from splitChars cannot be included in the final value. Values are also
 * splitted on paste.
 */
export const SplitChars: Story = {
  render: () => (
    <TagsInput
      label="Press Enter to submit a tag"
      placeholder="Enter tag"
      splitChars={[',', ';']}
    />
  ),
}

const largeData = Array(100_000)
  .fill(0)
  .map((_, index) => `Option ${index}`)
export const LargeDataSets: Story = {
  render: () => (
    <TagsInput
      label="100 000 options tags input"
      placeholder="Use limit to optimize performance"
      limit={5}
      data={largeData}
    />
  ),
}

/**
 * By default, `TagsInput` filters options by checking if the option label contains
 * input value. You can change this behavior with `filter` prop. `filter` function
 * receives an object with the following properties as a single argument:
 *
 * - `options` – array of options or options groups, all options are in
 * `{ value: string; label: string; disabled?: boolean }` format
 * - `search` – current search query
 * - `limit` – value of limit prop passed to TagsInput
 */
export const CustomizedOptionsFilter: Story = {
  render: () => {
    const optionsFilter: SelectOptionsFilter = ({ options, search }) => {
      const splittedSearch = search.toLowerCase().trim().split(' ')
      return (options as SelectItem[]).filter((option) => {
        const words = option.label.toLowerCase().trim().split(' ')
        return splittedSearch.every((searchWord) =>
          words.some((word) => word.includes(searchWord))
        )
      })
    }

    return (
      <TagsInput
        label="Press Enter to submit a tag"
        placeholder="Enter tag"
        data={['React', 'Angular', 'Svelte']}
        filter={optionsFilter}
      />
    )
  },
}
