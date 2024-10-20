import { useParams } from 'react-router'
import { useFetchRefresh, QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function DeleteFloorModal({ floor, onClose }) {
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.delete(`/api/sites/${params.siteId}/floors/${floor.id}`)
  }

  function handleSubmitted(modal) {
    modal.closeAll()

    fetchRefresh('floors')
  }

  return (
    <QuestionModal
      header={t('headers.deleteFloor')}
      question={t('questions.sureToDelete')}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
      onClose={onClose}
    >
      "{floor.name}"?
    </QuestionModal>
  )
}
