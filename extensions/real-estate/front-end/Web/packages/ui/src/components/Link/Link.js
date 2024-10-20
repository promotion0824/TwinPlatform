import styled from 'styled-components'
import { forwardRef } from 'react'
import { Link as ReactRouterLink } from 'react-router-dom'
import cx from 'classnames'
import useLink from './useLink'
import useLinkEvents from './useLinkEvents'
import styles from './Link.css'

export default forwardRef(function Link(props, forwardedRef) {
  const link = useLink(props, forwardedRef)
  const linkEvents = useLinkEvents(link)

  const cxClassName = cx(
    styles.link,
    {
      [styles.isClickable]: link.isClickable,
    },
    link.className
  )

  return link.isReactRouterLink ? (
    <StyledRouterLink
      innerRef={(innerRef) => {
        link.linkRef.current = innerRef
      }}
      to={link.to}
      {...link.rest}
      className={cxClassName}
      onClick={linkEvents.handleClick}
      onPointerDown={linkEvents.handlePointerDown}
    >
      {link.children}
    </StyledRouterLink>
  ) : (
    <StyledLink
      ref={link.linkRef}
      href={link.href}
      target={link.target}
      rel={link.rel}
      {...link.rest}
      className={cxClassName}
      onClick={linkEvents.handleClick}
      onPointerDown={linkEvents.handlePointerDown}
    >
      {link.children}
    </StyledLink>
  )
})

const StyledRouterLink = styled(ReactRouterLink)(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
  },
}))

const StyledLink = styled.a(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
  },
}))
