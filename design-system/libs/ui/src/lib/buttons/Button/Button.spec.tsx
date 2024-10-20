import { act, render, screen } from '../../../jest/testUtils'
import userEvent from '@testing-library/user-event'

import { Button } from '.'

describe('Button', () => {
  it('should have type = button attribute if no as or type provided', () => {
    const { getByRole } = render(<Button>Button</Button>)
    expect(getByRole('button')).toHaveAttribute('type', 'button')
  })

  it('should have type = submit attribute if provided', () => {
    const { getByRole } = render(<Button type="submit">Button</Button>)
    expect(getByRole('button')).toHaveAttribute('type', 'submit')
  })

  it('should have type = reset attribute if provided', () => {
    const { getByRole } = render(<Button type="reset">Button</Button>)
    expect(getByRole('button')).toHaveAttribute('type', 'reset')
  })
})

describe('the onClick functionality', () => {
  it('should be triggered when click the button', async () => {
    const mockOnClick = jest.fn()
    render(<Button onClick={mockOnClick}>button</Button>)

    await userEvent.click(screen.getByRole('button'))
    expect(mockOnClick.mock.calls.length).toBe(1)

    await userEvent.click(screen.getByRole('button'))
    expect(mockOnClick.mock.calls.length).toBe(2)
  })

  it('should be triggered when press down Enter key', async () => {
    const mockOnClick = jest.fn()
    render(<Button onClick={mockOnClick}>button</Button>)

    // this act wrapper is required due to base library implementation
    act(() => {
      screen.getByRole('button').focus()
    })
    expect(screen.getByRole('button')).toHaveFocus()

    await userEvent.keyboard('{enter}')
    expect(mockOnClick.mock.calls.length).toBe(1)

    await userEvent.keyboard('{enter}')
    expect(mockOnClick.mock.calls.length).toBe(2)
  })

  it('should be triggered when press down Space key', async () => {
    const mockOnClick = jest.fn()
    render(<Button onClick={mockOnClick}>button</Button>)

    // this act wrapper is required to pass the test
    act(() => {
      screen.getByRole('button').focus()
    })
    expect(screen.getByRole('button')).toHaveFocus()

    await userEvent.keyboard(' ')
    expect(mockOnClick.mock.calls.length).toBe(1)

    await userEvent.keyboard(' ')
    expect(mockOnClick.mock.calls.length).toBe(2)
  })

  it('should be disabled when disabled is true', async () => {
    const mockOnClick = jest.fn()
    render(
      <Button onClick={mockOnClick} disabled>
        button
      </Button>
    )

    expect(screen.getByRole('button')).toBeDisabled()
  })
})

describe('Link Button', () => {
  it('should trigger onClick when clicked', async () => {
    const mockOnClick = jest.fn()
    render(
      <Button onClick={mockOnClick} href="#">
        link
      </Button>
    )

    await userEvent.click(screen.getByText('link'))
    expect(mockOnClick.mock.calls.length).toBe(1)

    await userEvent.click(screen.getByText('link'))
    expect(mockOnClick.mock.calls.length).toBe(2)
  })

  it('should render as an anchor element if href provided', () => {
    render(<Button href="#">link</Button>)

    expect(screen.getByRole('link')).toBeInTheDocument()
  })

  it('should have href attribute', () => {
    const testUrl = 'test.url'
    render(<Button href={testUrl}>link</Button>)

    expect(screen.getByRole('link')).toHaveAttribute('href', testUrl)
  })

  it('should have target attribute if provided', () => {
    const testTarget = '_blank'
    render(
      <Button href="#" target={testTarget}>
        link
      </Button>
    )

    expect(screen.getByRole('link')).toHaveAttribute('target', testTarget)
  })

  it('should not have target attribute if rendered as button', () => {
    render(<Button target="_blank">button</Button>)

    expect(screen.getByRole('button')).not.toHaveAttribute('target')
  })

  it('should not have type = button attribute', () => {
    const { queryByText } = render(<Button href="#">Button</Button>)
    expect(queryByText('Button')).not.toHaveAttribute('type')
  })
})
