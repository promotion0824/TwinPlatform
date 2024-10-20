/* eslint-disable complexity */
import { useSize } from '@willow/ui'
import { Box, Group } from '@willowinc/ui'
import { useEffect, useLayoutEffect, useRef, useState } from 'react'
import { css } from 'styled-components'
import { useAutodeskViewer } from '../../AutodeskViewerContext'
import colors from './colors'
import {
  CriticalInsightSprite,
  CriticalTicketSprite,
  NoncriticalInsightSprite,
  NoncriticalTicketSprite,
} from './CustomSvg'

// As a legacy way to allow onClick in iframe to
// trigger selectAsset function in parent window
// defined in '../../AutodeskViewerProvider',
// as a result, TypeScript declaration is needed here.
declare global {
  interface Window {
    selectAssetFromViewer?: (assetId: string) => void
  }
}

type Node = {
  dbId: number
  forgeViewerAssetId: string
  box: THREE.Box3
  hasCriticalInsight?: boolean
  hasCriticalTicket?: boolean
  insightCount?: number
  ticketCount?: number
}

/**
 * When the model is shown, this component will color the model based on the insight stats.
 */
export default function ShowColorfulModel({
  model,
}: {
  model: {
    nodes: Node[]
    model: Autodesk.Viewing.Model
  }
}) {
  const autodeskViewer = useAutodeskViewer()
  const [coloredNodes, setColoredNodes] = useState<Node[]>([])

  useEffect(() => {
    autodeskViewer.viewer.showModel(model.model.id)
    autodeskViewer.handleShowModel(model)

    return () => {
      autodeskViewer.viewer.hideModel(model.model.id)
    }
  }, [])

  useEffect(() => {
    const localColoredNodes: Node[] = []
    if (!autodeskViewer.isInsightStatsOn && !autodeskViewer.isTicketStatsOn) {
      autodeskViewer.viewer.clearThemingColors(model.model)
    }
    // Since performance is absolutely critical here in 3D viewer,
    // We loop through the nodes exactly twice and set the color for each node;
    // The first time is to color everything object grey,
    // and the second time is to color the nodes according to highest insight/ticket priority.
    // The coloring has to be done twice because there could be cases where a parent object
    // has a insight priority while its children have no priority, and we want to color the parent object.
    else if (
      (autodeskViewer.isInsightStatsOn || autodeskViewer.isTicketStatsOn) &&
      !!autodeskViewer.dataStats
    ) {
      for (const node of model.nodes) {
        autodeskViewer.viewer.setThemingColor(
          node.dbId,
          colors.grey,
          model.model,
          true
        )
      }

      for (const node of model.nodes) {
        const stats = autodeskViewer.dataStats[node.forgeViewerAssetId]

        // When both insight and ticket stats are on, we color the model based on the highest priority;
        // otherwise, we color the model based on the highest priority of the turned-on stats.
        // Note: The priority is 1 for critical, 2 for high, 3 for medium, and 4 for low.
        // so the highest priority is the lowest number that is greater than 0.
        const highestPriority = Math.max(
          Math.min(
            autodeskViewer.isInsightStatsOn && stats?.insightHighestPriority
              ? stats.insightHighestPriority
              : Infinity,
            autodeskViewer.isTicketStatsOn && stats?.ticketHighestPriority
              ? stats.ticketHighestPriority
              : Infinity
          ),
          0
        )

        if (stats) {
          autodeskViewer.viewer.setThemingColor(
            node.dbId,
            colorMap[highestPriority] ?? colors.grey,
            model.model,
            true
          )

          if (stats.insightCount > 0 || stats.ticketCount > 0) {
            localColoredNodes.push({
              ...node,
              hasCriticalInsight: stats.insightHighestPriority === 1,
              hasCriticalTicket: stats.ticketHighestPriority === 1,
              insightCount: stats.insightCount ?? 0,
              ticketCount: stats.ticketCount ?? 0,
            })
          }
        }
      }
      // Force the edges to be displayed when the model is colored
      // as there is IPad user feedback that the edges are not visible
      // when turning on the insight layer
      autodeskViewer.viewer.setDisplayEdges(true)
    }

    setColoredNodes(localColoredNodes)
  }, [
    autodeskViewer.viewer.setThemingColor,
    autodeskViewer.dataStats,
    autodeskViewer.isInsightStatsOn,
    autodeskViewer.isTicketStatsOn,
    autodeskViewer.viewer,
    model.model,
    model.nodes,
  ])

  // Only show tooltip for nodes with insights/tickets and at least 1 layer is on
  if (
    (!autodeskViewer.isInsightStatsOn && !autodeskViewer.isTicketStatsOn) ||
    !autodeskViewer.dataStats
  ) {
    return null
  }

  // If there are no colored nodes, don't render the tooltip
  if (coloredNodes.length === 0) {
    return null
  }

  return (
    <>
      {coloredNodes.map((node) => (
        <Tooltip
          key={node.forgeViewerAssetId}
          viewer={autodeskViewer.viewer}
          node={node}
          onClick={(tab) => {
            autodeskViewer.selectAsset(node)
            // This was how iframe triggering select asset function in parent window
            // Similar to what has been done in '../../AutodeskViewerProvider'
            window.parent.document?.defaultView?.selectAssetFromViewer?.(
              node.forgeViewerAssetId
            )
            window.parent.postMessage(
              {
                type: 'selectedDataTabChange',
                selectedDataTab: tab,
              },
              window.location.origin
            )
          }}
          isInsightStatsOn={autodeskViewer.isInsightStatsOn}
          isTicketStatsOn={autodeskViewer.isTicketStatsOn}
        />
      ))}
    </>
  )
}

/**
 * Color mapping for coloring nodes on 3D viewer based on hightest priority of insights.
 */
const colorMap = {
  1: colors.critical,
  2: colors.high,
  3: colors.medium,
  4: colors.low,
}

/**
 * This component is used to render the tooltip on the colored nodes in the model.
 * It is an adaptation of the Tooltip from '../../Tooltip/Tooltip'
 */
function Tooltip({
  viewer,
  node,
  onClick,
  isInsightStatsOn = false,
  isTicketStatsOn = false,
}: {
  viewer: Autodesk.Viewing.Viewer3D
  node: Node
  onClick?: (tab: string) => void
  isInsightStatsOn: boolean
  isTicketStatsOn: boolean
}) {
  const cameraPosition = viewer.getCamera().position
  const tooltipRef = useRef<HTMLDivElement>(null)
  const [style, setStyle] = useState({
    left: 0,
    top: 0,
  })

  const size = useSize(tooltipRef)

  const { box } = node
  const position = box?.getCenter(new THREE.Vector3())
  position?.setZ(box?.max.z ?? 0)

  useLayoutEffect(() => {
    const refresh = () => {
      if (position == null) {
        return
      }

      const { x, y } = viewer.worldToClient(position)

      setStyle({
        left: x,
        top: y + (tooltipRef.current?.offsetHeight ?? 0) / 2,
      })
    }

    eventsToRefresh.forEach((event) => {
      viewer.addEventListener(event, refresh)
    })
    refresh()

    return () => {
      eventsToRefresh.forEach((event) => {
        viewer.removeEventListener(event, refresh)
      })
    }
  }, [size])

  if (
    position == null ||
    isOutOfView(style.left, style.top, viewer) ||
    cameraPosition.distanceTo(position) > 100
  ) {
    return null
  }

  return (
    <Box
      component="div"
      css={css(({ theme }) => ({
        transform: 'translate(-50%, -100%)',
        position: 'absolute',
        whiteSpace: 'nowrap',
        zIndex: 6,
        color: theme.color.neutral.fg.muted,
        borderRadius: theme.radius.round,
        display: 'flex',
        justifyContent: 'center',
        cursor: 'pointer',
      }))}
      ref={tooltipRef}
      style={style}
    >
      <Group
        gap={0}
        // Slightly move the 2nd icon to the left per design, if both icons are shown
        css={`
          & > *:nth-child(2) {
            transform: translateX(
              ${isTicketStatsOn && isTicketStatsOn ? '-15px' : '0'}
            );
          }
        `}
      >
        <Box onClick={() => onClick?.('tickets')}>
          {isTicketStatsOn &&
            (node.hasCriticalTicket ? (
              <CriticalTicketSprite />
            ) : (node?.ticketCount ?? 0) > 0 ? (
              <NoncriticalTicketSprite />
            ) : null)}
        </Box>
        <Box onClick={() => onClick?.('insights')}>
          {isInsightStatsOn &&
            (node.hasCriticalInsight ? (
              <CriticalInsightSprite />
            ) : (node?.insightCount ?? 0) > 0 ? (
              <NoncriticalInsightSprite />
            ) : null)}
        </Box>
      </Group>
    </Box>
  )
}

const eventsToRefresh = [
  Autodesk.Viewing.CAMERA_CHANGE_EVENT,
  Autodesk.Viewing.ISOLATE_EVENT,
  Autodesk.Viewing.HIDE_EVENT,
  Autodesk.Viewing.SHOW_EVENT,
]

const isOutOfView = (
  x: number,
  y: number,
  viewer: Autodesk.Viewing.Viewer3D
) => {
  const { clientWidth, clientHeight } = viewer.container
  return x < 0 || y < 0 || x > clientWidth || y > clientHeight
}
