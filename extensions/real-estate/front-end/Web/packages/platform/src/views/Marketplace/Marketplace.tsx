import { ReactNode, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Redirect, Route, Switch, useHistory, useParams } from 'react-router'
import { Link } from 'react-router-dom'
import styled from 'styled-components'

import {
  FullSizeContainer,
  FullSizeLoader,
  NoPermissionState,
  titleCase,
} from '@willow/common'
import {
  ALL_LOCATIONS,
  DocumentTitle,
  FILTER_PANEL_BREAKPOINT,
  Message,
  ScopeSelectorWrapper,
  useAnalytics,
  useFeatureFlag,
  useLanguage,
  useScopeSelector,
  useSize,
  useUser,
} from '@willow/ui'
import {
  Button,
  Drawer,
  Group,
  Icon,
  Indicator,
  PageTitle,
  PageTitleItem,
  Panel,
  PanelContent,
  PanelGroup,
  SearchInput,
  useDisclosure,
} from '@willowinc/ui'

import { SiteSelect } from '../../components/SiteSelect'
import { useSite, useSites } from '../../providers'
import routes from '../../routes'
import { LayoutHeader } from '../Layout'
import GenericHeader from '../Layout/Layout/GenericHeader'
import { useHomeUrl } from '../Layout/Layout/Header/utils'
import App from './App/App'
import Apps from './Apps'
import MarketplaceFilters from './MarketplaceFilters'
import MarketplaceInvalid from './MarketplaceInvalid'
import useApps from './useApps'

const LayoutHeaderContent = styled.div({
  display: 'flex',
  justifyContent: 'center',
  flexDirection: 'column',
  height: '100%',
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

export default function Marketplace() {
  const analytics = useAnalytics()
  const featureFlags = useFeatureFlag()
  const {
    location: { pathname },
    push,
  } = useHistory()
  const scopeSelector = useScopeSelector()
  const site = useSite()
  const sites = useSites()
  const user = useUser()
  const { t } = useTranslation()

  const handleSiteChange = (newSite) => {
    analytics.track('Site Select', {
      site,
      customer: user?.customer ?? {},
    })

    if (pathname.includes(site.id)) {
      push(pathname.replace(site.id, newSite.id || ''))
    } else {
      push(routes.connectors_sites__siteId(newSite.id))
    }
  }

  const isValidSiteSelected =
    !scopeSelector.isScopeSelectorEnabled ||
    scopeSelector.isScopeUsedAsBuilding(scopeSelector.location)

  return (
    <>
      <DocumentTitle
        scopes={[t('headers.connectors'), scopeSelector.locationName]}
      />

      <LayoutHeader>
        <LayoutHeaderContent>
          {featureFlags.hasFeatureToggle('scopeSelector') ? (
            <ScopeSelectorWrapper
              onLocationChange={(loc) => {
                analytics.track('Location Select', {
                  location: loc,
                  customer: user?.customer ?? {},
                })
                const { twin } = loc
                if (twin?.id === ALL_LOCATIONS) {
                  push(routes.connectors)
                } else {
                  push(routes.connectors_scope__scopeId(twin.id))
                }
              }}
            />
          ) : (
            <SiteSelect
              isAllSiteIncluded={false}
              sites={sites}
              value={sites.find((s) => s.id === site.id) || sites[0]}
              onChange={handleSiteChange}
            />
          )}
        </LayoutHeaderContent>
      </LayoutHeader>

      {!isValidSiteSelected ? (
        <MarketplaceInvalid />
      ) : (
        <Switch>
          <Route
            path={[
              routes.connectors_sites__siteId(),
              routes.connectors_scope__scopeId(),
            ]}
            exact
          >
            <MarketplaceScopeRedirect
              redirect={routes.connectors_scope__scopeId}
            >
              <MarketplaceApps />
            </MarketplaceScopeRedirect>
          </Route>
          <Route
            path={[
              routes.connectors_sites__siteId_connector__connectorId(),
              routes.connectors_scope__scopeId_connector__connectorId(),
            ]}
          >
            <MarketplaceScopeRedirect
              redirect={routes.connectors_scope__scopeId_connector__connectorId}
            >
              <App />
            </MarketplaceScopeRedirect>
          </Route>
        </Switch>
      )}
    </>
  )
}

const MarketplaceScopeRedirect = ({
  redirect,
  children,
}: {
  redirect: (scopeId: string, connectorId?: string) => string
  children: React.ReactNode
}) => {
  const { isScopeSelectorEnabled, scopeLookup } = useScopeSelector()
  const { connectorId, siteId } =
    useParams<{ connectorId?: string; siteId?: string }>()

  const scopeIdOnSiteId = scopeLookup[siteId ?? '']?.twin?.id

  if (isScopeSelectorEnabled && scopeIdOnSiteId) {
    return (
      <Redirect
        to={
          connectorId
            ? redirect(scopeIdOnSiteId, connectorId)
            : redirect(scopeIdOnSiteId)
        }
      />
    )
  }

  return <>{children}</>
}

export function MarketplaceApps() {
  const { t } = useTranslation()
  const { location } = useScopeSelector()
  const containerRef = useRef(null)
  const { width } = useSize(containerRef)

  const homeUrl = useHomeUrl()

  const [appSearch, setAppSearch] = useState('')
  const [selectedCategories, setSelectedCategories] = useState<string[]>([])
  const [selectedStatuses, setSelectedStatuses] = useState<string[]>([])
  const [selectedActivatePacks, setSelectedActivatePacks] = useState<string[]>(
    []
  )
  const {
    data: apps,
    isLoading,
    isError,
    error,
  } = useApps({
    search: appSearch,
    selectedCategories,
    selectedStatuses,
    selectedActivatePacks,
  })
  const resetFilters = () => {
    setSelectedCategories([])
    setSelectedStatuses([])
    setSelectedActivatePacks([])
  }
  const noFiltersSelected =
    !selectedCategories.length &&
    !selectedStatuses.length &&
    !selectedActivatePacks.length

  const isSmallScreen = width < FILTER_PANEL_BREAKPOINT

  const filterComponent = (
    <MarketplaceFilters
      search={appSearch}
      selectedCategories={selectedCategories}
      selectedStatuses={selectedStatuses}
      selectedActivatePacks={selectedActivatePacks}
      onSearchChange={setAppSearch}
      onSelectedCategoriesChange={setSelectedCategories}
      onSelectedStatusesChange={setSelectedStatuses}
      onSelectedActivatePacksChange={setSelectedActivatePacks}
      hideSearchInput={isSmallScreen}
    />
  )

  if (isError) {
    return error?.response?.status === 403 ? (
      <NoPermissionState homeUrl={homeUrl} />
    ) : (
      <ErrorMessage />
    )
  }

  return (
    <MainContainer ref={containerRef}>
      {isLoading ? (
        <FullSizeLoader intent="secondary" />
      ) : apps ? (
        <>
          <GenericHeader
            topLeft={
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
              </PageTitle>
            }
            bottomLeft={
              isSmallScreen && (
                <SearchInput
                  onChange={(event) => setAppSearch(event.currentTarget.value)}
                  placeholder={t('placeholder.searchConnectors')}
                  value={appSearch}
                />
              )
            }
            bottomRight={
              isSmallScreen && (
                <FilterDrawer
                  hasIndicator={!noFiltersSelected}
                  key={isSmallScreen.toString()}
                  onClearButtonClick={resetFilters}
                  disableClearButton={noFiltersSelected}
                >
                  {filterComponent}
                </FilterDrawer>
              )
            }
          />
          <PanelGroup>
            {!isSmallScreen ? (
              <Panel collapsible defaultSize={320} title={t('headers.filters')}>
                {filterComponent}
              </Panel>
            ) : (
              <></>
            )}

            <Panel title={t('headers.connectors')}>
              <StyledPanelContent>
                <Apps apps={apps} />
              </StyledPanelContent>
            </Panel>
          </PanelGroup>
        </>
      ) : (
        <ErrorMessage />
      )}
    </MainContainer>
  )
}

const ErrorMessage = () => {
  const { t } = useTranslation()

  return (
    <FullSizeContainer>
      <Message icon="error">{t('plainText.errorOccurred')}</Message>
    </FullSizeContainer>
  )
}

const FilterDrawer = ({
  children,
  onClearButtonClick,
  disableClearButton,
  hasIndicator,
}: {
  children: ReactNode
  onClearButtonClick: () => void
  disableClearButton: boolean
  hasIndicator: boolean
}) => {
  const { t } = useTranslation()
  const { language } = useLanguage()
  const [opened, { open, close }] = useDisclosure(false)

  return (
    <>
      <Drawer
        opened={opened}
        onClose={close}
        header={t('headers.filters')}
        footer={
          <Group
            justify="flex-end"
            w="100%" // This can be removed once we have DrawerFooter width default to 100%
          >
            <Button
              onClick={onClearButtonClick}
              kind="secondary"
              disabled={disableClearButton}
            >
              {titleCase({ text: t('plainText.clearFilters'), language })}
            </Button>
          </Group>
        }
      >
        {children}
      </Drawer>

      <Indicator disabled={!hasIndicator}>
        <Button
          prefix={<Icon icon="filter_list" />}
          kind="secondary"
          onClick={open}
        >
          {t('headers.filters')}
        </Button>
      </Indicator>
    </>
  )
}
