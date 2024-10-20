/* eslint-disable complexity */
import { qs, siteAdminUserRole } from '@willow/common'
import { Link } from 'react-router-dom'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { DocumentTitle, Fetch, useLanguage, useScopeSelector } from '@willow/ui'
import {
  SegmentedControl,
  PanelGroup,
  PageTitle,
  PageTitleItem,
  Select,
} from '@willowinc/ui'
import { useSite } from 'providers'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams, useHistory } from 'react-router'
import styled, { css } from 'styled-components'
import { useDashboard } from '../DashboardContext'
import Floor from './Floor/Floor'
import LoadImage from './LoadImage'
import NewLevel from './NewLevel/NewLevel'
import HeaderWithTabs from '../../../Layout/Layout/HeaderWithTabs'
import routes from '../../../../routes'
import EditorTypeButtons from './Floor/Editor/TabsMenu/EditorTypeButtons'

const ControlsContainer = styled.div(({ theme }) => ({
  display: 'flex',
  gap: theme.spacing.s8,

  '&& div': {
    marginLeft: '0 !important',
  },
}))

export default function FloorViewer({ floors }) {
  const history = useHistory()
  const { isReadOnly } = useDashboard()
  const params = useParams()
  const site = useSite()
  const { language } = useLanguage()
  const { t } = useTranslation()
  const { locationName } = useScopeSelector()

  const [equipmentId] = useState(() => qs.get('equipmentId'))

  const [{ admin, floorViewType = '3D' }, setParams] = useMultipleSearchParams([
    'admin',
    'floorViewType',
  ])

  const is2DDisabled = site.features.is2DViewerDisabled

  return (
    <>
      <DocumentTitle scopes={[t('plainText.3dViewer'), locationName]} />
      <PanelGroup
        direction="vertical"
        css={css(({ theme }) => `padding: ${theme.spacing.s16};`)}
      >
        <HeaderWithTabs
          css={{ borderBottom: 'none' }}
          titleRow={[
            <PageTitle key="pageTitle">
              <PageTitleItem>
                <Link
                  to={`${window.location.pathname}${window.location.search}`}
                >
                  {t('plainText.3dViewer')}
                </Link>
              </PageTitleItem>
            </PageTitle>,
            <div tw="flex items-center" key="additionalControls">
              <ControlsContainer>
                <EditorTypeButtons
                  floorViewType={floorViewType}
                  onFloorViewTypeChange={(nextOption) => {
                    // we allow insight/ticket layer to be controlled via query param,
                    // and query param update will trigger an event to be sent
                    // to the iframe to toggle the layer, we therefore remove
                    // the query param to avoid complications
                    setParams({
                      floorViewType: nextOption,
                      isInsightStatsLayerOn: undefined,
                      isTicketStatsLayerOn: undefined,
                    })
                  }}
                />
                <Select
                  data={(floors ?? []).map((floor) => ({
                    label: floor?.name ? floor.name : t('plainText.unknown'),
                    value: floor.id,
                  }))}
                  value={params.floorId}
                  onChange={(value) =>
                    history.push(
                      `${routes.sites__siteId_floors__floorId(site.id, value)}${
                        admin === 'true' ? `?admin=${admin}` : ''
                      }`
                    )
                  }
                />
                {site.userRole === siteAdminUserRole && (
                  <SegmentedControl
                    data={[
                      {
                        label: t('headers.admin'),
                        value: 'admin',
                      },
                      {
                        label: t('plainText.user'),
                        value: 'user',
                      },
                    ]}
                    value={admin === 'true' ? 'admin' : 'user'}
                    onChange={(value) =>
                      setParams({
                        admin: value === 'admin' ? 'true' : undefined,
                      })
                    }
                  />
                )}
              </ControlsContainer>
            </div>,
          ]}
        />
        <Fetch
          url={
            equipmentId != null
              ? `/api/sites/${params.siteId}/assets/${equipmentId}`
              : undefined
          }
          headers={{ language }}
          error={null}
        >
          {(equipment) => (
            <Fetch
              name="floor"
              url={[
                `/api/sites/${params.siteId}/floors/${params.floorId}/layerGroups`,
                `/api/sites/${params.siteId}/preferences/moduleGroups`,
              ]}
            >
              {/*
                Floor detailed property like isSiteWide will not be returned from
                /api/sites/${params.siteId}/floors/${params.floorId}/layerGroups;
                instead, they come from individual floor of "floors" prop of FloorViewer
            */}
              {([floor, disciplinesSortOrder]) => {
                const isSiteWide =
                  floors?.find((oneFloor) => oneFloor.id === floor.floorId)
                    ?.isSiteWide ?? false
                return floor.modules2D.length > 0 ? (
                  <LoadImage src={floor.modules2D[0].url}>
                    {(dimensions) => (
                      <Floor
                        floor={{
                          ...dimensions,
                          ...floor,
                          isSiteWide,
                        }}
                        equipment={equipment}
                        disciplinesSortOrder={disciplinesSortOrder}
                      />
                    )}
                  </LoadImage>
                ) : is2DDisabled ||
                  isReadOnly ||
                  site.features.isNonTenancyFloorsEnabled ? (
                  <Floor
                    floor={{
                      ...floor,
                      isSiteWide,
                    }}
                    equipment={equipment}
                    disciplinesSortOrder={disciplinesSortOrder}
                    isFloorEditorDisabled={
                      !site.features.isNonTenancyFloorsEnabled && !is2DDisabled
                    }
                  />
                ) : (
                  <NewLevel floor={floor} />
                )
              }}
            </Fetch>
          )}
        </Fetch>
      </PanelGroup>
    </>
  )
}
