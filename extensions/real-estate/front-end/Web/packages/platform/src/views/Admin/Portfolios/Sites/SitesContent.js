import { useState } from 'react'
import {
  BackButton,
  Card,
  CardButton,
  Flex,
  NotFound,
  Text,
  DocumentTitle,
} from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { styled } from 'twin.macro'
import { useTranslation } from 'react-i18next'
import LayoutHeaderPanel from 'views/Layout/Layout/LayoutHeaderPanel'
import SiteModal from './SiteModal/SiteModal'

const Image = styled.img({
  height: '100%',
  objectFit: 'cover',
  width: '100%',
  cursor: 'pointer',
})

export default function SitesContent({ portfolio }) {
  const { t } = useTranslation()
  const [selectedSite, setSelectedSite] = useState()

  function handleAddSiteClick() {
    setSelectedSite({
      features: {
        is2DViewerDisabled: false,
        is3DAutoOffsetEnabled: false,
        isInsightsDisabled: false,
        isInspectionEnabled: false,
        isOccupancyEnabled: false,
        isPreventativeMaintenanceEnabled: false,
        isTicketingDisabled: false,
        isReportsEnabled: true,
      },
      timezoneId: Intl.DateTimeFormat().resolvedOptions().timeZone,
    })
  }

  return (
    <>
      <DocumentTitle scopes={[t('plainText.buildings'), t('headers.admin')]} />

      <LayoutHeaderPanel fill="content">
        <BackButton />
        <Flex horizontal align="middle" size="medium" padding="large">
          <Text type="h2">{portfolio.portfolioName}</Text>
        </Flex>
        {portfolio.role === 'Admin' && (
          <Flex align="middle" padding="0 large">
            <Button onClick={handleAddSiteClick} prefix={<Icon icon="add" />}>
              {t('plainText.addSite')}
            </Button>
          </Flex>
        )}
      </LayoutHeaderPanel>
      {portfolio.sites.length > 0 && (
        <Flex padding="small">
          <Flex horizontal fill="wrap" size="tiny">
            {portfolio.sites.map((site) => (
              <Card
                key={site.siteId}
                header={site.siteName}
                selected={selectedSite === site}
                onClick={() => setSelectedSite(site)}
              >
                {site.logoUrl != null && (
                  <Image
                    src={site.logoUrl}
                    alt={`${site.siteName}`}
                    onClick={() => setSelectedSite(site)}
                  />
                )}
                <CardButton
                  icon="users"
                  to={`/admin/users?siteId=${site.siteId}`}
                />
                <CardButton
                  icon="floorsAdmin"
                  to={`/admin/portfolios/${portfolio.portfolioId}/sites/${site.siteId}/floors`}
                />
                <CardButton
                  icon="power"
                  to={`/admin/portfolios/${portfolio.portfolioId}/sites/${site.siteId}/connectors`}
                />
                <CardButton
                  icon="floors"
                  to={`/sites/${site.siteId}?admin=true`}
                />
                <CardButton
                  icon="right"
                  onClick={() => setSelectedSite(site)}
                />
              </Card>
            ))}
          </Flex>
        </Flex>
      )}
      {portfolio.sites.length === 0 && (
        <NotFound>{t('plainText.noSitesFound')}</NotFound>
      )}
      {selectedSite != null && (
        <SiteModal site={selectedSite} onClose={() => setSelectedSite()} />
      )}
    </>
  )
}
