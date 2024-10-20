import type { Meta, StoryObj } from '@storybook/react'
import { debounce } from 'lodash'
import { useRef, useState } from 'react'
import styled from 'styled-components'

import { SearchInput } from '.'
import { Loader } from '../../feedback/Loader'
import { storybookAutoSourceParameters } from '../../utils/constant'

const MOCK_DATA = [
  'apple',
  'banana',
  'cherry',
  'date',
  'fig',
  'grape',
  'kiwi',
  'lemon',
  'mango',
  'nectarine',
  'orange',
  'papaya',
]

const filterData = (query: string) =>
  MOCK_DATA.filter((item) => item.toLowerCase().includes(query.toLowerCase()))

const meta: Meta<typeof SearchInput> = {
  title: 'SearchInput',
  component: SearchInput,
  args: {
    placeholder: 'Placeholder Text',
  },
}
export default meta

type Story = StoryObj<typeof SearchInput>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const WithDefaultValue: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: 'Value Text',
  },
}

export const WithLabelAndDescription: Story = {
  ...storybookAutoSourceParameters,
  args: {
    label: 'Label',
    description: 'This is a description text',
  },
}

export const Disabled: Story = {
  ...storybookAutoSourceParameters,
  args: {
    disabled: true,
  },
}

export const AsyncSearch: Story = {
  render: () => {
    const [searchState, setSearchState] = useState<
      'idle' | 'loading' | 'complete'
    >('idle')
    const [results, setResults] = useState<string[]>([])
    const timeoutRef = useRef<NodeJS.Timeout>()

    const performSearch = async (query: string) => {
      // Clear any existing timeouts
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }

      // If query is empty, reset results and return
      if (!query.trim()) {
        setResults([])
        setSearchState('idle')
        return
      }

      setSearchState('loading')
      // Use timeout to simulate a delay
      timeoutRef.current = setTimeout(() => {
        setResults(filterData(query))
        setSearchState('complete')
      }, 1500)
    }

    const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
      const query = event.target.value
      performSearch(query)
    }

    return (
      <StyledWrapper>
        <SearchInput label="Async Search" onChange={handleChange} />
        <div css={{ marginTop: 16 }}>
          {searchState === 'loading' && <Loader />}
        </div>
        {searchState === 'complete' && (
          <ul>
            {results.map((result, index) => (
              <li key={index}>{result}</li>
            ))}
          </ul>
        )}
      </StyledWrapper>
    )
  },
}

export const DebouncedSearch: Story = {
  render: () => {
    const [results, setResults] = useState<string[]>([])
    const debouncedSearch = debounce((query: string) => {
      setResults(filterData(query))
    }, 1000)

    const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
      const query = event.target.value

      // If query is empty, reset results immediately and cancel any debounced calls
      if (!query.trim()) {
        debouncedSearch.cancel()
        setResults([])
        return
      }

      debouncedSearch(query)
    }

    return (
      <StyledWrapper>
        <SearchInput label="Debounced Search" onChange={handleChange} />
        <ul>
          {results.map((result, index) => (
            <li key={index}>{result}</li>
          ))}
        </ul>
      </StyledWrapper>
    )
  },
}

const StyledWrapper = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
}))
