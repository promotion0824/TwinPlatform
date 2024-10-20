import { useState } from 'react'
import cx from 'classnames'
import {
  useApi,
  useFetchRefresh,
  useSnackbar,
  Checkbox,
  Flex,
  Footer,
  Form,
  Input,
  Label,
  NumberInput,
  useFeatureFlag,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import _ from 'lodash'
import ArchiveConnectorModal from '../ArchiveConnectorModal'
import Toggle from '../Toggle/Toggle'
import ConnectorColumns from '../ConnectorColumns'
import InputWithButton from '../InputWithButton/InputWithButton'
import EditConnectorModal from '../EditConnectorModal'
import willowConnectorNames from '../willowConnectorNames'
import ConnectorHeader from './ConnectorHeader'
import styles from './ConnectorConfiguration.css'

export default function ConnectorConfiguration({
  connector = undefined,
  connectorType = undefined,
  connectorTypeColumns = undefined,
  setExpandedConnector = undefined,
  expanded = undefined,
  className = undefined,
  invalidateConnectorQueries = undefined,
  ...rest
}) {
  const featureFlags = useFeatureFlag()
  const api = useApi()
  const fetchRefresh = useFetchRefresh()
  const snackbar = useSnackbar()
  const { t } = useTranslation()

  const [password, setPassword] = useState('')
  const [passwordIsGenerating, setPasswordIsGenerating] = useState(false)
  const [isEnabled, setIsEnabled] = useState(connector.isEnabled ?? false)
  const [editConnectorRequest, setEditConnectorRequest] = useState()
  const [isArchived, setIsArchived] = useState(false)
  const [isNameDefined, setIsNameDefined] = useState(true)

  const cxClassName = cx(
    styles.expandablePanel,
    {
      [styles.isOpen]: expanded,
    },
    className
  )

  let connectorConfiguration = {}
  try {
    connectorConfiguration = JSON.parse(connector.configuration)
  } catch (err) {
    // do nothing
  }

  // Add connectorType to connector object
  connector.connectorType = connectorType

  function handleArchiveConnection() {
    setIsArchived(true)
  }

  function handleHeaderClick() {
    if (featureFlags?.hasFeatureToggle('connectivityPage')) {
      return
    }
    setExpandedConnector(!expanded ? connector : null)
  }

  function handleArchiveConnectorSubmitted() {
    setExpandedConnector()
    if (featureFlags?.hasFeatureToggle('connectivityPage')) {
      // After successful edit,
      // - Invalidate connectorQuery, so changes are persistent
      //   when you go back and forth from ConnectorsTables to ConnectorDetails.
      // - Invalidate connectorsStatsQuery, to update list of connectors in ConnectorTables
      invalidateConnectorQueries()
    }

    fetchRefresh('connectors')
  }

  async function handleGeneratePasswordClick() {
    try {
      setPasswordIsGenerating(true)
      const response = await api.post(
        `/api/sites/${connector.siteId}/connectors/${connector.id}/password`
      )
      setPassword(response.password)
    } catch (error) {
      snackbar.show(t('plainText.errorGeneratePass'))
    } finally {
      setPasswordIsGenerating(false)
    }
  }

  function handleSubmit(form) {
    if (form.data.name === '') {
      setIsNameDefined(false)
    }
    const configurationEntries =
      form.data.connectorType === null
        ? []
        : connectorTypeColumns
            .filter((column) => form.data[column.name] !== '')
            .map((column) => [column.name, form.data[column.name]])
    const configuration = Object.fromEntries(configurationEntries)
    setEditConnectorRequest({
      configuration: JSON.stringify(configuration),
      errorThreshold: form.data.errorThreshold,
      isLoggingEnabled: form.data.isLoggingEnabled,
      name: form.data.name,
    })
  }

  function handleEditConnectorSubmitted() {
    if (featureFlags?.hasFeatureToggle('connectivityPage')) {
      // After successful edit,
      // - Invalidate connectorQuery, so changes are persistent
      //   when you go back and forth from ConnectorsTables to ConnectorDetails.
      // - Invalidate connectorsStatsQuery, to update list of connectors in ConnectorTables
      invalidateConnectorQueries()
      return
    }
    setExpandedConnector()
    fetchRefresh('connectors')
  }

  async function handleIsEnabledChange(newIsEnabled) {
    setIsEnabled((prev) => !prev)
    try {
      await api.put(
        `/api/sites/${connector.siteId}/connectors/${connector.id}/isEnabled?enabled=${newIsEnabled}`
      )
      if (featureFlags?.hasFeatureToggle('connectivityPage')) {
        invalidateConnectorQueries()
      }
    } catch {
      snackbar.show(
        newIsEnabled
          ? t('plainText.errorEnablePass')
          : t('plainText.errorDisablePass')
      )
      setIsEnabled(!newIsEnabled)
    }
  }

  const connectionTypeMapping = {
    vm: 'VM Service',
    iotedge: 'IotEdge',
    publicapi: 'Public Api',
    streamanalyticseventhub: 'Stream Analytics EventHub',
    streamanalyticsiothub: 'Stream Analytics IoTHub',
  }
  const connectionTypeLabel =
    connectionTypeMapping[connector.connectionType] || connector.connectionType
  const isReadOnly = !willowConnectorNames.includes(connectorType?.name)

  return (
    <Flex {...rest} className={cxClassName}>
      <Flex fill="content">
        <ConnectorHeader
          connector={connector}
          connectorTypeLabel={connectionTypeLabel}
          expanded={expanded}
          onHeaderClick={handleHeaderClick}
        />
        {expanded && (
          <Form
            defaultValue={{
              ...connector,
              ...connectorConfiguration,
            }}
            readOnly={isReadOnly}
          >
            {(form) => (
              <Flex fill="header">
                <Flex size="medium" padding="large">
                  <Toggle
                    onLabel={t('plainText.enabled')}
                    offLabel={t('plainText.disabled')}
                    value={isEnabled}
                    name="isEnabled"
                    onChange={handleIsEnabledChange}
                  />
                  <Flex size="large">
                    {isNameDefined && (
                      <Input name="name" label={t('labels.name')} required />
                    )}
                    {!isNameDefined && (
                      <Input
                        name="name"
                        error="Name must not be empty"
                        required
                      />
                    )}
                    <ConnectorColumns columns={connectorTypeColumns} />
                    <NumberInput
                      name="errorThreshold"
                      label={t('labels.errorThreshold')}
                      min={0}
                    />
                    <Checkbox
                      name="isLoggingEnabled"
                      label={t('labels.loggingEnabled')}
                    />
                    {!isReadOnly && (
                      <>
                        <hr />
                        <Label label={t('labels.generateAPassword')}>
                          <InputWithButton
                            className={styles.generateInput}
                            text="Generate"
                            value={password}
                            onClick={handleGeneratePasswordClick}
                            isLoading={passwordIsGenerating}
                            disabled={isEnabled}
                          />
                        </Label>
                      </>
                    )}

                    <Button
                      kind="negative"
                      onClick={handleArchiveConnection}
                      disabled={connector.isEnabled}
                      css={`
                        align-self: center;
                      `}
                    >
                      {t('plainText.archiveThisConnection')}
                    </Button>
                  </Flex>
                </Flex>
                <Footer>
                  <Button kind="secondary" onClick={setExpandedConnector}>
                    {t('plainText.cancel')}
                  </Button>
                  <Button
                    // If connectivityPage feature flag is on,
                    // Disable save button when form has changed.
                    // - Ignore pointsCount as this value is always changing over time.
                    // - Ignore configuration, for the case when a connector initially
                    //   has an empty config then it gets saved with a new config.
                    // - Ignore isEnabled, as that toggle button is separate from the form.
                    disabled={
                      featureFlags?.hasFeatureToggle('connectivityPage')
                        ? _.isEqual(
                            _.omit(form.initialData, [
                              'pointsCount',
                              'configuration',
                              'isEnabled',
                            ]),
                            _.omit(form.data, [
                              'pointsCount',
                              'configuration',
                              'isEnabled',
                            ])
                          )
                        : isEnabled
                    }
                    onClick={() => handleSubmit(form)}
                  >
                    {t('headers.saveConnector')}
                  </Button>
                </Footer>
              </Flex>
            )}
          </Form>
        )}
        {editConnectorRequest != null && (
          <EditConnectorModal
            request={editConnectorRequest}
            connector={connector}
            onClose={() => setEditConnectorRequest()}
            onSubmitted={handleEditConnectorSubmitted}
          />
        )}
        {isArchived && (
          <ArchiveConnectorModal
            request={isArchived}
            connector={connector}
            onClose={() => setIsArchived(false)}
            onSubmitted={handleArchiveConnectorSubmitted}
          />
        )}
      </Flex>
    </Flex>
  )
}
