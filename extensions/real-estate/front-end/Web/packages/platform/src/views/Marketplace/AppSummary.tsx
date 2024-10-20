import { Badge } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import styled from 'styled-components'
import { useSite } from '../../providers'
import MarketplaceAppLogo from './MarketplaceAppLogo'
import MarketplaceCategoryBadges from './MarketplaceCategoryBadges'
import { MarketplaceApp } from './types'
import routes from '../../routes'

const AppTitle = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
}))

const AppDescription = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  display: '-webkit-box',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  WebkitBoxOrient: 'vertical',
  WebkitLineClamp: 4,
}))

const Card = styled(Link)(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.accent.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: theme.radius.r2,
  color: theme.color.neutral.fg.default,
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s12,
  height: '300px',
  minWidth: '270px',
  padding: theme.spacing.s16,
  textDecoration: 'none',

  '&:focus, &:hover': {
    backgroundColor: theme.color.neutral.bg.accent.hovered,
    outline: 'none',
  },
}))

const CardBody = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s4,
  flex: '1 1 0',
  minHeight: 0,
}))

const CardHeader = styled.div({
  display: 'flex',
  justifyContent: 'space-between',
})

const Separator = styled.div(({ theme }) => ({
  backgroundColor: theme.color.neutral.border.default,
  flexShrink: 0,
  height: '1px',
  width: '100%',
}))

export default function AppSummary({
  app,
  scopeId,
}: {
  app: MarketplaceApp
  scopeId?: string
}) {
  const site = useSite()
  const { t } = useTranslation()

  const isPoweredByMapped =
    app.developer?.name === 'mapped' &&
    app.supportedApplicationKinds?.includes('marketing')

  return (
    <Card
      to={
        scopeId
          ? routes.connectors_scope__scopeId_connector__connectorId(
              scopeId,
              app.id
            )
          : routes.connectors_sites__siteId_connector__connectorId(
              site.id,
              app.id
            )
      }
    >
      <CardHeader>
        <MarketplaceAppLogo app={app} />

        {app.isInstalled && (
          <Badge color="green" size="md" variant="dot">
            {t('plainText.installed')}
          </Badge>
        )}
      </CardHeader>

      <Separator />

      <CardBody>
        <AppTitle>{app.name}</AppTitle>
        <AppDescription>{app.description}</AppDescription>
      </CardBody>

      {(app.categoryNames.length > 0 || isPoweredByMapped) && (
        <>
          <Separator />
          <MarketplaceCategoryBadges app={app} />
        </>
      )}
    </Card>
  )
}
