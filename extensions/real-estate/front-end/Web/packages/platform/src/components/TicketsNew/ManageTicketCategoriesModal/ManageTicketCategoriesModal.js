import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { Fetch, Modal } from '@willow/ui'
import TicketCategoriesForm from './TicketCategoriesForm'

export default function ManageTicketCategoriesModal({ onClose }) {
  const params = useParams()
  const { t } = useTranslation()

  return (
    <Modal
      header={t('headers.ticketCategories')}
      size="medium"
      onClose={onClose}
    >
      <Fetch
        name="ticket-categories"
        url={`/api/sites/${params.siteId}/tickets/categories`}
      >
        {(response) => <TicketCategoriesForm ticketCategories={response} />}
      </Fetch>
    </Modal>
  )
}
