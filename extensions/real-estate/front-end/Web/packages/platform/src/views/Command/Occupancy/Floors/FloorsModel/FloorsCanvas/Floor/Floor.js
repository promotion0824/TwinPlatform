import { useMemo } from 'react'
import * as THREE from 'three'
import { MeshLineMaterial } from 'threejs-meshline'
import cx from 'classnames'
import { Flex, Icon, Text } from '@willow/ui'
import { Html } from '@react-three/drei'
import FloorZone from './FloorZone'
import colors from '../../colors.json'
import styles from './Floor.css'

export default function Floor({ floor, position, isHovering, ...rest }) {
  let color = 'text'
  if (floor.people > floor.peopleLimit) color = 'red'
  if (isHovering) color = `${color}Light`
  color = colors[color]

  const meshMaterial = useMemo(
    () =>
      new THREE.MeshPhongMaterial({
        color,
        opacity: isHovering ? 1 : 0.5,
        transparent: !isHovering,
      }),
    [isHovering]
  )

  const lineMaterial = useMemo(
    () =>
      new MeshLineMaterial({
        lineWidth: 0.15,
        color: colors.light,
      }),
    []
  )

  const cxPeopleContainerClassName = cx(styles.peopleContainer, {
    [styles.peopleOverLimit]: floor.people > floor.peopleLimit,
  })

  return (
    <mesh position={position}>
      {floor.geometry.map((geometry, i) => (
        <FloorZone
          key={`${floor.id} ${i}`} // eslint-disable-line
          {...rest}
          geometry={geometry}
          meshMaterial={meshMaterial}
          lineMaterial={lineMaterial}
        />
      ))}
      {floor.peoplePosition.length > 0 && (
        <Html
          position={[
            floor.peoplePosition[0] + 5,
            floor.peoplePosition[1] + 2,
            5,
          ]}
          className={cxPeopleContainerClassName}
        >
          <Flex
            horizontal
            align="middle"
            size="small"
            padding="tiny medium"
            className={styles.people}
          >
            <Icon icon="user" size="tiny" />
            <Text type="small">{floor.people}</Text>
          </Flex>
        </Html>
      )}
    </mesh>
  )
}
