/* eslint-disable react/require-default-props */
import { Button, Icon, IconNew, useDateTime, Text } from '@willow/ui'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import tw, { styled } from 'twin.macro'
import { LiveDataPoint } from '../../views/Portfolio/twins/hooks/useGetLiveDataPoints'
import { SensorPoint } from '../../views/Portfolio/twins/view/useGroupedSensors'
import LiveDataValue from '../LiveDataValue/LiveDataValue'
import { useSelectedPoints } from '../MiniTimeSeries'

type StatusColor = 'red' | 'grey' | 'green'

type TimestampProps = {
  timestamp: string
}

export const PointContainer = styled.li({
  display: 'flex',
  padding: 'var(--padding) 0',
})

export const PointInfo = styled.div({
  flex: 1,
  display: 'flex',
})

export const PointName = styled.div({
  fontSize: 'var(--font-small)',
  color: 'var(--light)',
  fontWeight: 'var(--font-weight-500)',
  marginRight: 'var(--padding)',
})

export const PointId = styled.div(({ theme }) => ({
  color: theme.color.neutral.fg.subtle,
  fontSize: 'var(--font-extra-tiny)',
  fontWeight: 'var(--font-weight-500)',
  paddingLeft: '36px',
  display: 'inline-flex',
  alignItems: 'center',
  width: '100%',
}))

export const Status = styled.div(({ theme }) => ({
  display: 'inline-flex',
  color: theme.color.neutral.fg.subtle,
  alignItems: 'center',
  marginRight: 'var(--padding-small)',
}))

export const LiveDataContainer = styled.div({
  color: 'var(--lighter)',
  textTransform: 'uppercase',
  fontSize: 'var(--font-extra-tiny)',
  fontWeight: 'var(--font-weight-500)',
  display: 'inline-flex',
  alignItems: 'center',
  whiteSpace: 'nowrap',
})

export const TinyIcon = styled(Icon)({
  width: '10px',
  height: '10px',
})

export const Dot = styled.span(({ theme }) => ({
  background: theme.color.neutral.fg.subtle,
  borderRadius: '100%',
  width: '2px',
  height: '2px',
  margin: '0 var(--padding)',
}))

export const MonitorButton = styled(Button)<{ $isSelected: boolean }>(
  ({ $isSelected }) => ({
    fontSize: 10,
    fontWeight: 500,
    padding: '0px 8px',
    height: 'auto',
    color: $isSelected ? 'var(--light)' : undefined,
  })
)

export const styles = {
  red: tw`color[var(--red-light)]`,
  grey: tw`color[var(--theme-color-neutral-fg-subtle)]`,
  green: tw`color[var(--green)]`,
}

const ONE_MINUTE = 60 * 1000

/**
 * Status color as follows:
 * - Loading                        => null
 * - Timestamp < 1h                 => Color = green
 * - 1d > Timestamp >= 1h           => Color = green
 * - Timestamp >= 1d                => Color = red
 * - Data not found (No timestamp)  => Color = red
 */
const getStatusColor = (
  datetime,
  isLoading: boolean,
  liveData?: LiveDataPoint
) => {
  if (isLoading) return undefined

  let color: StatusColor | undefined

  if (liveData?.liveDataTimestamp) {
    const now = datetime.now()

    const numDaysAgo = now.differenceInDays(liveData.liveDataTimestamp)
    const numHoursAgo = now.differenceInHours(liveData.liveDataTimestamp)
    const numMillisecondsAgo = now.differenceInMilliseconds(
      liveData.liveDataTimestamp
    )

    if (numDaysAgo) {
      color = 'red'
    } else if (numHoursAgo) {
      color = 'green'
    } else if (numMillisecondsAgo) {
      color = 'green'
    }
  } else {
    color = 'red'
  }

  return color
}

export const StatusIcon = ({
  liveData,
  isLoading,
}: {
  liveData?: LiveDataPoint
  isLoading: boolean
}) => {
  const datetime = useDateTime()
  const [iconColor, setIconColor] = useState<StatusColor | undefined>(() =>
    getStatusColor(datetime, isLoading, liveData)
  )

  useEffect(() => {
    const updateColor = () => {
      setIconColor(getStatusColor(datetime, isLoading, liveData))
    }
    updateColor()
    const interval = setInterval(updateColor, ONE_MINUTE)
    return () => clearInterval(interval)
  }, [datetime, isLoading, liveData])

  return (
    <IconNew
      icon="status"
      size="tiny"
      css={iconColor ? styles[iconColor] : undefined}
      tw="margin-left[-4px]"
    />
  )
}

const Timestamp = ({ timestamp }: TimestampProps) => {
  const datetime = useDateTime()
  const [timeAgo, setTimeAgo] = useState<string>()

  useEffect(() => {
    const updateTimeAgo = () => {
      if (timestamp) setTimeAgo(datetime(timestamp).format('ago'))
    }
    updateTimeAgo()
    const interval = setInterval(updateTimeAgo, ONE_MINUTE)
    return () => clearInterval(interval)
  }, [timestamp, datetime])

  return <>{timeAgo}</>
}

const LiveDataDisplay = ({
  liveDataValue,
  unit,
  liveDataTimestamp,
}: LiveDataPoint) => {
  const { t } = useTranslation()

  return liveDataValue != null ? (
    <>
      <LiveDataValue unit={unit} liveDataValue={liveDataValue} />
      <Dot />
      {liveDataTimestamp != null && <Timestamp timestamp={liveDataTimestamp} />}
    </>
  ) : (
    <>{t('plainText.dataNotFound')}</>
  )
}

/**
 * Display a point's name, ID, status, and a Monitor button. The Monitor button
 * is a toggle button which is synced with the Time Series tab. Toggling the
 * Monitor button will toggle the point in the Time Series tab, and toggling
 * the respective point in the Time Series tab will toggle the button state.
 */
const Point = ({
  name,
  externalId,
  isLoadingLiveData,
  liveData,
  trendId,
  properties,
  onTogglePoint,
}: {
  isLoadingLiveData: boolean
  liveData?: LiveDataPoint
  onTogglePoint: (
    selectedPoint: { name: string; sitePointId: string },
    isSelected: boolean
  ) => void
} & SensorPoint) => {
  const { t } = useTranslation()
  const selectedPoints = useSelectedPoints()

  const sitePointId = `${properties.siteID.value}_${trendId}`
  const isSelected = selectedPoints.pointIds.includes(sitePointId)

  const togglePointSelection = () => {
    const nextSelected = !isSelected

    selectedPoints.onSelectPoint(sitePointId, nextSelected)
    onTogglePoint({ name, sitePointId }, nextSelected)
  }

  return (
    <PointContainer>
      <div tw="flex-shrink">
        <PointInfo>
          <Status>
            <IconNew icon="dashedLine" size="small" />
            <StatusIcon liveData={liveData} isLoading={isLoadingLiveData} />
          </Status>
          <PointName>{name}</PointName>
          <LiveDataContainer>
            {isLoadingLiveData ? (
              <TinyIcon icon="progress" size="tiny" />
            ) : (
              !!liveData && <LiveDataDisplay {...liveData} />
            )}
          </LiveDataContainer>
        </PointInfo>
        <PointId>
          <Text title={externalId}>{externalId}</Text>
        </PointId>
      </div>
      <div tw="flex-grow text-right ml-2">
        <MonitorButton
          color="transparent"
          onClick={togglePointSelection}
          $isSelected={isSelected}
        >
          {isSelected ? (
            <IconNew
              icon="layersFilled"
              // For a selected point, draw the icon in the same colour the
              // Time Series tab uses to draw the graph.
              style={{ fill: selectedPoints.pointColorMap[sitePointId] }}
            />
          ) : (
            <IconNew icon="layers" />
          )}
          <span tw="ml-1">{t('plainText.monitor')}</span>
        </MonitorButton>
      </div>
    </PointContainer>
  )
}

export default Point
