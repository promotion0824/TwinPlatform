import { useEffect } from 'react'
import { useLocation } from 'react-router'
import _ from 'lodash'
import cx from 'classnames'
import { Button, Flex } from '@willow/ui'
import { useLayout } from '../LayoutContext'
import styles from './LayoutTab.css'

export default function LayoutTab({
  header,
  subHeader,
  to,
  disabled,
  exclude,
  include,
  className,
  activeTab,
  ...rest
}) {
  const layout = useLayout()
  const location = useLocation()

  useEffect(() => {
    const menuItem = {
      id: _.uniqueId(),
      header,
      subHeader,
      to,
      disabled,
      exclude,
      include,
    }

    layout.setMenuItems((prevMenuItems) => [...prevMenuItems, menuItem])

    return () =>
      layout.setMenuItems((prevMenuItems) =>
        prevMenuItems.filter((prevMenuItem) => prevMenuItem !== menuItem)
      )
  }, [])

  const isRoot = to === '/portfolio' && location.pathname === '/portfolio'

  const pathIsExcluded = (exclude ?? []).some((url) =>
    location.pathname.startsWith(`${to}${url}`)
  )

  const pathIsIncluded = (include ?? []).some((url) =>
    location.pathname.startsWith(url)
  )

  const selected =
    isRoot ||
    (to !== '/portfolio' &&
      location.pathname.startsWith(to) &&
      !pathIsExcluded) ||
    pathIsIncluded ||
    activeTab

  const cxClassName = cx(
    styles.layoutTab,
    {
      [styles.disabled]: disabled,
      [styles.selected]: selected,
    },
    className
  )

  return (
    <Button
      {...rest}
      to={to}
      selected={selected}
      disabled={disabled}
      ripple={!selected}
      className={cxClassName}
    >
      <Flex>
        <span className={styles.content}>{header}</span>
        <span className={styles.subHeader}>{subHeader}</span>
      </Flex>
    </Button>
  )
}
