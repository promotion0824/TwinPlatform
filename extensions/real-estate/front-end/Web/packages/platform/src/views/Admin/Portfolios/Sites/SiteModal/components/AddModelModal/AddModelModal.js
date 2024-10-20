import { useState, useEffect } from 'react'
import { Flex, Form, Modal, ModalSubmitButton, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import styles from './AddModelModal.css'
import {
  TwinFilesSelect,
  ProgressBar,
  TwinImageIcon,
} from './styled-components/index'

/**
 * Known issue:
 * 1. Abiguity on Delete action after overwriting a file.
 * 2. It takes some time to retrieve updated data after uploading/deleting due to AutoDesk process behind.
 * 2.1 Required action: take around few secs until AutoDesk update file information
 *
 * TODO: https://dev.azure.com/willowdev/Unified/_workitems/edit/53529/
 * 1. Replace the components that do not use TW
 * 2. Remove css file
 */
export default function AddModelModal({
  onClose,
  onChange,
  onSubmit,
  onSubmitted,
  percentage = 0,
  errorMessage,
  successful = false,
  loading = false,
  value,
}) {
  const { t } = useTranslation()
  const [file, setFile] = useState()

  useEffect(() => {
    setFile(value ? [value] : [])
  }, [value])

  /**
   * FileSelect does not provide accessible key when multiple prop is false
   * TODO: FileSelect should provide meaningful key, https://dev.azure.com/willowdev/Unified/_workitems/edit/53531
   */
  const handleSubmit = ({ data }) => {
    const [submitData] = Object.values(data)
    onSubmit(submitData)
  }

  const handleChange = (data) => {
    setFile(data)
    onChange(data)
  }

  return (
    <Modal header={t('headers.addModel')} size="small" onClose={onClose}>
      <Form
        onSubmit={handleSubmit}
        onSubmitted={onSubmitted}
        preventBlockOnSubmitted
      >
        <Flex fill="header">
          <Flex padding="large">
            <TwinFilesSelect
              name="file"
              accept=".nwd,.obj"
              align="center"
              multiple={false}
              buttonClassName={styles.filesSelectButton}
              buttonContentClassName={styles.filesSelectButtonContent}
              contentClassName={styles.filesSelectContent}
              error={errorMessage}
              onChange={handleChange}
              value={file}
            >
              <Flex align="center middle" size="tiny">
                <TwinImageIcon />
                <Text type="message" size="large">
                  {t('plainText.addAModel')}
                </Text>
              </Flex>
            </TwinFilesSelect>
            {percentage !== null && <ProgressBar percentage={percentage} />}
          </Flex>
          <ModalSubmitButton loading={loading} successful={successful}>
            {t('plainText.add')}
          </ModalSubmitButton>
        </Flex>
      </Form>
    </Modal>
  )
}
