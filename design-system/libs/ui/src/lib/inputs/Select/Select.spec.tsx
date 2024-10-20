import {
  act,
  render,
  screen,
  userEvent,
  waitFor,
} from '../../../jest/testUtils'

import { Select, SelectProps, SelectItem } from '.'

const defaultProps: { data: Exclude<SelectProps['data'], undefined> } = {
  data: [
    { value: 'apple', label: 'Apple' },
    { value: 'banana', label: 'Banana' },
  ],
}
const getSelectionBox = () => screen.getByRole('textbox') as HTMLInputElement
const getDropdown = () => screen.getByRole('listbox')
const getOptions = () => screen.queryAllByRole('option')

const toggleDropdown = async () => await userEvent.click(getSelectionBox())

const selectOption = async (order: number) => {
  const options = getOptions()
  return await userEvent.click(options[order])
}
describe('the dropdown', () => {
  it('should be toggled when click the select box', async () => {
    render(<Select {...defaultProps} />)

    // open dropdown
    await toggleDropdown()

    expect(getOptions()).toHaveLength(defaultProps.data.length)

    // close dropdown
    await toggleDropdown()

    expect(getOptions()).toHaveLength(0)
  })

  it('should be toggled when use KeyDown key', async () => {
    render(<Select {...defaultProps} />)

    getSelectionBox().focus()
    expect(getSelectionBox()).toHaveFocus()

    // open
    await userEvent.keyboard('{ArrowDown}')

    expect(getOptions()).toHaveLength(defaultProps.data.length)

    // close
    // cannot close the dropdown with keyboard (unless select an option)
    await toggleDropdown()

    expect(getOptions()).toHaveLength(0)
  })

  it('should not be able to toggle when select is disabled', async () => {
    render(<Select {...defaultProps} disabled />)

    expect(getSelectionBox()).toBeDisabled()
  })

  it('should not be able to toggle when select is readOnly', async () => {
    render(<Select {...defaultProps} readOnly />)

    const dropdown = screen.queryByRole('listbox')

    // cannot open by click
    await toggleDropdown()

    expect(dropdown).not.toBeInTheDocument()

    // cannot open by enter key
    getSelectionBox().focus()
    await userEvent.keyboard('{enter}')

    expect(dropdown).not.toBeInTheDocument()

    // cannot open by arrowDown key
    getSelectionBox().focus()
    await userEvent.keyboard('{ArrowDown}')

    expect(dropdown).not.toBeInTheDocument()
  })
})

describe('the selected item', () => {
  it('should be updated with click on the option', async () => {
    render(<Select {...defaultProps} defaultValue="Apple" />)

    await toggleDropdown() // open
    await selectOption(1) // select second option

    expect(getSelectionBox()).toHaveValue(
      (defaultProps.data[1] as SelectItem).label
    )
  })

  it('should be filter the options when typing the initial value in the select box', async () => {
    render(<Select {...defaultProps} searchable />)

    act(() => getSelectionBox().focus())
    await userEvent.paste('ap')

    expect(getDropdown()).toHaveTextContent('Apple')
    expect(getDropdown()).not.toHaveTextContent('Banana')
  })

  it('should be able to be updated with keyboard', async () => {
    render(<Select {...defaultProps} />)

    // Select the second option
    getSelectionBox().focus()
    act(() => {
      userEvent.keyboard('{ArrowDown}')
      userEvent.keyboard('{ArrowDown}')
      userEvent.keyboard('{enter}')
    })

    await waitFor(() => {
      expect(getSelectionBox()).toHaveValue(
        (defaultProps.data[1] as SelectItem).label
      )
    })

    // Select the first option
    getSelectionBox().focus()
    act(() => {
      // first ArrowDown opens dropdown
      userEvent.keyboard('{ArrowDown}')
      userEvent.keyboard('{ArrowUp}')
      userEvent.keyboard('{enter}')
    })

    await waitFor(() => {
      expect(getSelectionBox()).toHaveValue(
        (defaultProps.data[0] as SelectItem).label
      )
    })
  })

  it('should show the defaultValue if provided', () => {
    const option = defaultProps.data[1] as SelectItem

    render(<Select {...defaultProps} defaultValue={option.value} />)

    expect(getSelectionBox()).toHaveValue(option.label)
  })

  it('should be the value provided and it will outweigh the defaultValue', () => {
    const option = defaultProps.data[1] as SelectItem
    render(
      <Select
        {...defaultProps}
        defaultValue={(defaultProps.data[0] as SelectItem).value}
        value={option.value}
      />
    )

    expect(getSelectionBox()).toHaveValue(option.label)
  })

  it('should trigger onChange when update the selected value', async () => {
    const mocked = jest.fn()

    render(<Select {...defaultProps} onChange={mocked} />)

    await toggleDropdown() // open dropdown
    await selectOption(1) // select second option

    expect(mocked.mock.calls.length).toBe(1)

    await toggleDropdown() // open dropdown
    await selectOption(0) // select second option

    expect(mocked.mock.calls.length).toBe(2)
  })
})
