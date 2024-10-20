import { useEffect, useState } from 'react'
import { Portal, Flex, Button, useModal, useForm } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useScheduleModal } from '../Hooks/ScheduleModalContext'

export default function PushScheduledTicketSubmitButton() {
  const modal = useModal()
  const { t } = useTranslation()
  const {
    isPushScheduledTickets,
    setIsPushScheduledTickets,
    isFutureStartDate,
  } = useScheduleModal()
  const { submit } = useForm()
  const [loaded, setLoaded] = useState(false)

  useEffect(() => {
    setLoaded(true)
  }, [])

  useEffect(() => {
    // Condition prevents calling submit immediately
    if (loaded)
      /**
       * Once submit is called, state is reseted (ie. isPushScheduledTickets is set to undefined).
       * This is handled by handleSubmitted in ../ScheduleForm.js
       *  */
      submit()
  }, [isPushScheduledTickets])

  return (
    <Portal target={modal.modalSubmitButtonRef}>
      <Flex horizontal size="small">
        <Button
          color="transparent"
          type={!isPushScheduledTickets ? 'submit' : undefined}
          onClick={() => {
            setIsPushScheduledTickets(false)
          }}
        >
          {t('plainText.dontAllow')}
        </Button>

        <Button
          color="purple"
          type={isPushScheduledTickets ? 'submit' : undefined}
          onClick={() => {
            setIsPushScheduledTickets(!isFutureStartDate ?? true)
          }}
        >
          {t('plainText.confirm')}
        </Button>
      </Flex>
    </Portal>
  )
}
