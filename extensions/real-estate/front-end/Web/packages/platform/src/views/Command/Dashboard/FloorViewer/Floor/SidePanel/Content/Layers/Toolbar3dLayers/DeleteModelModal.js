import { useParams } from 'react-router'
import { useApi, useFetchRefresh, QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useFloor } from '../../../../FloorContext'

export default function DeleteModelModal({ layer, onClose }) {
  const api = useApi()
  const fetchRefresh = useFetchRefresh()
  const floor = useFloor()
  const params = useParams()
  const { t } = useTranslation()

  function handleSubmit() {
    return api.delete(
      `/api/sites/${params.siteId}/floors/${params.floorId}/module/${layer.id}`
    )
  }

  function handleSubmitted(modal) {
    modal.close(true)

    floor.removeModel(layer.id)

    fetchRefresh('floor')
  }

  return (
    <QuestionModal
      header={t('headers.deleteModel')}
      question={t('questions.sureToDelete')}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
      onClose={onClose}
    >
      "{layer.name}"
    </QuestionModal>
  )
}
