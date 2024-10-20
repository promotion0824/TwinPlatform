import {
  capitalizeFirstChar,
  FullSizeContainer,
  FullSizeLoader,
  siteAdminUserRole,
} from '@willow/common'
import {
  DocumentTitle,
  Message,
  Permission,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import {
  Badge,
  EmptyState,
  PageTitle,
  PageTitleItem,
  Panel,
  PanelContent,
  PanelGroup,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import styled from 'styled-components'
import useGetSiteApp from '../../../hooks/Marketplace/useGetSiteApp'
import { useSite } from '../../../providers'
import routes from '../../../routes'
import AppInstallButton from './AppInstallButton'
import AppSummaryPanel from './AppSummaryPanel'

const ConfigurationIFrame = styled.iframe({
  border: 'none',
  height: '100%',
  width: '100%',
})

const Header = styled.div({
  display: 'flex',
  justifyContent: 'space-between',
})

const MainContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
  padding: theme.spacing.s16,
}))

const StyledPanelContent = styled(PanelContent)({
  height: '100%',
})

export default function App() {
  const params = useParams<{ connectorId: string }>()
  const site = useSite()
  const { location, locationName } = useScopeSelector()
  const { t } = useTranslation()
  const user = useUser()

  const appQuery = useGetSiteApp(site.id, params.connectorId)

  if (appQuery.isError) {
    return (
      <FullSizeContainer>
        <Message icon="error">{t('plainText.errorOccurred')}</Message>
      </FullSizeContainer>
    )
  }

  if (!appQuery.isSuccess) return <FullSizeLoader intent="secondary" />

  const app = appQuery.data
  const isAdmin = site.userRole === siteAdminUserRole

  return (
    <MainContainer>
      <DocumentTitle
        scopes={[app.name, t('headers.connectors'), locationName]}
      />

      <Header>
        <PageTitle>
          <PageTitleItem>
            <Link
              to={
                location?.twin?.id
                  ? routes.connectors_scope__scopeId(location.twin.id)
                  : routes.connectors
              }
            >
              {t('headers.connectors')}
            </Link>
          </PageTitleItem>
          <PageTitleItem
            suffix={
              app.isInstalled ? (
                <Badge color="green" size="lg" variant="dot">
                  {t('plainText.installed')}
                </Badge>
              ) : undefined
            }
          >
            <Link to={window.location.pathname}>{app.name}</Link>
          </PageTitleItem>
        </PageTitle>

        {(isAdmin || user.hasPermissions(Permission.CanInstallConnectors)) && (
          <AppInstallButton app={app} />
        )}
      </Header>

      <PanelGroup>
        <Panel collapsible defaultSize={320} title={t('labels.summary')}>
          <AppSummaryPanel app={app} />
        </Panel>

        <Panel title={t('headers.configuration')}>
          <StyledPanelContent>
            {!app.isInstalled ? (
              <EmptyState
                h="100%"
                icon="cable"
                title={capitalizeFirstChar(
                  t('plainText.connectorNotInstalled')
                )}
              />
            ) : !(
                isAdmin || user.hasPermissions(Permission.CanInstallConnectors)
              ) ? (
              <EmptyState
                h="100%"
                icon="cable"
                title={t('plainText.connectorsNonAdmin')}
              />
            ) : !app.manifest.configurationUrl ? (
              <EmptyState
                h="100%"
                icon="cable"
                title={t('plainText.connectorsNoConfiguration')}
              />
            ) : (
              <ConfigurationIFrame
                src={app.manifest.configurationUrl}
                title={app.name}
              />
            )}
          </StyledPanelContent>
        </Panel>
      </PanelGroup>
    </MainContainer>
  )
}
