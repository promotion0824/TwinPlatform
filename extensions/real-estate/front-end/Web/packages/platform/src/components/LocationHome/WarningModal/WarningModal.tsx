import { Modal, Stack, Group, Button } from '@willowinc/ui'
import { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'

export interface WarningModalProps {
  opened: boolean
  onClose: () => void
  /**
   * Click cancel button will invoke `onCancel` if provided,
   * and will always invoke `onClose`.
   */
  onCancel?: () => void
  /**
   * Click warning confirmation button will invoke
   * `onWarningConfirm` and `onClose`.
   */
  onWarningConfirm: () => void
  header?: ReactNode
  children: ReactNode
  confirmationButtonLabel: string
}

/**
 * A pre structure Modal component with header, warning content, cancel button
 * and a warning confirmation button.
 */
const WarningModal = ({
  opened,
  onClose,
  onCancel,
  onWarningConfirm,
  header,
  children,
  confirmationButtonLabel,
}: WarningModalProps) => {
  const { t } = useTranslation()

  return (
    <Modal
      opened={opened}
      onClose={() => {
        onCancel?.()
        onClose()
      }}
      header={header ?? t('headers.warning')}
      centered
    >
      <Stack p="s16" gap="s16">
        {children}
        <Group justify="flex-end">
          <Button
            kind="secondary"
            onClick={() => {
              onCancel?.()
              onClose()
            }}
            css={{
              textTransform: 'capitalize',
            }}
          >
            {t('plainText.cancel')}
          </Button>
          <Button
            kind="negative"
            onClick={() => {
              onWarningConfirm()
              onClose()
            }}
            css={{
              textTransform: 'capitalize',
            }}
          >
            {confirmationButtonLabel}
          </Button>
        </Group>
      </Stack>
    </Modal>
  )
}

export default WarningModal
