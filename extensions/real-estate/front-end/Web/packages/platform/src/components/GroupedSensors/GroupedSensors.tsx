/* eslint-disable react/require-default-props */
import React from 'react'
import { styled } from 'twin.macro'
import Point from './Point'
import GroupHeader, { GroupHeaderProps } from './GroupHeader'
import { SensorPoint } from '../../views/Portfolio/twins/view/useGroupedSensors'
import { LiveDataPoints } from '../../views/Portfolio/twins/hooks/useGetLiveDataPoints'

const PointList = styled.ul({
  listStyle: 'none',
  padding: 0,
})

const GroupedSensorsContainer = styled.div(({ theme }) => ({
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  marginBottom: 'var(--padding-large)',
}))

/**
 * List of sensors grouped by hostedBy device & connector.
 */
const GroupedSensors = ({
  hostedBy,
  connector,
  points,
  liveDataPoints,
  isLoadingLiveData = false,
  onTogglePoint,
}: {
  hostedBy?: GroupHeaderProps['hostedBy']
  /**
   * Note: It is rare case where connector is not present, but can happen
   * due to data issue. However, as connector is not a mandatory field in the
   * DTO, we support cases where group of sensors doesn't have connector.
   */
  connector?: GroupHeaderProps['connector']
  points: SensorPoint[]
  liveDataPoints?: LiveDataPoints
  isLoadingLiveData?: boolean
  onTogglePoint: (
    selectedPoint: { name: string; sitePointId: string },
    isSelected: boolean
  ) => void
}) => (
  <GroupedSensorsContainer>
    {connector != null && (
      <GroupHeader hostedBy={hostedBy} connector={connector} />
    )}
    <PointList data-testid="twin-sensors-result">
      {points.map((point) => {
        const liveData = liveDataPoints?.[point.trendId]
        return (
          <Point
            key={point.externalId}
            isLoadingLiveData={isLoadingLiveData}
            liveData={liveData}
            onTogglePoint={onTogglePoint}
            {...point}
          />
        )
      })}
    </PointList>
  </GroupedSensorsContainer>
)

export default GroupedSensors
