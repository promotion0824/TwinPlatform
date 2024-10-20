import styled from 'styled-components'
import { MarketplaceApp } from './types'

const Logo = styled.div(({ theme }) => ({
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: theme.radius.r2,
  padding: theme.spacing.s8,
  height: '70px',
  width: '70px',

  img: {
    height: '100%',
    width: '100%',
  },
}))

export default function MarketplaceAppLogo({ app }: { app: MarketplaceApp }) {
  return (
    <Logo>
      <img alt={app.name} src={app.iconUrl} />
    </Logo>
  )
}
