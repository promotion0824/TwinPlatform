import { FullSizeLoader, titleCase } from '@willow/common'
import { Site } from '@willow/common/site/site/types'
import { DocumentTitle, useFeatureFlag, useScopeSelector } from '@willow/ui'
import { Button, Icon, PageTitle, PageTitleItem } from '@willowinc/ui'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, Redirect, useParams } from 'react-router-dom'
import styled, { css } from 'styled-components'
import routes from '../../../routes'
import { useBuildingHomeSlice } from '../../../store/buildingHomeSlice'
import { useClassicExplorerLandingPath } from '../../Layout/Layout/Header/utils'
import HeaderWithTabs from '../../Layout/Layout/HeaderWithTabs'
import BuildingHomeContent from './BuildingHomeContent'
import BuildingHomeDraggableDashboard from './BuildingHomeDraggableDashboard'
import WidgetLayoutControls from './widgets/WidgetLayoutControls'

/**
 * The Home page for a selected building, (used to called Site View) .
 */
export default function BuildingHome({
  days,
  floors,
  site,
  onDateChange,
}: {
  days: string
  floors: Array<any>
  site: Site
  onDateChange: (days: string) => void
}) {
  const { siteId: siteIdFromParams } = useParams<{ siteId?: string }>()
  const { isScopeSelectorEnabled, location, locationName, scopeId } =
    useScopeSelector()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const classicExplorerLandingPath = useClassicExplorerLandingPath()
  const newHomePage = useFeatureFlag().hasFeatureToggle(
    'buildingHomeDragAndDropRedesign'
  )
  const { initialize, isLoading } = useBuildingHomeSlice()

  useEffect(() => {
    if (scopeId) {
      initialize(scopeId)
    }
  }, [initialize, scopeId])

  if (isScopeSelectorEnabled && siteIdFromParams && location) {
    return <Redirect to={routes.home_scope__scopeId(location.twin.id)} />
  }

  return (
    <>
      <DocumentTitle scopes={[t('headers.home'), locationName]} />
      {newHomePage && isLoading ? (
        <FullSizeLoader />
      ) : (
        <>
          <HeaderWithTabs
            titleRow={[
              <PageTitle key="pageTitle">
                <PageTitleItem>
                  <Link to={window.location.pathname}>
                    {t('headers.home')} - {site.name}
                  </Link>
                </PageTitleItem>
              </PageTitle>,
              <HeaderControlsContainer key="headerControls">
                {!newHomePage && classicExplorerLandingPath && (
                  <Link to={classicExplorerLandingPath}>
                    <Button
                      kind="secondary"
                      prefix={<Icon icon="arrow_forward" />}
                    >
                      {titleCase({
                        text: t('plainText.viewInViewer'),
                        language,
                      }).replace('3d', '3D')}
                    </Button>
                  </Link>
                )}
                {newHomePage && <WidgetLayoutControls />}
              </HeaderControlsContainer>,
            ]}
            css={css(({ theme }) => ({ paddingBottom: theme.spacing.s16 }))}
          />
          {newHomePage ? (
            <BuildingHomeDraggableDashboard />
          ) : (
            <BuildingHomeContent
              floors={floors}
              onDateChange={onDateChange}
              days={days}
              site={site}
            />
          )}
        </>
      )}
    </>
  )
}

const HeaderControlsContainer = styled.div(
  ({ theme }) => css`
    display: flex;
    flex-direction: row;
    gap: ${theme.spacing.s8};
  `
)
