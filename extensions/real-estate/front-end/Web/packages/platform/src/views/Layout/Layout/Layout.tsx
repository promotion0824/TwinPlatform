import { getContainmentHelper, useFeatureFlag, useSize } from '@willow/ui'
import { ReactNode, useEffect, useRef, useState } from 'react'
import { css } from 'styled-components'
import Header from './Header/Header'
import { LayoutContext, useLayout } from './LayoutContext'
import LayoutSidebar from './LayoutSidebar'

const MIN_SIDEBAR_WIDTH = 47 as const
const WILLOW_SIDEBAR_COLLAPSED_KEY = 'willow-sidebar-collapsed'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

function ContentWrapper({
  children,
  sidebarEnabled,
}: {
  children: ReactNode
  sidebarEnabled: boolean
}) {
  const { headerPanelRef } = useLayout()
  const { height: headerHeight } = useSize(headerPanelRef)

  return (
    <div
      css={css(({ theme }) => ({
        display: 'flex',
        flexDirection: 'column',
        height: '100%',
        overflowY: 'auto',
        width: `calc(100% - ${sidebarEnabled ? MIN_SIDEBAR_WIDTH : 0}px)`,

        [getContainerQuery(`width < ${theme.breakpoints.mobile}`)]: {
          width: '100%',
        },
      }))}
    >
      <div
        css={css(({ theme }) => ({
          position: 'sticky',
          top: 0,
          width: '100%',
          zIndex: theme.zIndex.sticky,
        }))}
        ref={headerPanelRef}
      />
      <div
        css={{
          flexGrow: 1,
          height: `calc(100% - ${headerHeight}px)`,
          width: '100%',

          '> div': {
            height: '100%',
          },
        }}
      >
        {children}
      </div>
    </div>
  )
}

export default function LayoutComponent({ children }: { children: ReactNode }) {
  const featureFlags = useFeatureFlag()

  const [hasRendered, setHasRendered] = useState(false)
  const [menuItems, setMenuItems] = useState([])
  const headerRef = useRef()
  const headerPanelRef = useRef()
  const sidebarEnabled = featureFlags.hasFeatureToggle('globalSidebar')
  const sidebarRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    setHasRendered(true)
  }, [])

  const sidebarCollapsed =
    window.localStorage.getItem(WILLOW_SIDEBAR_COLLAPSED_KEY) === 'true'

  return (
    <LayoutContext.Provider
      value={{
        headerRef,
        headerPanelRef,
        menuItems,
        setMenuItems,
      }}
    >
      <Header />
      <ContainmentWrapper>
        {/* Unfortunately Group/Stack components don't work correctly in this Layout component,
        so regular flex divs need to be used. */}
        <div
          css={{
            display: 'flex',
            flexDirection: 'row',
            height: '100%',
            width: '100vw',
          }}
        >
          {sidebarEnabled && (
            <LayoutSidebar
              css={css(({ theme }) => ({
                [getContainerQuery(`max-width: ${theme.breakpoints.mobile}`)]: {
                  display: 'none',
                },
              }))}
              collapsedByDefault={sidebarCollapsed}
              onChange={(isCollapsed) =>
                window.localStorage.setItem(
                  WILLOW_SIDEBAR_COLLAPSED_KEY,
                  isCollapsed.toString()
                )
              }
              ref={sidebarRef}
            />
          )}

          <ContentWrapper sidebarEnabled={sidebarEnabled}>
            {hasRendered && (children ?? null)}
          </ContentWrapper>
        </div>
      </ContainmentWrapper>
    </LayoutContext.Provider>
  )
}
