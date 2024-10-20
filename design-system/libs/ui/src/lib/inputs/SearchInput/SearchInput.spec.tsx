import { render, fireEvent } from '../../../jest/testUtils'
import { SearchInput } from './SearchInput'

describe('SearchInput', () => {
  test('input value should be empty initially', () => {
    const { getByPlaceholderText } = render(
      <SearchInput placeholder="Placeholder Text" />
    )
    expect(getByPlaceholderText('Placeholder Text')).toHaveValue('')
  })

  test('typing in the search box works correctly', () => {
    const { getByPlaceholderText } = render(
      <SearchInput placeholder="Placeholder Text" />
    )
    const input = getByPlaceholderText('Placeholder Text')
    fireEvent.change(input, { target: { value: 'test' } })
    expect(input).toHaveValue('test')
  })

  test('input value should be initialized with defaultValue', () => {
    const { getByPlaceholderText } = render(
      <SearchInput
        placeholder="Placeholder Text"
        defaultValue="initial value"
      />
    )
    expect(getByPlaceholderText('Placeholder Text')).toHaveValue(
      'initial value'
    )
  })

  test('input should be disabled when disabled prop is true', () => {
    const { getByPlaceholderText } = render(
      <SearchInput placeholder="Placeholder Text" disabled />
    )
    expect(getByPlaceholderText('Placeholder Text')).toBeDisabled()
  })
})
