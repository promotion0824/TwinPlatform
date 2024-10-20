import { useLayoutEffect } from 'react'
import { useLocation } from 'react-router'
import cx from 'classnames'
import Button from 'components/Button/Button'
import Portal from 'components/Portal/Portal'
import Text from 'components/Text/Text'
import { useUniqueIdNew } from 'hooks'
import { useTabs } from './TabsContext'
import styles from './Tab.css'

export default function Tab({
  header,
  selected,
  to,
  href,
  root = '/',
  autoFocus = false,
  className,
  children,
  onClick = () => {},
  ...rest
}) {
  const location = useLocation()
  const tabId = useUniqueIdNew()
  const tabs = useTabs()

  const link = to || href

  let isSelected = selected
  if (isSelected == null) {
    isSelected =
      link != null
        ? (link === root && location.pathname === root) ||
          (link !== root &&
            location.pathname.toLowerCase().startsWith(link?.toLowerCase()))
        : tabs.selectedTabId === tabId
  }

  const cxClassName = cx(
    styles.tab,
    {
      [styles.typePrimary]: tabs.type === 'primary',
      [styles.typeMobile]: tabs.type === 'mobile',
      [styles.colorDark]: isSelected && tabs.color === 'dark',
      [styles.colorLight]: isSelected && tabs.color === 'light',
      [styles.isSelected]: isSelected,
    },
    className
  )

  useLayoutEffect(() => {
    tabs.registerTab(tabId)

    if (autoFocus) {
      tabs.selectTab(tabId)
    }

    return () => {
      tabs.unregisterTab(tabId)
    }
  }, [])

  function handleClick() {
    tabs.selectTab(tabId)
    onClick()
  }

  return (
    <>
      <Button
        {...rest}
        to={to}
        href={href}
        preventDefault={false}
        selected={isSelected}
        className={cxClassName}
        disabled={isSelected}
        onClick={handleClick}
      >
        <Text type="h3" className={styles.buttonText}>
          {header}
        </Text>
      </Button>
      {isSelected && (
        <Portal target={tabs.contentRef.current}>{children}</Portal>
      )}
    </>
  )
}
