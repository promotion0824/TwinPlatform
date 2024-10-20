import { Fetch, useFetchRefresh } from '@willow/ui'
import { siteAdminUserRole } from '@willow/common'
import { Panel, PanelGroup, Tabs } from '@willowinc/ui'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { styled } from 'twin.macro'
import { useSites } from '../../../../providers'
import useCommandAnalytics from '../../useCommandAnalytics.ts'
import ArchiveInspectionModal from '../InspectionModal/ArchiveInspectionModal'
import InspectionModal from '../InspectionModal/InspectionModal.tsx'
import { useInspections } from '../InspectionsProvider'
import InspectionsDataGrid from './InspectionsDataGrid'
import { getInspectionsPageTitle } from '../getPageTitles'

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

function Inspections() {
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const { t } = useTranslation()
  const sites = useSites()
  const [selectedTab, setSelectedTab] = useState(STATUS.due)
  const commandAnalytics = useCommandAnalytics(params.siteId)

  const [selectedInspection, setSelectedInspection] = useState()
  const [inspectionToArchive, setInspectionToArchive] = useState()
  const { setPageTitles } = useInspections()

  useEffect(() => {
    setPageTitles([
      getInspectionsPageTitle({
        siteId: params.siteId,
        title: t('headers.inspections'),
      }),
    ])
  }, [params.siteId, setPageTitles, t])

  useEffect(() => {
    commandAnalytics.pageInspections(selectedTab)
  }, [commandAnalytics, selectedTab])

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

  return (
    <Fetch
      name="inspections"
      url={
        params.siteId
          ? `/api/sites/${params.siteId}/inspections`
          : '/api/inspections'
      }
    >
      {(inspections) => {
        const nextInspections = inspections
          .map((inspection) => {
            const site = sites.find((s) => s.id === inspection.siteId)

            return {
              ...inspection,
              status: getInspectionStatus(inspection.checks),
              siteName: site.name,
              isSiteAdmin: site.userRole === siteAdminUserRole,
            }
          })
          .filter((inspection) => inspection.status !== '-')

        return (
          <>
            <StyledPanelGroup>
              <Panel
                tabs={
                  <Tabs onTabChange={setSelectedTab} value={selectedTab}>
                    <Tabs.List>
                      {tabs.map(({ header, tab, value: id }) => (
                        <Tabs.Tab data-testid={id} key={id} value={tab}>
                          {header}
                        </Tabs.Tab>
                      ))}
                    </Tabs.List>
                    {tabs.map(({ tab, id }) => (
                      <StyledTabsPanel key={id} value={tab}>
                        <InspectionsDataGrid
                          siteId={params.siteId}
                          inspections={nextInspections.filter((inspection) =>
                            tab === STATUS.completed
                              ? inspection.status === STATUS.completed
                              : inspection.status !== STATUS.completed
                          )}
                          isCompletedTab={selectedTab === STATUS.completed}
                          onSelect={setSelectedInspection}
                          onArchive={setInspectionToArchive}
                          showSiteColumn={params.siteId == null}
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
                onClose={() => setSelectedInspection()}
              />
            )}
            {inspectionToArchive != null && (
              <ArchiveInspectionModal
                inspection={inspectionToArchive}
                onClose={(response) => {
                  setInspectionToArchive()

                  if (response === 'submitted') {
                    fetchRefresh('inspections')
                  }
                }}
              />
            )}
          </>
        )
      }}
    </Fetch>
  )
}
const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))

const StyledTabsPanel = styled(Tabs.Panel)({
  height: '100%',
})

export default Inspections
