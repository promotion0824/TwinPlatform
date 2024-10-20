import { useLayoutEffect, useRef } from 'react'
import { useLocation } from 'react-router'
import cx from 'classnames'
import { useUniqueId } from '@willow/ui'
import Button from 'components/Button/Button'
import Portal from 'components/Portal/Portal'
import Text from 'components/Text/Text'
import { styled } from 'twin.macro'
import { useTabs } from './TabsContext'
import styles from './Tab.css'
import Count from '../Count/Count'

const ContentWrapper = styled.div({
  display: ({ $hidden }) => ($hidden ? 'none' : undefined),
  flexGrow: 1,
})

export default function Tab({
  header,
  count = undefined,
  selected = undefined,
  /**
   * By default, when a tab is inactive the content is removed from the DOM.
   * When enabled, the tab's content will instead be hidden with CSS while inactive.
   */
  persist = undefined,
  to = undefined,
  autoFocus = undefined,
  className = undefined,
  children,
  onClick = undefined,
  type = undefined,
  ...rest
}) {
  const location = useLocation()
  const tabId = useUniqueId()
  const tabs = useTabs()
  const tabRef = useRef()

  let isSelected = selected
  if (isSelected == null) {
    if (to != null) {
      let pathLower = location.pathname.toLowerCase()
      if (tabs.includeQueryStringForSelectedTab) {
        pathLower += location.search.toLowerCase()
      }
      const linkLower = to.toLowerCase()

      isSelected = pathLower === linkLower
    } else {
      isSelected = tabs.selectedTabId === tabId
    }
  }

  useLayoutEffect(() => {
    tabs.registerTabId(tabId, tabRef)

    if (autoFocus || isSelected) {
      tabs.selectTab(tabId)
    }

    return () => {
      tabs.unregisterTabId(tabId)
    }
  }, [])

  function handleClick(e) {
    tabs.selectTab(tabId)

    onClick?.(e)
  }

  const cxClassName = cx(
    styles.tab,
    {
      [styles.selected]: isSelected,
      [styles.modal]: type === 'modal',
    },
    className
  )

  return (
    <>
      <Portal target={tabs.tabsRef}>
        <Button
          {...rest}
          role="tab"
          ref={tabRef}
          to={to}
          ripple={!isSelected}
          selected={isSelected}
          className={cxClassName}
          onClick={handleClick}
        >
          <Text type="message" textTransform={false}>
            {header}
          </Text>
          {count ? <Count isSelected={isSelected}>{count}</Count> : null}
        </Button>
      </Portal>
      {(isSelected || persist) && (
        <Portal target={tabs.contentRef}>
          {persist ? (
            <ContentWrapper $hidden={!isSelected}>{children}</ContentWrapper>
          ) : (
            children
          )}
        </Portal>
      )}
    </>
  )
}
