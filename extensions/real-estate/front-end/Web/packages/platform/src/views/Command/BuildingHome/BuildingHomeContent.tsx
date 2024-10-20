import { qs } from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { NotFound } from '@willow/ui'
import { Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import { css } from 'styled-components'
import { useSite } from '../../../providers'
import { useDashboard } from '../Dashboard/DashboardContext'
import BuildingHomeInsightsPanel from './BuildingHomeInsightsPanel'
import TicketsDataGrid from './Tickets/TicketsDataGrid'
import ThreeDViewModel from './widgets/ThreeDModel/ThreeDModelView'

// eslint-disable-next-line complexity
export default function BuildingHomeContent({
  floors,
  onDateChange,
  days,
  site: siteProp,
}) {
  const [{ ticketId }, setSearchParams] = useMultipleSearchParams([
    'insightId',
    'metric',
    'ticketId',
  ])
  const { isReadOnly } = useDashboard()
  const history = useHistory()
  const siteFromContext = useSite()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const site = siteProp ?? siteFromContext

  const showTicketingOrInsights =
    !site.features.isTicketingDisabled || !site.features.isInsightsDisabled

  useEffect(() => {
    const shouldHideDashboard =
      site.features.isTicketingDisabled &&
      site.features.isInsightsDisabled &&
      site.features.is2DViewerDisabled &&
      floors.length > 0

    if (shouldHideDashboard) {
      history.push(
        `/sites/${site.id}/floors/${floors[0].id}${
          qs.get('admin') ? '?admin=true' : ''
        }`
      )
    }
  }, [
    floors,
    history,
    site.features.is2DViewerDisabled,
    site.features.isInsightsDisabled,
    site.features.isTicketingDisabled,
    site.id,
  ])

  if (!isReadOnly) {
    return <NotFound>{t('plainText.noFloorSelected')}</NotFound>
  }

  return (
    <PanelGroup
      resizable
      direction="horizontal"
      css={css(({ theme }) => ({ padding: theme.spacing.s16 }))}
    >
      {showTicketingOrInsights ? (
        <PanelGroup direction="vertical" resizable>
          {!site.features.isInsightsDisabled ? (
            <BuildingHomeInsightsPanel
              days={days}
              onDateChange={onDateChange}
              site={site}
            />
          ) : (
            <></>
          )}
          {!site.features.isTicketingDisabled ? (
            <Panel collapsible title={t('headers.tickets')}>
              <PanelContent css={{ height: '100%' }}>
                <TicketsDataGrid
                  t={t}
                  language={language}
                  selectedTicketId={ticketId}
                  onSelectedTicketIdChange={(newTicketId) =>
                    setSearchParams({ ticketId: newTicketId })
                  }
                />
              </PanelContent>
            </Panel>
          ) : (
            <></>
          )}
        </PanelGroup>
      ) : (
        <></>
      )}

      {/* by adding key to panel, it will ensure the updated 3d model is shown when site id changes */}
      <Panel collapsible title={t('headers.spatialView')} key={site.id}>
        <PanelContent css={{ height: '100%' }}>
          <ThreeDViewModel css={{ height: '100%' }} />
        </PanelContent>
      </Panel>
    </PanelGroup>
  )
}
