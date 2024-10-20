import { act, render } from '@testing-library/react'
import { NavTab, NavTabs } from '@willow/ui'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'

describe('NavTab', () => {
  test('expect the first tab to be selected by default', () => {
    const { getAllByRole } = render(<SampleTabs />, {
      wrapper: Wrapper,
    })

    const tabs = getAllByRole('tab')
    expect(tabs[0]).toHaveAttribute('aria-selected', 'true')
    expect(tabs[1]).toHaveAttribute('aria-selected', 'false')
    expect(tabs[2]).toHaveAttribute('aria-selected', 'false')
  })

  test('expect a default tab to be selected by one is specified', () => {
    const { getAllByRole } = render(<SampleTabs defaultValue="third" />, {
      wrapper: Wrapper,
    })

    const tabs = getAllByRole('tab')
    expect(tabs[0]).toHaveAttribute('aria-selected', 'false')
    expect(tabs[1]).toHaveAttribute('aria-selected', 'false')
    expect(tabs[2]).toHaveAttribute('aria-selected', 'true')
  })

  test('expect a tab to be selected if its "to" route matches the current route', () => {
    const { getAllByRole } = render(<SampleTabs />, {
      wrapper: ({ children }) => (
        <Wrapper initialEntries={['/second']}>{children}</Wrapper>
      ),
    })

    const tabs = getAllByRole('tab')
    expect(tabs[0]).toHaveAttribute('aria-selected', 'false')
    expect(tabs[1]).toHaveAttribute('aria-selected', 'true')
    expect(tabs[2]).toHaveAttribute('aria-selected', 'false')
  })
})

test('expect a tab to be selected if any of its "include" routes matches the current route', () => {
  const { getAllByRole } = render(<SampleTabs />, {
    wrapper: ({ children }) => (
      <Wrapper initialEntries={['/three']}>{children}</Wrapper>
    ),
  })

  const tabs = getAllByRole('tab')
  expect(tabs[0]).toHaveAttribute('aria-selected', 'false')
  expect(tabs[1]).toHaveAttribute('aria-selected', 'false')
  expect(tabs[2]).toHaveAttribute('aria-selected', 'true')
})

test('expect a tab to be selected if it is clicked', () => {
  const { getAllByRole } = render(<SampleTabs />, {
    wrapper: Wrapper,
  })

  const tabs = getAllByRole('tab')
  act(() => tabs[2].click())
  expect(tabs[0]).toHaveAttribute('aria-selected', 'false')
  expect(tabs[1]).toHaveAttribute('aria-selected', 'false')
  expect(tabs[2]).toHaveAttribute('aria-selected', 'true')
})

const SampleTabs = (props) => (
  <NavTabs
    {...props}
    tabs={[
      <NavTab to="/first" value="first">
        First
      </NavTab>,
      <NavTab to="/second" value="second">
        Second
      </NavTab>,
      <NavTab include={['/three']} to="/third" value="third">
        Third
      </NavTab>,
    ]}
  />
)
