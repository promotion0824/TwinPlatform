import { Canvas } from '@react-three/fiber'
import { titleCase } from '@willow/common'
import {
  Flex,
  NotFound,
  useAnalytics,
  useFeatureFlag,
  useScopeSelector,
  useSnackbar,
  useUserAgent,
} from '@willow/ui'
import { updateTooltipStyles } from '@willow/ui/components/Viewer3D/utils'
import { Box, Button, Group, Tooltip } from '@willowinc/ui'
import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory, useParams } from 'react-router'
import styled from 'styled-components'
import { useDashboard } from '../../../Dashboard/DashboardContext'
import Controls from './Controls'
import Floor from './Floor/Floor'
import styles from './Floors.css'

const Subheading = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  marginLeft: theme.spacing.s4,
}))

function toRadians(angle) {
  return (angle * Math.PI) / 180
}

export default function Floors({ floors }) {
  const { scopeLookup } = useScopeSelector()
  const analytics = useAnalytics()
  const dashboard = useDashboard()
  const featureFlags = useFeatureFlag()
  const history = useHistory()
  const params = useParams()
  const userAgent = useUserAgent()
  const {
    i18n: { language },
    t,
  } = useTranslation()
  const snackbar = useSnackbar()

  const [isCameraMoved, setIsCameraMoved] = useState(false)
  const [mousePosition, setMousePosition] = useState()
  const [resetCamera, setResetCamera] = useState(false)

  const max = 100 * floors.length

  const cameraPosition = useMemo(() => {
    let radius = 180
    if (floors.length > 10) radius = 300
    if (floors.length > 30) radius = 500
    if (floors.length > 60) radius = 700

    const yaw = toRadians(-30)
    const pitch = toRadians(floors.length <= 50 ? -30 : 0)

    return [
      Math.sin(yaw) * Math.cos(pitch) * radius,
      Math.sin(yaw) * Math.sin(pitch) * radius,
      Math.cos(yaw) * radius,
    ]
  }, [floors.length])

  useEffect(
    () => () => {
      document.body.style.cursor = ''
    },
    []
  )

  const handleFloorClicked = (floor) => {
    if (floor.id) {
      const { siteId, scopeId } = params
      const siteIdFromScope = scopeLookup[scopeId]?.twin?.siteId
      const siteIdForNavigation = siteIdFromScope || siteId

      const { id: floorId } = floor
      analytics.track('Floor select from building', {
        siteId,
        floorId,
      })
      if (siteIdForNavigation) {
        history.push(`/sites/${siteIdForNavigation}/floors/${floorId}`)
      }
    } else {
      snackbar.show('Floor id does not exist', { icon: 'error' })
    }
  }

  const handleResetButtonClick = () => {
    setResetCamera(true)
  }

  useEffect(() => {
    const $tooltipTriggerEl = document.getElementById('floors-tooltip-trigger')

    if (dashboard.hoverFloorId && $tooltipTriggerEl) {
      updateTooltipStyles($tooltipTriggerEl, {
        mousePosition,
        display: true,
      })
    }
  }, [dashboard.hoverFloorId, mousePosition])

  return (
    <Flex position="absolute" className={styles.floors}>
      {floors.length === 0 && (
        <NotFound>{t('plainText.noFloorsFound')}</NotFound>
      )}
      {floors.length > 0 && (
        <Canvas
          camera={{
            position: cameraPosition,
            fov: 60,
          }}
          linear
        >
          <ambientLight />
          <pointLight position={[max, max, max]} intensity={0.4} />
          <mesh
            position={[0, (floors.length * 10) / 2, 0]}
            rotation={[Math.PI / 2, 0, 0]}
          >
            {floors.map((floor, i) => (
              <Floor
                key={floor.id}
                floor={floor}
                position={[0, 0, i * 10]}
                isHovering={
                  dashboard.hoverFloorId === floor.id ||
                  dashboard.isHoverFloorWholeSite
                }
                onClick={(e) => {
                  e.stopPropagation()
                  handleFloorClicked(floor)
                }}
                onPointerDown={() => {
                  if (userAgent.isIpad) {
                    handleFloorClicked(floor)
                  }
                }}
                onPointerOver={(e) => {
                  e.stopPropagation()
                  document.body.style.cursor = 'pointer'
                  dashboard.setHoverFloor(floor)
                }}
                onPointerMove={(e) => {
                  e.stopPropagation()
                  document.body.style.cursor = 'pointer'
                  setMousePosition({ x: e.offsetX, y: e.offsetY })
                  dashboard.setHoverFloor(floor)
                }}
                onPointerOut={() => {
                  dashboard.setHoverFloorId((prevHoverFloorId) => {
                    if (prevHoverFloorId === floor.id) {
                      document.body.style.cursor = ''
                      return undefined
                    }

                    return prevHoverFloorId
                  })
                }}
              />
            ))}
          </mesh>
          <Controls
            onControlsChange={() => {
              if (!isCameraMoved) {
                setIsCameraMoved(true)
              }
            }}
            resetCamera={resetCamera}
            onFinishResetCamera={() => {
              setResetCamera(false)
              setTimeout(() => {
                setResetCamera(false)
                setIsCameraMoved(false)
              }, 80)
            }}
          />
        </Canvas>
      )}

      {dashboard.hoverFloorId && (
        <Tooltip
          label={
            floors.find((floor) => floor.id === dashboard.hoverFloorId)?.name ??
            ''
          }
          opened
          position="bottom"
        >
          <Box id="floors-tooltip-trigger" pos="absolute" pt="s16" />
        </Tooltip>
      )}

      <Box pos="absolute" w="100%">
        <Group justify="space-between" m="s8">
          <Subheading>
            {featureFlags.hasFeatureToggle('buildingHomeDragAndDropRedesign')
              ? titleCase({ language, text: t('headers.activeInsights') })
              : ''}
          </Subheading>
          <Button kind="secondary" onClick={handleResetButtonClick}>
            {titleCase({ language, text: t('plainText.resetView') })}
          </Button>
        </Group>
      </Box>
    </Flex>
  )
}
