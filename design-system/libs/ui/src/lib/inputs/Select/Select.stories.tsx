import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'

import { Select } from '.'
import { Loader } from '../../feedback/Loader'
import { Icon } from '../../misc/Icon'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof Select> = {
  title: 'Select',
  component: Select,
  decorators: [
    (Story) => (
      <div css={{ height: 210 }}>
        <Story />
      </div>
    ),
  ],
  args: {
    data: [
      { value: 'react', label: 'React' },
      { value: 'ng', label: 'Angular' },
      { value: 'svelte', label: 'Svelte' },
      { value: 'vue', label: 'Vue' },
    ],
  },
}
export default meta

type Story = StoryObj<typeof Select>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    initiallyOpened: true,
    defaultValue: 'react',
  },
}

export const DisabledOption: Story = {
  ...storybookAutoSourceParameters,
  args: {
    data: [
      { value: 'react', label: 'React', disabled: true },
      { value: 'ng', label: 'Angular' },
      { value: 'svelte', label: 'Svelte', disabled: true },
      { value: 'vue', label: 'Vue' },
    ],
  },
}

export const Disabled: Story = {
  ...storybookAutoSourceParameters,
  args: {
    disabled: true,
    value: 'vue',
  },
}

export const Readonly: Story = {
  ...storybookAutoSourceParameters,
  args: {
    readOnly: true,
    value: 'vue',
  },
}

export const Error: Story = {
  ...storybookAutoSourceParameters,
  args: {
    error: true,
  },
}

export const WithDescription: Story = {
  ...storybookAutoSourceParameters,
  args: {
    description: 'Select description text',
  },
}

export const WithLabelAndErrorMessage: Story = {
  ...storybookAutoSourceParameters,
  args: {
    error: 'This is an error message',
    label: 'This is a label',
    required: true,
  },
}

export const HorizontalLayout: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'This is a label',
    error: 'This is an error message',
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    labelWidth: 300,
    label: 'This is a label',
    error: 'This is an error message',
  },
}

export const Prefix: Story = {
  ...storybookAutoSourceParameters,
  args: {
    prefix: <Icon icon="info" />,
  },
}

export const GroupingItems: Story = {
  ...storybookAutoSourceParameters,
  args: {
    data: [
      { group: 'Group A', items: [{ value: 'rick', label: 'Rick' }] },
      { group: 'Group B', items: [{ value: 'morty', label: 'Morty' }] },
      { group: ' ', items: [{ value: 'summer', label: 'Summer' }] },
    ],
  },
}

export const ControlledValue: Story = {
  render: (props) => {
    const [value, setValue] = useState<string | null>('vue')

    return <Select {...props} value={value} onChange={setValue} />
  },
}

export const Searchable: Story = {
  render: (props) => (
    <Select
      {...props}
      placeholder="Type any word to search"
      searchable
      nothingFound="No results found"
    />
  ),
}

export const ControlledSearchValue: Story = {
  render: (props) => {
    const [searchValue, setSearchValue] = useState<string>('')
    return (
      <Select
        {...props}
        placeholder="Type any word to search"
        searchable
        searchValue={searchValue}
        onSearchChange={setSearchValue}
      />
    )
  },
}
export const Clearable: Story = {
  render: (props) => <Select {...props} defaultValue="react" clearable />,
}

const getAsyncData = () =>
  new Promise<string[]>((resolve) => {
    window.setTimeout(() => {
      resolve(['React', 'Angular', 'Svelte', 'Vue', 'React Native'])
    }, 2000)
  })

export const AsyncDataLoading: Story = {
  render: () => {
    const [loading, setLoading] = useState(false)
    const [data, setData] = useState<string[]>([])

    const onOpen = () => {
      setData([]) // you don't have to clear loaded data each time
      setLoading(true)
      getAsyncData().then((response) => {
        setData(response)
        setLoading(false)
      })
    }

    return (
      <Select
        label="Async data loading"
        placeholder="Click to load data"
        suffix={loading ? <Loader /> : undefined}
        data={data}
        onDropdownOpen={onOpen}
      />
    )
  },
}
