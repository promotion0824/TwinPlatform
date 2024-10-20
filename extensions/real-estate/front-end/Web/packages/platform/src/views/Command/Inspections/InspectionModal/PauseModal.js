import { useState } from 'react'
import { useForm, DatePicker, Flex, Modal, ModalSubmitButton } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function PauseModal({ check, onClose }) {
  const form = useForm()
  const { t } = useTranslation()

  const [state, setState] = useState(() => ({
    startTime: new Date().toISOString(),
    endTime: null,
  }))

  function handleSubmit(modal) {
    form.setData((prevData) => ({
      ...prevData,
      checks: prevData.checks.map((prevCheck) =>
        prevCheck.localId === check.localId
          ? {
              ...prevCheck,
              isPaused: true,
              pauseStartDate: state.startTime,
              pauseEndDate: state.endTime,
            }
          : prevCheck
      ),
    }))

    modal.close()
  }

  return (
    <Modal header={t('headers.pauseCheck')} size="small" onClose={onClose}>
      {(modal) => (
        <Flex fill="header">
          <Flex padding="large" size="large">
            <DatePicker
              type="date-time"
              label={t('labels.startTime')}
              value={state.startTime}
              onChange={(startTime) => setState({ startTime, endTime: null })}
            />
            <DatePicker
              type="date-time"
              label={t('labels.endTime')}
              min={state.startTime}
              value={state.endTime}
              onChange={(endTime) =>
                setState((prevState) => ({ ...prevState, endTime }))
              }
            />
          </Flex>
          <ModalSubmitButton type="button" onClick={() => handleSubmit(modal)}>
            {t('plainText.setValue')}
          </ModalSubmitButton>
        </Flex>
      )}
    </Modal>
  )
}
