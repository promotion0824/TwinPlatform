import {
  ByRoleOptions,
  render,
  screen,
  waitFor,
  userEvent,
} from '../../../jest/testUtils'

import { Popover, PopoverProps } from '.'

function trigger(options?: ByRoleOptions) {
  return screen.getByText('Trigger', options)
}

function popoverDropdown() {
  return screen.getByRole('dialog')
}

const DefaultPopover = (props: Partial<PopoverProps>) => (
  <Popover {...props}>
    <Popover.Target>
      <button>Trigger</button>
    </Popover.Target>
    <Popover.Dropdown>Content</Popover.Dropdown>
  </Popover>
)

describe('Popover content', () => {
  it('should be initially hidden', async () => {
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('should show when click the trigger', async () => {
    render(<DefaultPopover />)

    await userEvent.click(trigger())

    await waitFor(() => expect(popoverDropdown()).toBeVisible())
  })

  it('should hide when click the trigger twice', async () => {
    render(<DefaultPopover />)
    await userEvent.click(trigger())

    await userEvent.click(trigger())

    // Wait for the dialog to be removed from the DOM
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
  })

  it('should hide when press escape key', async () => {
    render(<DefaultPopover />)
    await userEvent.hover(trigger())

    await userEvent.keyboard('{escape}')

    // Wait for the dialog to be removed from the DOM
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
  })

  it('should not be able to trigger with disabled trigger component with click', async () => {
    render(
      <Popover>
        <Popover.Target>
          <button disabled>Trigger</button>
        </Popover.Target>
        <Popover.Dropdown>Popover Content</Popover.Dropdown>
      </Popover>
    )

    await userEvent.click(trigger())

    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
  })
})

describe('With defaultOpened = true, the Popover content', () => {
  it('should be initially visible', async () => {
    render(<DefaultPopover defaultOpened />)
    await screen.findByRole('dialog')

    await waitFor(() => expect(popoverDropdown()).toBeVisible())
  })

  it('should hide after click the trigger once', async () => {
    render(<DefaultPopover defaultOpened />)
    await userEvent.click(trigger())

    // Wait for the dialog to be removed from the DOM
    await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull())
  })
})

describe('With opened = true, the Popover content', () => {
  it('should be initially visible', async () => {
    render(<DefaultPopover opened />)
    await screen.findByRole('dialog')

    await waitFor(() => expect(popoverDropdown()).toBeVisible())
  })

  it('should stay visible when click the trigger twice', async () => {
    render(<DefaultPopover opened />)
    await userEvent.click(trigger())

    await userEvent.click(trigger())

    await waitFor(() => expect(popoverDropdown()).toBeVisible())
  })
})

describe('onChange', () => {
  it('should call onChange when clicking', async () => {
    const mockOnChange = jest.fn()
    render(<DefaultPopover onChange={mockOnChange} />)

    await userEvent.click(trigger())

    expect(mockOnChange.mock.calls.length).toEqual(1)
  })
})
