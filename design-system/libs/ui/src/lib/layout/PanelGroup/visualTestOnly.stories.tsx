import { Div } from '@storybook/components'
import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'
import { css } from 'styled-components'
import { Panel, PanelGroup, PanelContent } from '.'
import { StoryFlexContainer } from '../../../storybookUtils'
import { Icon } from '../../misc/Icon'
import { Tabs } from '../../navigation/Tabs'

type Story = StoryObj<typeof PanelGroup>

const defaultStory: Meta<typeof Div> = {
  component: Div, // doesn't matter what element is
  title: 'PanelGroup',
  decorators: [
    (Story) => (
      <div
        css={css(({ theme }) => ({
          height: 400,
          background: theme.color.neutral.bg.base.default,
          color: theme.color.neutral.fg.default,
        }))}
      >
        <Story />
      </div>
    ),
  ],
}

export default defaultStory

export const HorizontalPanels: Story = {
  render: () => (
    <PanelGroup>
      <Panel data-testid="panel-1" collapsible title="Panel 1" />
      <PanelGroup resizable>
        <Panel
          data-testid="panel-2"
          collapsible
          defaultSize={20}
          title="Panel 2"
        />
        <Panel
          data-testid="panel-3"
          collapsible
          defaultSize={30}
          title="Panel 3"
        />
        <Panel data-testid="panel-4" title="Panel 4" />
      </PanelGroup>
    </PanelGroup>
  ),
}

export const VerticalPanels: Story = {
  render: () => (
    <PanelGroup direction="vertical">
      <Panel data-testid="panel-1" collapsible title="Panel 1" />
      <PanelGroup resizable direction="vertical">
        <Panel
          data-testid="panel-2"
          collapsible
          defaultSize={20}
          title="Panel 2"
        />
        <Panel
          data-testid="panel-3"
          collapsible
          defaultSize={30}
          title="Panel 3"
        />
        <Panel data-testid="panel-4" title="Panel 4" />
      </PanelGroup>
    </PanelGroup>
  ),
}

export const CollapsibleComplexPanels: Story = {
  render: () => (
    <PanelGroup direction="vertical">
      <Panel data-testid="panel-1" collapsible title="Panel 1" />
      <PanelGroup resizable>
        <Panel data-testid="panel-2" collapsible title="Panel 2" />
        <PanelGroup direction="vertical" resizable>
          <Panel data-testid="panel-3" collapsible title="Panel 3" />
          <Panel data-testid="panel-4" collapsible title="Panel 4" />
          <Panel data-testid="panel-5" collapsible title="Panel 5" />
        </PanelGroup>
      </PanelGroup>
    </PanelGroup>
  ),
}

export const PersistedNestedPanels: Story = {
  render: () => (
    <PanelGroup
      direction="vertical"
      resizable
      autoSaveId="HiddenPersistedNestedPanels-1"
    >
      <Panel title="Panel 1" data-testid="panel-1" collapsible></Panel>
      <PanelGroup resizable autoSaveId="HiddenPersistedNestedPanels-2">
        <Panel collapsible data-testid="panel-2" title="Panel 2"></Panel>
        <PanelGroup
          direction="vertical"
          resizable
          autoSaveId="HiddenPersistedNestedPanels-3"
        >
          <Panel collapsible title="Panel 3" data-testid="panel-3"></Panel>
          <Panel collapsible title="Panel 4" data-testid="panel-4"></Panel>
          <Panel collapsible title="Panel 5" data-testid="panel-5"></Panel>
        </PanelGroup>
      </PanelGroup>
    </PanelGroup>
  ),
  decorators: [
    (Story) => (
      <StoryFlexContainer css={{ width: '100%', height: '100%' }}>
        <Story />
      </StoryFlexContainer>
    ),
  ],
}

export const CollapsibleTabsInPanels: Story = {
  render: () => (
    <PanelGroup>
      <Panel
        collapsible
        defaultSize={510}
        tabs={
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
            <Tabs.Panel style={{ margin: '1rem' }} value="gallery">
              Gallery tab content.
            </Tabs.Panel>
            <Tabs.Panel style={{ margin: '1rem' }} value="messages">
              Messages tab content.
            </Tabs.Panel>
            <Tabs.Panel style={{ margin: '1rem' }} value="settings">
              Settings tab content.
            </Tabs.Panel>
            <Tabs.Panel style={{ margin: '1rem' }} value="home">
              Home tab content.
            </Tabs.Panel>
            <Tabs.Panel style={{ margin: '1rem' }} value="music">
              Music tab content.
            </Tabs.Panel>
            <Tabs.Panel style={{ margin: '1rem' }} value="movies">
              Movies tab content.
            </Tabs.Panel>
            <Tabs.Panel style={{ margin: '1rem' }} value="contacts">
              Contacts tab content.
            </Tabs.Panel>
          </Tabs>
        }
      />
      <Panel title="Panel 2" />
    </PanelGroup>
  ),
}

export const ControlledCollapsibleTabsInPanels: Story = {
  render: () => {
    const [tab, setTab] = useState<string | null>()

    return (
      <PanelGroup resizable>
        <Panel
          collapsible
          tabs={
            <Tabs defaultValue="gallery" onTabChange={setTab} value={tab}>
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
              <Tabs.Panel style={{ margin: '1rem' }} value="gallery">
                Gallery tab content.
              </Tabs.Panel>
              <Tabs.Panel style={{ margin: '1rem' }} value="messages">
                Messages tab content.
              </Tabs.Panel>
              <Tabs.Panel style={{ margin: '1rem' }} value="settings">
                Settings tab content.
              </Tabs.Panel>
              <Tabs.Panel style={{ margin: '1rem' }} value="home">
                Home tab content.
              </Tabs.Panel>
              <Tabs.Panel style={{ margin: '1rem' }} value="music">
                Music tab content.
              </Tabs.Panel>
              <Tabs.Panel style={{ margin: '1rem' }} value="movies">
                Movies tab content.
              </Tabs.Panel>
              <Tabs.Panel style={{ margin: '1rem' }} value="contacts">
                Contacts tab content.
              </Tabs.Panel>
            </Tabs>
          }
        />
        <Panel title="Panel 2" />
      </PanelGroup>
    )
  },
}

export const FixedPanel: Story = {
  render: () => (
    <PanelGroup>
      <Panel m="s12" p={20} h="300px" title="panel title">
        Panel content
      </Panel>
    </PanelGroup>
  ),
}

export const CollapsiblePanel: Story = {
  render: () => (
    <PanelGroup>
      <Panel m="s12" p={20} h="300px" collapsible>
        Panel content
      </Panel>
    </PanelGroup>
  ),
}

export const WithPanelContent: Story = {
  render: () => (
    <PanelGroup>
      <Panel>
        <PanelContent m="s12" p={20} h="300px">
          Panel content
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}
