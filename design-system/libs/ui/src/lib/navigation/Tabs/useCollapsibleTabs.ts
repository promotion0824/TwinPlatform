import { debounce } from 'lodash'
import { useLayoutEffect, useState } from 'react'
import { PANEL_HEADER_CONTROLS } from '../../layout/PanelGroup/PanelHeader'
import { COLLAPSIBLE_TABS_BUTTON } from './List'
import { TabWithWidth } from './TabsContext'

function areTabWidthsSet(
  tabWidths: TabWithWidth[]
): tabWidths is Array<{ value: string; width: number }> {
  return !!tabWidths.length && tabWidths.every((tab) => tab.width !== undefined)
}

/**
 * Calculates how many tabs can be visible in the container targeted by the ref.
 *
 * To calculate this:
 * - Tab widths are stored into the tabs context and updated whenever their layout changes.
 * - The available width is calculated by taking the width of the container and subtracting
 *   the collapsible tabs dropdown menu button and the panel header controls, if they exist.
 * - The tab widths are summed up until the available width is reached in order to find
 *   the maximum amount of tabs that can be displayed.
 */
export default function useCollapsibleList({
  gap,
  minVisibleItems = 1,
  ref,
  tabWidths,
}: {
  gap: number
  minVisibleItems?: number
  ref: React.RefObject<HTMLDivElement | null>
  tabWidths: TabWithWidth[]
}) {
  const [visibleCount, setVisibleCount] = useState<number>(Infinity)

  const setVisibleCountDebounced = debounce(setVisibleCount)

  useLayoutEffect(() => {
    const container = ref.current
    if (!container) return

    const observer = new ResizeObserver(() => {
      if (!areTabWidthsSet(tabWidths)) return

      const tabSizes = tabWidths.map((tab, index) => {
        return tab.width + (index === tabWidths.length - 1 ? 0 : gap)
      })

      const collapsibleTabsButton = container.querySelector<HTMLElement>(
        `.${COLLAPSIBLE_TABS_BUTTON}`
      )

      const panelHeaderControls = container.querySelector<HTMLElement>(
        `.${PANEL_HEADER_CONTROLS}`
      )

      let currentWidth = 0

      const availableWidth =
        container.offsetWidth -
        (panelHeaderControls ? panelHeaderControls.offsetWidth : 0) -
        (collapsibleTabsButton ? collapsibleTabsButton.offsetWidth : 0)

      for (let i = 0; i < tabSizes.length; i++) {
        const tab = tabSizes[i]
        currentWidth += tab

        if (currentWidth > availableWidth) {
          setVisibleCountDebounced(Math.max(minVisibleItems, i))
          break
        } else if (availableWidth >= currentWidth) {
          setVisibleCountDebounced(Infinity)
        } else if (i > visibleCount) {
          setVisibleCountDebounced(Math.max(minVisibleItems, i))
        }
      }
    })

    observer.observe(container)
    return () => observer.disconnect()
  }, [
    gap,
    minVisibleItems,
    ref,
    setVisibleCountDebounced,
    tabWidths,
    visibleCount,
  ])

  return visibleCount
}
