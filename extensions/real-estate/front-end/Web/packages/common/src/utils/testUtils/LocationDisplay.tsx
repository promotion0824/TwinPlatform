import { screen } from '@testing-library/react'
import { useLocation } from 'react-router'

/**
 * A component to be used in tests to assert on current pathname
 * reference: https://testing-library.com/docs/example-react-router
 */
export const LocationDisplay = () => {
  const location = useLocation()

  return <div data-testid="location-display">{location.pathname}</div>
}

export const assertPathnameContains = (expectedPathname: string) =>
  expect(screen.queryByTestId('location-display')).toHaveTextContent(
    expectedPathname
  )
