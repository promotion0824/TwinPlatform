import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'

import { Tabs } from '.'
import { Badge } from '../../data-display/Badge'
import { Icon } from '../../misc/Icon'

const meta: Meta<typeof Tabs> = {
  title: 'Tabs',
  component: Tabs,
}
export default meta

type Story = StoryObj<typeof Tabs>

export const DefaultSuffix: Story = {
  render: () => (
    <Tabs defaultValue="gallery">
      <Tabs.List>
        <Tabs.Tab value="gallery" suffix={<Icon icon="photo" />}>
          Gallery
        </Tabs.Tab>
        <Tabs.Tab value="messages" suffix={<Icon icon="chat" />}>
          Messages
        </Tabs.Tab>
        <Tabs.Tab value="settings" suffix={<Icon icon="settings" />} disabled>
          Settings
        </Tabs.Tab>
      </Tabs.List>
    </Tabs>
  ),
}

export const OutlineSuffix: Story = {
  render: () => (
    <Tabs defaultValue="gallery" variant="outline">
      <Tabs.List>
        <Tabs.Tab value="gallery" suffix={<Icon icon="photo" />}>
          Gallery
        </Tabs.Tab>
        <Tabs.Tab value="messages" suffix={<Icon icon="chat" />}>
          Messages
        </Tabs.Tab>
        <Tabs.Tab value="settings" suffix={<Icon icon="settings" />} disabled>
          Settings
        </Tabs.Tab>
      </Tabs.List>
    </Tabs>
  ),
}

export const PillsSuffix: Story = {
  render: () => (
    <Tabs defaultValue="gallery" variant="pills">
      <Tabs.List>
        <Tabs.Tab value="gallery" suffix={<Icon icon="photo" />}>
          Gallery
        </Tabs.Tab>
        <Tabs.Tab value="messages" suffix={<Icon icon="chat" />}>
          Messages
        </Tabs.Tab>
        <Tabs.Tab value="settings" suffix={<Icon icon="settings" />} disabled>
          Settings
        </Tabs.Tab>
      </Tabs.List>
    </Tabs>
  ),
}

export const CollapsibleTabsAsync: Story = {
  render: () => {
    const [showSuffix, setShowSuffix] = useState(false)
    const suffix = <Badge color="purple">500</Badge>

    setTimeout(() => setShowSuffix(true), 1000)

    return (
      <div style={{ maxWidth: '500px' }}>
        <Tabs defaultValue="gallery">
          <Tabs.List>
            <Tabs.Tab
              value="gallery"
              prefix={<Icon icon="photo" />}
              suffix={showSuffix ? suffix : undefined}
            >
              Gallery
            </Tabs.Tab>
            <Tabs.Tab
              value="messages"
              prefix={<Icon icon="chat" />}
              suffix={showSuffix ? suffix : undefined}
            >
              Messages
            </Tabs.Tab>
            <Tabs.Tab
              value="settings"
              prefix={<Icon icon="settings" />}
              suffix={showSuffix ? suffix : undefined}
            >
              Settings
            </Tabs.Tab>
            <Tabs.Tab
              value="home"
              prefix={<Icon icon="home" />}
              suffix={showSuffix ? suffix : undefined}
            >
              Home
            </Tabs.Tab>
            <Tabs.Tab
              value="music"
              prefix={<Icon icon="headphones" />}
              suffix={showSuffix ? suffix : undefined}
            >
              Music
            </Tabs.Tab>
            <Tabs.Tab
              value="movies"
              prefix={<Icon icon="movie" />}
              suffix={showSuffix ? suffix : undefined}
            >
              Movies
            </Tabs.Tab>
            <Tabs.Tab
              value="contacts"
              prefix={<Icon icon="contacts" />}
              suffix={showSuffix ? suffix : undefined}
            >
              Contacts
            </Tabs.Tab>
          </Tabs.List>
        </Tabs>
      </div>
    )
  },
}

export const ChangingTabs: Story = {
  render: () => {
    const [showTab, setShowTab] = useState(false)

    setTimeout(() => setShowTab(true), 1000)

    return (
      <div style={{ maxWidth: '500px' }}>
        <Tabs defaultValue="gallery">
          <Tabs.List>
            <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
              Gallery
            </Tabs.Tab>
            {showTab && (
              <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
                Messages
              </Tabs.Tab>
            )}
            <Tabs.Tab value="settings" prefix={<Icon icon="settings" />}>
              Settings
            </Tabs.Tab>
            <Tabs.Tab value="home" prefix={<Icon icon="home" />}>
              Home
            </Tabs.Tab>
            <Tabs.Tab value="music" prefix={<Icon icon="headphones" />}>
              Music
            </Tabs.Tab>
            <Tabs.Tab value="movies" prefix={<Icon icon="movie" />}>
              Movies
            </Tabs.Tab>
            <Tabs.Tab value="contacts" prefix={<Icon icon="contacts" />}>
              Contacts
            </Tabs.Tab>
          </Tabs.List>
        </Tabs>
      </div>
    )
  },
}
