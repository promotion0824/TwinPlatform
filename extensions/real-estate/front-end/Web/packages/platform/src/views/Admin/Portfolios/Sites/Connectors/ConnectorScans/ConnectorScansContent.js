import { useParams } from 'react-router'
import { Flex, Table, Head, Body, Row, Cell, Time } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import DownloadScanDataButton from './DownloadScanDataButton/DownloadScanDataButton'
import ConnectorScansControlPanel from './ConnectorScansControlPanel/ConnectorScansControlPanel'
import StopScanButton from './StopScanButton/StopScanButton'
import ScanProgressBar from './ScanProgressBar/ScanProgressBar'
import RequestScanProgressBar from './RequestScanProgressBar/RequestScanProgressBar'

export default function ConnectorScansContent({
  connectorId,
  connectorType,
  connectorEnabled,
  scans,
}) {
  const params = useParams()
  const { t } = useTranslation()

  const isScanInProgress = scans.some(
    (scan) => scan.startTime != null && scan.endTime == null
  )

  const isRequestScanInProgress = scans.some((scan) => scan.status === 'new')

  function getConnectorScans() {
    if (connectorType?.name === 'DefaultChipkinBACnetConnector') {
      return scans.map((s) => {
        let clone = structuredClone(s)
        if (clone.configuration) {
          let configuration = JSON.parse(clone.configuration)
          clone.deviceWhoisSegmentSize = configuration.WhoisSegmentSize
          clone.minimumScanTime = configuration.MinScanTime
          clone.timeIntervalBetweenEachSegment = configuration.TimeInterval
          clone.inRangeOnly = configuration.InRangeOnly
        } else {
          clone.deviceWhoisSegmentSize = null
          clone.minimumScanTime = null
          clone.timeIntervalBetweenEachSegment = null
          clone.inRangeOnly = null
        }
        return clone
      })
    } else {
      return scans
    }
  }

  function getScanAdditionalHeader() {
    return (
      connectorType?.name === 'DefaultChipkinBACnetConnector' && (
        <>
          <Cell sort="deviceWhoisSegmentSize">
            {t('labels.deviceWhoisSegmentSize')}
          </Cell>
          <Cell sort="minimumScanTime">{t('labels.minimumScanTime')}</Cell>
          <Cell sort="timeIntervalBetweenEachSegment">
            {t('labels.timeIntervalBetweenEachSegment')}
          </Cell>
          <Cell sort="inRangeOnly">{t('labels.inRangeOnly')}</Cell>
        </>
      )
    )
  }

  function getScanAdditionalRowContent(scan) {
    return (
      connectorType?.name === 'DefaultChipkinBACnetConnector' && (
        <>
          <Cell>{scan.deviceWhoisSegmentSize}</Cell>
          <Cell>{scan.minimumScanTime}</Cell>
          <Cell>{scan.timeIntervalBetweenEachSegment}</Cell>
          <Cell>
            {scan.inRangeOnly != null ? String(scan.inRangeOnly) : ''}
          </Cell>
        </>
      )
    )
  }

  return (
    <Flex fill="content" padding="medium">
      <Flex>
        {!connectorEnabled && (
          <ConnectorScansControlPanel
            connectorId={connectorId}
            connectorType={connectorType}
            scanInProgress={isScanInProgress}
            connectorEnabled={connectorEnabled}
            requestScanInProgress={isRequestScanInProgress}
          />
        )}
        {isRequestScanInProgress && <RequestScanProgressBar />}
        {isScanInProgress && <ScanProgressBar />}
      </Flex>
      <Table
        items={getConnectorScans()}
        defaultSort={[['createdAt'], ['desc']]}
        notFound={t('plainText.noScansYet')}
      >
        {(items) => (
          <>
            <Head>
              <Row>
                <Cell sort="status">{t('labels.status')}</Cell>
                <Cell sort="message" width="1fr">
                  {t('labels.message')}
                </Cell>
                <Cell sort="devicesToScan">{t('labels.devicesToScan')}</Cell>
                {getScanAdditionalHeader()}
                <Cell sort="createdAt">{t('labels.created')}</Cell>
                <Cell sort="startTime">{t('labels.startTime')}</Cell>
                <Cell sort="endTime">{t('labels.endTime')}</Cell>
                <Cell sort="errorCount">{t('plainText.errors')}</Cell>
                <Cell>{t('plainText.action')}</Cell>
              </Row>
            </Head>
            <Body>
              {items.map((scan) => (
                <Row key={scan.id}>
                  <Cell>{scan.status}</Cell>
                  <Cell>{scan.message}</Cell>
                  <Cell>{scan.devicesToScan}</Cell>
                  {getScanAdditionalRowContent(scan)}
                  <Cell>
                    <Time value={scan.createdAt} />
                  </Cell>
                  <Cell>
                    <Time value={scan.startTime} />
                  </Cell>
                  <Cell>
                    <Time value={scan.endTime} />
                  </Cell>
                  <Cell>{scan.errorCount}</Cell>
                  <Cell type="fill">
                    {scan.status === 'failed' || scan.status === 'finished' ? (
                      <DownloadScanDataButton
                        siteId={params.siteId}
                        connectorId={connectorId}
                        scanId={scan.id}
                      />
                    ) : (
                      <StopScanButton
                        siteId={params.siteId}
                        connectorId={connectorId}
                        scanId={scan.id}
                      />
                    )}
                  </Cell>
                </Row>
              ))}
            </Body>
          </>
        )}
      </Table>
    </Flex>
  )
}
