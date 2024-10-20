import _ from 'lodash'
import {
  getApiGlobalPrefix,
  Button,
  Error,
  Fetch,
  Flex,
  Header,
  Icon,
  NotFound,
  Table,
  Head,
  Body,
  Row,
  Cell,
  Text,
  useLanguage,
} from '@willow/ui'

import { useTranslation } from 'react-i18next'

const formatValue = (value) =>
  typeof value === 'string' ? value : JSON.stringify(value)

export default function Details({ assetId, siteId }) {
  const { t } = useTranslation()
  const { language } = useLanguage()
  return (
    <Fetch
      url={`/api/sites/${siteId}/assets/${assetId}`}
      headers={{ language }}
    >
      {(response) => (
        <Fetch
          url={`/api/sites/${siteId}/assets/${assetId}/pinOnLayer`}
          headers={{ language }}
          error={null}
        >
          {(pinOnLayerResponse) => {
            const asset = {
              ...response,
              properties: [
                ...(response.properties ?? []),
                ...(pinOnLayerResponse?.liveDataPoints ?? [])
                  .filter(
                    (liveDataPoint) => liveDataPoint.liveDataValue != null
                  )
                  .map((liveDataPoint) => ({
                    displayName: liveDataPoint.tag,
                    value: liveDataPoint.liveDataValue,
                  })),
              ],
            }

            return (
              <Flex fill="content">
                <Header align="middle">
                  <Flex size="tiny">
                    {asset.identifier != null && (
                      <Text type="message" size="tiny" color="grey">
                        {asset.identifier}
                      </Text>
                    )}
                    <Text type="h3">{asset.name}</Text>
                  </Flex>
                </Header>
                <Flex>
                  <Flex>
                    <Table>
                      <Head isVisible={false}>
                        <Row>
                          <Cell />
                          <Cell />
                        </Row>
                      </Head>
                      <Body>
                        {(asset.properties ?? []).map((property, i) => {
                          const values = _(property.value)
                            .omit('$metadata')
                            .map((value, name) => ({
                              name,
                              value,
                              metadata: _(property.value.$metadata?.[name])
                                .map((metadataValue, metadataName) => ({
                                  name: metadataName,
                                  value: metadataValue,
                                }))
                                .value(),
                            }))
                            .value()

                          const metadata = _(property.value?.$metadata)
                            .map((metadataValue, metadataName) => ({
                              name: metadataName,
                              metadata: _(metadataValue)
                                .map((nextValue, nextName) => ({
                                  name: nextName,
                                  value: nextValue,
                                }))
                                .value(),
                            }))
                            .filter(
                              (item) =>
                                !values
                                  .map((prevItem) => prevItem.name)
                                  .includes(item.name)
                            )
                            .value()

                          return (
                            <Row
                              // eslint-disable-next-line react/no-array-index-key
                              key={i}
                            >
                              <Cell align="top">
                                <Text whiteSpace="normal">
                                  {!_.isObject(property.displayName)
                                    ? property.displayName
                                    : '-'}
                                </Text>
                              </Cell>
                              <Cell align="top">
                                <Text color="white" whiteSpace="normal">
                                  {!_.isObject(property.value) &&
                                    property.value}
                                  {_.isObject(property.value) && (
                                    <Flex size="small">
                                      {values.map((value) => (
                                        <Flex key={value.name}>
                                          {value.name}
                                          {': '}
                                          {formatValue(value.value)}
                                          {value.metadata.map((metadataObj) => (
                                            <Flex
                                              key={metadataObj.name}
                                              padding="tiny 0 0 large"
                                            >
                                              <Text color="grey">
                                                {metadataObj.name}
                                                {': '}
                                                {formatValue(metadataObj.value)}
                                              </Text>
                                            </Flex>
                                          ))}
                                        </Flex>
                                      ))}
                                      {metadata.map((metadataObj) => (
                                        <Flex key={metadataObj.name}>
                                          <Text color="grey">
                                            {metadataObj.name}
                                            <Flex>
                                              {metadataObj.metadata.map(
                                                (metadataOnMetadataObj) => (
                                                  <Flex padding="tiny 0 0 large">
                                                    <Text>
                                                      {
                                                        metadataOnMetadataObj.name
                                                      }
                                                      {': '}
                                                      {formatValue(
                                                        metadataOnMetadataObj.value
                                                      )}
                                                    </Text>
                                                  </Flex>
                                                )
                                              )}
                                            </Flex>
                                          </Text>
                                        </Flex>
                                      ))}
                                      {values.length === 0 &&
                                        metadata.length === 0 && <span>-</span>}
                                    </Flex>
                                  )}
                                </Text>
                              </Cell>
                            </Row>
                          )
                        })}
                      </Body>
                    </Table>
                  </Flex>
                  {(asset.properties ?? []).length === 0 && (
                    <Flex padding="large">
                      <NotFound>{t('plainText.noPropertiesFound')}</NotFound>
                    </Flex>
                  )}
                  <Fetch
                    url={`/api/sites/${siteId}/assets/${assetId}/files`}
                    progress={
                      <Flex padding="large" align="center">
                        <Icon icon="progress" />
                      </Flex>
                    }
                    error={
                      <Flex>
                        <Error>{t('plainText.errorLoadingFiles')}</Error>
                      </Flex>
                    }
                  >
                    {(files) =>
                      files.length > 0 && (
                        <Flex padding="large large medium">
                          <Text type="h3">{t('headers.files')}</Text>
                          <Flex padding="small 0">
                            {files.map((file) => (
                              <Flex key={file.id} horizontal>
                                <Button
                                  href={`${getApiGlobalPrefix()}/api/sites/${siteId}/assets/${assetId}/files/${
                                    file.id
                                  }?inline=true`}
                                  target="_blank"
                                  data-segment="File Downloaded"
                                >
                                  <Flex padding="medium">
                                    <Icon icon="file" />
                                  </Flex>
                                  {file.fileName}
                                </Button>
                              </Flex>
                            ))}
                          </Flex>
                        </Flex>
                      )
                    }
                  </Fetch>
                </Flex>
              </Flex>
            )
          }}
        </Fetch>
      )}
    </Fetch>
  )
}
