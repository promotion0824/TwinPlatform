import { useFetchRefresh } from '@willow/ui'
import { Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import styled from 'styled-components'
import ArchiveInspectionModal from '../InspectionModal/ArchiveInspectionModal'
import InspectionModal from '../InspectionModal/InspectionModal'
import { useInspections } from '../InspectionsProvider'
import InspectionsTable from './InspectionsTable'
import {
  getInspectionsPageTitle,
  getZonePageTitle,
  getZonesPageTitle,
} from '../getPageTitles'

export default function ZoneContent({ zone }) {
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const { t } = useTranslation()
  const { setPageTitles } = useInspections()

  useEffect(() => {
    setPageTitles([
      getInspectionsPageTitle({
        siteId: params.siteId,
        title: t('headers.inspections'),
      }),
      getZonesPageTitle({ siteId: params.siteId, title: t('headers.zones') }),
      getZonePageTitle({
        siteId: params.siteId,
        zoneId: params.zoneId,
        zoneName: zone.name,
      }),
    ])
  }, [params.siteId, params.zoneId, setPageTitles, t, zone.name])

  const [selectedInspection, setSelectedInspection] = useState()
  const [inspectionToArchive, setInspectionToArchive] = useState()

  return (
    <>
      <StyledPanelGroup>
        <Panel title={t('headers.inspections')}>
          <PanelContent
            css={{
              // to be able to centralize not found message
              height: '100%',
            }}
          >
            <InspectionsTable
              inspections={zone.inspections}
              onSelect={setSelectedInspection}
              onArchive={setInspectionToArchive}
              showSiteColumn={params.siteId == null}
            />
          </PanelContent>
        </Panel>
      </StyledPanelGroup>

      {selectedInspection != null && (
        <InspectionModal
          inspection={selectedInspection}
          zoneId={zone.id}
          onClose={(response) => {
            setSelectedInspection()

            if (response === 'submitted') {
              fetchRefresh('zone')
            }
          }}
        />
      )}
      {inspectionToArchive != null && (
        <ArchiveInspectionModal
          inspection={inspectionToArchive}
          onClose={(response) => {
            setInspectionToArchive()

            if (response === 'submitted') {
              fetchRefresh('zone')
            }
          }}
        />
      )}
    </>
  )
}

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))
