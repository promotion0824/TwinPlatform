import { useState } from 'react'
import { useParams } from 'react-router'
import cx from 'classnames'
import { Component, Fetch, Flex, Icon, Text } from '@willow/ui'
import Tooltip from '../Tooltip/Tooltip'
import Point from './Point'
import styles from './EquipmentTooltip.css'

export default function EquipmentTooltip({
  equipmentRef,
  equipment,
  selected,
  priority,
  onPointerDown,
  onClick,
}) {
  const params = useParams()

  const [isMouseOver, setIsMouseOver] = useState(false)
  const [hasMouseEntered, setHasMouseEntered] = useState(false)
  const [hasPoints, setHasPoints] = useState(false)

  const cxClassName = cx(styles.tooltip, {
    [styles.selected]: selected || isMouseOver,
    [styles.isMouseOver]: isMouseOver,
    [styles.hasPoints]: hasPoints,
    [styles.colorRed]: priority === 'red',
    [styles.colorOrange]: priority === 'orange',
    [styles.colorYellow]: priority === 'yellow',
  })

  let zIndex
  if (selected) zIndex = 1
  if (isMouseOver) zIndex = 2

  return (
    <Tooltip
      targetRef={equipmentRef}
      point={equipment.points}
      clickable
      selected={selected}
      className={cxClassName}
      onPointerDown={onPointerDown}
      onMouseOver={() => {
        setHasMouseEntered(true)
        setIsMouseOver(true)
      }}
      onMouseOut={() => setIsMouseOver(false)}
      onClick={onClick}
      style={{ zIndex }}
    >
      <Fetch
        url={
          hasMouseEntered
            ? `/api/sites/${params.siteId}/assets/${equipment.id}/pinOnLayer`
            : undefined
        }
        loader={null}
      >
        {(response, { isLoading }) => {
          let uniquePoints
          if (response?.liveDataPoints != null) {
            uniquePoints = _.uniqBy(response.liveDataPoints, (p) => p.id)
          }

          return (
            <>
              <Flex
                horizontal
                fill="header"
                size="medium"
                className={styles.header}
              >
                <Flex horizontal size="medium" padding="medium large">
                  <Text type="h3">{equipment.name}</Text>
                  {isLoading && <Icon icon="progress" size="small" />}
                </Flex>
              </Flex>
              <div className={styles.content}>
                {uniquePoints?.map((point) => (
                  <Point key={point.id} point={point} />
                ))}
                {uniquePoints?.length > 0 && (
                  <Component onMount={() => setHasPoints(true)} />
                )}
              </div>
            </>
          )
        }}
      </Fetch>
    </Tooltip>
  )
}
