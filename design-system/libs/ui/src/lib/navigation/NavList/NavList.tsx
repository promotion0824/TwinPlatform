import { HTMLAttributes } from 'react'
import styled from 'styled-components'

import { NavListGroup } from './NavListGroup'
import { NavListItem } from './NavListItem'
import { ForwardRefWithStaticComponents } from '../../utils'

export type NavListProps = ForwardRefWithStaticComponents<
  HTMLAttributes<HTMLDivElement>,
  {
    Group: typeof NavListGroup
    Item: typeof NavListItem
  }
>

const Container = styled.div(({ theme }) => ({
  padding: `${theme.spacing.s12} 0`,
}))

/**
 * A `NavList` displays a list of links, optionally grouped together.
 */
export const NavList: NavListProps = ({ children, ...rest }) => {
  return <Container {...rest}>{children}</Container>
}

NavList.displayName = 'NavList'
NavList.Group = NavListGroup
NavList.Item = NavListItem
