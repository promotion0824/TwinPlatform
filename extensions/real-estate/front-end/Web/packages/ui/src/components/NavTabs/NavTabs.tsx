import { useState } from 'react'
import { useLocation } from 'react-router'
import { Tabs, TabsProps } from '@willowinc/ui'
import { NavTabProps } from '../NavTab/NavTab'

interface NavTabsProps extends Omit<TabsProps, 'children'> {
  /** An array of NavTab components. */
  tabs: React.ReactElement<NavTabProps>[]
}

export default function NavTabs({ defaultValue, tabs, ...rest }: NavTabsProps) {
  const { pathname } = useLocation()
  const [tabsDefaultValue] = useState(getDefaultTab)

  function getDefaultTab() {
    for (const tab of tabs) {
      const { include, to, value } = tab.props
      const routeMatches = pathname === to
      const routeIsIncluded = include?.some((route) =>
        pathname.startsWith(route)
      )

      if (routeMatches || routeIsIncluded) {
        return value
      }
    }

    return defaultValue ?? tabs[0]?.props?.value
  }

  return (
    <Tabs defaultValue={tabsDefaultValue} {...rest}>
      <Tabs.List>
        {tabs.map((tab) => ({
          ...tab,
          key: tab.key || tab.props.value,
        }))}
      </Tabs.List>
    </Tabs>
  )
}
