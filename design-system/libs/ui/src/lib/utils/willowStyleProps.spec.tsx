import { renderHook } from '@testing-library/react'
import { darkTheme } from '@willowinc/theme'
import { get } from 'lodash'
import { mockMatchMedia } from '../../jest/testUtils'
import { ThemeProvider } from '../theme'
import { useWillowStyleProps } from './willowStyleProps'

const spacingTokenTestCases = [
  'm',
  'mt',
  'mb',
  'ml',
  'mr',
  'mx',
  'my',
  'ms',
  'me',

  'p',
  'pt',
  'pb',
  'pl',
  'pr',
  'px',
  'py',
  'ps',
  'pe',

  'top',
  'left',
  'bottom',
  'right',
] as const
const freeDimensionTestCases = ['w', 'miw', 'maw', 'h', 'mih', 'mah'] as const
const colorTokenTestCases = ['bg', 'c'] as const

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <ThemeProvider name="dark">{children}</ThemeProvider>
)

describe('useWillowStyleProps', () => {
  beforeAll(mockMatchMedia)

  it.each(spacingTokenTestCases)(
    'should return the correct style s12 token value for %s',
    (prop) => {
      const spacingToken = 's12'

      const { result } = renderHook(
        () => useWillowStyleProps({ [prop]: spacingToken }),
        { wrapper }
      )

      expect(result.current[prop]).toEqual(darkTheme.spacing[spacingToken])
    }
  )

  it.each([...freeDimensionTestCases, ...spacingTokenTestCases])(
    'should return the identical size value for %s',
    (prop) => {
      const size = '20px'

      const { result } = renderHook(
        () => useWillowStyleProps({ [prop]: size }),
        { wrapper }
      )

      expect(result.current[prop]).toEqual(size)
    }
  )

  it.each([...freeDimensionTestCases, ...spacingTokenTestCases])(
    'should return undefined if %s is undefined',
    (prop) => {
      const { result } = renderHook(() => useWillowStyleProps({}), { wrapper })
      expect(result.current[prop]).toBeUndefined()
    }
  )

  it.each(colorTokenTestCases)(
    'should return the correct color token value for %s',
    (prop) => {
      const colorToken = 'neutral.fg.default'

      const { result } = renderHook(
        () => useWillowStyleProps({ [prop]: colorToken }),
        { wrapper }
      )

      expect(result.current[prop]).toEqual(get(darkTheme.color, colorToken))
    }
  )
})
