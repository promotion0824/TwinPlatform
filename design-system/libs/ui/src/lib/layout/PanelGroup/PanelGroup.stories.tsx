import type { Meta, StoryObj } from '@storybook/react'
import styled, { css } from 'styled-components'

import { Panel, PanelContent, PanelGroup } from '.'
import { Button } from '../../buttons/Button'
import { useCombineTabs, type TabAndPanel } from '../../hooks'
import { Icon } from '../../misc/Icon'
import { Tabs } from '../../navigation/Tabs'

const meta: Meta<typeof PanelGroup> = {
  title: 'PanelGroup',
  component: PanelGroup,
  decorators: [
    (Story) => (
      <div
        css={css(({ theme }) => ({
          height: 400,
          background: theme.color.neutral.bg.panel.default,
          color: theme.color.neutral.fg.default,
        }))}
      >
        <Story />
      </div>
    ),
  ],
}

export default meta

type Story = StoryObj<typeof PanelGroup>

const InnerContentContainer = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  margin: theme.spacing.s16,
}))

const InnerContent = () => (
  <InnerContentContainer>
    <p>
      Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed ac odio
      maximus, egestas nisl in, tincidunt metus. Donec eu risus enim. Ut sit
      amet ipsum lorem. Nam vitae odio nec justo viverra tincidunt. Vestibulum
      pulvinar sagittis neque, id interdum ante. Fusce dolor libero, dictum et
      ornare vitae, dignissim non erat. Maecenas fringilla dui nec odio pretium
      semper. Suspendisse commodo sem nec sodales ornare. Mauris volutpat
      ultrices tortor cursus tincidunt.
    </p>

    <p>
      Proin vel dui accumsan, malesuada magna id, lobortis nisi. Cras mollis nec
      felis in consequat. Morbi porttitor mi non est lacinia porta. Ut orci
      nibh, posuere non massa vel, mattis dictum tortor. Praesent metus erat,
      convallis et tristique vitae, tincidunt vitae augue. Curabitur orci mi,
      malesuada a ante sollicitudin, lobortis sagittis sapien. Vestibulum at
      felis ex. Proin eros mauris, suscipit vel turpis a, elementum accumsan
      nunc. Proin posuere metus gravida erat porta, quis scelerisque neque
      dapibus. Praesent finibus nisi eu magna lobortis efficitur.
    </p>

    <p>
      Nulla facilisi. Morbi libero eros, sagittis ac lobortis sed, rutrum at
      enim. Integer convallis a massa et pellentesque. Sed convallis sapien nec
      nisl hendrerit, non rutrum nunc vulputate. Phasellus maximus libero enim,
      vel accumsan lorem malesuada eu. Pellentesque habitant morbi tristique
      senectus et netus et malesuada fames ac turpis egestas. Nullam in orci sit
      amet leo malesuada dapibus.
    </p>

    <p>
      Morbi lacinia ex ac sapien ultrices consectetur. Aenean fermentum lobortis
      mauris et condimentum. Maecenas turpis felis, tempus ut sodales id,
      pellentesque in ipsum. Aenean feugiat, risus ac aliquet vehicula, urna
      ligula tempor ante, dignissim commodo metus nisi ut dui. Sed pulvinar arcu
      et placerat tempus. Donec sit amet fringilla erat. Aliquam erat volutpat.
      Vivamus iaculis lectus at mauris tincidunt, quis egestas leo posuere.
    </p>
  </InnerContentContainer>
)

export const Playground: Story = {
  render: () => (
    <PanelGroup>
      <Panel defaultSize={150} title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <PanelGroup resizable>
        <Panel collapsible defaultSize={20} title="Panel 2">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel collapsible defaultSize={30} title="Panel 3">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel title="Panel 4">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
      </PanelGroup>
    </PanelGroup>
  ),
}

export const HorizontalFixedPanels: Story = {
  render: () => (
    <PanelGroup>
      <Panel defaultSize={150} title="Panel 1 (150px)">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel defaultSize="40%" title="Panel 2 (40%)">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel title="Panel 3">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}

export const HorizontalResizablePanels: Story = {
  render: () => (
    <PanelGroup resizable>
      <Panel title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel title="Panel 2">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel title="Panel 3">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}

export const HorizontalResizableAndCollapsiblePanels: Story = {
  render: () => (
    <PanelGroup>
      <Panel collapsible title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <PanelGroup resizable>
        <Panel collapsible title="Panel 2">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel title="Panel 3">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel collapsible title="Panel 4">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
      </PanelGroup>
    </PanelGroup>
  ),
}

export const VerticalFixedPanels: Story = {
  render: () => (
    <PanelGroup direction="vertical">
      <Panel defaultSize={150} title="Panel 1 (150px)">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel defaultSize="40%" title="Panel 2 (40%)">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel title="Panel 3">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}

export const VerticalResizablePanels: Story = {
  render: () => (
    <PanelGroup resizable direction="vertical">
      <Panel title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel title="Panel 2">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel title="Panel 3">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}

export const VerticalResizableAndCollapsiblePanels: Story = {
  render: () => (
    <PanelGroup direction="vertical">
      <Panel defaultSize="25%" title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <PanelGroup direction="vertical" resizable>
        <Panel collapsible title="Panel 2">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel title="Panel 3">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel collapsible title="Panel 4">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
      </PanelGroup>
    </PanelGroup>
  ),
}

export const ComplexPanels: Story = {
  render: () => (
    <PanelGroup direction="vertical">
      <Panel title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <PanelGroup resizable>
        <Panel collapsible title="Panel 2">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <PanelGroup direction="vertical" resizable>
          <Panel collapsible title="Panel 3">
            <PanelContent>
              <InnerContent />
            </PanelContent>
          </Panel>
          <Panel collapsible title="Panel 4">
            <PanelContent>
              <InnerContent />
            </PanelContent>
          </Panel>
          <Panel collapsible title="Panel 5">
            <PanelContent>
              <InnerContent />
            </PanelContent>
          </Panel>
        </PanelGroup>
      </PanelGroup>
    </PanelGroup>
  ),
}

export const HeaderVariants: Story = {
  render: () => (
    <PanelGroup>
      <Panel defaultSize={150} title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <PanelGroup resizable>
        <Panel>
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel collapsible>
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel collapsible title={<div>Panel 4</div>}>
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
      </PanelGroup>
    </PanelGroup>
  ),
}

export const TabsVariant: Story = {
  render: () => (
    <PanelGroup resizable>
      <Panel
        collapsible
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
          </Tabs>
        }
      />
      <Panel collapsible title="Panel 2">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}

/**
 * If you need to combine tabs that are being imported from external components,
 * use the `useCombineTabs` hook. To use this the tabs must be exported as
 * hooks that export an array containing a `Tabs.Tab` and `Tabs.Panel`. These
 * can then all be passed into `useCombineTabs` which will return an array containing
 * `[tabs, tabsPanels]` ready to be used in the `Panel` component.
 */
export const ExternalTabs: Story = {
  render: () => {
    function useGalleryTab(): TabAndPanel {
      return [
        <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
          Gallery
        </Tabs.Tab>,
        <Tabs.Panel style={{ margin: '1rem' }} value="gallery">
          Gallery tab content.
        </Tabs.Panel>,
      ]
    }

    function useMessagesTab(): TabAndPanel {
      return [
        <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
          Messages
        </Tabs.Tab>,
        <Tabs.Panel style={{ margin: '1rem' }} value="messages">
          Messages tab content.
        </Tabs.Panel>,
      ]
    }

    const [tabs, tabsPanels] = useCombineTabs([
      useGalleryTab(),
      useMessagesTab(),
    ])

    return (
      <PanelGroup>
        <Panel
          tabs={
            <Tabs defaultValue="gallery">
              <Tabs.List>{tabs}</Tabs.List>
              {tabsPanels}
            </Tabs>
          }
        />
      </PanelGroup>
    )
  },
}

/**
 * If a panel is being imported from an external component, that component needs to receive
 * an `id` property and pass this to the `Panel` component. The `id` will automatically be
 * provided when the `PanelGroup` is rendered. If this property isn't provided, the panel
 * won't resize correctly.
 */
export const ExternalPanelComponents: Story = {
  render: () => {
    function PanelTwo({ id }: { id?: string }) {
      return (
        <Panel collapsible id={id} title="Panel 2">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
      )
    }

    return (
      <PanelGroup resizable>
        <Panel collapsible title="Panel 1">
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <PanelTwo />
      </PanelGroup>
    )
  },
}

export const HeaderControls: Story = {
  render: () => (
    <PanelGroup resizable>
      <Panel
        collapsible
        headerControls={<Button>Save</Button>}
        tabs={
          <Tabs defaultValue="gallery">
            <Tabs.List>
              <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
                Gallery
              </Tabs.Tab>
              <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
                Messages
              </Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel style={{ margin: '1rem' }} value="gallery">
              Gallery tab content.
            </Tabs.Panel>
            <Tabs.Panel style={{ margin: '1rem' }} value="messages">
              Messages tab content.
            </Tabs.Panel>
          </Tabs>
        }
      />
      <Panel collapsible headerControls={<Button>Save</Button>} title="Panel 2">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel headerControls={<Button>Save</Button>} title="Panel 3">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}

export const Footer: Story = {
  render: () => (
    <PanelGroup resizable>
      <Panel collapsible footer={<Button>Submit</Button>} title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel
        collapsible
        footer={
          <div
            style={{
              display: 'flex',
              justifyContent: 'flex-end',
              width: '100%',
            }}
          >
            <Button>Submit</Button>
          </div>
        }
        tabs={
          <Tabs defaultValue="gallery">
            <Tabs.List>
              <Tabs.Tab value="gallery" prefix={<Icon icon="photo" />}>
                Gallery
              </Tabs.Tab>
              <Tabs.Tab value="messages" prefix={<Icon icon="chat" />}>
                Messages
              </Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel value="gallery">
              <InnerContent />
            </Tabs.Panel>
            <Tabs.Panel style={{ margin: '1rem' }} value="messages">
              Messages tab content.
            </Tabs.Panel>
          </Tabs>
        }
      />
    </PanelGroup>
  ),
}

export const HideHeaderBorder: Story = {
  render: () => (
    <PanelGroup resizable>
      <Panel collapsible hideHeaderBorder>
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel collapsible title="Panel 2">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}

/**
 * The `small` gap size is available to use as a transitionary variant of the `PanelGroup`.
 */
export const SmallGapSize: Story = {
  render: () => (
    <PanelGroup gapSize="small" resizable>
      <Panel collapsible title="Panel 1">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel collapsible title="Panel 2">
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
    </PanelGroup>
  ),
}

/**
 * By default, a PanelGroup with an `autoSaveId` will store layout information
 * in localStorage. If it's nested PanelGroups, each child PanelGroup will need it's
 * own unique `autoSaveId` if you wish to persist their layout status too.
 */
export const PanelWithPersistentSize: Story = {
  render: () => (
    <PanelGroup resizable autoSaveId="panelId_1">
      <Panel title="Panel 1" collapsible minSize={20}>
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <Panel title="Panel 2" collapsible>
        <PanelContent>
          <InnerContent />
        </PanelContent>
      </Panel>
      <PanelGroup resizable autoSaveId="panelId_2" direction="vertical">
        <Panel title="Panel 3" collapsible>
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
        <Panel title="Panel 4" collapsible>
          <PanelContent>
            <InnerContent />
          </PanelContent>
        </Panel>
      </PanelGroup>
    </PanelGroup>
  ),
}
