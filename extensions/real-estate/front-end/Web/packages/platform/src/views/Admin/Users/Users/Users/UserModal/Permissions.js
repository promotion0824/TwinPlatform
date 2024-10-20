import { Fragment } from 'react'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useForm, Fieldset } from '@willow/ui'
import PermissionSelect from './PermissionSelect'

export default function Permissions({ portfoliosResponse, readOnly = false }) {
  const form = useForm()
  const { t } = useTranslation()

  const portfolios = _(portfoliosResponse)
    .filter(
      (portfolio) =>
        portfolio.role === 'Admin' ||
        portfolio.sites.some((site) => site.role === 'Admin')
    )
    .orderBy((portfolio) => portfolio.portfolioName.toLowerCase())
    .map((portfolio) => {
      const portfolioValue = form.data.portfolioRoles.find(
        (portfolioRole) => portfolioRole.portfolioId === portfolio.portfolioId
      )?.role

      return {
        ...portfolio,
        isVisible: portfolio.role === 'Admin',
        value: portfolioValue,
        sites: _(portfolio.sites)
          .filter((site) => site.role === 'Admin')
          .orderBy((site) => site.siteName.toLowerCase())
          .map((site) => {
            let siteValue = form.data.siteRoles.find(
              (siteRole) => siteRole.siteId === site.siteId
            )?.role

            if (siteValue == null) {
              if (portfolioValue === 'Viewer') {
                siteValue = 'Viewer'
              }
            }

            return {
              ...site,
              isVisible: portfolioValue !== 'Admin',
              value: siteValue,
            }
          })
          .value(),
      }
    })
    .value()

  function handlePortfolioRoleChange(portfolio, role) {
    form.setData((prevState) => ({
      ...prevState,
      portfolioRoles: _(prevState.portfolioRoles)
        .map((prevPortfolioRole) => ({
          ...prevPortfolioRole,
          role:
            role != null &&
            prevPortfolioRole.portfolioId === portfolio.portfolioId
              ? role
              : prevPortfolioRole.role,
        }))
        .filter(
          (prevPortfolioRole) =>
            role != null ||
            prevPortfolioRole.portfolioId !== portfolio.portfolioId
        )
        .thru((prevPortfolios) =>
          prevPortfolios.some(
            (prevPortfolio) =>
              prevPortfolio.portfolioId === portfolio.portfolioId
          )
            ? prevPortfolios
            : [...prevPortfolios, { portfolioId: portfolio.portfolioId, role }]
        )
        .value(),
    }))
  }

  function handleSiteRoleChange(site, role) {
    form.setData((prevState) => ({
      ...prevState,
      siteRoles: _(prevState.siteRoles)
        .map((prevSiteRole) => ({
          ...prevSiteRole,
          role:
            role != null && prevSiteRole.siteId === site.siteId
              ? role
              : prevSiteRole.role,
        }))
        .filter(
          (prevSiteRole) => role != null || prevSiteRole.siteId !== site.siteId
        )
        .thru((prevSites) =>
          prevSites.some((prevSite) => prevSite.siteId === site.siteId)
            ? prevSites
            : [...prevSites, { siteId: site.siteId, role }]
        )
        .value(),
    }))
  }

  return (
    <Fieldset legend={t('plainText.sitePermissions')} size="medium">
      {portfolios.map((portfolio) => (
        <Fragment key={portfolio.portfolioId}>
          <hr />
          <PermissionSelect
            isVisible={portfolio.isVisible}
            value={portfolio.value}
            onChange={(role) => handlePortfolioRoleChange(portfolio, role)}
            readOnly={readOnly}
          >
            {portfolio.portfolioName}
          </PermissionSelect>
          {portfolio.sites.map((site) => (
            <PermissionSelect
              key={site.siteId}
              type="site"
              isVisible={site.isVisible}
              value={site.value}
              onChange={(role) => handleSiteRoleChange(site, role)}
              readOnly={readOnly}
            >
              {site.siteName}
            </PermissionSelect>
          ))}
        </Fragment>
      ))}
    </Fieldset>
  )
}
