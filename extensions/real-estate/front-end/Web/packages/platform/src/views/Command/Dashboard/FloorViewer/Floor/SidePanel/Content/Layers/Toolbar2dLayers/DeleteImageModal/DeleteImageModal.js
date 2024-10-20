import { QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useFloor } from '../../../../../FloorContext'

export default function DeleteImageModal({ image, onClose }) {
  const floor = useFloor()
  const { t } = useTranslation()

  function handleSubmitted(modal) {
    modal.close(true)
    floor.removeImage(image.id)
  }

  return (
    <QuestionModal
      header={t('headers.deleteItem')}
      question={t('questions.sureToDelete')}
      onSubmitted={handleSubmitted}
      onClose={onClose}
    >
      "{image.name}"
    </QuestionModal>
  )
}
