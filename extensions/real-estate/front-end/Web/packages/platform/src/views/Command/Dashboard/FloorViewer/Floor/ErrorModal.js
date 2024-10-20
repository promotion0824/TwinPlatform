import {
  useFetchRefresh,
  Flex,
  Message,
  Modal,
  ModalSubmitButton,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function ErrorModal({ onClose }) {
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()

  function handleRefreshClick(modal) {
    modal.close()

    fetchRefresh('floor')
  }

  return (
    <Modal
      header={t('headers.error')}
      size="small"
      closeOnClickOutside={false}
      onClose={onClose}
    >
      {(modal) => (
        <Flex fill="header">
          <Flex padding="large">
            <Message icon="error">
              An error has occurred saving the floor.
            </Message>
          </Flex>
          <ModalSubmitButton
            type="button"
            showCancelButton={false}
            onClick={() => handleRefreshClick(modal)}
          >
            {t('plainText.refresh')}
          </ModalSubmitButton>
        </Flex>
      )}
    </Modal>
  )
}
