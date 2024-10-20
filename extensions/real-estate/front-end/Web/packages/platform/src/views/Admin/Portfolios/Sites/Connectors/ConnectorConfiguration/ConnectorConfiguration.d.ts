import { ReactNode, ReactElement } from 'react'
import { ConnectorDetails } from '../ManageConnectors/types/index'
import {
  ConnectorType,
  ConnectorTypeColumn,
} from '../../../../../../services/ConnectorTypes/ConnectorTypesService'

export default function TabConnectorConfiguration(props: {
  connector?: ConnectorDetails
  connectorType?: ConnectorType
  connectorTypeColumns?: ConnectorTypeColumn[]
  setExpandedConnector?: () => void
  overflow?: string
  expanded?: boolean
  invalidateConnectorQueries?: () => void
  children?: ReactNode
  onClick?: () => void
}): ReactElement
