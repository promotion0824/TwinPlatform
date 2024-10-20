import type { Meta, StoryObj } from '@storybook/react'
import { Link as ReactRouterLink } from 'react-router-dom'
import { Sidebar, SidebarGroup, SidebarLink } from '.'
import {
  MemoryRouterDecorator,
  storyContainerTestId,
} from '../../../storybookUtils'

const SidebarContainer = (Story: React.ComponentType) => (
  <div
    data-testid={storyContainerTestId}
    style={{ height: '450px', width: 'fit-content' }}
  >
    <Story />
  </div>
)

const meta: Meta<typeof Sidebar> = {
  title: 'Sidebar',
  component: Sidebar,
  decorators: [SidebarContainer],
}
export default meta

type Story = StoryObj<typeof Sidebar>

export const Playground: Story = {
  render: () => (
    <Sidebar>
      <SidebarGroup>
        <SidebarLink href="/home" icon="home" isActive label="Home" />
        <SidebarLink href="/dashboards" icon="dashboard" label="Dashboards" />
        <SidebarLink href="/reports" icon="assignment" label="Reports" />
      </SidebarGroup>
    </Sidebar>
  ),
}

export const MultipleGroups: Story = {
  render: () => (
    <Sidebar>
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
  ),
}

/** A group can be set to `fill` if you want to push the following groups to the end of the `Sidebar`. */
export const Fill: Story = {
  render: () => (
    <Sidebar>
      <SidebarGroup>
        <SidebarLink href="/home" icon="home" isActive label="Home" />
        <SidebarLink href="/dashboards" icon="dashboard" label="Dashboards" />
        <SidebarLink href="/reports" icon="assignment" label="Reports" />
      </SidebarGroup>
      <SidebarGroup fill>
        <SidebarLink href="/time-series" icon="timeline" label="Time Series" />
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
  ),
}

export const CollapsedByDefault: Story = {
  render: () => (
    <Sidebar collapsedByDefault>
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
  ),
}

export const HideFooter: Story = {
  render: () => (
    <Sidebar withFooter={false}>
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
  ),
}

/** You can render the links as a different component by using the `component` prop. */
export const Polymorphism: Story = {
  decorators: [MemoryRouterDecorator],
  render: () => (
    <Sidebar>
      <SidebarGroup>
        <SidebarLink
          component={ReactRouterLink}
          icon="home"
          isActive
          label="Home"
          to="/home"
        />
        <SidebarLink
          component={ReactRouterLink}
          icon="dashboard"
          label="Dashboards"
          to="/dashboards"
        />
        <SidebarLink
          component={ReactRouterLink}
          icon="assignment"
          label="Reports"
          to="/reports"
        />
      </SidebarGroup>
    </Sidebar>
  ),
}
