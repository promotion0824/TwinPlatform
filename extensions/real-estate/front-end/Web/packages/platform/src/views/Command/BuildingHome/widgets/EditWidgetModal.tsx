import { Button, Group, Modal, Stack } from '@willowinc/ui'
import { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

const Footer = styled(Group)(({ theme }) => ({
  borderTop: `1px solid ${theme.color.neutral.border.default}`,
}))

const EditWidgetModal = ({
  children,
  header,
  onCancel,
  onClose,
  onSave,
  opened,
  ...restProps
}: {
  children: ReactNode
  header: ReactNode
  onClose: () => void
  /** Will only invoke onClose if not provided */
  onCancel?: () => void
  onSave: () => void
  opened: boolean
}) => {
  const { t } = useTranslation()

  return (
    <Modal
      centered
      header={header}
      onClose={() => {
        onCancel?.()
        onClose()
      }}
      opened={opened}
      {...restProps}
    >
      <Stack p="s16">{children}</Stack>
      <Footer justify="flex-end" py="s12" px="s16">
        <Button
          kind="secondary"
          onClick={() => {
            onCancel?.()
            onClose()
          }}
        >
          {t('plainText.cancel')}
        </Button>
        <Button
          type="submit"
          kind="primary"
          onClick={() => {
            onSave()
            onClose()
          }}
        >
          {t('plainText.save')}
        </Button>
      </Footer>
    </Modal>
  )
}

export default EditWidgetModal
