import {
  Tabs as MantineTabs,
  TabsListProps as MantineTabsListProps,
} from '@mantine/core'
import { Children, forwardRef, isValidElement, useRef, useState } from 'react'
import { Menu } from '../../overlays/Menu'
import { Tooltip } from '../../overlays/Tooltip'
import { Tab } from './Tab'
import { useTabsContext } from './TabsContext'

export const COLLAPSIBLE_TABS_BUTTON = 'collapsible-tabs-button' as const

export interface ListProps extends MantineTabsListProps {
  children: MantineTabsListProps['children']
}

/**
 * `Tabs.List` is a set of tab elements contained in a tablist element.
 *
 * @see TODO: add link to storybook
 */
export const List = forwardRef<HTMLDivElement, ListProps>(
  ({ children, ...restProps }, ref) => {
    const isMenuButtonHovered = useRef(false)
    const [menuOpened, setMenuOpened] = useState(false)
    const [tooltipOpened, setTooltipOpened] = useState(false)
    const tabsContext = useTabsContext()

    const childrenArray = Children.toArray(children)
    const visibleCount = tabsContext.visibleCount ?? childrenArray.length
    const visibleChildren = childrenArray.slice(0, visibleCount)
    const hiddenChildren = childrenArray.slice(visibleCount)

    const hiddenSelectedTabIndex = hiddenChildren.findIndex(
      (child) =>
        isValidElement(child) && child.props.value === tabsContext.selectedTab
    )

    if (hiddenSelectedTabIndex > -1) {
      const lastVisible = visibleChildren.pop()
      if (lastVisible) {
        const selectedTab = hiddenChildren.splice(
          hiddenSelectedTabIndex,
          1,
          lastVisible
        )
        visibleChildren.push(selectedTab[0])
      }
    }

    const hiddenChildrenTooltip = hiddenChildren
      .map((child) =>
        isValidElement(child) ? child.props.children : undefined
      )
      .filter((child) => child)
      .join(', ')

    return (
      <MantineTabs.List {...restProps} ref={ref}>
        {visibleChildren}

        {hiddenChildren.length > 0 && (
          <Menu onChange={setMenuOpened} opened={menuOpened}>
            <Menu.Target>
              <Tab
                onClick={(event) => {
                  event.preventDefault()
                  event.stopPropagation()
                }}
                value={COLLAPSIBLE_TABS_BUTTON}
              >
                <Tooltip
                  label={hiddenChildrenTooltip}
                  opened={!menuOpened && tooltipOpened}
                  position="top"
                  withArrow
                  withinPortal
                >
                  <div
                    className={COLLAPSIBLE_TABS_BUTTON}
                    onMouseEnter={() => {
                      isMenuButtonHovered.current = true
                      setTimeout(() => {
                        if (isMenuButtonHovered.current) {
                          setTooltipOpened(true)
                        }
                      }, 500)
                    }}
                    onMouseLeave={() => {
                      isMenuButtonHovered.current = false
                      setTooltipOpened(false)
                    }}
                  >
                    +{hiddenChildren.length}
                  </div>
                </Tooltip>
              </Tab>
            </Menu.Target>
            <Menu.Dropdown>
              {Children.map(hiddenChildren, (child) =>
                isValidElement(child) ? (
                  <Menu.Item
                    onClick={() =>
                      tabsContext.setSelectedTab(child.props.value)
                    }
                  >
                    {child.props.children}
                  </Menu.Item>
                ) : null
              )}
            </Menu.Dropdown>
          </Menu>
        )}
      </MantineTabs.List>
    )
  }
)
