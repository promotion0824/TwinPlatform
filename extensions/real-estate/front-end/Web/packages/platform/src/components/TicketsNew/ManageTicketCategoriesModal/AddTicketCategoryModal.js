import { useParams } from 'react-router'
import {
  useFetchRefresh,
  Flex,
  Form,
  ValidationError,
  Input,
  Modal,
  ModalSubmitButton,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function AddTicketCategoryModal({
  ticketCategory,
  ticketCategories,
  onClose,
}) {
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const { t } = useTranslation()

  const isEditMode = !!ticketCategory.id

  async function handleSubmit(form) {
    if (form.data.name.trim() === '') {
      throw new ValidationError({
        name: 'name',
        message: t('messages.nameRequired'),
      })
    }

    const hasExistingName = ticketCategories.find(
      (existingCheck) =>
        existingCheck.name.toLowerCase() === form.data.name.toLowerCase()
    )

    if (hasExistingName) {
      throw new ValidationError({
        name: 'name',
        message: t('messages.nameUnique'),
      })
    }

    if (isEditMode) {
      await form.api.put(
        `/api/sites/${params.siteId}/tickets/categories/${form.data.id}`,
        {
          name: form.data.name,
        }
      )
    } else {
      await form.api.post(`/api/sites/${params.siteId}/tickets/categories`, {
        name: form.data.name,
      })
    }

    fetchRefresh('ticket-categories')
  }

  return (
    <Modal
      header={
        isEditMode
          ? t('headers.editTicketCategory')
          : t('plainText.addTicketCategory')
      }
      size="small"
      onClose={onClose}
    >
      <Form
        defaultValue={ticketCategory}
        onSubmit={handleSubmit}
        onSubmitted={(form) => form.modal.close()}
      >
        <Flex fill="header">
          <Flex size="large" padding="large">
            <Input name="name" label={t('labels.name')} required />
          </Flex>
          <ModalSubmitButton>{t('plainText.save')}</ModalSubmitButton>
        </Flex>
      </Form>
    </Modal>
  )
}
