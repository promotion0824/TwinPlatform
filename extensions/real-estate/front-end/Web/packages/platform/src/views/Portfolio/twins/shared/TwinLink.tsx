import { ReactNode } from 'react'
import { styled } from 'twin.macro'
import { Link } from 'react-router-dom'
import { useLocation } from 'react-router'

import routes from '../../../../routes'

export default function TwinLink({
  twin,
  children,
  className,
  disabled = false,
  onClick,
}: {
  twin: {
    id: string
    siteId: string
  }
  children: ReactNode
  className?: string
  disabled?: boolean
  onClick?: (e: React.MouseEvent) => void
}) {
  const location = useLocation()
  return disabled ? (
    <div className={className}>{children}</div>
  ) : (
    <Link
      className={className}
      to={{
        pathname: routes.portfolio_twins_view__siteId__twinId(
          twin.siteId,
          twin.id
        ),
        state: {
          from: location,
        },
      }}
      onClick={onClick}
    >
      {children}
    </Link>
  )
}

export const TwinLinkListItem = styled(TwinLink)({
  padding: 16,
  paddingLeft: 8,
  minHeight: 82,
  display: 'block',
  textDecoration: 'none',
  '&:hover': {
    backgroundColor: 'var(--theme-color-neutral-bg-accent-default)',
    textDecoration: 'none',
  },
})
