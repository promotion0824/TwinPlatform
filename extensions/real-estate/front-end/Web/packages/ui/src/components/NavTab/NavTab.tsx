import { TabProps, Tabs } from '@willowinc/ui'
import { Link } from 'react-router-dom'
import { styled } from 'twin.macro'

export interface NavTabProps extends TabProps {
  /** Additional routes that should cause this tab to be selected. */
  include?: string[]
  /** The route for this tab. */
  to: string
}

/** A Tab component that is used for navigation. */
export default function NavTab({ to, disabled, ...rest }: NavTabProps) {
  const tabComponent = <Tabs.Tab {...rest} disabled={disabled} />
  return disabled ? (
    tabComponent
  ) : (
    <StyledLink to={to}>{tabComponent}</StyledLink>
  )
}

const StyledLink = styled(Link)({
  textDecoration: 'none',
})
