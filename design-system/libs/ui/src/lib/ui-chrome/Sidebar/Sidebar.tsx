import { forwardRef, ReactNode, useState } from 'react'
import styled from 'styled-components'
import { Stack, StackProps } from '../../layout/Stack'
import { SidebarContext } from './SidebarContext'
import { SidebarFooter } from './SidebarFooter'

const Container = styled(Stack)<{ $isCollapsed: boolean }>(
  ({ $isCollapsed, theme }) => {
    const width = $isCollapsed ? '48px' : '256px'

    return {
      backgroundColor: theme.color.neutral.bg.panel.default,
      borderRight: `1px solid ${theme.color.neutral.border.default}`,
      height: '100%',
      minWidth: width,
      width,

      ':nth-last-child(1 of .sidebar-group)': {
        borderBottom: 'none',
      },
    }
  }
)

const ScrollContainer = styled(Stack)({
  height: '100%',
  overflowX: 'clip',
  overflowY: 'auto',
})

export interface SidebarProps extends Omit<StackProps, 'onChange'> {
  /** `SidebarGroup` components containing `SidebarLink` components. */
  children: ReactNode
  /**
   * Set the sidebar to be collapsed by default.
   * @default false
   */
  collapsedByDefault?: boolean
  /** Called when the sidebar is expanded/collapsed. */
  onChange?: (isCollapsed: boolean) => void
  /**
   * Display the footer.
   * @default true
   */
  withFooter?: boolean
}

/**
 * `Sidebar` is used to provide top level navigation menus.
 */
export const Sidebar = forwardRef<HTMLDivElement, SidebarProps>(
  (
    {
      children,
      collapsedByDefault = false,
      onChange,
      withFooter = true,
      ...restProps
    },
    ref
  ) => {
    const [isCollapsed, setIsCollapsed] = useState(collapsedByDefault)

    return (
      <SidebarContext.Provider value={{ isCollapsed }}>
        <Container {...restProps} $isCollapsed={isCollapsed} gap={0} ref={ref}>
          <ScrollContainer gap={0}>{children}</ScrollContainer>

          {withFooter && (
            <SidebarFooter
              isCollapsed={isCollapsed}
              onClick={() => {
                onChange?.(!isCollapsed)
                setIsCollapsed(!isCollapsed)
              }}
            />
          )}
        </Container>
      </SidebarContext.Provider>
    )
  }
)
