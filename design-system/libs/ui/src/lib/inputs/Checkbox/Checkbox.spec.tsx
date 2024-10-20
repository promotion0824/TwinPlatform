import { render, screen } from '../../../jest/testUtils'
import { Checkbox } from './'

describe('Checkbox', () => {
  it('renders correctly', () => {
    const { container } = render(<Checkbox label="Test label" />)
    expect(container).toBeInTheDocument()
  })

  it('renders the correct label', () => {
    render(<Checkbox label="Test label" />)
    const label = screen.getByText('Test label')
    expect(label).toBeInTheDocument()
  })

  it('renders disabled when disabled is true', () => {
    render(<Checkbox label="Test label" disabled />)
    const checkbox = screen.getByRole('checkbox')
    expect(checkbox).toBeDisabled()
  })

  it('renders with an error message when error prop is provided', () => {
    const errorMessage = 'This field is required'
    render(<Checkbox label="Test label" error={errorMessage} />)
    const error = screen.getByText(errorMessage)
    expect(error).toBeInTheDocument()
  })

  it('height should remain 16px when error is true', () => {
    const { container } = render(<Checkbox label="Label" error />)
    expect(container).toHaveStyle('height: 16')
  })
})
