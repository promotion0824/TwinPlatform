import userEvent from '@testing-library/user-event'
import { render, screen } from '../../../jest/testUtils'

import { Textarea } from '.'

describe('The onChange function', () => {
  const mocked = jest.fn()

  afterEach(() => {
    jest.clearAllMocks()
  })

  it('should be triggered when clear default value and typing', async () => {
    render(<Textarea onChange={mocked} defaultValue="Default text" />)

    const textarea = screen.getByRole('textbox')
    const text = 'value'
    await userEvent.clear(textarea)
    await userEvent.type(textarea, text)

    expect(mocked.mock.calls.length).toBe(1 + text.length)
    expect(textarea).toHaveValue(text)
  })

  it('should not be triggered when it is readonly', async () => {
    render(<Textarea readOnly value="Value Text" onChange={mocked} />)

    const textarea = screen.getByRole('textbox')
    await userEvent.type(textarea, 'value')

    expect(mocked.mock.calls.length).toBe(0)
  })

  it('should not be triggered when it is disabled', () => {
    render(
      <Textarea onChange={mocked} disabled placeholder="Placeholder Text" />
    )

    const textarea = screen.getByRole('textbox')

    expect(textarea).toBeDisabled()
  })
})

// Won't test for maxLength as it's feature managed by HTML
// Won't test for resize as it's feature managed by HTML
// Won't test for maxRows and minRows as it's feature managed by Mantine
