import { useTranslation } from 'react-i18next'
import _ from 'lodash'
import { Tabs, Tab, Flex, Panel, Error } from '@willow/ui'
import { styled } from 'twin.macro'
import ConnectorConfiguration from '../ConnectorConfiguration/ConnectorConfiguration'
import columnOrder from '../columnOrder'
import ConnectorLogsTable from '../ConnectorLogs/ConnectorLogsTable'
import ConnectorScans from '../ConnectorScans/ConnectorScans'
import { Loader } from './styled-components'
import { useManageConnectors } from './providers/ManageConnectorsProvider'
import LiveStreamingData from './LiveStreamingData'

const ConnectorConfigurationContainer = styled(Flex)({ width: '450px' })

// View/edit selected connector
export default function ConnectorDetails() {
  const { t } = useTranslation()

  const {
    connectorQuery,
    setConnectorId,
    connectorTypesData,
    connectorDetails: connector,
    invalidateConnectorQueries,
  } = useManageConnectors()

  const { isLoading, isError, isSuccess } = connectorQuery

  const connectorType = connectorTypesData?.find(
    (type) => type?.id === connector?.connectorTypeId
  )

  const columns = _.orderBy(
    connectorType?.columns ?? [],
    columnOrder.map((name) => (column) => column.name !== name)
  )

  return (
    <Flex horizontal fill="content" size="small" padding="small 0 0 0">
      <ConnectorConfigurationContainer fill="header">
        <Panel $borderWidth="1px 1px 0 0">
          {isLoading ? (
            <Loader />
          ) : isSuccess ? (
            <ConnectorConfiguration
              connector={connector}
              connectorType={connectorType}
              connectorTypeColumns={columns}
              overflow="hidden"
              setExpandedConnector={() => {
                setConnectorId?.(undefined)
              }}
              expanded
              // Invalidate queries after successful edit
              // 1. Update cache values for connectorQuery, so the changes are persistent
              //    when you go back and forth from ConnectorsTables to ConnectorDetails
              // 2. Update the list of connectors in ConnectorsTables with the recent changes
              invalidateConnectorQueries={invalidateConnectorQueries}
            />
          ) : (
            isError && <Error />
          )}
        </Panel>
      </ConnectorConfigurationContainer>

      {isSuccess && connector ? (
        <Tabs $borderWidth="1px 0 0 1px">
          <Tab header={t('plainText.liveStreamingData')}>
            <LiveStreamingData telemetry={connector.telemetry} />
          </Tab>
          <Tab header={t('headers.logs')}>
            <ConnectorLogsTable connectorId={connector.id} />
          </Tab>
          <Tab header={t('headers.scanner')}>
            <ConnectorScans
              connectorId={connector.id}
              connectorType={connector.connectorType}
              connectorEnabled={connector.isEnabled ?? false}
            />
          </Tab>
        </Tabs>
      ) : (
        <Panel />
      )}
    </Flex>
  )
}
