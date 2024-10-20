import { QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function EditConnectorModal({
  request,
  connector,
  onSubmitted,
  onClose,
}) {
  const { t } = useTranslation()
  function handleSubmit(form) {
    return form.api.put(
      `/api/sites/${connector.siteId}/connectors/${connector.id}`,
      request
    )
  }

  function handleSubmitted(modal) {
    modal.close('submitted')
    onSubmitted()
  }

  return (
    <QuestionModal
      header={t('headers.saveConnector')}
      question={t('questions.reviewChange')}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
      onClose={onClose}
    >
      "{connector.name}"?
    </QuestionModal>
  )
}
