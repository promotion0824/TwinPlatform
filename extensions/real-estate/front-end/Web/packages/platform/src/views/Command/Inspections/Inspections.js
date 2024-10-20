/* eslint-disable complexity */
/* eslint-disable react/no-children-prop */
import { useState } from 'react'
import { useQuery, useQueryClient } from 'react-query'
import { FullSizeLoader, siteAdminUserRole } from '@willow/common'
import {
  NavTab,
  NavTabs,
  useScopeSelector,
  api,
  useFetchRefresh,
  DocumentTitle,
} from '@willow/ui'
import { PageTitle, PageTitleItem, Button, Icon } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Route, Switch, useParams } from 'react-router'
import { Link } from 'react-router-dom'
import styled from 'styled-components'

import DisabledWarning from '../../../components/DisabledWarning/DisabledWarning.tsx'
import { useSites } from '../../../providers'
import HeaderWithTabs from '../../Layout/Layout/HeaderWithTabs'
import TimeRangePicker from './InspectionHistory/CheckTimeRangePicker'
import { InspectionsProvider, useInspections } from './InspectionsProvider'
import ZoneModal from './Zones/ZoneModal'
import InspectionModal from './InspectionModal/InspectionModal'
import getInspectionsPath from './getInspectionsPath.ts'
import makeScopedInspectionsPath from './makeScopedInspectionsPath'
import {
  legacySiteInspectionRoutes,
  makeScopedInspectionRoutes,
} from './inspectionRoutesUtils'

const PAGES = {
  results: 'results',
  insights: 'insights',
  usage: 'usage',
  zones: 'zones',
}

function InspectionsComponent() {
  const {
    location,
    descendantSiteIds,
    isScopeSelectorEnabled,
    twinQuery,
    scopeLookup,
    isScopeUsedAsBuilding,
  } = useScopeSelector()
  const params = useParams()
  const sites = useSites()
  const { t } = useTranslation()

  const site = params.siteId
    ? sites.find((s) => s.id === params.siteId)
    : undefined

  const isAllSites = site == null
  const isInspectionEnabled = isAllSites
    ? sites.some((s) => s.features.isInspectionEnabled)
    : site.features.isInspectionEnabled

  const inspectionQuery = useQuery(
    ['inspection', params.inspectionId],
    async () => {
      const { data } = await api.get(`/inspections/${params.inspectionId}`)
      return data
    },
    {
      enabled: !!params.inspectionId && isScopeSelectorEnabled,
    }
  )

  const zoneInspectionsQuery = useQuery(
    ['zone-inspections', params.zoneId],
    async () => {
      const { data } = await api.get(
        `/sites/${location?.twin?.siteId}/inspectionZones/${params.zoneId}/inspections`
      )
      return data
    },
    {
      // Keep previous data so when user is changing sort order, they will still be presented with
      // inspection data grid until inspection data with new order is fetched
      keepPreviousData: true,
      // inspections associated with a zone are still fetched by using siteId
      // since Zone and Zones are only valid for building twin;
      // therefore, we enable this query only when the scope is a building twin in nature
      // when scope selector feature is turned on.
      enabled:
        !!params.zoneId &&
        isScopeSelectorEnabled &&
        !!location?.twin?.siteId &&
        isScopeUsedAsBuilding(location),
    }
  )

  if (isScopeSelectorEnabled) {
    if (twinQuery.status === 'loading') {
      return <FullSizeLoader />
    }

    const siteIdInspectionFeatureMap = Object.fromEntries(
      sites.map((s) => [s.id, s.features.isInspectionEnabled])
    )

    // when the scope is a building twin in nature, check value of its inspection feature,
    // otherwise, check value of its descendant sites' inspection feature
    const isInspectionEnabledForScope = isScopeUsedAsBuilding(location)
      ? siteIdInspectionFeatureMap[location?.twin?.siteId]
      : descendantSiteIds.some((id) => !!siteIdInspectionFeatureMap[id])

    // in case user is trying to access the legacy /sites/:siteId/inspections route,
    // we find the scope id (twinId) based on the siteId and redirect to the new route
    // at /inspections/:scopeId and we fire onLocationChange to update the scope selector
    const scopeOnSiteId = scopeLookup[params?.siteId]

    const userRole = sites.find(
      (s) => s.id === location?.twin?.siteId
    )?.userRole

    return !isInspectionEnabledForScope ? (
      <DisabledWarning title={t('plainText.inspectionsNotEnabled')} />
    ) : (
      <Switch>
        {makeScopedInspectionRoutes({
          t,
          scopeId: params.scopeId ?? scopeOnSiteId?.twin?.id,
          inspectionQuery,
          checkId: params.checkId,
          inspectionId: params.inspectionId,
          siteIdForBuildingScope: location?.twin?.siteId,
          userRole,
          zoneId: params.zoneId,
          zoneInspectionsQuery,
        }).map(({ path, routeOptions, child, controls }) => (
          <Route path={path} key={path} {...routeOptions}>
            <CommonHeader
              {...controls}
              // we do not display usages/zones tabs for scope that
              // is not a building twin in nature for now
              enableExtraTabs={isScopeUsedAsBuilding(location)}
              resultsPath={makeScopedInspectionsPath(
                params.scopeId ?? scopeOnSiteId?.twin?.id
              )}
              zonesPath={makeScopedInspectionsPath(
                params.scopeId ?? scopeOnSiteId?.twin?.id,
                { pageName: PAGES.zones }
              )}
              usagePath={makeScopedInspectionsPath(
                params.scopeId ?? scopeOnSiteId?.twin?.id,
                { pageName: PAGES.usage }
              )}
              userRole={userRole}
              siteId={location?.twin?.siteId}
            />
            {child}
          </Route>
        ))}
      </Switch>
    )
  } else {
    if (!isInspectionEnabled) {
      return <DisabledWarning title={t('plainText.inspectionsNotEnabled')} />
    }
    return (
      <Switch>
        {legacySiteInspectionRoutes.map(
          ({ path, routeOptions, child, controls }) => (
            <Route path={path} key={path} {...routeOptions}>
              <CommonHeader
                {...controls}
                enableExtraTabs={!isAllSites}
                userRole={site?.userRole}
                siteId={site?.id}
              />
              {child}
            </Route>
          )
        )}
      </Switch>
    )
  }
}

function CommonHeader({
  // initialise all controls as false
  showTimePicker = false,
  showTabs = false,
  showAddZoneButton = false,
  showAddInspectionButton = false,
  pageTitles: injectedPageTitles,
  enableExtraTabs = false,
  resultsPath,
  zonesPath,
  usagePath,
  userRole,
  siteId,
}) {
  const params = useParams()
  const { pageTitles: contextPageTitles } = useInspections()
  const pageTitles = injectedPageTitles || contextPageTitles

  return (
    <HeaderWithTabs
      titleRow={[
        pageTitles.length > 0 ? (
          <PageTitles key="pageTitle" pageTitles={pageTitles} />
        ) : (
          <div />
        ),
        <TitleRowControlsContainer key="titleRowControls">
          {showTimePicker && <TimeRangePicker />}
          {showAddInspectionButton && (
            <AddInspectionButton
              userRole={userRole}
              siteId={siteId}
              zoneId={params.zoneId}
            />
          )}
        </TitleRowControlsContainer>,
      ]}
      tabs={
        showTabs && (
          <HeaderTabs
            enableExtraTabs={enableExtraTabs}
            resultsPath={resultsPath}
            zonesPath={zonesPath}
            usagePath={usagePath}
          />
        )
      }
      controlsOnTabs={
        showAddZoneButton && (
          <AddZoneControl userRole={userRole} siteId={siteId} />
        )
      }
    />
  )
}

function PageTitles({ pageTitles }) {
  return (
    <PageTitle>
      {pageTitles.map(({ title, href }) => (
        <PageTitleItem key={title}>
          <Link to={href}>{title}</Link>
        </PageTitleItem>
      ))}
    </PageTitle>
  )
}

function HeaderTabs({
  enableExtraTabs = false,
  resultsPath,
  zonesPath,
  usagePath,
}) {
  const params = useParams()
  const { t } = useTranslation()
  const pageTab = getTabByPathname(window.location.pathname)
  const { locationName } = useScopeSelector()

  const tabsText = {
    [PAGES.results]: t('headers.results'),
    [PAGES.usage]: t('headers.usage'),
    [PAGES.zones]: t('headers.zones'),
  }

  return (
    <>
      <DocumentTitle
        scopes={[tabsText[pageTab], t('headers.inspections'), locationName]}
      />

      <NavTabs
        value={pageTab}
        // TODO: add scoped path values for usage and zones tabs
        tabs={[
          <NavTab
            data-testid="subMenu-inspection-result-button"
            to={resultsPath || getInspectionsPath(params.siteId)}
            value={PAGES.results}
          >
            {tabsText[PAGES.results]}
          </NavTab>,
          <NavTab
            data-testid="subMenu-usage-button"
            to={
              usagePath ||
              getInspectionsPath(params.siteId, { pageName: PAGES.usage })
            }
            value={PAGES.usage}
            disabled={!enableExtraTabs}
          >
            {tabsText[PAGES.usage]}
          </NavTab>,
          <NavTab
            data-cy="inspections-zones-button"
            data-testid="subMenu-zones-button"
            to={
              zonesPath ||
              getInspectionsPath(params.siteId, { pageName: PAGES.zones })
            }
            value={PAGES.zones}
            disabled={!enableExtraTabs}
          >
            {tabsText[PAGES.zones]}
          </NavTab>,
        ]}
      />
    </>
  )
}

function getTabByPathname(pathname) {
  if (pathname.includes(PAGES.zones)) {
    return PAGES.zones
  }

  if (pathname.includes(PAGES.usage)) {
    return PAGES.usage
  }

  // go to results with invalid pathname
  return PAGES.results
}

const TitleRowControlsContainer = styled.div(({ theme }) => ({
  gap: theme.spacing.s8,
  display: 'flex',
}))

export default function Inspections() {
  const { locationName } = useScopeSelector()
  const { t } = useTranslation()
  return (
    <InspectionsProvider>
      <DocumentTitle scopes={[t('headers.inspections'), locationName]} />

      <InspectionsComponent />
    </InspectionsProvider>
  )
}

/**
 * The component contains a combination of a button and a modal to add a Zone
 * on Zones page.
 */
const AddZoneControl = ({ userRole, siteId }) => {
  const { t } = useTranslation()
  const [open, setOpen] = useState(false)

  return (
    userRole === siteAdminUserRole && (
      <>
        <Button onClick={() => setOpen(true)} prefix={<Icon icon="add" />}>
          {t('plainText.addZone')}
        </Button>
        {open && <ZoneModal onClose={() => setOpen(false)} siteId={siteId} />}
      </>
    )
  )
}

/**
 * The component contains a combination of a button and a modal to add an inspection
 * to a Zone.
 */
const AddInspectionButton = ({ userRole, siteId, zoneId }) => {
  const fetchRefresh = useFetchRefresh()
  const queryClient = useQueryClient()
  const { t } = useTranslation()
  const [open, setOpen] = useState(false)

  return (
    <>
      {userRole === siteAdminUserRole && (
        <Button
          data-cy="add-inspection-button"
          onClick={() => setOpen(true)}
          prefix={<Icon icon="add" />}
        >
          {t('plainText.addInspection')}
        </Button>
      )}

      {open && (
        <InspectionModal
          inspection={{ siteId }}
          zoneId={zoneId}
          onClose={async (response) => {
            setOpen(false)

            if (response === 'submitted') {
              fetchRefresh('zone')
              await queryClient.invalidateQueries('zone-inspections')
            }
          }}
        />
      )}
    </>
  )
}
