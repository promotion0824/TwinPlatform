import { useParams } from 'react-router'
import _ from 'lodash'
import {
  Fetch,
  Header,
  NotFound,
  Spacing,
  Table,
  Body,
  Row,
  Cell,
  Text,
} from '@willow/mobile-ui'
import { useLayout } from 'providers'
import styles from './Details.css'

export default function Details() {
  const params = useParams()
  const { setShowBackButton } = useLayout()

  setShowBackButton(true)

  return (
    <Fetch url={`/api/sites/${params.siteId}/assets/${params.assetId}`}>
      {(asset) => {
        const assetParameters = asset.properties ?? asset.assetParameters ?? []

        return (
          <Spacing type="content">
            <Header>
              <Text
                type="label"
                whiteSpace="normal"
                className={styles.headerText}
              >
                {asset.identifier}
              </Text>
              <Text type="h3" whiteSpace="normal" className={styles.headerText}>
                {asset.name}
              </Text>
            </Header>
            {assetParameters.length > 0 && (
              <Spacing size="medium">
                <Table className={styles.details}>
                  <Body>
                    {assetParameters.map((item, i) => (
                      // eslint-disable-next-line
                      <Row key={i}>
                        <Cell>
                          <Text
                            type="label"
                            whiteSpace="normal"
                            className={styles.text}
                          >
                            {item.value?.displayName ?? item.displayName}
                          </Text>
                        </Cell>
                        <Cell>
                          <Text whiteSpace="normal" className={styles.text}>
                            {!_.isObject(item.value?.value ?? item.value)
                              ? item.value?.value ?? item.value
                              : '-'}
                          </Text>
                        </Cell>
                      </Row>
                    ))}
                  </Body>
                </Table>
              </Spacing>
            )}
            {assetParameters.length === 0 && (
              <Spacing>
                <NotFound>No details found</NotFound>
              </Spacing>
            )}
          </Spacing>
        )
      }}
    </Fetch>
  )
}
