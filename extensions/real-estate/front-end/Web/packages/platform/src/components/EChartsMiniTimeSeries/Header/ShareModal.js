import { useSnackbar, Flex, Input, Modal, ModalSubmitButton } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function ShareModal({ onClose, 'data-segment': dataSegment }) {
  const snackbar = useSnackbar()
  const { t } = useTranslation()

  function handleClick() {
    navigator.clipboard.writeText(window.location.href)

    snackbar.show(t('plainText.linkCopied'), {
      icon: 'ok',
    })
  }

  return (
    <Modal
      header={t('headers.shareTheCurrentView')}
      size="small"
      onClose={onClose}
    >
      <Flex size="medium" padding="large">
        <span>Here's a link to your view</span>
        <Input value={window.location.href} readOnly />
      </Flex>
      <ModalSubmitButton
        type="button"
        onClick={handleClick}
        data-segment={dataSegment}
      >
        {t('plainText.copyLink')}
      </ModalSubmitButton>
    </Modal>
  )
}
