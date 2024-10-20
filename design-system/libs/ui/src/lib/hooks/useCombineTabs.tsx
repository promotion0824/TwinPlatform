import { cloneElement, type ReactElement } from 'react'

export type TabAndPanel = [ReactElement, ReactElement]

/**
 * If you need to combine tabs that are being imported from external components,
 * use this hook. To use this the tabs must be exported as hooks that export an
 * array containing a `Tabs.Tab` and `Tabs.Panel`. These can then all be passed into
 * this hook which will return an array containing `[tabs, tabsPanels]`.
 *
 * @returns [tabs, tabsPanels]
 */
function useCombineTabs(tabGroups: TabAndPanel[]) {
  return tabGroups.reduce<[ReactElement[], ReactElement[]]>(
    ([accTabs, accPanels], [tab, panel]) => [
      [
        ...accTabs,
        tab.key ? tab : cloneElement(tab, { key: panel.props.value }),
      ],
      [
        ...accPanels,
        panel.key ? panel : cloneElement(panel, { key: panel.props.value }),
      ],
    ],
    [[], []]
  )
}

export default useCombineTabs
