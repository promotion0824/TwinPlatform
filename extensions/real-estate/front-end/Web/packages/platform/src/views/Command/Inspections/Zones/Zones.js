import { Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import { useSite } from 'providers'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import useCommandAnalytics from '../../useCommandAnalytics.ts'
import { useInspections } from '../InspectionsProvider'
import ZoneModal from './ZoneModal'
import ZonesDataGrid from './ZonesDataGrid'
import { getInspectionsPageTitle } from '../getPageTitles'

export default function Zones() {
  const site = useSite()
  const { t } = useTranslation()
  const commandAnalytics = useCommandAnalytics(site.id)
  const [selectedZone, setSelectedZone] = useState()
  const { setPageTitles } = useInspections()

  useEffect(() => {
    setPageTitles([
      getInspectionsPageTitle({
        siteId: site.id,
        title: t('headers.inspections'),
      }),
    ])
  }, [site.id, setPageTitles, t])

  useEffect(() => {
    commandAnalytics.pageInspections('zones')
  }, [commandAnalytics])

  return (
    <StyledPanelGroup>
      <Panel title={t('headers.zones')}>
        <PanelContent
          css={{
            width: '100%',
            height: '100%',
            overflowY: 'auto',
          }}
        >
          <ZonesDataGrid
            siteId={site.id}
            onZoneClick={setSelectedZone}
            userRole={site.userRole}
          />
          {selectedZone != null && (
            <ZoneModal
              zone={selectedZone}
              onClose={setSelectedZone}
              siteId={site.id}
            />
          )}
        </PanelContent>
      </Panel>
    </StyledPanelGroup>
  )
}

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))
