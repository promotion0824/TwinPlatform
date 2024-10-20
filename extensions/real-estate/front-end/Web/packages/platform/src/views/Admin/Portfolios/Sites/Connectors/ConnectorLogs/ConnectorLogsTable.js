import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { Fetch, Table, Body, Head, Row, Cell, Time } from '@willow/ui'
import ExportConnectorLogButton from './ExportConnectorLogButton/ExportConnectorLogButton'

export default function ConnectorLogsTable({ connectorId }) {
  const params = useParams()
  const { t } = useTranslation()

  return (
    <Fetch
      name="connectorLogs"
      url={`/api/sites/${params.siteId}/connectors/${connectorId}/logs`}
    >
      {(logs) => (
        <Table items={logs} notFound={t('plainText.noLogsFound')}>
          {(items) => (
            <>
              <Head>
                <Row>
                  <Cell sort="startTime">{t('plainText.trendStartTime')}</Cell>
                  <Cell sort="endTime">{t('plainText.trendEndTime')}</Cell>
                  <Cell sort="pointCount">{t('plainText.pointCount')}</Cell>
                  <Cell sort="errorCount">{t('plainText.errorCount')}</Cell>
                  <Cell sort="retryCount">{t('plainText.retryCount')}</Cell>
                  <Cell sort="source" width="1fr">
                    {t('labels.source')}
                  </Cell>
                  <Cell>{t('plainText.exportLog')}</Cell>
                </Row>
              </Head>
              <Body>
                {items.map((log) => (
                  <Row key={log.id}>
                    <Cell>
                      <Time value={log.startTime} />
                    </Cell>
                    <Cell>
                      <Time value={log.endTime} />
                    </Cell>
                    <Cell>{log.pointCount}</Cell>
                    <Cell>{log.errorCount}</Cell>
                    <Cell>{log.retryCount}</Cell>
                    <Cell>{log.source}</Cell>
                    <Cell type="fill">
                      <ExportConnectorLogButton
                        siteId={params.siteId}
                        connectorId={connectorId}
                        logId={log.id}
                      />
                    </Cell>
                  </Row>
                ))}
              </Body>
            </>
          )}
        </Table>
      )}
    </Fetch>
  )
}
