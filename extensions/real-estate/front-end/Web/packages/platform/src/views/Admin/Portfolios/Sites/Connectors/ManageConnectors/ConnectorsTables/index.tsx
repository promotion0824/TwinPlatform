import { useState } from 'react'
import { Tabs, Tab, TabsContent } from '@willow/ui'
import { styled } from 'twin.macro'
import Connectors from './Connectors'

const connectors = 'Connectors'

export default function ConnectorsTable() {
  const tabs = [connectors]
  const [selectedTab, setSelectedTab] = useState(connectors)

  const handleTabChange = (newTab: string) => {
    if (selectedTab !== newTab) {
      setSelectedTab(newTab)
    }
  }

  return (
    <Container>
      <Tabs $borderWidth="1px 0 0 0">
        {tabs.map((tab) => (
          <Tab
            key={tab}
            header={tab}
            selected={selectedTab === tab}
            onClick={() => handleTabChange(tab)}
          />
        ))}
        <TabsContent>
          {selectedTab === connectors ? <Connectors /> : null}
        </TabsContent>
      </Tabs>
    </Container>
  )
}

const Container = styled.div({
  height: '100%',
  width: '100%',
  backgroundColor: '#252525',
  'margin-top': 'var(--padding-small)',
  overflow: 'hidden !important',
})
