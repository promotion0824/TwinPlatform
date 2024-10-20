import { useParams, Redirect } from 'react-router'
import _ from 'lodash'
import { Fetch } from '@willow/ui'
import SitesContent from './SitesContent'

export default function Sites() {
  const params = useParams()

  return (
    <Fetch url="/api/management/managedPortfolios">
      {(portfolios) => {
        const selectedPortfolio = portfolios
          .map((portfolio) => ({
            ...portfolio,
            sites:
              _(portfolio.sites)
                .filter((site) => site.role === 'Admin')
                .orderBy((site) => site.siteName.toLowerCase())
                .value() ?? [],
          }))
          .find((portfolio) => portfolio.portfolioId === params.portfolioId)

        if (selectedPortfolio == null) {
          return <Redirect to="/admin" />
        }

        return <SitesContent portfolio={selectedPortfolio} />
      }}
    </Fetch>
  )
}
