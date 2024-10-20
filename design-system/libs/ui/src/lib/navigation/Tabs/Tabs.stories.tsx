import type { Meta, StoryObj } from '@storybook/react'
import styled, { css } from 'styled-components'

import { Tabs } from '.'
import { Icon } from '../../misc/Icon'

const meta: Meta<typeof Tabs> = {
  title: 'Tabs',
  component: Tabs,
}
export default meta

type Story = StoryObj<typeof Tabs>

const TabPanelContainer = styled.div(({ theme }) => ({
  padding: theme.spacing.s8,
}))

export const Playground: Story = {
  render: () => (
    <Tabs defaultValue="gallery">
      <Tabs.List>
        <Tabs.Tab value="gallery">Gallery</Tabs.Tab>
        <Tabs.Tab value="messages">Messages</Tabs.Tab>
        <Tabs.Tab value="settings" disabled>
          Settings
        </Tabs.Tab>
      </Tabs.List>
    </Tabs>
  ),
}

export const Icons: Story = {
  render: () => (
    <Tabs defaultValue="gallery">
      <Tabs.List>
        <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
          Gallery
        </Tabs.Tab>
        <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
          Messages
        </Tabs.Tab>
        <Tabs.Tab value="settings" prefix={<Icon icon="settings" />} disabled>
          Settings
        </Tabs.Tab>
      </Tabs.List>
    </Tabs>
  ),
}

export const Outline: Story = {
  render: () => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <Tabs defaultValue="gallery" variant="outline">
        <Tabs.List>
          <Tabs.Tab value="gallery">Gallery</Tabs.Tab>
          <Tabs.Tab value="messages">Messages</Tabs.Tab>
          <Tabs.Tab value="settings" disabled>
            Settings
          </Tabs.Tab>
        </Tabs.List>
      </Tabs>
      <Tabs defaultValue="gallery" variant="outline">
        <Tabs.List>
          <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
            Gallery
          </Tabs.Tab>
          <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
            Messages
          </Tabs.Tab>
          <Tabs.Tab value="settings" prefix={<Icon icon="settings" />} disabled>
            Settings
          </Tabs.Tab>
        </Tabs.List>
      </Tabs>
    </div>
  ),
}

export const Pills: Story = {
  render: () => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <Tabs defaultValue="gallery" variant="pills">
        <Tabs.List>
          <Tabs.Tab value="gallery">Gallery</Tabs.Tab>
          <Tabs.Tab value="messages">Messages</Tabs.Tab>
          <Tabs.Tab value="settings" disabled>
            Settings
          </Tabs.Tab>
        </Tabs.List>
      </Tabs>
      <Tabs defaultValue="gallery" variant="pills">
        <Tabs.List>
          <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
            Gallery
          </Tabs.Tab>
          <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
            Messages
          </Tabs.Tab>
          <Tabs.Tab value="settings" prefix={<Icon icon="settings" />} disabled>
            Settings
          </Tabs.Tab>
        </Tabs.List>
      </Tabs>
    </div>
  ),
}

/** Panel comes without style, you could customize the panel styles with border when wanted. */
export const OutlineTabsWithBorderedPanel: Story = {
  render: () => (
    <Tabs defaultValue="gallery" variant="outline">
      <Tabs.List>
        <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
          Gallery
        </Tabs.Tab>
        <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
          Messages
        </Tabs.Tab>
        <Tabs.Tab value="settings" prefix={<Icon icon="settings" />} disabled>
          Settings
        </Tabs.Tab>
      </Tabs.List>

      <div
        css={css(({ theme }) => ({
          border: `1px solid ${theme.color.neutral.border.default}`,
          borderTop: 'none',
        }))}
      >
        <Tabs.Panel value="gallery">
          <TabPanelContainer>Gallery tab content</TabPanelContainer>
        </Tabs.Panel>
        <Tabs.Panel value="messages">
          <TabPanelContainer>Messages tab content</TabPanelContainer>
        </Tabs.Panel>
        <Tabs.Panel value="settings">
          <TabPanelContainer>Settings tab content</TabPanelContainer>
        </Tabs.Panel>
      </div>
    </Tabs>
  ),
}

/** Panel comes without style, you could customize the panel styles with margin when wanted. */
export const PillsTabsWithPanel: Story = {
  render: () => (
    <Tabs defaultValue="gallery" variant="pills">
      <Tabs.List>
        <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
          Gallery
        </Tabs.Tab>
        <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
          Messages
        </Tabs.Tab>
        <Tabs.Tab value="settings" prefix={<Icon icon="settings" />} disabled>
          Settings
        </Tabs.Tab>
      </Tabs.List>

      <div
        css={css(({ theme }) => ({
          marginTop: theme.spacing.s8,
        }))}
      >
        <Tabs.Panel value="gallery">Gallery tab content</Tabs.Panel>
        <Tabs.Panel value="messages">Messages tab content</Tabs.Panel>
        <Tabs.Panel value="settings">Settings tab content</Tabs.Panel>
      </div>
    </Tabs>
  ),
}

export const CollapsibleTabsDefault: Story = {
  render: () => (
    <div style={{ maxWidth: '500px' }}>
      <Tabs defaultValue="gallery">
        <Tabs.List>
          <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
            Gallery
          </Tabs.Tab>
          <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
            Messages
          </Tabs.Tab>
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
  ),
}

export const CollapsibleTabsOutline: Story = {
  render: () => (
    <div style={{ maxWidth: '500px' }}>
      <Tabs defaultValue="gallery" variant="outline">
        <Tabs.List>
          <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
            Gallery
          </Tabs.Tab>
          <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
            Messages
          </Tabs.Tab>
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
  ),
}

export const CollapsibleTabsPills: Story = {
  render: () => (
    <div style={{ maxWidth: '500px' }}>
      <Tabs defaultValue="gallery" variant="pills">
        <Tabs.List>
          <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
            Gallery
          </Tabs.Tab>
          <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
            Messages
          </Tabs.Tab>
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
  ),
}
