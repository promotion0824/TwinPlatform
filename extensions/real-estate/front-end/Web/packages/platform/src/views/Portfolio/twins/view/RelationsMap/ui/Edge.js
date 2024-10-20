// This file is adapted from
// https://reactflow.dev/docs/examples/floating-edges/
//
// It gives us the ability to have edges that touch the edges of the actual
// nodes, whereas by default the edges stop just short of the nodes and assume
// that you have a marker (basically a circle) covering the rest of the
// distance. It would be super cool not to have to do all this just to make the
// lines go a few pixels longer but I have not figured out a better way.
//
// We have also added the ability to control the line animation direction, by
// specifying { animation: "forward" } or { animation: "backward" } in the
// `data` prop. Note that the top-level `animate` prop supported by the default
// React Flow edge is *not* supported.

import { useCallback } from 'react'
import {
  Position,
  getBezierPath,
  getEdgeCenter,
  useStore,
} from 'react-flow-renderer'
import styled, { css, keyframes } from 'styled-components'
import { RELATIONS_MAP_NODE_SIZE } from '../funcs/utils'

/**
 * Make an edge from the expand button of the source node to the expand button
 * of the target node. This component contains hard-coded assumptions about the
 * sizes and locations of the expand nodes, in order to calculate the correct
 * line paths.
 */
export default function Edge(props) {
  const { id, source, target, markerEnd, style, label, labelStyle, data } =
    props
  const sourceNode = useStore(
    useCallback((store) => store.nodeInternals.get(source), [source])
  )
  const targetNode = useStore(
    useCallback((store) => store.nodeInternals.get(target), [target])
  )

  if (!sourceNode || !targetNode) {
    return null
  }

  const nodeCenterX = RELATIONS_MAP_NODE_SIZE / 2

  // We have two ways of drawing paths between nodes.
  //
  // If we are drawing a line to/from a twin node, the endpoint of the line is
  // the relevant expand button in the twin node. These look best when the
  // endpoint is roughly at the center of the expand button.
  //
  // Model nodes do not have expand buttons. So if we are drawing a line to/from
  // a model node, we use the default React Flow behaviour, whereby the endpoint
  // of the line is any point along the relevant edge.
  //
  // We support all permutations - model to model, model to twin, twin to
  // model, and twin to twin.

  let sourceBox, targetBox
  if (data.sourceNode.type === 'twin') {
    sourceBox = {
      width: 20,
      height: 20,
      position: {
        x: sourceNode.position.x + nodeCenterX,
        y: sourceNode.position.y + 5,
      },
    }
  } else {
    sourceBox = sourceNode
  }

  if (data.targetNode.type === 'twin') {
    targetBox = {
      width: 20,
      height: 20,
      position: {
        x: targetNode.position.x + nodeCenterX,
        y: targetNode.position.y + 60,
      },
    }
  } else {
    targetBox = targetNode
  }

  let { sx, sy, tx, ty, sourcePos, targetPos } = getEdgeParams(
    sourceBox,
    targetBox
  )

  // For twin nodes, we ignore the `sx`, `sy`, `tx` and `ty` attributes of the
  // result of `getEdgeParams`. It's not clear what they are for or why they
  // differ from the input source and target positions. And we want our lines
  // to begin and end at expand nodes, which are quite small, so diverging from
  // the coordinates of the input positions here means our lines don't go where
  // we want them.
  if (data.sourceNode.type === 'twin') {
    sx = sourceBox.position.x
    sy = sourceBox.position.y
  }
  if (data.targetNode.type === 'twin') {
    tx = targetBox.position.x
    ty = targetBox.position.y
  }

  const d = getBezierPath({
    sourceX: sx,
    sourceY: sy,
    sourcePosition: sourcePos,
    targetPosition: targetPos,
    targetX: tx,
    targetY: ty,
  })

  const [centerX, centerY] = getEdgeCenter({
    sourceX: sx,
    sourceY: sy,
    targetX: tx,
    targetY: ty,
  })

  return (
    <g className="react-flow__connection">
      <Path
        id={id}
        className="react-flow__edge-path"
        $animate={data.animate}
        d={d}
        markerEnd={markerEnd}
        style={style}
      />
      <g transform={`translate(${centerX}, ${centerY})`}>
        {/**
           Create two text elements with the same text: the first one functions as the
           background - it's the same as the background colour behind the line, and stops
           us drawing the label on top of the line. Then we draw the actual label.
           https://stackoverflow.com/a/41902064

           Also note that this section is the only place where this file
           differs from the example file it was taken from.
          */}
        <text
          className="react-flow__edge-text"
          style={{
            ...labelStyle,
            stroke: '#252525',
            strokeWidth: 6,
            textAnchor: 'middle',
          }}
        >
          {label}
        </text>
        <text
          className="react-flow__edge-text"
          style={{ ...labelStyle, fill: 'white', textAnchor: 'middle' }}
        >
          {label}
        </text>
      </g>
    </g>
  )
}

const animateForward = keyframes`
  from {
    stroke-dashoffset: 10;
  }

  to {
    stroke-dashoffset: 0;
  }
`

const animateBackward = keyframes`
  from {
    stroke-dashoffset: 0;
  }

  to {
    stroke-dashoffset: 10;
  }
`

const Path = styled.path`
  stroke-dasharray: 5;
  animation: ${({ $animate }) =>
    ['forward', 'backward'].includes($animate)
      ? css`
          ${$animate === 'forward'
            ? animateForward
            : animateBackward} 0.5s linear infinite;
        `
      : null};
`

// returns the parameters (sx, sy, tx, ty, sourcePos, targetPos) you need to create an edge
function getEdgeParams(source, target) {
  const sourceIntersectionPoint = getNodeIntersection(source, target)
  const targetIntersectionPoint = getNodeIntersection(target, source)

  const sourcePos = getEdgePosition(source, sourceIntersectionPoint)
  const targetPos = getEdgePosition(target, targetIntersectionPoint)

  return {
    sx: sourceIntersectionPoint.x,
    sy: sourceIntersectionPoint.y,
    tx: targetIntersectionPoint.x,
    ty: targetIntersectionPoint.y,
    sourcePos,
    targetPos,
  }
}

// this helper function returns the intersection point
// of the line between the center of the intersectionNode and the target node
function getNodeIntersection(intersectionNode, targetNode) {
  // https://math.stackexchange.com/questions/1724792/an-algorithm-for-finding-the-intersection-point-between-a-center-of-vision-and-a
  const {
    width: intersectionNodeWidth,
    height: intersectionNodeHeight,
    position: intersectionNodePosition,
  } = intersectionNode
  const targetPosition = targetNode.position

  const w = intersectionNodeWidth / 2
  const h = intersectionNodeHeight / 2

  const x2 = intersectionNodePosition.x + w
  const y2 = intersectionNodePosition.y + h
  const x1 = targetPosition.x + w
  const y1 = targetPosition.y + h

  const xx1 = (x1 - x2) / (2 * w) - (y1 - y2) / (2 * h)
  const yy1 = (x1 - x2) / (2 * w) + (y1 - y2) / (2 * h)
  const a = 1 / (Math.abs(xx1) + Math.abs(yy1))
  const xx3 = a * xx1
  const yy3 = a * yy1
  const x = w * (xx3 + yy3) + x2
  const y = h * (-xx3 + yy3) + y2

  return { x, y }
}

// returns the position (top,right,bottom or right) passed node compared to the intersection point
function getEdgePosition(node, intersectionPoint) {
  const n = { ...node.position, ...node }
  const nx = Math.round(n.x)
  const ny = Math.round(n.y)
  const px = Math.round(intersectionPoint.x)
  const py = Math.round(intersectionPoint.y)

  if (px <= nx + 1) {
    return Position.Left
  }
  if (px >= nx + n.width - 1) {
    return Position.Right
  }
  if (py <= ny + 1) {
    return Position.Top
  }
  if (py >= n.y + n.height - 1) {
    return Position.Bottom
  }

  return Position.Top
}
