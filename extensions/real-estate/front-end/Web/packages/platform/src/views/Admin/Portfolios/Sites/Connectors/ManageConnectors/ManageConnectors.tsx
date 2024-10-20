import ConnectorDetails from './ConnectorDetails'
import ConnectorsTables from './ConnectorsTables'
import ConnectorsModal from '../ConnectorsModal'
import { useManageConnectors } from './providers/ManageConnectorsProvider'
import { SetShowAddConnectorModal } from './types'

export default function ManageConnectors({
  showAddConnectorModal,
  setShowAddConnectorModal,
}: {
  showAddConnectorModal: boolean
  setShowAddConnectorModal: SetShowAddConnectorModal
}) {
  const { selectedConnector, connectorsStatsQuery, connectorTypesData } =
    useManageConnectors()

  const { refetch: connectorsStatsQueryRefetch } = connectorsStatsQuery

  return (
    <>
      {selectedConnector == null ? <ConnectorsTables /> : <ConnectorDetails />}

      {showAddConnectorModal && (
        <ConnectorsModal
          connector={selectedConnector}
          connectorTypes={connectorTypesData}
          onClose={() => setShowAddConnectorModal(false)}
          refetch={connectorsStatsQueryRefetch}
        />
      )}
    </>
  )
}
