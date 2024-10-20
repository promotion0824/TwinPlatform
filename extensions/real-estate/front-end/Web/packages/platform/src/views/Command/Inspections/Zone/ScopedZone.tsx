import { useState } from 'react'
import { styled } from 'twin.macro'
import { useTranslation } from 'react-i18next'
import { UseQueryResult, useQueryClient } from 'react-query'
import { Message, DocumentTitle, useScopeSelector, api } from '@willow/ui'
import { PanelGroup, Panel, PanelContent, useSnackbar } from '@willowinc/ui'
import { FullSizeContainer } from '@willow/common'

import { Inspection } from '../../../../services/Inspections/InspectionsServices'
import InspectionModal from '../InspectionModal/InspectionModal'
import ArchiveInspectionModal from '../InspectionModal/ArchiveInspectionModal'
import ZoneInspectionsDataGrid from './ZoneInspectionsDataGrid'

/**
 * This is the Zone component working with scope selector feature;
 * it will display inspections for a zone. Please note
 * Zones and Zone are only available for building twins.
 *
 * TODO: remove packages\platform\src\views\Command\Inspections\Zone\Zone.js and
 * packages\platform\src\views\Command\Inspections\Zone\ZoneContent.js
 * once scope selector feature is complete.
 */
export default function ScopedZone({
  zoneId,
  zoneInspectionsQuery,
}: {
  zoneId?: string
  zoneInspectionsQuery: UseQueryResult<{
    id: string
    siteId?: string
    inspections?: Array<Inspection>
    name?: string
  }>
}) {
  const snackbar = useSnackbar()
  const queryClient = useQueryClient()
  const { t } = useTranslation()
  const { locationName } = useScopeSelector()

  const [selectedInspection, setSelectedInspection] = useState<Inspection>()
  const [inspectionToArchive, setInspectionToArchive] = useState<Inspection>()

  const [isUpdatingOrder, setIsUpdatingOrder] = useState(false)

  const handleCloseModal = async (response?: string) => {
    setSelectedInspection(undefined)
    setInspectionToArchive(undefined)

    if (response === 'submitted') {
      // refetch inspections for the zone
      await queryClient.invalidateQueries('zone-inspections')
    }
  }

  const handleSortOrderChange = async ({
    siteId,
    zoneId,
    inspectionIds,
  }: {
    siteId: string
    zoneId: string
    inspectionIds: string[]
  }) => {
    try {
      setIsUpdatingOrder(true)
      await api.put(`/sites/${siteId}/zones/${zoneId}/inspections/sortOrder`, {
        inspectionIds,
      })
      await queryClient.invalidateQueries(['zone-inspections', zoneId])
    } catch (error) {
      snackbar.show({
        title: t('plainText.errorOccurred'),
        intent: 'negative',
      })
    } finally {
      setIsUpdatingOrder(false)
    }
  }

  return zoneInspectionsQuery.isError ? (
    <FullSizeContainer>
      <Message icon="error">{t('plainText.errorOccurred')}</Message>
    </FullSizeContainer>
  ) : (
    <>
      <DocumentTitle
        scopes={[
          zoneInspectionsQuery?.data?.name,
          t('headers.zones'),
          t('headers.inspections'),
          locationName,
        ]}
      />
      <StyledPanelGroup>
        <Panel title={t('headers.inspections')}>
          <PanelContent
            css={{
              // to be able to centralize not found message
              height: '100%',
            }}
          >
            <ZoneInspectionsDataGrid
              inspections={
                // Use previous query data when not fetching or is updating order;
                // initially shows loading spinner, then loader with unchanged data during sort order changes,
                // and finally updates to show new order data.
                !zoneInspectionsQuery.isFetching || isUpdatingOrder
                  ? zoneInspectionsQuery.data?.inspections ?? []
                  : []
              }
              onSelect={setSelectedInspection}
              onArchive={setInspectionToArchive}
              onSortOrderChange={handleSortOrderChange}
              isLoading={zoneInspectionsQuery.isFetching}
            />
          </PanelContent>
        </Panel>
      </StyledPanelGroup>

      {selectedInspection != null && (
        <InspectionModal
          inspection={selectedInspection}
          zoneId={zoneId}
          onClose={handleCloseModal}
          readOnly={false}
        />
      )}
      {inspectionToArchive != null && (
        <ArchiveInspectionModal
          inspection={inspectionToArchive}
          onClose={handleCloseModal}
        />
      )}
    </>
  )
}

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))
