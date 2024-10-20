import { useState } from 'react'
import { useParams } from 'react-router'
import {
  cookie,
  useFetchRefresh,
  FilesSelect,
  Flex,
  Form,
  ValidationError,
  Icon,
  Modal,
  ModalSubmitButton,
  Text,
  useAnalytics,
} from '@willow/ui'
import { useSites } from 'providers'
import { useTranslation } from 'react-i18next'
import { useFloor } from '../../../../FloorContext'
import UploadError from './UploadError'
import styles from './AddModelModal.css'

export default function AddModelModal({ onClose }) {
  const fetchRefresh = useFetchRefresh()
  const floor = useFloor()
  const params = useParams()
  const { t } = useTranslation()
  const analytics = useAnalytics()
  const sites = useSites()

  const [percentage, setPercentage] = useState()

  function handleSubmit(form) {
    return new Promise((resolve, reject) => {
      const prefix = `/${cookie.get('api')}`

      if (form.data.files.length === 0) {
        throw new ValidationError({
          name: 'files',
          message: t('messages.fileGiven'),
        })
      }

      const formData = new FormData()
      form.data.files.forEach((item) => {
        formData.append('files', item)
      })

      const req = new XMLHttpRequest()
      req.upload.addEventListener(
        'progress',
        (e) => {
          setPercentage((e.loaded / e.total) * 100)
        },
        false
      )
      req.addEventListener('load', () => {
        if (req?.status >= 200 && req?.status < 300) {
          resolve()
        } else {
          let data
          try {
            data = JSON.parse(req?.response)
          } catch (err) {
            // do nothing
          }

          setPercentage()
          reject(
            new UploadError({
              status: req?.status,
              data,
            })
          )
        }
      })
      req.addEventListener('error', () => {
        setPercentage()
        reject()
      })

      req.open(
        'POST',
        `${prefix}/api/sites/${params.siteId}/floors/${floor.floorId}/3dmodules`
      )
      req.send(formData)
    })
  }

  function handleSubmitted(form) {
    form.modal.close()
    fetchRefresh('floor')

    analytics.track('Floor_Model_Added', {
      Site: sites.find((siteObject) => siteObject.id === params.siteId),
      floor_name: floor.floorName,
      page: 'Dashboard Floor 3D',
    })
  }

  return (
    <Modal header={t('headers.addModel')} size="small" onClose={onClose}>
      <Form onSubmit={handleSubmit} onSubmitted={handleSubmitted}>
        <Flex fill="header">
          <Flex padding="large">
            <FilesSelect
              name="files"
              accept=".nwd,.obj"
              align="center"
              buttonClassName={styles.filesSelectButton}
              buttonContentClassName={styles.filesSelectButtonContent}
              contentClassName={styles.filesSelectContent}
            >
              <Flex align="center middle" size="tiny">
                <Icon icon="image" className={styles.icon} />
                <Text type="message" size="large">
                  {t('plainText.addModels')}
                </Text>
                <Text type="message" color="grey">
                  {t('plainText.addMoreFloorModels')}
                </Text>
              </Flex>
            </FilesSelect>
            {percentage != null && (
              <span
                className={styles.progress}
                style={{ width: `${percentage}%` }}
              />
            )}
          </Flex>
          <ModalSubmitButton>{t('plainText.add')}</ModalSubmitButton>
        </Flex>
      </Form>
    </Modal>
  )
}
