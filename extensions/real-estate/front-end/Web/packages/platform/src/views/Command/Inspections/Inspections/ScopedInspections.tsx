import { useQuery, useQueryClient } from 'react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'

import { siteAdminUserRole } from '@willow/common'
import { Message, useScopeSelector } from '@willow/ui'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import FullSizeLoader from '@willow/common/components/FullSizeLoader'
import { Panel, PanelGroup, Tabs } from '@willowinc/ui'

import ArchiveInspectionModal from '../InspectionModal/ArchiveInspectionModal'
import InspectionModal from '../InspectionModal/InspectionModal'
import InspectionsDataGrid from './InspectionsDataGrid'
import { useSites } from '../../../../providers'
import {
  Inspection,
  getScopedInspections,
} from '../../../../services/Inspections/InspectionsServices'

const STATUS = {
  due: 'due',
  completed: 'completed',
  overdue: 'overdue',
}

const getInspectionStatus = (checks) => {
  if (
    checks.some((check) => check.statistics.workableCheckStatus === STATUS.due)
  ) {
    return STATUS.due
  } else if (
    checks.some(
      (check) => check.statistics.workableCheckStatus === STATUS.overdue
    )
  ) {
    return STATUS.overdue
  } else if (
    checks.length > 0 &&
    checks.every(
      (check) => check.statistics.workableCheckStatus === STATUS.completed
    )
  ) {
    return STATUS.completed
  }

  return '-'
}

/**
 * This is the Inspection component incorporating the scope selector feature.
 *
 * TODO: remove packages\platform\src\views\Command\Inspections\Inspections\Inspections.js
 * once scope selector feature is complete.
 */
function ScopedInspections({ scopeId }: { scopeId?: string }) {
  const [{ tab = STATUS.due }, setSearchParam] = useMultipleSearchParams([
    'tab',
  ])
  const queryClient = useQueryClient()
  const { t } = useTranslation()
  const sites = useSites()
  const { location, isScopeUsedAsBuilding } = useScopeSelector()

  const isBuildingTwin = isScopeUsedAsBuilding(location)

  const [selectedInspection, setSelectedInspection] = useState<
    Inspection | undefined
  >()
  const [inspectionToArchive, setInspectionToArchive] = useState<
    Inspection | undefined
  >()

  const scopedInspectionsQuery = useQuery(
    ['scopedInspections', scopeId],
    () => getScopedInspections(scopeId),
    {
      select: (inspections) =>
        inspections
          .map((inspection) => {
            const site = sites.find((s) => s.id === inspection.siteId)

            return {
              ...inspection,
              status: getInspectionStatus(inspection.checks),
              siteName: site?.name,
              isSiteAdmin: site?.userRole === siteAdminUserRole,
            }
          })
          .filter((inspection) => inspection.status !== '-'),
    }
  )

  const tabs = [
    {
      header: t('headers.dueInspections'),
      tab: STATUS.due,
      id: 'inspections-tab-due',
    },
    {
      header: t('headers.completed'),
      tab: STATUS.completed,
      id: 'inspections-tab-completed',
    },
  ]

  return scopedInspectionsQuery.isError ? (
    <Message tw="h-full" icon="error">
      {t('plainText.errorOccurred')}
    </Message>
  ) : scopedInspectionsQuery.isLoading ? (
    <FullSizeLoader />
  ) : (
    <>
      <StyledPanelGroup>
        <Panel
          tabs={
            <Tabs value={tab as string}>
              <Tabs.List>
                {tabs.map(({ header, tab, id }) => (
                  <Tabs.Tab
                    data-testid={id}
                    key={id}
                    value={tab}
                    onClick={() =>
                      setSearchParam({
                        tab,
                      })
                    }
                  >
                    {header}
                  </Tabs.Tab>
                ))}
              </Tabs.List>
              {tabs.map(({ tab, id }) => (
                <StyledTabsPanel key={id} value={tab}>
                  <InspectionsDataGrid
                    inspections={(scopedInspectionsQuery.data ?? []).filter(
                      (inspection) =>
                        tab === STATUS.completed
                          ? inspection.status === STATUS.completed
                          : inspection.status !== STATUS.completed
                    )}
                    onSelect={setSelectedInspection}
                    onArchive={setInspectionToArchive}
                    showSiteColumn={!isBuildingTwin}
                    isCompletedTab={tab === STATUS.completed}
                  />
                </StyledTabsPanel>
              ))}
            </Tabs>
          }
        />
      </StyledPanelGroup>
      {selectedInspection && (
        <InspectionModal
          inspection={selectedInspection}
          onClose={() => setSelectedInspection(undefined)}
          readOnly={false}
        />
      )}
      {inspectionToArchive != null && (
        <ArchiveInspectionModal
          inspection={inspectionToArchive}
          onClose={(response) => {
            setInspectionToArchive(undefined)

            if (response === 'submitted') {
              queryClient.invalidateQueries('scopedInspections')
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

const StyledTabsPanel = styled(Tabs.Panel)({
  height: '100%',
})

export default ScopedInspections
