import _ from 'lodash'
import { useEffect, useState } from 'react'
import { Redirect } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { styled } from 'twin.macro'

import {
  QuestionModal,
  useApi,
  useFetchRefresh,
  useScopeSelector,
} from '@willow/ui'
import { qs } from '@willow/common'
import { useQuery } from 'react-query'
import { Button, Icon, Panel, PanelGroup, Tabs } from '@willowinc/ui'
import SchedulesDataGrid from '../../BuildingHome/Tickets/SchedulesDataGrid'

import useCommandAnalytics from '../../useCommandAnalytics.ts'
import { ScheduleModalProvider } from './ScheduleModal/Hooks/ScheduleModalProvider'
import ScheduleModal from './ScheduleModal/ScheduleModal'

const StyledPanelGroup = styled(PanelGroup)(({ theme }) => ({
  padding: theme.spacing.s16,
}))

export default function Schedules({ redirect }) {
  const { location, isScopeSelectorEnabled } = useScopeSelector()
  const params = useParams()
  const { t } = useTranslation()
  const siteIdToQuerySchedules =
    isScopeSelectorEnabled && location?.twin?.siteId
      ? location.twin.siteId
      : params.siteId

  const [tab, setTab] = useState('active')
  const [selectedScheduleId, setSelectedScheduleId] = useState()
  const [isQuestionModalVisible, setIsQuestionModalVisible] = useState(false)
  const [scheduleToArchive, setScheduleToArchive] = useState({})
  const [isReadOnly, setIsReadOnly] = useState(false)
  const [modalData, setModalData] = useState({})
  const api = useApi()
  const fetchRefresh = useFetchRefresh()
  const commandAnalytics = useCommandAnalytics(siteIdToQuerySchedules)

  function handleArchive(scheduleRow) {
    setScheduleToArchive(scheduleRow)
    const { recurrence } = scheduleRow
    const endDate = new Date().toISOString()
    api.put(
      `/api/sites/${siteIdToQuerySchedules}/tickettemplate/${scheduleRow.id}`,
      {
        ...scheduleRow,
        recurrence: {
          ...recurrence,
          endDate,
        },
        status: 'closed',
      }
    )

    commandAnalytics.trackTicketsArchiveSchedules(scheduleRow)
    fetchRefresh('schedules')
  }

  function handleShowModal(e, schedule) {
    e.stopPropagation()
    setIsQuestionModalVisible(true)
    setModalData(schedule)
  }

  useEffect(() => {
    commandAnalytics.pageTickets('schedules')
  }, [commandAnalytics])

  useEffect(() => {
    commandAnalytics.pageTickets('schedules', tab)
  }, [commandAnalytics, tab])

  const isArchived = tab === 'archived'

  const { status, data, refetch } = useQuery(
    ['schedules', siteIdToQuerySchedules, isArchived, modalData],
    async () => {
      const response = await api.get(
        qs.createUrl(`/api/sites/${siteIdToQuerySchedules}/tickettemplate`, {
          archived: isArchived,
        })
      )
      return response
    }
  )
  if (redirect) {
    return <Redirect to={redirect} />
  }

  return (
    <>
      <StyledPanelGroup>
        <Panel
          tabs={
            <Tabs onTabChange={setTab} value={tab}>
              <Tabs.List>
                <Tabs.Tab data-testid="schedules-tab-active" value="active">
                  {t('headers.active')}
                </Tabs.Tab>
                <Tabs.Tab data-testid="schedules-tab-archived" value="archived">
                  {t('headers.archived')}
                </Tabs.Tab>
              </Tabs.List>

              <Tabs.Panel value="active">
                <SchedulesDataGrid
                  onShowModal={handleShowModal}
                  onSelectedScheduleId={setSelectedScheduleId}
                  schedules={
                    data?.filter(
                      (schedule) => schedule.id !== scheduleToArchive.id
                    ) ?? []
                  }
                  status={status}
                  isArchived={isArchived}
                />
                {isQuestionModalVisible && (
                  <QuestionModal
                    onClose={() => {
                      setIsQuestionModalVisible(false)
                    }}
                    header={t('headers.archiveSchedule')}
                    question={`${t('questions.sureToArchive')} ${
                      modalData.description
                    }?`}
                    onSubmitted={(modal) => {
                      modal.close('submitted')
                    }}
                    onSubmit={() => handleArchive(modalData)}
                  />
                )}
              </Tabs.Panel>

              <Tabs.Panel value="archived">
                <SchedulesDataGrid
                  onShowModal={handleShowModal}
                  onSelectedScheduleId={setSelectedScheduleId}
                  schedules={data ?? []}
                  status={status}
                  isArchived={isArchived}
                />
              </Tabs.Panel>
            </Tabs>
          }
        />
      </StyledPanelGroup>

      {selectedScheduleId != null && (
        <ScheduleModalProvider>
          <ScheduleModal
            siteId={siteIdToQuerySchedules}
            scheduleId={selectedScheduleId}
            isReadOnly={isReadOnly}
            onClose={() => {
              setSelectedScheduleId()
              setIsReadOnly(false)
            }}
          />
        </ScheduleModalProvider>
      )}
    </>
  )
}

export const SchedulesControls = ({ siteId }) => {
  const { t } = useTranslation()
  const [scheduleModalOpen, setScheduleModalOpen] = useState(false)
  return (
    <>
      <Button
        prefix={<Icon icon="add" />}
        onClick={() => setScheduleModalOpen(true)}
        data-segment="New schedule"
        data-testid="new-schedule-button"
      >
        {t('plainText.addSchedule')}
      </Button>
      {scheduleModalOpen && (
        <ScheduleModalProvider>
          <ScheduleModal
            siteId={siteId}
            scheduleId="new"
            isReadOnly={false}
            onClose={() => {
              setScheduleModalOpen(false)
            }}
          />
        </ScheduleModalProvider>
      )}
    </>
  )
}
