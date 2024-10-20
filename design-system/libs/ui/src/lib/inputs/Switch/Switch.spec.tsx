import userEvent from '@testing-library/user-event'
import { Switch } from '.'
import { render, screen } from '../../../jest/testUtils'

const switchToggle = () => screen.getByRole('switch')
describe('the onChange function', () => {
  const mocked = jest.fn()

  afterEach(() => {
    jest.clearAllMocks()
  })

  it('should be triggered when the switch is clicked', async () => {
    render(<Switch onClick={mocked} />)

    await userEvent.click(switchToggle())
    expect(mocked.mock.calls.length).toBe(1)

    await userEvent.click(switchToggle())
    expect(mocked.mock.calls.length).toBe(2)
  })

  it('should be triggered when focused and the space key is pressed', async () => {
    render(<Switch onClick={mocked} />)

    switchToggle().focus()

    await userEvent.keyboard(' ')
    expect(mocked.mock.calls.length).toBe(1)

    await userEvent.keyboard(' ')
    expect(mocked.mock.calls.length).toBe(2)
  })

  it('should be triggered when the label is clicked', async () => {
    render(<Switch onClick={mocked} label="Label" />)
    const labelText = screen.getByText('Label')

    await userEvent.click(labelText)
    expect(mocked.mock.calls.length).toBe(1)

    await userEvent.click(labelText)
    expect(mocked.mock.calls.length).toBe(2)
  })

  it('should be disabled when disabled is true', async () => {
    render(<Switch onClick={mocked} disabled label="Label" />)

    expect(switchToggle()).toBeDisabled()

    const labelText = screen.getByText('Label')
    await userEvent.click(labelText)

    // onChange will not be triggered by clicking the label text
    expect(mocked.mock.calls.length).toBe(0)
  })
})
