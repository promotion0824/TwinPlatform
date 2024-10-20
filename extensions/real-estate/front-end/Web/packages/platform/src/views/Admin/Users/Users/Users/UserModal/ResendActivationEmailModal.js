import { useUser, QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function ResendActivationEmailModal({ user, onClose }) {
  const currentUser = useUser()
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.post(
      `/api/customers/${currentUser.customer.id}/users/${user.id}/sendActivation`
    )
  }

  function handleSubmitted(modal) {
    modal.close()
  }

  return (
    <QuestionModal
      header={t('headers.resendActivationEmail')}
      question={t('questions.sureToResend')}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
      onClose={onClose}
    >
      "{user.email}"?
    </QuestionModal>
  )
}
