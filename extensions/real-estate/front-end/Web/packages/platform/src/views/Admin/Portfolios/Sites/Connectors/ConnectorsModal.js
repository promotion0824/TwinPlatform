import { useState } from 'react'
import { useParams } from 'react-router'
import {
  useFetchRefresh,
  useSnackbar,
  Flex,
  Form,
  Fieldset,
  Input,
  Modal,
  ModalSubmitButton,
  Select,
  Option,
  useFeatureFlag,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import ConnectorColumns from './ConnectorColumns'
import willowConnectorNames from './willowConnectorNames'

export default function ConnectorsModal({
  connector,
  connectorTypes,
  onClose,
  refetch,
}) {
  const featureFlags = useFeatureFlag()
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const snackbar = useSnackbar()
  const { t } = useTranslation()

  const [selectedTypeColumns, setSelectedTypeColumns] = useState([])

  function handleSubmit(form) {
    const configurationEntries =
      form.data.connectorType === null
        ? []
        : form.data.connectorType.columns
            .filter((column) => form.data[column.name] !== '')
            .map((column) => [column.name, form.data[column.name]])
    const configuration = Object.fromEntries(configurationEntries)

    return form.api.post(`/api/sites/${params.siteId}/connectors`, {
      name: form.data.name,
      connectorTypeId: form.data.connectorType?.id,
      connectionType: form.data.connectionType,
      configuration: JSON.stringify(configuration),
    })
  }

  function handleSubmitted(form) {
    if (form.response?.message != null) {
      snackbar.show(form.response.message, {
        icon: 'ok',
      })
    }

    onClose()

    if (featureFlags?.hasFeatureToggle('connectivityPage')) {
      // After a successful submit, refetch useGetConnectors query to refresh ConnectorTable with the new list of connectors
      refetch()
    } else {
      fetchRefresh('connectors')
    }
  }

  return (
    <Modal header={t('headers.newConnectors')} size="small" onClose={onClose}>
      <Form
        defaultValue={{ ...connector }}
        onSubmit={handleSubmit}
        onSubmitted={handleSubmitted}
      >
        {(form) => (
          <Flex fill="header">
            <div>
              <Fieldset>
                <Input name="name" label={t('labels.name')} required />
                <Select
                  name="connectorType"
                  label={t('labels.connectorType')}
                  unselectable
                  required
                  onChange={(connectorType) => {
                    form.setData((prevData) => ({
                      ...prevData,
                      connectorType,
                    }))

                    setSelectedTypeColumns(connectorType?.columns ?? [])
                  }}
                >
                  {connectorTypes
                    .filter((connectorType) =>
                      willowConnectorNames.includes(connectorType.name)
                    )
                    .map((type) => (
                      <Option key={type.id} value={type}>
                        {type.name}
                      </Option>
                    ))}
                </Select>
                <Select
                  name="connectionType"
                  label={t('labels.connectionType')}
                  unselectable
                  required
                >
                  <Option value="iotedge">{t('plainText.iotEdge')}</Option>
                  <Option value="publicapi">{t('plainText.publicApi')}</Option>
                  <Option value="streamanalyticseventhub">
                    {t('plainText.streamAnalyticEventHub')}
                  </Option>
                  <Option value="streamanalyticsiothub">
                    {t('plainText.streamAnalyticIotHub')}
                  </Option>
                  <Option value="vm">{t('plainText.vmService')}</Option>
                </Select>
                <ConnectorColumns columns={selectedTypeColumns} />
              </Fieldset>
            </div>
            <ModalSubmitButton
              disabled={
                !['name', 'connectorType', 'connectionType'].every(
                  (value) => form.data[value]
                )
              }
            >
              {t('plainText.save')}
            </ModalSubmitButton>
          </Flex>
        )}
      </Form>
    </Modal>
  )
}
