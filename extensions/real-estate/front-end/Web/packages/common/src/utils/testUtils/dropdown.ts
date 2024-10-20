import { fireEvent, within, screen } from '@testing-library/react'

/**
 * Do some setup to make the openDropdown function work, and clean up when
 * tests are done.
 */
export function supportDropdowns() {
  const hadPointerEvent = window.PointerEvent != null

  beforeAll(() => {
    // This is a workaround to make testing-library populate the `button`
    // property of pointer events - without this, the dropdown will not open.
    // https://github.com/testing-library/user-event/issues/926
    if (!window.PointerEvent) {
      ;(window as any).PointerEvent = window.MouseEvent
    }
  })

  afterAll(() => {
    if (!hadPointerEvent) {
      delete (window as any).PointerEvent
    }
  })
}

export function openDropdown(element: HTMLElement) {
  // The dropdown only listens to pointer down, so a regular click event will
  // not cause it to open.
  fireEvent.pointerDown(element, { button: 0 })
}

export function closeDropdown(element: HTMLElement) {
  // To close a dropdown we just click the button again.
  fireEvent.pointerDown(element, { button: 0 })
}

/**
 * Get the element containing the content for a currently-open dropdown. We do
 * not have a good way of specifying *which* dropdown, but it does not appear
 * to be possible to have more than one dropdown open at the same time anyway.
 */
export function getDropdownContent() {
  return screen.getByTestId('dropdown-content')
}

export function getDropdownButton(
  container: HTMLElement,
  currentValue: string
) {
  return within(container).getByText(currentValue)
}
