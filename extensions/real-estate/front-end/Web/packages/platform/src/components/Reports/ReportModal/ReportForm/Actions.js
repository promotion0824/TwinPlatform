import { useTranslation } from 'react-i18next'
import {
  useForm,
  Fieldset,
  Flex,
  useApi,
  useSnackbar,
  QuestionModal,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useState } from 'react'

export function ConfirmModal({ onSubmit, onClose, header, question, text }) {
  const questionText = `${question} "${text}"?`
  return (
    <QuestionModal
      header={header}
      question={questionText}
      onSubmit={onSubmit}
      onClose={onClose}
    />
  )
}

function Actions({ report, onClose, getUpdatedData }) {
  const { t } = useTranslation()
  const form = useForm()
  const api = useApi()
  const snackbar = useSnackbar()
  const [showConfirmModal, setShowConfirmModal] = useState(false)

  const handleDeleteClick = () => {
    setShowConfirmModal(true)
  }

  const handleSubmit = async () => {
    try {
      await api.delete(`/api/dashboard/${report.id}?resetLinked=true`)
      onClose(false)
      getUpdatedData()
    } catch (err) {
      snackbar.show(t('plainText.errorOccurred'))
      console.error(err)
    } finally {
      setShowConfirmModal(false)
    }
  }

  return (
    <>
      <Fieldset icon="reset" legend={t('plainText.actions')}>
        {form.data?.id != null && (
          <Flex align="center">
            <Button kind="negative" onClick={handleDeleteClick}>
              {t('plainText.deleteReportConfiguration')}
            </Button>
          </Flex>
        )}
        {showConfirmModal && (
          <ConfirmModal
            onSubmit={handleSubmit}
            onClose={() => {
              setShowConfirmModal(false)
            }}
            header={t('plainText.deleteReportConfiguration')}
            question={t('questions.sureToDelete')}
            text={report.metadata.name}
          />
        )}
      </Fieldset>
    </>
  )
}

export default Actions
