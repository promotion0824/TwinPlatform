import { useState, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Fetch, Modal } from '@willow/ui'
import ScheduleForm from './ScheduleForm'
import { useScheduleModal } from './Hooks/ScheduleModalContext'

export default function ScheduleModal({
  siteId,
  scheduleId: scheduleIdProp,
  isReadOnly,
  onClose,
}) {
  const { t } = useTranslation()
  const [scheduleId, setScheduleId] = useState(scheduleIdProp)
  const [modalProps, setModalProps] = useState(null)
  const { showPushScheduledTicket, isFutureStartDate } = useScheduleModal()

  /**
   * 2-step form modal
   * Display 2nd step when you're in an existing scheduled, added new assets, and have clicked on 'Confirm' button.
   */
  useEffect(() => {
    const pushScheduledTicketModalProps = {
      header: t('plainText.pushScheduledTickets'),
      size: 'medium',
      icon: 'info',
      iconColor: 'purple',
    }

    const scheduleModalProps = {
      header: t('headers.schedules'),
      size: 'medium',
    }

    setModalProps(
      !isFutureStartDate && showPushScheduledTicket
        ? pushScheduledTicketModalProps
        : scheduleModalProps
    )
  }, [showPushScheduledTicket, isFutureStartDate])

  return (
    <Modal {...modalProps} onClose={onClose}>
      <Fetch
        name="schedule"
        url={
          scheduleId !== 'new'
            ? `/api/sites/${siteId}/tickettemplate/${scheduleId}`
            : undefined
        }
      >
        {(schedule) => {
          const nextSchedule =
            schedule != null
              ? {
                  ...schedule,
                  assets: schedule.assets.map((asset) => ({
                    id: asset.id,
                    name: asset.assetName,
                  })),
                  recurrence: {
                    ...schedule.recurrence,
                    endDate:
                      schedule.recurrence.endDate !== ''
                        ? schedule.recurrence.endDate
                        : null,
                  },
                }
              : null

          return (
            <ScheduleForm
              siteId={siteId}
              schedule={nextSchedule}
              onScheduleIdChange={setScheduleId}
              isReadOnly={isReadOnly}
              scheduleId={scheduleId}
            />
          )
        }}
      </Fetch>
    </Modal>
  )
}
