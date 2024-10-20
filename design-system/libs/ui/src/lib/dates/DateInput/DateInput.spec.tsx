import { act, render } from '../../../jest/testUtils'

import { DateInput } from '.'

describe('DateInput', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<DateInput />)
    expect(baseElement).toBeTruthy()
  })

  it('should clear the date value when clear button clicked', () => {
    const defaultDate = 'October 9, 2023'
    const { getByRole } = render(
      <DateInput clearable defaultValue={new Date(defaultDate)} />
    )
    const input = getByRole('textbox')
    const clearButton = getByRole('button', { name: 'close' })

    expect(input).toHaveValue(defaultDate)
    expect(clearButton).toBeInTheDocument()

    act(() => {
      clearButton.click()
    })
    expect(input).toHaveValue('')
  })
})
