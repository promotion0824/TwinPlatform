import React from 'react'
import { render, screen, within } from '@testing-library/react'
import Tabs, { Tab, TabsHeader } from '../Tabs'
import Wrapper from '../../../utils/testUtils/Wrapper'

describe('Tabs', () => {
  test('Basic tabs', async () => {
    render(
      <Tabs>
        <Tab header="Tab 1">A</Tab>
        <Tab header="Tab 2">B</Tab>
        <Tab header="Tab 3">C</Tab>
      </Tabs>,
      { wrapper: Wrapper }
    )

    const tablist = screen.getByRole('tablist')
    const tabs = await within(tablist).findAllByRole('tab')
    const tabpanel = await screen.findByRole('tabpanel')

    expect(tabs.length).toBe(3)
    expect(tabs[0].textContent).toBe('Tab 1')
    expect(tabs[1].textContent).toBe('Tab 2')
    expect(tabs[2].textContent).toBe('Tab 3')

    expect(tabpanel.textContent).toBe('A')
  })

  test('Basic tabs with header', async () => {
    render(
      <Tabs>
        <Tab header="Tab 1">A</Tab>
        <Tab header="Tab 2">B</Tab>
        <Tab header="Tab 3">C</Tab>
        <TabsHeader>My tabsHeader</TabsHeader>
      </Tabs>,
      { wrapper: Wrapper }
    )

    const tablist = screen.getByRole('tablist')
    const tabs = await within(tablist).findAllByRole('tab')
    const tabsHeader = await within(tablist).findByText('My tabsHeader')

    expect(tabs.length).toBe(3)
    expect(tabsHeader).toBeInTheDocument()

    expect((await screen.findByRole('tabpanel')).textContent).toBe('A')
  })
})
