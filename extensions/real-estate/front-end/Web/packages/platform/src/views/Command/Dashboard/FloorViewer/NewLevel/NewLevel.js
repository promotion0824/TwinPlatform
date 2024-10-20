import { useParams } from 'react-router'
import {
  useFetchRefresh,
  Button,
  FilesSelect,
  Flex,
  Form,
  Icon,
  Input,
  Text,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import styles from './NewLevel.css'

export default function NewLevel({ floor }) {
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const { t } = useTranslation()

  async function handleSubmit(form) {
    await form.api.put(`/api/sites/${params.siteId}/floors/${floor.floorId}`, {
      name: form.data.name,
    })

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

  function handleSubmitted() {
    fetchRefresh('floor')
  }

  return (
    <Flex height="100%" className={styles.container}>
      <Form
        defaultValue={{ ...floor, name: floor.floorName }}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        {(form) => (
          <Flex height="100%">
            <Flex size="medium" align="center middle" padding="large">
              <Text type="message" size="large">
                Start by uploading floor images.
              </Text>
              <Flex
                align="center"
                size="large"
                className={styles.panel}
                padding="large"
              >
                <Input
                  name="name"
                  label={t('labels.floorName')}
                  className={styles.floorName}
                />
                <hr className={styles.hr} />
                <Flex align="center" size="large">
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
                        {t('plainText.newImages')}
                      </Text>
                      <Text type="message" color="grey">
                        {t('plainText.startFromNewImages')}
                      </Text>
                    </Flex>
                  </FilesSelect>
                  {form.data.files?.length > 0 && (
                    <Button
                      type="submit"
                      color="purple"
                      className={styles.upload}
                    >
                      {t('plainText.upload')}
                    </Button>
                  )}
                </Flex>
              </Flex>
            </Flex>
          </Flex>
        )}
      </Form>
    </Flex>
  )
}
