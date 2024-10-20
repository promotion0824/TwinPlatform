import { Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

import { InspectionZone } from '../../../../services/Inspections/InspectionsServices'
import ZoneModal from './ZoneModal'
import ZonesDataGrid from './ZonesDataGrid'

/**
 * This is the component incorporating the scope selector feature
 * to display various zones belong to a scope that is a building twin in nature.
 * Note: Zones and Zone are only available for building twins, thus, we still
 * query zones/zone data with siteId.
 *
 * TODO: remove packages\platform\src\views\Command\Inspections\Zones\Zones.js
 * once scope selector feature is complete.
 */
export default function ScopedZones({
  siteId,
  userRole,
}: {
  siteId?: string
  userRole?: string
}) {
  const { t } = useTranslation()
  const [selectedZone, setSelectedZone] = useState<InspectionZone | undefined>()

  return (
    <StyledPanelGroup>
      <Panel title={t('headers.zones')}>
        <PanelContent
          css={{
            height: '100%',
          }}
        >
          <ZonesDataGrid
            siteId={siteId}
            onZoneClick={setSelectedZone}
            userRole={userRole}
          />
          {selectedZone != null && (
            <ZoneModal
              zone={selectedZone}
              onClose={setSelectedZone}
              siteId={siteId}
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
