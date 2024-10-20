import {
  useFetchRefresh,
  Fieldset,
  Flex,
  Form,
  ValidationError,
  Input,
  Modal,
  ModalSubmitButton,
  Select,
  Option,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function RequestorsModal({ requestor, sites, ...rest }) {
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()

  const isNewRequestor = requestor.id == null

  function handleSubmit(form) {
    if (form.data.siteId == null) {
      throw new ValidationError({
        name: 'siteId',
        message: t('messages.siteGiven'),
      })
    }

    if (!isNewRequestor) {
      return form.api.put(
        `/api/sites/${form.data.siteId}/persons/${requestor.id}`,
        {
          company: form.data.company ?? '',
          contactNumber: form.data.contact,
          email: form.data.email,
          fullName: form.data.name,
          type: 'reporter',
        }
      )
    }

    return form.api.post(`/api/sites/${form.data.siteId}/persons`, {
      company: form.data.company ?? '',
      contactNumber: form.data.contact,
      email: form.data.email,
      fullName: form.data.name,
      type: 'reporter',
    })
  }

  function handleSubmitted(form) {
    form.modal.close()

    fetchRefresh('requestors')
  }

  return (
    <Modal
      header={
        isNewRequestor
          ? t('plainText.addRequestor')
          : t('plainText.editRequestor')
      }
      size="small"
      {...rest}
    >
      <Form
        defaultValue={requestor}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        <Flex fill="header">
          <Fieldset legend={t('plainText.generalInfo')}>
            <Select
              name="siteId"
              label={t('labels.site')}
              required
              readOnly={!isNewRequestor}
            >
              {sites.map((site) => (
                <Option key={site.id} value={site.id}>
                  {site.name}
                </Option>
              ))}
            </Select>
            <Input
              name="name"
              errorName="fullName"
              label={t('labels.name')}
              required
            />
            <Input name="email" label={t('labels.emailAddress')} required />
            <Input
              name="contact"
              errorName="contactNumber"
              label={t('labels.contact')}
              required
            />
            <Input name="company" label={t('labels.company')} />
          </Fieldset>
          <ModalSubmitButton>{t('plainText.save')}</ModalSubmitButton>
        </Flex>
      </Form>
    </Modal>
  )
}
