import { act, fireEvent, render } from '../../../jest/testUtils'

import { Icon } from '../../misc/Icon'
import { TextInput } from '.'

describe('TextInput', () => {
  it('should successfully clear a value (uncontrolled)', () => {
    const { getByLabelText, getByText } = render(
      <TextInput clearable label="input" />
    )

    const input = getByLabelText('input') as HTMLInputElement
    fireEvent.input(input, { target: { value: 'bulbasaur' } })
    expect(input.value).toBe('bulbasaur')

    const clearButton = getByText('close')
    act(() => clearButton.click())
    expect(input.value).toBe('')
  })

  it('should successfully clear a value (controlled)', () => {
    const setValue = jest.fn()

    const { getByLabelText, getByText } = render(
      <TextInput
        clearable
        label="input"
        onChange={(event) => setValue(event.target.value)}
        value="charmander"
      />
    )

    const input = getByLabelText('input') as HTMLInputElement
    expect(input.value).toBe('charmander')

    const clearButton = getByText('close')
    act(() => clearButton.click())
    expect(setValue.mock.lastCall[0]).toBe('')
  })

  it('should successfully clear a default value', () => {
    const { getByLabelText, getByText } = render(
      <TextInput clearable defaultValue="squirtle" label="input" />
    )

    const input = getByLabelText('input') as HTMLInputElement
    expect(input.value).toBe('squirtle')

    const clearButton = getByText('close')
    act(() => clearButton.click())
    expect(input.value).toBe('')
  })

  it('should switch between the suffix and clear button when using the clearable prop', () => {
    const { getByLabelText, getByText } = render(
      <TextInput
        clearable
        defaultValue="pikachu"
        label="input"
        suffix={<Icon icon="search" />}
      />
    )

    const clearButton = getByText('close')
    expect(clearButton).toBeVisible()
    act(() => clearButton.click())

    const suffix = getByText('search')
    expect(suffix).toBeVisible()

    const input = getByLabelText('input') as HTMLInputElement
    fireEvent.input(input, { target: { value: 'pikachu' } })
    expect(getByText('close')).toBeVisible()
  })
})
