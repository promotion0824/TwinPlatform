import { useState } from 'react'
import { useParams } from 'react-router'
import { useFetchRefresh, QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function DeleteDisciplineModal({ discipline, onClose }) {
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const { t } = useTranslation()

  const [isError, setIsError] = useState(false)
  const [title, setTitle] = useState(t('questions.sureToDelete'))
  const [text, setText] = useState(`${discipline.name}?`)

  function handleSubmit(form) {
    return form.api
      .delete(`/api/sites/${params.siteId}/ModuleTypes/${discipline.id}`)
      .then(() => {
        form.modal.closeAll()
        fetchRefresh('disciplines')
      })
      .catch((e) => {
        setIsError(true)
        setTitle('Error occurred')
        setText('Module type can not be deleted, it has module assignments')
        throw e
      })
  }

  return (
    <QuestionModal
      header={t('headers.deleteDiscipline')}
      question={title}
      onSubmit={handleSubmit}
      onSubmitted={() => {}}
      onClose={onClose}
      submitButtonDisabled={isError}
      FormProps={{
        preventBlockOnSubmitted: true,
        skipErrorSnackbar: true,
      }}
    >
      {text}
    </QuestionModal>
  )
}
