import { useParams } from 'react-router'
import { QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function DeleteTicketCategoryModal({ ticketCategory, onClose }) {
  const params = useParams()
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.delete(
      `/api/sites/${params.siteId}/tickets/categories/${ticketCategory.id}`
    )
  }

  return (
    <QuestionModal
      header={t('headers.deleteTicketCategory')}
      question={t('questions.sureToDelete')}
      onSubmit={handleSubmit}
      onSubmitted={(modal) => modal.close('submitted')}
      onClose={onClose}
    >
      "{ticketCategory.name}"?
    </QuestionModal>
  )
}
