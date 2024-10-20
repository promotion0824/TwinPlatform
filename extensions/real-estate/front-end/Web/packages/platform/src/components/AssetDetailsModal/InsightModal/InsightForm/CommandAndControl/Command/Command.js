import { useState } from 'react'
import {
  useFetchRefresh,
  Button,
  Flex,
  Form,
  ValidationError,
  NotFound,
  Number,
  NumberInput,
  Text,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import TextLabel from '../TextLabel/TextLabel'
import ConfirmModal from './ConfirmModal'
import Duration from './Duration/Duration'

export default function Command({ siteId, command }) {
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()

  const [showConfirmModal, setShowConfirmModal] = useState(false)

  if (command == null) {
    return <NotFound>{t('plainText.noCommandFound')}</NotFound>
  }

  function handleSubmit(form) {
    if (form.data.desiredValue == null) {
      throw new ValidationError({
        name: 'desiredValue',
        message: t('messages.setpointMustGiven'),
      })
    }
    if (
      !form.data.duration.days &&
      !form.data.duration.hours &&
      !form.data.duration.minutes
    ) {
      throw new ValidationError({
        name: 'desiredDuration',
        message: t('messages.durationMustGiven'),
      })
    }

    const desiredDurationMinutes =
      (form.data.duration.days ?? 0) * 24 * 60 +
      (form.data.duration.hours ?? 0) * 60 +
      (form.data.duration.minutes ?? 0)

    return form.api.post(`/api/sites/${siteId}/commands`, {
      insightId: form.data.insightId,
      pointId: form.data.pointId,
      setPointId: form.data.setPointId,
      originalValue: form.data.originalValue,
      desiredValue: form.data.desiredValue,
      desiredDurationMinutes,
    })
  }

  function handleSubmitted() {
    fetchRefresh('insight')
    fetchRefresh('insights')
  }

  return (
    <Form
      defaultValue={{
        ...command,
        duration: {
          days: null,
          hours: null,
          minutes: null,
        },
      }}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
    >
      {(form) => (
        <Flex size="large">
          <TextLabel label={t('labels.currentSetpoint')}>
            <Number value={form.data.originalValue} format=",.000" />
          </TextLabel>
          <TextLabel label={`Current ${form.data.type}`}>
            <Text color="green">
              <Number value={form.data.currentReading} format=",.000" />
              <span> {form.data.unit}</span>
            </Text>
          </TextLabel>
          <NumberInput
            name="desiredValue"
            label={t('labels.newSetpoint')}
            format="0.000"
          />
          <Duration
            name="duration"
            errorName="desiredDuration"
            label={t('labels.duration')}
          />
          <Button
            type="submit"
            color="purple"
            width="medium"
            onClick={(e) => {
              e.preventDefault()

              setShowConfirmModal(true)
            }}
          >
            {t('plainText.sendCommand')}
          </Button>
          {showConfirmModal && (
            <ConfirmModal
              command={form.data}
              onClose={(response) => {
                setShowConfirmModal(false)

                if (response) {
                  form.submit()
                }
              }}
            />
          )}
        </Flex>
      )}
    </Form>
  )
}
