import 'regenerator-runtime/runtime'
import '@testing-library/jest-dom'
import * as jestExtendedMatchers from 'jest-extended'
import getRandomValues from 'polyfill-crypto.getrandomvalues'
import ResizeObserver from 'resize-observer-polyfill'
import { configure } from '@testing-library/react'
import { TextEncoder, TextDecoder } from 'util'

// crypto api(standard) in uuid package is not supported in testing env
window.crypto = { getRandomValues }

// Set ResizeObserver with polyfilled version as it is not provided by jsdom.
window.ResizeObserver = ResizeObserver

expect.extend(jestExtendedMatchers)

// Mock out some modules that cause errors on import if we try to use them from
// tests, likely because they assume the existence of a real browser. In future
// we may want to turn the Mapbox mock into something that more realistically
// mocks the Mapbox interface. We are deprecating PowerBI so we can remove that mock
// when we do that.

jest.mock('mapbox-gl/dist/mapbox-gl', () => ({
  Map: () => ({}),
}))

jest.mock('powerbi-client-react', () => ({
  PowerBIEmbed: () => ({}),
}))

jest.mock('react-markdown', () => ({ children }) => <>{children}</>)

globalThis.TextEncoder = TextEncoder
globalThis.TextDecoder = TextDecoder

configure({
  asyncUtilTimeout: 3000,
})

// virtualization helps to render minimum number of rows and columns for performance
// but it will cause issue when testing with MUI's DataGrid, so we disable it in testing
// reference: https://mui.com/x/react-data-grid/virtualization/#disable-virtualization
jest.mock('@willowinc/ui', () => {
  const { DataGrid } = jest.requireActual('@willowinc/ui')
  return {
    ...jest.requireActual('@willowinc/ui'),
    DataGrid: (props) => <DataGrid {...props} disableVirtualization />,
  }
})

// This is mocked as @container selectors aren't supported in JSDOM.
jest.mock('./packages/ui/src/components/ContainmentWrapper', () => {
  const getContainmentHelper = jest.requireActual(
    './packages/ui/src/components/ContainmentWrapper'
  )

  const { ContainmentWrapper } = getContainmentHelper.default()

  return () => ({
    ContainmentWrapper,
    getContainerQuery: () => '',
  })
})

// reference: https://jestjs.io/docs/manual-mocks#mocking-methods-which-are-not-implemented-in-jsdom
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(), // deprecated
    removeListener: jest.fn(), // deprecated
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
})

// reference: https://github.com/jsdom/jsdom/issues/1695#issuecomment-559095940
window.HTMLElement.prototype.scrollIntoView = jest.fn()
