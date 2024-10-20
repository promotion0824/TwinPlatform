import { useRef, useState } from 'react'
import cx from 'classnames'
import { TabsContext } from './TabsContext'
import TabsContent from './TabsContent'
import styles from './Tabs.css'

export { default as Tab } from './Tab'
export { default as TabsHeader } from './TabsHeader'
export { default as TabsContent } from './TabsContent'

export default function Tabs({
  color = 'dark',
  type = 'normal',
  className,
  headerClassName,
  contentClassName,
  children,
}) {
  const headerRef = useRef()
  const contentRef = useRef()

  const [state, setState] = useState({
    tabIds: [],
    selectedTabId: undefined,
  })

  const cxClassName = cx(
    styles.tabs,
    {
      [styles.typeHeader]: type === 'header',
      [styles.typePrimary]: type === 'primary',
      [styles.mobileHeader]: type === 'mobile',
    },
    className
  )
  const cxHeaderClassName = cx(styles.header, headerClassName)

  const context = {
    headerRef,
    contentRef,
    color,
    type,
    selectedTabId: state.selectedTabId,

    registerTab(tabId) {
      setState((prevState) => ({
        ...prevState,
        tabIds: [...prevState.tabIds, tabId],
        selectedTabId:
          prevState.selectedTabId == null ? tabId : prevState.selectedTabId,
      }))
    },

    unregisterTab(tabId) {
      setState((prevState) => {
        const tabIds = prevState.tabIds.filter(
          (prevTabId) => prevTabId !== tabId
        )

        return {
          ...prevState,
          tabIds,
          selectedTabId:
            prevState.selectedTabId === tabId
              ? tabIds[0]
              : prevState.selectedTabId,
        }
      })
    },

    selectTab(tabId) {
      setState((prevState) => ({
        ...prevState,
        selectedTabId: tabId,
      }))
    },
  }

  return (
    <TabsContext.Provider value={context}>
      <div className={cxClassName}>
        <div className={cxHeaderClassName}>
          {children}
          <div ref={headerRef} className={styles.tabsHeader} />
        </div>
        <TabsContent className={contentClassName} />
      </div>
    </TabsContext.Provider>
  )
}
