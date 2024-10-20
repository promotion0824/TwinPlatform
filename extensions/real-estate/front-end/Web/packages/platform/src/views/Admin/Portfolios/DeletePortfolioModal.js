import { useFetchRefresh, useUser, QuestionModal } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function DeletePortfolioModal({ portfolio, onClose }) {
  const fetchRefresh = useFetchRefresh()
  const user = useUser()
  const { t } = useTranslation()

  function handleSubmit(form) {
    return form.api.delete(
      `/api/customers/${user.customer.id}/portfolios/${portfolio.portfolioId}`
    )
  }

  function handleSubmitted(modal) {
    modal.closeAll()

    fetchRefresh('portfolios')
  }

  return (
    <QuestionModal
      header={t('headers.deletePortfolio')}
      question={t('questions.sureToDelete')}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
      onClose={onClose}
    >
      "{portfolio.portfolioName}"?
    </QuestionModal>
  )
}
