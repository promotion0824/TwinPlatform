import { useState } from 'react'
import { useHistory, useParams } from 'react-router'
import { styled } from 'twin.macro'
import {
  BackButton,
  Fetch,
  Flex,
  Icon as LegacyIcon,
  Text,
  useFeatureFlag,
  useAnalytics,
  useUser,
  DocumentTitle,
} from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { useSites } from 'providers'
import LayoutHeaderPanel from 'views/Layout/Layout/LayoutHeaderPanel'
import { useTranslation } from 'react-i18next'
import { SiteSelect } from 'components/SiteSelect'
import ConnectorsModal from './ConnectorsModal'
import ConnectorsContent from './ConnectorsContent'
import CategoryButton from '../../CategoryButton/CategoryButton'
import styles from './Connectors.css'
import ManageConnectors from './ManageConnectors/ManageConnectors'
import ManageConnectorsProvider from './ManageConnectors/providers/ManageConnectorsProvider'
import routes from '../../../../../routes'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'

export default function Connectors() {
  const user = useUser()
  const analytics = useAnalytics()
  const sites = useSites()
  const { push } = useHistory()
  const featureFlags = useFeatureFlag()
  const params = useParams()
  const { t } = useTranslation()
  const [searchParams] = useMultipleSearchParams([
    { name: 'connectorId', type: 'string' },
  ])

  const isCustomerAdminOrPortfolioUser =
    user.showPortfolioTab || user.isCustomerAdmin

  const [showAddConnectorModal, setShowAddConnectorModal] = useState(false)

  function handleAddConnectorClick() {
    setShowAddConnectorModal(true)
  }

  const handleSiteChange = (site) => {
    analytics.track('Site Select', {
      site,
      customer: user?.customer ?? {},
    })
    push(
      routes.admin_portfolios__portfolioId_sites__siteId_connectors(
        params.portfolioId,
        site.id
      )
    )
  }

  // if user lands on a page where connectorId exists in url search params,
  // click "back" button will redirect user to
  // routes.admin_portfolios__portfolioId_sites__siteId_connectors,
  // where all connectors are listed for a single site;
  // otherwise, redirect user back to routes.admin_portfolios__portfolioId_connectivity
  const url = searchParams.connectorId
    ? routes.admin_portfolios__portfolioId_sites__siteId_connectors(
        params.portfolioId,
        params.siteId
      )
    : routes.admin_portfolios__portfolioId_connectivity(params.portfolioId)

  return (
    <>
      <DocumentTitle scopes={[t('headers.connectivity'), t('headers.admin')]} />

      <LayoutHeaderPanel fill="header">
        <Flex horizontal fill="content">
          <BackButton to={url} />
          <Flex horizontal size="large" padding="0 large">
            <Flex horizontal align="middle" size="medium">
              <LegacyIcon icon="plugWithWire" />
              <Text type="h2">{t('plainText.manageConnectors')}</Text>
            </Flex>
            <Flex className={styles.left}>
              <Flex align="middle" padding="0 large">
                <SiteSelect
                  isAllSiteIncluded={false}
                  sites={sites}
                  value={sites.find((s) => s.id === params.siteId)}
                  onChange={handleSiteChange}
                />
              </Flex>
            </Flex>
          </Flex>
          <Flex horizontal align="middle" padding="0 large">
            <Button
              onClick={handleAddConnectorClick}
              prefix={<Icon icon="add" />}
            >
              {t('plainText.addConnector')}
            </Button>
            {isCustomerAdminOrPortfolioUser &&
              featureFlags?.hasFeatureToggle('connectivityPage') && (
                <StyledCategoryButton
                  to={routes.admin_portfolios__portfolioId_connectivity(
                    params.portfolioId
                  )}
                >
                  {t('plainText.viewConnectivityInPortfolio')}
                </StyledCategoryButton>
              )}
          </Flex>
        </Flex>
      </LayoutHeaderPanel>

      {!featureFlags?.hasFeatureToggle('connectivityPage') ? (
        <Fetch
          name="connectors"
          url={[
            `/api/sites/${params.siteId}/connectorTypes`,
            `/api/sites/${params.siteId}/connectors`,
          ]}
        >
          {([connectorTypes, connectors]) => (
            <>
              <ConnectorsContent
                connectorTypes={connectorTypes}
                connectors={connectors}
              />

              {showAddConnectorModal && (
                <ConnectorsModal
                  connector={{}}
                  connectorTypes={connectorTypes}
                  onClose={() => setShowAddConnectorModal(false)}
                />
              )}
            </>
          )}
        </Fetch>
      ) : (
        <ManageConnectorsProvider>
          <ManageConnectors
            showAddConnectorModal={showAddConnectorModal}
            setShowAddConnectorModal={setShowAddConnectorModal}
          />
        </ManageConnectorsProvider>
      )}
    </>
  )
}

const unsetStyle = 'unset !important'

const StyledCategoryButton = styled(CategoryButton)({
  height: '28px',
  marginLeft: '33px',
  maxWidth: unsetStyle,
  width: unsetStyle,
  padding: unsetStyle,

  '>div>span': { padding: '1px 17px 0px 6px' },
  '>div>svg': { marginRight: '6px' },
})
