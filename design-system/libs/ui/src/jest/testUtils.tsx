import { render, RenderOptions } from '@testing-library/react'
import { ReactElement, ReactNode } from 'react'

import { ThemeProvider } from '../'

// To enable 'data-' attribute as a valid type for HTMLAttributes
declare module 'react' {
  interface HTMLAttributes<T> extends AriaAttributes, DOMAttributes<T> {
    [key: `data-${string}`]: unknown
  }
}

const AllProviders = ({ children }: { children: ReactNode }) => {
  return <ThemeProvider name="dark">{children}</ThemeProvider>
}

const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'queries'>
) => render(ui, { wrapper: AllProviders, ...options })

export const mockMatchMedia = () => {
  // https://jestjs.io/docs/manual-mocks#mocking-methods-which-are-not-implemented-in-jsdom
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: jest.fn().mockImplementation((query) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: jest.fn(), // Deprecated
      removeListener: jest.fn(), // Deprecated
      addEventListener: jest.fn(),
      removeEventListener: jest.fn(),
      dispatchEvent: jest.fn(),
    })),
  })
}

mockMatchMedia()

window.ResizeObserver =
  window.ResizeObserver ||
  jest.fn().mockImplementation(() => ({
    disconnect: jest.fn(),
    observe: jest.fn(),
    unobserve: jest.fn(),
  }))

window.HTMLElement.prototype.scrollIntoView = jest.fn()

// re-export everything
export * from '@testing-library/react'
// override render method
export { customRender as render }
// export everything from user-event
export { default as userEvent } from '@testing-library/user-event'
