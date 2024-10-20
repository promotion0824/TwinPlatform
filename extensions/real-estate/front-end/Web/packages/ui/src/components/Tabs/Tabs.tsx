import React, {
  HTMLProps,
  ReactNode,
  useEffect,
  useRef,
  useState,
  RefObject,
  useMemo,
} from 'react'
import tw, { styled } from 'twin.macro'
import { CSSObject, DefaultTheme } from 'styled-components'
import { useTranslation } from 'react-i18next'
import Panel from '../Panel/Panel'
import { TabsContext } from './TabsContext'
import Button from '../Button/Button'
import useSize from '../../hooks/useSize/useSize'

export { default as Tab } from './Tab'
export { default as TabBackButton } from './TabBackButton'
export { default as TabsHeader } from './TabsHeader'
export { default as TabsContent } from './TabsContent'

/**
 * Scroll width for continuous tab scrolling when << and >> button is triggered.
 */
const tabScrollWidth = 3

type MoreTabs = {
  left: boolean
  right: boolean
}

/**
 * Using a fake border bottom which can be camouflaged away when a tab is selected.
 */
const getFakeBorderBottomStyle = (theme: DefaultTheme): CSSObject => ({
  content: "''",
  backgroundColor: theme.color.neutral.border.default,
  position: 'absolute',
  bottom: 0,
  height: 1,
  left: 0,
  right: 0,
  width: '100%',
})

const TabList = styled.div(({ theme }) => ({
  display: 'flex',
  height: 39,
  position: 'relative',

  '&:before': getFakeBorderBottomStyle(theme),
}))

const TabsWrapper = styled.div<{ $isFlex: boolean }>(({ $isFlex }) => ({
  display: 'flex',
  flexDirection: 'row',
  flex: $isFlex ? 1 : undefined,
  overflow: 'auto',
  position: 'relative',
}))

/**
 * Horizontal list with hidden scrollbar so scrolling works as it is,
 * with ability to scroll being triggered by - Chevron buttons, or
 * trackpad, or touch event
 */
const HiddenScrollbarContainer = styled.div({
  whiteSpace: 'nowrap',
  overflowY: 'hidden',
  overflowX: 'auto',
  scrollbarWidth: 'none' /* hide scrollbar in firefox */,

  '&::-webkit-scrollbar': {
    /* hide scrollbar non-firefox browsers */
    display: 'none',
  },
})

const TabsHeader = styled.div<{ $isFlex: boolean }>(({ $isFlex }) => ({
  display: 'flex',
  flex: $isFlex ? 1 : undefined,

  '& > :first-child': {
    width: $isFlex ? '100%' : undefined,
  },
}))

const Content = styled.div({
  display: 'flex',
  flexDirection: 'column',
  // opt out of overflow-anchor to minimize the chance of content jumping
  // https://developer.mozilla.org/en-US/docs/Web/CSS/overflow-anchor
  '& *': {
    overflowAnchor: 'none',
  },
})

const ChevronButton = styled(Button)(({ icon, theme }) => ({
  border: `0 solid ${theme.color.neutral.border.default}`,
  padding: 'var(--padding) var(--padding-large)',
  background: theme.color.neutral.bg.panel.default,
  position: 'absolute',
  zIndex: 'var(--z-toolbar)',

  '&:before': {
    ...getFakeBorderBottomStyle(theme),
    width: 'calc(100% + 1px)', // Add 1px to account for button's border
    bottom: -1,
    left: icon === 'chevronFwd' ? -1 : 0,
  },
}))

const MoreTabsButton = ({
  className,
  direction,
  tabsRef,
}: {
  className?: string
  direction: keyof MoreTabs
  tabsRef: RefObject<HTMLElement | undefined>
}) => {
  const { t } = useTranslation()
  const [isPointerDown, setPointerDown] = useState<boolean>(false)

  useEffect(() => {
    if (isPointerDown) {
      const intervalId = setInterval(() => {
        tabsRef.current?.scrollBy(
          (direction === 'left' ? -1 : 1) * tabScrollWidth,
          0
        )
      })

      return () => {
        clearInterval(intervalId)
      }
    }
    return undefined
  }, [isPointerDown, tabsRef, direction])

  return (
    <ChevronButton
      className={className}
      icon={direction === 'left' ? 'chevronBack' : 'chevronFwd'}
      iconSize="small"
      height="large"
      onPointerDown={() => setPointerDown(true)}
      onPointerUp={() => setPointerDown(false)}
      title={t('plainText.more')}
    />
  )
}

/**
 * If all tabs are not visible within provided tabs container, then
 * return the direction (left/right) where more tabs can be seen.
 */
const getShowMoreTabs = (tabsEl?: HTMLElement | null): MoreTabs => {
  if (tabsEl != null) {
    const { scrollWidth, scrollLeft, offsetWidth } = tabsEl

    if (offsetWidth < scrollWidth) {
      return {
        left: scrollLeft > 0,
        // As noted in MDN web docs, the offsetWidth & scrollWidth are rounded values,
        // but scrollLeft is not. So we allow 1px of inaccuracy here to prevent right
        // button from showing due to precision error.
        // (Example values - ScrollWidth: 202, ScrollLeft: 1.328, offsetWidth: 200)
        right: scrollWidth - scrollLeft - offsetWidth > 1,
      }
    }
  }

  return {
    left: false,
    right: false,
  }
}

const scrollToTabIfNotInView = (
  tabsEl?: HTMLElement | null,
  targetingTabEl?: HTMLElement
) => {
  if (tabsEl != null && targetingTabEl != null) {
    // Note: Approximate check here so no need to round scrollLeft here.
    const isNotInView =
      targetingTabEl.offsetLeft < tabsEl.scrollLeft ||
      tabsEl.scrollLeft + tabsEl.offsetWidth <
        targetingTabEl.offsetLeft + targetingTabEl.offsetWidth

    if (isNotInView) {
      targetingTabEl.scrollIntoView({
        inline: 'center',
      })
    }
  }
}
/**
 * Single selectable tabs component containing the Tablist and Tabpanel.
 *
 * Tablist comprises of:
 * - list of tabs (Tab component or TabBackButton component)
 * - optional TabsHeader which consist of additional controls (input/buttons)
 *
 * The Tabpanel is used to display the children of the selected Tab component or TabContent component.
 */
export default function Tabs({
  className,
  children,
  includeQueryStringForSelectedTab,
  $borderWidth = '1px',
}: HTMLProps<HTMLElement> & {
  className?: string
  children?: ReactNode
  includeQueryStringForSelectedTab?: boolean
  $borderWidth?: string
}) {
  // Container to render Tab/TabBackButton component
  const tabsRef = useRef<HTMLDivElement>(null)
  // Container to render TabHeader component
  const tabsHeaderRef = useRef<HTMLDivElement>(null)
  // Container to render the content of the tab
  const contentRef = useRef<HTMLDivElement>(null)

  // TabId -> TabRef map, used to keep track of position of a tab when its selected.
  const tabRefMapping = useRef<{
    [tabId: string]: RefObject<HTMLElement> | undefined
  }>({})

  const tabsContainerSize = useSize(tabsRef)

  const [showMoreTabs, setShowMoreTabs] = useState<MoreTabs>(
    getShowMoreTabs(tabsRef.current)
  )

  const isSingleTab = Object.values(tabRefMapping.current).length <= 1

  const [state, setState] = useState<{
    tabIds: string[]
    selectedTabId?: string
  }>({
    tabIds: [],
    selectedTabId: undefined,
  })

  const context = useMemo(
    () => ({
      includeQueryStringForSelectedTab,
      tabsRef,
      contentRef,
      tabsHeaderRef,

      selectedTabId: state.selectedTabId,

      selectTab(selectedTabId: string) {
        setState((prevState) => ({
          ...prevState,
          selectedTabId,
        }))
      },

      registerTabId(tabId: string, tabRef: RefObject<HTMLElement>) {
        setState((prevState) => ({
          ...prevState,
          tabIds: [...prevState.tabIds, tabId],
          selectedTabId:
            prevState.selectedTabId == null ? tabId : prevState.selectedTabId,
        }))
        tabRefMapping.current[tabId] = tabRef
      },

      unregisterTabId(tabId: string) {
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
        tabRefMapping.current[tabId] = undefined
      },
    }),
    [includeQueryStringForSelectedTab, state.selectedTabId]
  )

  useEffect(() => {
    // When the size of tabs container has changed, or selectedTabId changed, ensure
    // that the selectedTab is always in view and update the showMoreTabs state.
    if (tabsContainerSize.width > 0 && state.selectedTabId != null) {
      scrollToTabIfNotInView(
        tabsRef.current,
        tabRefMapping.current[state.selectedTabId]?.current ?? undefined
      )
      const nextShowMoreTabs = getShowMoreTabs(tabsRef.current)
      setShowMoreTabs(nextShowMoreTabs)
    }
  }, [tabsContainerSize.width, state.selectedTabId])

  return (
    <TabsContext.Provider value={context}>
      <Panel fill="content" className={className} $borderWidth={$borderWidth}>
        <TabList role="tablist">
          <TabsWrapper $isFlex={!isSingleTab}>
            {!isSingleTab && (
              <>
                {showMoreTabs?.left && (
                  <MoreTabsButton
                    tw="left-0 border-right-width[1px]"
                    direction="left"
                    tabsRef={tabsRef}
                  />
                )}
                {showMoreTabs?.right && (
                  <MoreTabsButton
                    tw="right-0 border-left-width[1px]"
                    direction="right"
                    tabsRef={tabsRef}
                  />
                )}
              </>
            )}
            <HiddenScrollbarContainer
              ref={tabsRef}
              onScroll={() => {
                if (showMoreTabs?.left || showMoreTabs?.right) {
                  setShowMoreTabs(getShowMoreTabs(tabsRef.current))
                }
              }}
            />
          </TabsWrapper>
          <TabsHeader ref={tabsHeaderRef} $isFlex={isSingleTab}>
            {children}
          </TabsHeader>
        </TabList>
        <Content
          ref={contentRef}
          role="tabpanel"
          aria-labelledby={state.selectedTabId}
        />
      </Panel>
    </TabsContext.Provider>
  )
}
