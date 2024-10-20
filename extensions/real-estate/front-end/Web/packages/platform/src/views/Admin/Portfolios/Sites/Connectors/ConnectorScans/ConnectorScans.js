import { useParams } from 'react-router'
import { Fetch } from '@willow/ui'
import ConnectorScansContent from './ConnectorScansContent'

export default function ConnectorScans({
  connectorId,
  connectorType,
  connectorEnabled,
}) {
  const params = useParams()
  return (
    <Fetch
      name="connectorScans"
      url={`/api/sites/${params.siteId}/connectors/${connectorId}/scans`}
      poll={10000}
    >
      {(scans) => (
        <ConnectorScansContent
          scans={scans ? scans : []}
          connectorId={connectorId}
          connectorType={connectorType}
          connectorEnabled={connectorEnabled}
        />
      )}
    </Fetch>
  )
}
