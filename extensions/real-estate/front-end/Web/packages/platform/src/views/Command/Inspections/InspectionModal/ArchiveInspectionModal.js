import { QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function ArchiveInspectionModal({ inspection, onClose }) {
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.post(
      `/api/sites/${inspection.siteId}/inspections/${inspection.id}/archive`
    )
  }

  return (
    <QuestionModal
      header={t('headers.archiveInspection')}
      question={t('questions.sureToArchive')}
      onSubmit={handleSubmit}
      onSubmitted={(modal) => modal.close('submitted')}
      onClose={onClose}
    >
      "{inspection.name}"?
    </QuestionModal>
  )
}
