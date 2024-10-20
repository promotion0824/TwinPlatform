import { Fragment, useRef, useState } from 'react'
import _ from 'lodash'
import { useDateTime, Flex, Text, Time } from '@willow/ui'
import { useGraph } from '../GraphContext'
import LineGraph from './LineGraph/LineGraph'
import GraphTooltip from './GraphTooltip/GraphTooltip'
import styles from './GraphContent.css'

export default function GraphContent() {
  const dateTime = useDateTime()
  const graphContext = useGraph()

  const lineRef = useRef()
  const [state, setState] = useState()

  let timeWindow
  if (state?.startX != null && state?.nextX != null) {
    const xValues = _.orderBy([state.startX, state.nextX])
    timeWindow = {
      left: xValues[0],
      width: xValues[1] - xValues[0],
    }
  }

  function getXValue(x, next) {
    if (graphContext.contentRef.current?.childNodes?.[0] == null) {
      return null
    }

    const rect = graphContext.contentRef.current.getBoundingClientRect()
    const childRect =
      graphContext.contentRef.current.childNodes[0].getBoundingClientRect()
    const times = [
      dateTime(graphContext.times[0]).valueOf(),
      dateTime(graphContext.times[1]).valueOf(),
    ]

    const relativeX = (x - childRect.x) / childRect.width
    if (relativeX < 0 || relativeX > 1) {
      return null
    }

    const FIFTEEN_MINUTES = 15 * 60 * 1000
    let time = relativeX * (times[1] - times[0]) + times[0]
    time -= time % FIFTEEN_MINUTES
    if (next) {
      time += FIFTEEN_MINUTES
    }

    const relativeStartX = (time - times[0]) / (times[1] - times[0])

    const xValue = relativeStartX * childRect.width + (childRect.x - rect.x)

    return xValue
  }

  function getTime(x) {
    const rect = graphContext.contentRef.current.getBoundingClientRect()
    const childRect =
      graphContext.contentRef.current.childNodes[0].getBoundingClientRect()

    const { bounds } = graphContext.graphs[0]

    const diff = childRect.left - rect.left
    const relativeStartX = (x - diff) / childRect.width

    return dateTime(
      relativeStartX * (bounds.maxX - bounds.minX) + bounds.minX
    ).format()
  }

  function handlePointerDown(e) {
    if (e.button === 0) {
      const startX = getXValue(e.clientX)
      if (startX == null) {
        return
      }

      let index
      const graphElement = e.target.closest('[data-graph]')
      if (graphElement != null) {
        index = [...graphElement.parentNode.children].indexOf(graphElement)
      }

      setState((prevState) => ({
        ...prevState,
        startX,
        index,
      }))
    }
  }

  function handlePointerUp() {
    if (timeWindow != null) {
      const start = getTime(timeWindow.left)
      const end = getTime(timeWindow.left + timeWindow.width)

      if (start !== end && dateTime(end).differenceInMinutes(start) > 15) {
        graphContext.onTimesChange([start, end])
      }

      setState((prevState) => ({
        ...prevState,
        startX: undefined,
      }))
    }
  }

  function handlePointerMove(e) {
    if (graphContext.contentRef.current?.childNodes?.[0] == null) {
      return
    }

    let index
    const graphElement = e.target.closest('[data-graph]')
    if (graphElement != null) {
      index = [...graphElement.parentNode.children].indexOf(graphElement)
    }

    const rect = graphContext.contentRef.current.getBoundingClientRect()
    const childRect =
      graphContext.contentRef.current.childNodes[0].getBoundingClientRect()
    const relativeX = (e.clientX - childRect.x) / childRect.width

    if (relativeX < 0 || relativeX >= 1.0) {
      setState()
      return
    }

    const x = graphContext.xValues.find((nextXValue) => nextXValue > relativeX)

    let nextX = getXValue(e.clientX, true)
    if (nextX <= state?.startX) {
      nextX = getXValue(e.clientX, false)
    }

    if (x == null) {
      setState()
      return
    }

    const left = x * childRect.width + childRect.x - rect.x
    const indexGraph = graphContext.graphs[index]

    const lines = graphContext.graphs
      .filter(
        (graph) =>
          (indexGraph?.type === 'boolean' && graph.type === 'boolean') ||
          indexGraph === graph
      )
      .flatMap((graph) => graph.lines)
      .map((line) => ({
        ...line,
        item: line.data[line.points.findIndex((point) => point.x === x)],
      }))
      .filter((line) => line.item != null)

    setState((prevState) => ({
      ...prevState,
      x,
      index,
      nextX,
      left,
      lines,
    }))
  }

  function handlePointerLeave() {
    setState()
  }

  return (
    <Flex
      ref={graphContext.contentRef}
      className={styles.graphContent}
      onPointerMove={handlePointerMove}
      onPointerLeave={handlePointerLeave}
      onPointerDown={handlePointerDown}
      onPointerUp={handlePointerUp}
    >
      <Flex height="100%" size="medium">
        {graphContext.graphs.map((graph) => (
          <Fragment key={`${graph.id} ${graph.lines.length}`}>
            {graph.type === 'line' && <LineGraph x={state?.x} graph={graph} />}
          </Fragment>
        ))}
        {state?.lines?.length > 0 && (
          <>
            <div
              ref={lineRef}
              className={styles.line}
              style={{ left: state.left }}
            />
            {state.lines[0]?.item != null && (
              <Flex
                align="center"
                className={styles.time}
                style={{ left: state.left }}
              >
                <Text size="tiny">
                  <div>
                    <Time value={state.lines[0].item.x} format="date" />
                  </div>
                  <div>
                    <Time value={state.lines[0].item.x} format="time" />
                  </div>
                </Text>
              </Flex>
            )}
            <GraphTooltip
              key={state.index}
              target={lineRef.current}
              lines={state.lines}
              index={state.index}
            />
            {timeWindow != null && (
              <div
                className={styles.timeWindow}
                style={{
                  left: timeWindow.left,
                  width: timeWindow.width,
                }}
              />
            )}
          </>
        )}
      </Flex>
    </Flex>
  )
}
