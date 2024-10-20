import { ReactNode } from 'react'
import { styled } from 'styled-components'
import { Stack, StackProps } from '../../layout/Stack'

const Container = styled(Stack)(({ theme }) => ({
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
}))

export interface SidebarGroupProps extends StackProps {
  /** `SidebarLink` components. */
  children: ReactNode
  /**
   * Force a group to fill to its maximum height.
   * @default false
   */
  fill?: boolean
}

export const SidebarGroup = ({ children, fill = false }: SidebarGroupProps) => {
  return (
    <Container
      className="sidebar-group"
      gap={0}
      {...(fill ? { h: '100%' } : {})}
      pb="s12"
      pt="s12"
    >
      {children}
    </Container>
  )
}
