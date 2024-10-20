import { QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function ArchiveInspectionModal({ siteId, zone, onClose }) {
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.post(`/api/sites/${siteId}/zones/${zone.id}/archive`)
  }

  return (
    <QuestionModal
      header={t('headers.archiveZone')}
      question={t('questions.sureToArchive')}
      onSubmit={handleSubmit}
      onSubmitted={(modal) => modal.close('submitted')}
      onClose={onClose}
    >
      "{zone.name}"?
    </QuestionModal>
  )
}
