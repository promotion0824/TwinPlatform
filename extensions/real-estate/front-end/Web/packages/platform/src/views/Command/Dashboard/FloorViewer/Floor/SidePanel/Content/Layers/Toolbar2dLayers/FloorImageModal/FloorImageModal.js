import { useParams } from 'react-router'
import {
  useFetchRefresh,
  FilesSelect,
  Flex,
  Form,
  Icon,
  Modal,
  ModalSubmitButton,
  Text,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useFloor } from '../../../../../FloorContext'
import styles from './FloorImageModal.css'

export default function FloorImageModal({ onClose }) {
  const fetchRefresh = useFetchRefresh()
  const floor = useFloor()
  const params = useParams()
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.post(
      `/api/sites/${params.siteId}/floors/${floor.floorId}/2dmodules`,
      {
        files: form.data.files,
      },
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    )
  }

  function handleSubmitted(form) {
    form.modal.close()

    fetchRefresh('floor')
  }

  return (
    <Modal header={t('headers.addFloorImages')} size="small" onClose={onClose}>
      <Form onSubmit={handleSubmit} onSubmitted={handleSubmitted}>
        <Flex fill="header">
          <Flex padding="large">
            <FilesSelect
              name="files"
              align="center"
              buttonClassName={styles.filesSelectButton}
              buttonContentClassName={styles.filesSelectButtonContent}
              contentClassName={styles.filesSelectContent}
            >
              <Flex align="center middle" size="tiny">
                <Icon icon="image" className={styles.icon} />
                <Text type="message" size="large">
                  {t('plainText.addImages')}
                </Text>
                <Text type="message" color="grey">
                  {t('plainText.addMoreFloorImages')}
                </Text>
              </Flex>
            </FilesSelect>
          </Flex>
          <ModalSubmitButton>{t('plainText.add')}</ModalSubmitButton>
        </Flex>
      </Form>
    </Modal>
  )
}
