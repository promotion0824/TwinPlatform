import {
  Card,
  CardButton,
  DocumentTitle,
  Error,
  Flex,
  Icon as LegacyIcon,
  Progress,
} from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import _ from 'lodash'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import routes from '../../../routes'
import SplitHeaderPanel from '../../Layout/Layout/SplitHeaderPanel'
import useManagedPortfolios, {
  ManagedPortfolio,
} from '../../Portfolio/useManagedPortfolios'
import AdminTabs from '../AdminTabs'
import PortfolioModal from './PortfolioModal'

export default function Portfolios({
  featureFlags,
  showPortfolioTab,
  isCustomerAdmin,
}: {
  featureFlags: { hasFeatureToggle: (featureFlag: string) => boolean }
  showPortfolioTab: boolean
  isCustomerAdmin: boolean
}) {
  const { t } = useTranslation()
  const { status, data } = useManagedPortfolios()
  const managedPortfolios = _(data)
    .filter(
      (portfolio) =>
        portfolio.role === 'Admin' ||
        (portfolio.sites ?? []).some((site) => site.role === 'Admin')
    )
    .orderBy((portfolio) => portfolio.portfolioName?.toLowerCase())
    .value()

  const [selectedPortfolio, setSelectedPortfolio] = useState<
    ManagedPortfolio | { portfolioName: string; features: {} }
  >()

  function handleAddPortfolioClick() {
    setSelectedPortfolio({
      portfolioName: '',
      features: {},
    })
  }

  const isCustomerAdminOrPortfolioUser = showPortfolioTab || isCustomerAdmin

  return (
    <>
      <DocumentTitle scopes={[t('headers.portfolio'), t('headers.admin')]} />

      <SplitHeaderPanel
        leftElement={<AdminTabs />}
        rightElement={
          isCustomerAdmin && (
            <Button
              onClick={handleAddPortfolioClick}
              prefix={<Icon icon="add" />}
            >
              {t('plainText.addPortfolio')}
            </Button>
          )
        }
      />
      {status === 'error' ? (
        <Error />
      ) : status === 'loading' ? (
        <Progress />
      ) : (
        <Flex padding="small">
          <Flex horizontal fill="wrap" size="tiny">
            {managedPortfolios.map((portfolio) => (
              <Card
                key={portfolio.portfolioId}
                header={portfolio.portfolioName}
                selected={selectedPortfolio === portfolio}
                onClick={() => setSelectedPortfolio(portfolio)}
              >
                {isCustomerAdminOrPortfolioUser && (
                  <CardButton
                    data-tooltip={t('headers.reports').toUpperCase()}
                    data-tooltip-position="left"
                    to={routes.admin_portfolios__portfolioId_reportsConfig(
                      portfolio.portfolioId
                    )}
                  >
                    <LegacyIcon style={{ stroke: 'none' }} icon="report" />
                  </CardButton>
                )}
                <CardButton
                  data-tooltip={t('plainText.buildings').toUpperCase()}
                  data-tooltip-position="left"
                  icon="site"
                  to={routes.admin_portfolios__portfolioId(
                    portfolio.portfolioId
                  )}
                />
                {isCustomerAdminOrPortfolioUser &&
                  featureFlags?.hasFeatureToggle('connectivityPage') && (
                    <CardButton
                      data-tooltip={t(
                        'plainText.viewConnectivity'
                      ).toUpperCase()}
                      data-tooltip-position="left"
                      icon="power"
                      to={routes.admin_portfolios__portfolioId_connectivity(
                        portfolio.portfolioId
                      )}
                    />
                  )}

                <CardButton
                  icon="right"
                  onClick={() => setSelectedPortfolio(portfolio)}
                />
              </Card>
            ))}
          </Flex>
        </Flex>
      )}
      {selectedPortfolio != null && (
        <PortfolioModal
          portfolio={selectedPortfolio}
          onClose={() => setSelectedPortfolio(undefined)}
        />
      )}
    </>
  )
}
