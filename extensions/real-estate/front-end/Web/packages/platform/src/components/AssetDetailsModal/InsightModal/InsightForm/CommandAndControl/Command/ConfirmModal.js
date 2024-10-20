import { Flex, Input, Modal, ModalSubmitButton, Number, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import TextLabel from '../TextLabel/TextLabel'
import Duration from './Duration/Duration'

export default function ConfirmModal({ command, onClose }) {
  const { t } = useTranslation()
  return (
    <Modal
      header={t('headers.reviewYourChange')}
      size="small"
      onClose={onClose}
    >
      {(modal) => (
        <Flex fill="header">
          <Flex size="large" padding="large">
            <TextLabel label={t('labels.currentSetpoint')}>
              <Number value={command.originalValue} format=",.000" />
            </TextLabel>
            <TextLabel label={`Current ${command.type}`}>
              <Text color="green">
                <Number value={command.currentReading} format=",.000" />
                <span> {command.unit}</span>
              </Text>
            </TextLabel>
            <Input
              label={t('labels.newSetpoint')}
              value={command.desiredValue}
              readOnly
            />
            <Duration
              label={t('labels.duration')}
              value={command.duration}
              readOnly
            />
          </Flex>
          <ModalSubmitButton onClick={() => modal.close(true)}>
            {t('plainText.sendCommand')}
          </ModalSubmitButton>
        </Flex>
      )}
    </Modal>
  )
}
