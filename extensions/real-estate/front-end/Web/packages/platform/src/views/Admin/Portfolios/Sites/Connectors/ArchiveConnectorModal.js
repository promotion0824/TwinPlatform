import { QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function ArchiveConnectorModal({
  request,
  connector,
  onSubmitted,
  onClose,
}) {
  const { t } = useTranslation()
  function handleSubmit(form) {
    return form.api.put(
      `/api/sites/${connector.siteId}/connectors/${connector.id}/isArchived`,
      request
    )
  }

  function handleSubmitted(modal) {
    modal.close('submitted')
    onSubmitted()
  }

  return (
    <QuestionModal
      header={t('headers.warning')}
      question={t('questions.sureToArchiveConnector')}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
      onClose={onClose}
      submitText={t('plainText.archiveConnector')}
    >
      "{connector.name}"?
    </QuestionModal>
  )
}
