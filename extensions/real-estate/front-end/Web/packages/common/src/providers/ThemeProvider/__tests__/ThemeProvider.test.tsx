import { renderHook, waitFor } from '@testing-library/react'
import '@willow/common/utils/testUtils/matchMediaMock'
import { useTheme } from '@willowinc/theme'
import ThemeProvider from '../ThemeProvider'
import flattenObject from '../flattenObject'

/**
 * Test containing snapshot of css-variables and theme tokens for future
 * maintenance purpose if there are any changes from @willowinc/theme library.
 */
describe('ThemeProvider', () => {
  /**
   * Snapshot css variables. If there is any changes to the css variables,
   * please ensure these are reflected in the code base.
   */
  test('CSS variables should match snapshot', async () => {
    const { result } = renderHook(() => useTheme(), {
      wrapper: ThemeProvider,
    })

    await waitFor(() => {
      expect(Object.keys(result.current).length > 0).toBeTrue()
    })

    // Replication of css-variables because we are unable to test Global Styled component
    // created by createGlobalStyle API https://github.com/styled-components/jest-styled-components/issues/324
    expect(flattenObject(result.current, '-', '--theme-')).toMatchSnapshot()
  })

  /**
   * Snapshot JS theme token. If there is any changes to the JS theme tokens,
   * please ensure these are reflected in the styled components in the code base.
   */
  test('JS theme token should match snapshot', async () => {
    const { result } = renderHook(() => useTheme(), {
      wrapper: ThemeProvider,
    })

    await waitFor(() => {
      expect(Object.keys(result.current).length > 0).toBeTrue()
    })

    expect(flattenObject(result.current, '.', 'theme.')).toMatchSnapshot()
  })
})
