import { useState } from 'react'
import _ from 'lodash'
import { Flex, Panel, Tabs, Tab } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import ConnectorConfiguration from './ConnectorConfiguration/ConnectorConfiguration'
import ConnectorLogsTable from './ConnectorLogs/ConnectorLogsTable'
import ConnectorScans from './ConnectorScans/ConnectorScans'
import columnOrder from './columnOrder'
import styles from './Connectors.css'

export default function ConnectorsContent({ connectorTypes, connectors }) {
  const { t } = useTranslation()
  const [expandedConnector, setExpandedConnector] = useState()

  return (
    <Flex horizontal fill="content" size="small" padding="small 0">
      <Flex fill="header" className={styles.connectorsPanel}>
        <Panel>
          {connectors
            .filter(
              (connector) =>
                !expandedConnector || connector.id === expandedConnector.id
            )
            .filter((connector) => !connector.isArchived)
            .map((connector) => {
              const connectorType = connectorTypes.find(
                (type) => type.id === connector.connectorTypeId
              )
              const columns = _.orderBy(
                connectorType?.columns ?? [],
                columnOrder.map((name) => (column) => column.name !== name)
              )
              return (
                <ConnectorConfiguration
                  key={connector.id}
                  connector={connector}
                  connectorType={connectorType}
                  connectorTypeColumns={columns}
                  overflow="hidden"
                  setExpandedConnector={setExpandedConnector}
                  expanded={
                    !!expandedConnector && connector.id === expandedConnector.id
                  }
                />
              )
            })}
        </Panel>
      </Flex>
      {expandedConnector != null ? (
        <Tabs>
          <Tab header={t('headers.logs')}>
            <ConnectorLogsTable connectorId={expandedConnector.id} />
          </Tab>
          <Tab header={t('headers.scanner')}>
            <ConnectorScans
              connectorId={expandedConnector.id}
              connectorType={expandedConnector.connectorType}
              connectorEnabled={expandedConnector.isEnabled ?? false}
            />
          </Tab>
        </Tabs>
      ) : (
        <Panel />
      )}
    </Flex>
  )
}
