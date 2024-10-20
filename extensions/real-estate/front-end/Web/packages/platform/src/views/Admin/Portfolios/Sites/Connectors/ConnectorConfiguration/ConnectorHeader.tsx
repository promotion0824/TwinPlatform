import { useTranslation } from 'react-i18next'
import {
  Button,
  Flex,
  Icon,
  Number,
  Pill,
  Text,
  useFeatureFlag,
} from '@willow/ui'
import { styled } from 'twin.macro'
import styles from './ConnectorConfiguration.css'
import { Connector } from '../../../../../../services/Connectivity/ConnectivityService'

const Container = styled.div<{ selected: boolean }>(({ theme }) => ({
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
}))

/**
 *  Header component for connector's view/edit component.
 *  Displays connector's id, connector type, and the amount of data received.
 *
 *  There's two different behaviors for ConnectorHeader, which is dependent upon the feature flag 'connectivityPage':
 *  1. If feature flag is off,
 *     - Header component is used as an element in a list of connectors.
 *     - When Header component is clicked, it is able to expand/collapse to show/hide the connector's view/edit component.
 *  2. If feature flag is on,
 *     - Header component just displays the connector's info in the view/edit component, and does not have any other functionality.
 */
export default function ConnectorHeader({
  connector,
  connectorTypeLabel,
  expanded,
  onHeaderClick,
}: {
  connector: Connector
  connectorTypeLabel: string
  expanded: boolean
  onHeaderClick: () => void
}) {
  const featureFlags = useFeatureFlag()
  const { t } = useTranslation()

  const Component = featureFlags?.hasFeatureToggle('connectivityPage')
    ? Container
    : Button

  return (
    <Component
      selected={expanded}
      className={styles.header}
      onClick={onHeaderClick}
    >
      <Flex horizontal fill="header" width="100%">
        <Flex size="tiny" padding="large">
          <Text color="white">{connector.name}</Text>
          <Text size="small" color="grey" className={styles.connectorId}>
            ID: {connector.id}
          </Text>
          <Text size="small" color="grey">
            {connectorTypeLabel}
          </Text>
        </Flex>
        <Flex horizontal align="middle" size="medium" padding="0 large 0 0">
          <Pill color="green">
            <Flex horizontal align="middle" size="medium">
              <Icon icon="pointsPill" />
              <Text>
                <Number value={connector.pointsCount} format="," />{' '}
                {t('plainText.points')}
              </Text>
            </Flex>
          </Pill>

          {!featureFlags?.hasFeatureToggle('connectivityPage') && (
            <Icon icon="chevron" className={styles.chevron} />
          )}
        </Flex>
      </Flex>
    </Component>
  )
}
