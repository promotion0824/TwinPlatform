import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import {
  useFetchRefresh,
  Button,
  Flex,
  Form,
  Input,
  Checkbox,
} from '@willow/ui'
import styles from './ConnectorScansControlPanel.css'
import { styled } from 'twin.macro'

const StyledFlex = styled(Flex)({
  '> *': { marginLeft: '10px' },
})

export default function ConnectorScansControlPanel({
  connectorId,
  connectorType,
  isScanInProgress,
  connectorEnabled,
}) {
  const fetchRefresh = useFetchRefresh()
  const params = useParams()
  const { t } = useTranslation()

  const [message, setMessage] = useState()
  const [devicesToScan, setDevicesToScan] = useState()
  const [deviceWhoisSegmentSize, setDeviceWhoisSegmentSize] = useState(100)
  const [timeIntervalBetweenEachSegment, setTimeIntervalBetweenEachSegment] =
    useState(1)
  const [minimumScanTime, setMinimumScanTime] = useState(60)
  const [isInRangeOnlyChecked, setIsInRangeOnlyChecked] = useState(true)

  function getConfiguration() {
    let configuration = null
    if (connectorType?.name === 'DefaultChipkinBACnetConnector') {
      let configurationObject = {
        WhoisSegmentSize: deviceWhoisSegmentSize,
        MinScanTime: minimumScanTime,
        TimeInterval: timeIntervalBetweenEachSegment,
        InRangeOnly: isInRangeOnlyChecked,
      }
      configuration = JSON.stringify(configurationObject)
    }

    return configuration
  }

  function handleSubmit(form) {
    let configuration = getConfiguration()
    return form.api.post(
      `/api/sites/${params.siteId}/connectors/${connectorId}/scans`,
      {
        message,
        devicesToScan,
        configuration,
      }
    )
  }

  function handleSubmitted() {
    setMessage()
    setDeviceWhoisSegmentSize()
    setTimeIntervalBetweenEachSegment()
    setMinimumScanTime()
    setDevicesToScan()
    setIsInRangeOnlyChecked()
    fetchRefresh('connectorScans')
  }

  function isRequestScanDisabled() {
    let isDisabled = !message || isScanInProgress || connectorEnabled
    if (connectorType?.name === 'DefaultChipkinBACnetConnector') {
      isDisabled =
        !deviceWhoisSegmentSize ||
        !timeIntervalBetweenEachSegment ||
        !minimumScanTime ||
        !devicesToScan ||
        isScanInProgress ||
        connectorEnabled
    }

    return isDisabled
  }

  function handleNumericKeyPress(event) {
    if (!/\d/.test(event.key)) {
      event.preventDefault()
    }
  }

  function getSpecificConnectorScanFields() {
    return (
      connectorType?.name === 'DefaultChipkinBACnetConnector' && (
        <>
          <Input
            className={styles.requestInput}
            label={t('labels.deviceWhoisSegmentSize')}
            value={deviceWhoisSegmentSize}
            onChange={setDeviceWhoisSegmentSize}
            name="WhoisSegmentSize"
            required
            onKeyPress={handleNumericKeyPress}
          />
          <Input
            className={styles.requestInput}
            label={t('labels.minimumScanTime')}
            value={minimumScanTime}
            onChange={setMinimumScanTime}
            name="MinScanTime"
            required
            onKeyPress={handleNumericKeyPress}
          />
          <Input
            className={styles.requestInput}
            label={t('labels.timeIntervalBetweenEachSegment')}
            value={timeIntervalBetweenEachSegment}
            onChange={setTimeIntervalBetweenEachSegment}
            name="TimeInterval"
            required
            onKeyPress={handleNumericKeyPress}
          />
          <Checkbox
            checked={isInRangeOnlyChecked}
            label={t('labels.inRangeOnly')}
            value={isInRangeOnlyChecked}
            onChange={setIsInRangeOnlyChecked}
          />
        </>
      )
    )
  }

  return (
    <Flex>
      <Form onSubmit={handleSubmit} onSubmitted={handleSubmitted}>
        <StyledFlex horizontal align="bottom" padding="medium">
          <Input
            className={styles.requestInput}
            label={t('labels.message')}
            value={message}
            onChange={setMessage}
          />
          <Input
            className={styles.requestInput}
            label={t('labels.devicesToScan')}
            value={devicesToScan}
            onChange={setDevicesToScan}
            required={connectorType?.name === 'DefaultChipkinBACnetConnector'}
          />
          {getSpecificConnectorScanFields()}
          <Button
            type="submit"
            color="purple"
            className={styles.addScanBtn}
            disabled={isRequestScanDisabled()}
          >
            {t('plainText.requestScan')}
          </Button>
        </StyledFlex>
      </Form>
    </Flex>
  )
}
