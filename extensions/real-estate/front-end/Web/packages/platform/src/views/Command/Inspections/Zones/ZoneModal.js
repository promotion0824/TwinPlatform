import {
  useFetchRefresh,
  Fieldset,
  Flex,
  Form,
  Input,
  Modal,
  ModalSubmitButton,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import useCommandAnalytics from '../../useCommandAnalytics.ts'

export default function ZoneModal({ zone, onClose, siteId }) {
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()
  const commandAnalytics = useCommandAnalytics(siteId)

  const isNewZone = zone?.id == null

  function handleSubmit(form) {
    commandAnalytics.trackInspectionsSaveZone()

    if (!isNewZone) {
      return form.api.put(
        `/api/sites/${siteId}/inspectionZones/${form.data.id}`,
        {
          name: form.data.name,
        }
      )
    }

    return form.api.post(`/api/sites/${siteId}/inspectionZones`, {
      name: form.data.name,
    })
  }

  function handleSubmitted(form) {
    form.modal.close()

    fetchRefresh('zones')
  }

  return (
    <Modal
      header={isNewZone ? t('plainText.addZone') : t('plainText.zone')}
      size="small"
      onClose={onClose}
    >
      <Form
        defaultValue={zone}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        <Flex fill="header">
          <Flex>
            <Fieldset padding="large">
              <Input name="name" label={t('labels.name')} />
            </Fieldset>
          </Flex>
          <ModalSubmitButton>{t('plainText.save')}</ModalSubmitButton>
        </Flex>
      </Form>
    </Modal>
  )
}
