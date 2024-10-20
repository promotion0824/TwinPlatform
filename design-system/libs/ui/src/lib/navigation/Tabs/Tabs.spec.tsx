import { render } from '../../../jest/testUtils'

import { Tabs } from '.'

describe('Tabs', () => {
  it('should render successfully', () => {
    const { baseElement } = render(
      <Tabs>
        <Tabs.List>
          <Tabs.Tab value="gallery">Gallery</Tabs.Tab>
          <Tabs.Tab value="messages">Messages</Tabs.Tab>
        </Tabs.List>
      </Tabs>
    )
    expect(baseElement).toBeTruthy()
  })
})
