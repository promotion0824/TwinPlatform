import { NotFound, useScopeSelector } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import AppSummary from './AppSummary'
import { MarketplaceApp } from './types'

const AppsGrid = styled.div(({ theme }) => ({
  display: 'grid',
  gap: theme.spacing.s12,
  gridTemplateColumns: 'repeat(auto-fill, minmax(270px, 1fr))',
  padding: theme.spacing.s16,
  width: '100%',
}))

export default function Apps({ apps }: { apps: MarketplaceApp[] }) {
  const { t } = useTranslation()
  const { location } = useScopeSelector()

  return apps.length > 0 ? (
    <AppsGrid>
      {apps.map((app) => (
        <AppSummary key={app.id} app={app} scopeId={location?.twin?.id} />
      ))}
    </AppsGrid>
  ) : (
    <NotFound>{t('plainText.noAppsFound')}</NotFound>
  )
}
