import type { StoryObj } from '@storybook/react'
import { Sidebar, SidebarGroup, SidebarLink } from '.'
import { storyContainerTestId } from '../../../storybookUtils'

const defaultStory = {
  component: Sidebar,
  title: 'Sidebar',
}

export default defaultStory

type Story = StoryObj<typeof Sidebar>

export const Overflow: Story = {
  render: () => (
    <div
      data-testid={storyContainerTestId}
      style={{ height: '280px', width: 'fit-content' }}
    >
      <Sidebar>
        <SidebarGroup>
          <SidebarLink href="/home" icon="home" isActive label="Home" />
          <SidebarLink href="/dashboards" icon="dashboard" label="Dashboards" />
          <SidebarLink href="/reports" icon="assignment" label="Reports" />
        </SidebarGroup>
        <SidebarGroup>
          <SidebarLink
            href="/time-series"
            icon="timeline"
            label="Time Series"
          />
          <SidebarLink
            href="/classic-explorer"
            icon="language"
            label="Classic Explorer"
          />
        </SidebarGroup>
        <SidebarGroup>
          <SidebarLink
            href="/marketplace"
            icon="inventory_2"
            label="Marketplace"
          />
          <SidebarLink href="/admin" icon="settings" label="Admin" />
        </SidebarGroup>
      </Sidebar>
    </div>
  ),
}
