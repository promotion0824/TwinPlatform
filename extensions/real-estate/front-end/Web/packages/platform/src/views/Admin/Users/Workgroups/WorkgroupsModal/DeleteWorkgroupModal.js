import { useFetchRefresh, QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function DeleteWorkgroupModal({ workgroup, onClose }) {
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.delete(
      `/api/management/sites/${workgroup.siteId}/workgroups/${workgroup.id}`
    )
  }

  function handleSubmitted(modal) {
    modal.closeAll()

    fetchRefresh('workgroups')
  }

  return (
    <QuestionModal
      header={t('headers.deleteWorkgroup')}
      question={t('questions.sureToDelete')}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
      onClose={onClose}
    >
      "{workgroup.name}"?
    </QuestionModal>
  )
}
