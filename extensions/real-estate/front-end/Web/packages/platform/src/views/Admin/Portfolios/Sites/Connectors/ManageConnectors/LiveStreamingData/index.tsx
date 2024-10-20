import { useRef } from 'react'
import { Flex, NotFound, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import Graph from './Graph'
import { Telemetries } from '../../../../../../../services/Connectors/ConnectorsService'

export default function LiveStreamingData({
  telemetry = [],
}: {
  telemetry: Telemetries
}) {
  const { t } = useTranslation()
  const liveStreamingDataRef = useRef<HTMLDivElement>(null)

  const graphData = constructGraphData(telemetry)

  return (
    <Container ref={liveStreamingDataRef}>
      <Flex fill="content" size="large">
        <Text type="h2">{t('plainText.liveStreamingData')}</Text>
        {graphData.length !== 0 ? (
          <Graph
            graphData={graphData}
            liveStreamingDataRef={liveStreamingDataRef}
          />
        ) : (
          <NotFound>{t('plainText.noDataFound')}</NotFound>
        )}
      </Flex>
    </Container>
  )
}

const Container = styled.div({
  height: '240px',
  borderBottom: '1px solid #383838',
  padding: '1.5em',
  minWidth: '800px',
})

export function constructGraphData(telemetry: Telemetries) {
  return telemetry.map(({ timestamp, totalTelemetryCount }) => ({
    timestamp,
    value: totalTelemetryCount,
  }))
}
