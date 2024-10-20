import { HTMLAttributes, forwardRef } from 'react'
import styled from 'styled-components'

import { rem } from '../../utils'

export interface NavListGroupProps extends HTMLAttributes<HTMLDivElement> {
  /** Title to be displayed above the group. */
  title?: string
}

const Container = styled.div(({ theme }) => ({
  '&:not(:last-child)::after': {
    backgroundColor: theme.color.neutral.border.default,
    content: '""',
    display: 'block',
    height: rem(1),
    margin: theme.spacing.s8,
  },
}))

const Heading = styled.div(({ theme }) => ({
  ...theme.font.heading.group,
  margin: `0 ${theme.spacing.s8}`,
  padding: `${theme.spacing.s8} ${theme.spacing.s6}`,
  textTransform: 'uppercase',
}))

/**
 * A `NavList.Group` groups a list of NavList.Items together.
 */
export const NavListGroup = forwardRef<HTMLDivElement, NavListGroupProps>(
  ({ children, title, ...rest }, ref) => {
    return (
      <Container ref={ref} {...rest}>
        <Heading>{title}</Heading>
        {children}
      </Container>
    )
  }
)
