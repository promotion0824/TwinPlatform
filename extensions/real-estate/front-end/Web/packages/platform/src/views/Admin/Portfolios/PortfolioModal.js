/* eslint-disable complexity */
import useGetSites from '@willow/common/hooks/useGetSites'
import {
  Flex,
  Form,
  Header,
  Input,
  Modal,
  ModalSubmitButton,
  api,
  useFeatureFlag,
  useFetchRefresh,
  useUser,
} from '@willow/ui'
import { isWillowUser } from '@willow/common'
import { Button, Checkbox, useTheme } from '@willowinc/ui'
import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'
import CategoryButton from './CategoryButton/CategoryButton'
import DeletePortfolioModal from './DeletePortfolioModal'

export default function PortfolioModal({ portfolio, onClose }) {
  const [sitesFeatures, setSitesFeatures] = useState()
  const sitesQuery = useGetSites({
    url: '/me/sites',
  })
  const fetchRefresh = useFetchRefresh()
  const user = useUser()
  const { t } = useTranslation()
  const featureFlags = useFeatureFlag()

  const sitesForPortfolio = useMemo(
    () =>
      sitesQuery.data?.filter(
        (site) => site.portfolioId === portfolio.portfolioId
      ),
    [sitesQuery.data, portfolio.portfolioId]
  )
  const currentAllSitesFeatures = useMemo(
    () =>
      sitesForPortfolio?.length
        ? getAllCheckedSitesFeatures(sitesForPortfolio)
        : undefined,
    [sitesForPortfolio]
  )

  useEffect(() => {
    if (currentAllSitesFeatures) {
      setSitesFeatures(currentAllSitesFeatures)
    }
  }, [portfolio.portfolioId, currentAllSitesFeatures])

  const featureDifferences = Object.keys(labelsMap)
    .map(
      (feature) =>
        sitesFeatures?.[feature] != null &&
        currentAllSitesFeatures?.[feature] !== sitesFeatures?.[feature] &&
        feature
    )
    .filter((feature) => feature)

  const [showDeletePortfolioModal, setShowDeletePortfolioModal] =
    useState(false)

  const isNewPortfolio = portfolio.portfolioId == null
  const isCustomerAdminOrPortfolioUser =
    user.showPortfolioTab || user.isCustomerAdmin

  function handleSubmit(form) {
    const submissionResult = isNewPortfolio
      ? form.api.post(`/api/customers/${user.customer.id}/portfolios`, {
          name: form.data.portfolioName,
          features: form.data.features,
        })
      : form.api.put(
          `/api/customers/${user.customer.id}/portfolios/${portfolio.portfolioId}`,
          {
            name: form.data.portfolioName,
            features: form.data.features,
          }
        )

    if (sitesForPortfolio && featureDifferences.length) {
      const siteUpdatePromises = sitesForPortfolio.map((site) =>
        api.put(
          `/customers/${user.customer.id}/portfolios/${portfolio.portfolioId}/sites/${site.id}`,
          {
            ...site,
            features: {
              ...site.features,
              ...featureDifferences.reduce(
                (acc, feature) => ({
                  ...acc,
                  [feature]: feature.includes('Disabled')
                    ? !sitesFeatures[feature]
                    : sitesFeatures[feature],
                }),
                {}
              ),
            },
          }
        )
      )

      return Promise.all([submissionResult, ...siteUpdatePromises])
    }

    return submissionResult
  }

  function handleSubmitted(form) {
    form.modal.close()

    fetchRefresh('portfolios')

    window.location.reload()
  }

  return (
    <Modal
      header={
        isNewPortfolio ? t('plainText.addPortfolio') : t('headers.portfolio')
      }
      size="small"
      onClose={onClose}
    >
      <Form
        defaultValue={portfolio}
        readOnly={!isNewPortfolio && portfolio.role !== 'Admin'}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        <Flex fill="content">
          {!isNewPortfolio ? (
            <StyledHeader>
              <CategoryButton
                data-testid="manage-sites-button"
                icon="site"
                to={`/admin/portfolios/${portfolio.portfolioId}`}
              >
                {t('plainText.manageSites')}
              </CategoryButton>

              {isCustomerAdminOrPortfolioUser &&
                featureFlags?.hasFeatureToggle('connectivityPage') && (
                  <CategoryButton
                    data-testid="connectivity-button"
                    icon="power"
                    to={`/admin/portfolios/${portfolio.portfolioId}/connectivity`}
                  >
                    {t('plainText.viewConnectivity')}
                  </CategoryButton>
                )}
            </StyledHeader>
          ) : (
            <div />
          )}
          <div>
            <Flex padding="large">
              <Input
                name="portfolioName"
                errorName="name"
                label={t('labels.name')}
                required
              />
            </Flex>
            {/* TODO: once we've done testing for large number of sites,
                we will either remove this feature flag (meaning mass updating site features work fine),
                or we remove the whole section (meaning this feature needs more work)
            */}
            {featureFlags?.hasFeatureToggle('siteFeaturesOnPortfolio') &&
              user.isCustomerAdmin &&
              isWillowUser(user?.email) &&
              sitesFeatures && (
                <AllSitesFeatures
                  featuresMap={sitesFeatures}
                  t={t}
                  onChange={(e) => {
                    setSitesFeatures((prev) => ({
                      ...prev,
                      [e.target.value]: e.target.checked,
                    }))
                  }}
                />
              )}
            {!isNewPortfolio && portfolio.role === 'Admin' && (
              <>
                <hr />
                <Flex padding="large" align="center">
                  <Button
                    kind="negative"
                    onClick={() => setShowDeletePortfolioModal(true)}
                  >
                    {t('headers.deletePortfolio')}
                  </Button>
                </Flex>
              </>
            )}
          </div>
        </Flex>
        <ModalSubmitButton
          showSubmitButton={isNewPortfolio || portfolio.role === 'Admin'}
        >
          {t('plainText.save')}
        </ModalSubmitButton>
      </Form>

      {showDeletePortfolioModal && (
        <DeletePortfolioModal
          portfolio={portfolio}
          onClose={() => setShowDeletePortfolioModal(false)}
        />
      )}
    </Modal>
  )
}

const StyledHeader = styled(Header)({ 'justify-content': 'space-between' })

/**
 * A section to allow user to turn on/off features for all sites in a portfolio
 * at once.
 */
function AllSitesFeatures({ t, featuresMap, onChange }) {
  const theme = useTheme()

  return (
    <div
      css={css`
        padding: ${theme.spacing.s16};
      `}
      data-testid="all-sites-features"
    >
      <div
        css={css`
          padding-bottom: ${theme.spacing.s16};
          color: ${theme.color.neutral.fg.default};
        `}
      >
        {t('plainText.allSitesFeatures').toUpperCase()}
      </div>
      {Object.keys(featuresMap).map((featureName) => (
        <Checkbox
          data-testid={featureName}
          key={featureName}
          value={featureName}
          css={css`
            padding: ${theme.spacing.s8};
          `}
          label={t(labelsMap[featureName])}
          checked={featuresMap[featureName]}
          onChange={onChange}
        />
      ))}
    </div>
  )
}

const labelsMap = {
  isTicketingDisabled: 'plainText.ticketingEnabled',
  isInsightsDisabled: 'plainText.insightsEnabled',
  isReportsEnabled: 'plainText.reportsEnabled',
  isInspectionEnabled: 'plainText.inspectionsEnabled',
  isScheduledTicketsEnabled: 'plainText.scheduleTicketsEnabled',
}

/**
 * Get features status for all sites in a portfolio.
 * - if feature is referred to "enabling", then it is enabled if all sites have it enabled
 * - if feature is referred to "disabling", then it is enabled if no site has it disabled
 */
const getAllCheckedSitesFeatures = (sites) => {
  const initialSitesFeatures = {
    isTicketingDisabled: true,
    isInsightsDisabled: true,
    isReportsEnabled: false,
    isInspectionEnabled: false,
    isScheduledTicketsEnabled: false,
  }
  Object.keys(initialSitesFeatures).forEach((feature) => {
    const isFeatureEnabled =
      (feature.includes('Enabled') &&
        sites.every((site) => site.features[feature])) ||
      (feature.includes('Disabled') &&
        sites.every((site) => !site.features[feature]))
    initialSitesFeatures[feature] = isFeatureEnabled
  })

  return initialSitesFeatures
}
