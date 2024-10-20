import userEvent from '@testing-library/user-event'
import { Sidebar, SidebarGroup, SidebarLink, SidebarProps } from '.'
import { act, render, screen, waitFor } from '../../../jest/testUtils'

const getSampleSidebar = (props?: Omit<SidebarProps, 'children'>) => (
  <Sidebar {...props}>
    <SidebarGroup>
      <SidebarLink href="/home" icon="home" isActive label="Home" />
      <SidebarLink href="/dashboards" icon="dashboard" label="Dashboards" />
      <SidebarLink href="/reports" icon="assignment" label="Reports" />
    </SidebarGroup>
    <SidebarGroup>
      <SidebarLink href="/time-series" icon="timeline" label="Time Series" />
      <SidebarLink
        href="/classic-explorer"
        icon="language"
        label="Classic Explorer"
      />
    </SidebarGroup>
  </Sidebar>
)

describe('Sidebar', () => {
  it('should collapse when the collapse button is pressed', () => {
    const { getByText } = render(getSampleSidebar())

    const homeLink = getByText('Home')
    expect(homeLink).toBeVisible()

    const collapseButton = getByText('first_page')
    act(() => collapseButton.click())
    expect(homeLink).not.toBeVisible()
  })

  it('should expand when the expand button is pressed', async () => {
    const { getByText, queryByText } = render(
      getSampleSidebar({ collapsedByDefault: true })
    )

    expect(queryByText('Home')).toBeNull()

    const collapseButton = getByText('last_page')
    act(() => collapseButton.click())
    expect(queryByText('Home')).toBeVisible()
  })

  it('should show a tooltip when hovering on a collapsed sidebar link', async () => {
    const { getByText } = render(getSampleSidebar({ collapsedByDefault: true }))

    const homeIcon = getByText('home')
    await userEvent.hover(homeIcon)

    await waitFor(() => expect(screen.getByRole('tooltip')).toBeInTheDocument())
  })
})
