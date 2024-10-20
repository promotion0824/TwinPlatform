import { useFetchRefresh, useUser, QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function DeleteUserModal({ user, onClose }) {
  const currentUser = useUser()
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.delete(
      `/api/management/customers/${currentUser.customer.id}/users/${user.id}`
    )
  }

  function handleSubmitted(modal) {
    modal.closeAll()

    fetchRefresh('users')
  }

  return (
    <QuestionModal
      header={t('headers.deleteUser')}
      question={t('questions.sureToDelete')}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
      onClose={onClose}
    >
      "{user.firstName} {user.lastName}"?
    </QuestionModal>
  )
}
