/* eslint-disable complexity */
import { Flex, Time, useAnalytics, useDateTime, useDuration } from '@willow/ui'
import { Icon, Select, useTheme } from '@willowinc/ui'
import _ from 'lodash'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'
import { v4 as uuidv4 } from 'uuid'
import GranularitySelect from '../Granularity/GranularitySelect'
import { useTimeSeriesGraph } from '../TimeSeriesGraphContext'
import Graph from './Graph'
import styles from './GraphContent.css'
import GraphTooltip from './GraphTooltip/GraphTooltip'

export default function GraphContent({
  enabledDisplayByAsset,
  graphs,
  onTimesChange,
  onTypeChange,
  granularity,
  type,
  times,
  onGranularityChange,
  loadingSitePointIds,
  shadedDurations,
  diagnosticBoundaries,
}) {
  const theme = useTheme()
  // Ref to detect touchDown event and user is moving within the graph.
  const isTouchDownRef = useRef(false)

  const dateTime = useDateTime()
  const duration = useDuration()
  const analytics = useAnalytics()
  const { t } = useTranslation()
  const timeSeriesGraph = useTimeSeriesGraph()

  const lineRef = useRef()
  const [state, setState] = useState()
  const [prevIndex, setPrevIndex] = useState(0)
  const [isGraphReady, setIsGraphReady] = useState(false)

  // without keeping track of the graph's readiness,
  // the shaded duration will not be rendered correctly
  // when the graph is first loaded
  useEffect(() => {
    if (timeSeriesGraph.contentRef.current != null) {
      setIsGraphReady(true)
    } else {
      setIsGraphReady(false)
    }
  }, [timeSeriesGraph.contentRef])

  const minX = new Date(timeSeriesGraph.times[0]).valueOf()
  const maxX = new Date(timeSeriesGraph.times[1]).valueOf()
  const diffX = maxX - minX
  /**
   * Array<{left: number, width: number, color: 'red' | 'orange', id: string}>
   * where left is the starting x position of the shaded duration and
   * width is the width of the shaded duration
   */
  const memoizedShadedDurations = useMemo(
    () =>
      shadedDurations
        ?.map(({ start, end, color }) => {
          const startInMilliseconds = new Date(start).valueOf()
          const endInMilliseconds = new Date(end).valueOf()
          const graphStartX = isGraphReady
            ? getGraphX(
                getRelativeXForTime(Math.max(startInMilliseconds, minX))
              )
            : 0
          return {
            left: graphStartX,
            width: isGraphReady
              ? getGraphX(
                  getRelativeXForTime(Math.min(endInMilliseconds, maxX))
                ) - graphStartX
              : 0,
            color,
            id: uuidv4(),
          }
        })
        .sort((a, b) => a.left - b.left)
        .filter(({ width }) => width > 0) ?? [],
    [shadedDurations, isGraphReady, getGraphX, getRelativeXForTime, minX, maxX]
  )

  const { insufficientOverlayBackground, faultyOverlayBackground } =
    useMemo(() => {
      const insufficientDurations = []
      const faultyDurations = []
      for (const { left, width, color } of memoizedShadedDurations) {
        if (color === 'red') {
          faultyDurations.push({ left, width })
        } else {
          insufficientDurations.push({ left, width })
        }
      }

      return {
        insufficientOverlayBackground: insufficientDurations
          .map(
            ({ left, width }) => `
        repeating-linear-gradient(-45deg, transparent, transparent 10px, ${theme.color.core.gray.bg.subtle.default} 10px, ${theme.color.core.gray.bg.subtle.default} 20px) ${left}px 0/${width}px 100% no-repeat  
        `
          )
          .join(', '),
        faultyOverlayBackground: faultyDurations
          .map(
            ({ left, width }) =>
              `linear-gradient(${red}, ${red}) ${left}px 0/${width}px 100% no-repeat`
          )
          .join(', '),
      }
    }, [memoizedShadedDurations])

  const memoizedDiagnosticBoundaries = useMemo(() => {
    if (diagnosticBoundaries == null) {
      return null
    }
    const [startBoundary, endBoundary] = diagnosticBoundaries
    const startInMilliseconds = new Date(startBoundary).valueOf()
    const endInMilliseconds = new Date(endBoundary).valueOf()
    const graphStartX = isGraphReady
      ? getGraphX(getRelativeXForTime(Math.max(startInMilliseconds, minX)))
      : 0
    return {
      left: graphStartX,
      width: isGraphReady
        ? getGraphX(getRelativeXForTime(Math.min(endInMilliseconds, maxX))) -
          graphStartX
        : 0,
      id: uuidv4(),
    }
  }, [
    diagnosticBoundaries,
    getGraphX,
    getRelativeXForTime,
    isGraphReady,
    maxX,
    minX,
  ])

  function getSvgGraphElement(parentRef) {
    return parentRef.current.querySelectorAll('[data-graph]')[0]
  }

  function getGraphX(relativeX) {
    const svgGraph = getSvgGraphElement(timeSeriesGraph.contentRef)
    const rect = timeSeriesGraph.contentRef.current.getBoundingClientRect()
    const childRect = svgGraph.getBoundingClientRect()
    return relativeX * childRect.width + childRect.x - rect.x
  }

  let timeWindow
  if (state?.startX != null && state?.nextX != null) {
    const xValues = _.orderBy([state.startX, state.nextX])
    timeWindow = {
      left: xValues[0],
      right: xValues[1],
      graphLeft: getGraphX(xValues[0]),
      width: getGraphX(xValues[1]) - getGraphX(xValues[0]),
    }
  }

  // This will return the relative positioning of the pointer on the graph, between 0 to 1.
  function getRelativeX(e) {
    const svgGraph = getSvgGraphElement(timeSeriesGraph.contentRef)
    const childRect = svgGraph.getBoundingClientRect()

    const clientX = e.touches?.[0].clientX ?? e.clientX
    return (clientX - childRect.x) / childRect.width
  }

  function getTime(relativeX) {
    return minX + relativeX * (maxX - minX)
  }

  function getRelativeXForTime(time) {
    return (time - minX) / diffX
  }

  // In time series, there can be multiple graphs stacks on top of each other.
  // This will return the closest graph's index where the cursor is located.
  function getGraphIndex(e) {
    let index
    const graphElement = e.target.closest('[data-graph]')
    if (graphElement != null) {
      index =
        [
          ...graphElement.parentNode.parentNode.parentNode.parentNode.parentNode
            .children,
        ].indexOf(graphElement.parentNode.parentNode.parentNode.parentNode) - 1
      setPrevIndex(index)
    } else {
      index = prevIndex
    }

    return index
  }

  function handleTypeChange(nextType) {
    analytics.track('Time Series Type Changed', {
      type: nextType === 'asset' ? 'Twin' : nextType,
    })
    onTypeChange(nextType)
  }

  // When hovered on the graph, calculate positioning used for
  // tooltips and the vertical line that shows current time position.
  function handlePointerMove(e) {
    if (!timeSeriesGraph.contentRef.current?.childNodes?.[0]) {
      return
    }

    const index = getGraphIndex(e)
    const indexGraph = graphs[index]
    const svgGraph = getSvgGraphElement(timeSeriesGraph.contentRef)
    const rect = timeSeriesGraph.contentRef.current.getBoundingClientRect()
    const childRect = svgGraph.getBoundingClientRect()

    // Display the tooltip only within the graph.
    // When relative x is < 0, then pointer is out of the graph on the left side.
    // When relative x is >= 1, then pointer is out of the graph on the right side.
    const relativeX = getRelativeX(e)
    // For touch devices, disabled this condition due to edge case when trying to view tooltip for first/last data point. Fat finger may go outside of the graph.
    if ((relativeX < 0 || relativeX >= 1.0) && !isTouchDownRef.current) {
      setState()
      return
    }

    const currentTime = getTime(relativeX)

    if (currentTime == null) {
      setState()
      return
    }

    const lines = graphs
      .filter(
        (graph) =>
          (indexGraph?.type === 'binary' && graph.type === 'binary') ||
          (indexGraph?.type === 'multiState' && graph.type === 'multiState') ||
          indexGraph === graph
      )
      .flatMap((graph) => graph.lines)
      .map((line) => ({
        ...line,
        item: line.data.find((x) => x.time >= currentTime),
      }))
      .filter((line) => line.item != null)

    const highlightPointTime = lines[0]?.item.time

    const timeIntervalOnRight =
      highlightPointTime + duration(timeSeriesGraph.granularity).milliseconds()

    let left = relativeX * childRect.width + childRect.x - rect.x
    if (highlightPointTime) {
      left = getGraphX(getRelativeXForTime(highlightPointTime))
    }

    let right = relativeX * childRect.width + childRect.x - rect.x
    if (timeIntervalOnRight) {
      right = getGraphX(getRelativeXForTime(timeIntervalOnRight))
    }
    const lineWidth = right - left

    setState((prevState) => ({
      ...prevState,
      index,
      nextX: relativeX,
      left,
      lines,
      highlightPointTime,
      lineWidth,
    }))
  }

  function handleTouchStart() {
    isTouchDownRef.current = true

    setState({ startX: undefined })
  }

  function handleTouchEnd() {
    isTouchDownRef.current = false
  }

  // When cursor is moved out of the graph, remove any current position tooltip and current position vertical line.
  // If it's a touch device, tapping the graph once will remove the position tooltips and vertical line.
  function handlePointerLeave() {
    setState()
    setPrevIndex()
  }

  // This is part of selecting datetime range functionality on the graph.
  // When click and hold, set initial start datetime and display purple highlighted section.
  function handlePointerDown(e) {
    if (e.button === 0 && !isTouchDownRef.current) {
      const startX = getRelativeX(e)
      if (startX == null) {
        return
      }

      const index = getGraphIndex(e)

      setState((prevState) => ({
        ...prevState,
        startX,
        index,
      }))
    }
  }

  // This is part of selecting datetime range functionality on the graph.
  // User should already be holding a click and purple highlighted section is being displayed,
  // When let go of the click, set datetime range to match the start and end of the purple highlighted section.
  function handlePointerUp() {
    if (timeWindow != null) {
      const start = getTime(timeWindow.left)
      const end = getTime(timeWindow.right)
      const startDateTime = dateTime(start, timeSeriesGraph.timeZone).format()
      const endDateTime = dateTime(end, timeSeriesGraph.timeZone).format()

      if (
        startDateTime !== endDateTime &&
        dateTime(endDateTime, timeSeriesGraph.timeZone).differenceInMinutes(
          startDateTime
        ) > 15
      ) {
        onTimesChange([startDateTime, endDateTime], true)
      }

      setState((prevState) => ({
        ...prevState,
        startX: undefined,
      }))
    }
  }

  return (
    <Container data-testid="graph-content-container">
      <SelectorsContainer>
        <GranularitySelect
          times={times}
          granularity={granularity}
          onGranularityChange={onGranularityChange}
          css={{ width: 120 }}
        />
        <Select
          css={{ width: 120 }}
          data={[
            ...(enabledDisplayByAsset
              ? [{ label: t('plainText.twin'), value: 'asset' }]
              : []),
            { label: t('plainText.grouped'), value: 'grouped' },
            { label: t('plainText.stacked'), value: 'stacked' },
            { label: t('plainText.shared'), value: 'shared' },
          ]}
          value={type}
          onChange={handleTypeChange}
          prefix={<Icon icon="layers" />}
        />
      </SelectorsContainer>
      <Flex
        ref={timeSeriesGraph.contentRef}
        size="medium"
        padding="medium 0"
        className={styles.content}
        onPointerDown={handlePointerDown}
        onPointerUp={handlePointerUp}
        onPointerMove={handlePointerMove}
        onPointerLeave={handlePointerLeave}
        onTouchStart={handleTouchStart}
        onTouchEnd={handleTouchEnd}
        onTouchCancel={handleTouchEnd}
      >
        <div />
        {graphs.map((graph) => (
          <Graph
            key={graph.key}
            graph={graph}
            highlightPointTime={state?.highlightPointTime}
            loadingSitePointIds={loadingSitePointIds}
          />
        ))}
        {/*  
          business requirement to shade mini time series graph with red overlay when
          occurrence data is faulty, or with gray striped overlay when occurrence data is insufficient
          as per design: https://www.figma.com/file/6BVcLlzhfo3KbNhAK52M7J/Time-Series?node-id=1048%3A13317&mode=dev       
        */}
        {isGraphReady && (
          <>
            <BaseOverlay
              data-testid="faulty-overlay"
              css={css({
                background: faultyOverlayBackground,
              })}
            />
            <BaseOverlay
              data-testid="insufficient-data-overlay"
              css={css({
                background: insufficientOverlayBackground,
              })}
            />
            {memoizedDiagnosticBoundaries &&
              memoizedDiagnosticBoundaries.width > 0 && (
                <DiagnosticOverlay
                  data-testid={`graph-overlay-${memoizedDiagnosticBoundaries.id}`}
                  left={memoizedDiagnosticBoundaries.left}
                  width={memoizedDiagnosticBoundaries.width}
                />
              )}
          </>
        )}
        {state?.lines?.length > 0 && (
          <>
            {/* Vertical line that indicate where the highlight point time is on the graph. */}
            <div
              ref={lineRef}
              className={styles.line}
              style={{
                left: state.left - state?.lineWidth / 2,
                width: state?.lineWidth,
              }}
            />
            {/* DateTime tooltip at the top of the graph. */}
            {state.lines[0]?.item != null && (
              <Flex
                align="center"
                className={styles.time}
                style={{ left: state.left }}
              >
                <div>
                  <Time
                    timezone={timeSeriesGraph.timeZone}
                    value={state.lines[0].item.time}
                    format="date"
                  />
                </div>
                <div>
                  <Time
                    timezone={timeSeriesGraph.timeZone}
                    value={state.lines[0].item.time}
                    format="time"
                  />
                </div>
                {memoizedDiagnosticBoundaries &&
                  (state?.left ?? 0) >= memoizedDiagnosticBoundaries.left &&
                  (state?.left ?? 0) <=
                    memoizedDiagnosticBoundaries.left +
                      memoizedDiagnosticBoundaries.width && (
                    <div>{t('plainText.diagnosticOccurrencePeriods')}</div>
                  )}
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
                  left: timeWindow.graphLeft,
                  width: timeWindow.width,
                }}
              />
            )}
          </>
        )}
      </Flex>
    </Container>
  )
}

const BaseOverlay = styled.div({
  width: '100%',
  height: 'calc(100% - 90px)',
  pointerEvents: 'none',
  position: 'absolute',
  top: '80px',
})

/**
 * an opaque overlay with dashed left/right border to indicate the diagnostic occurrence periods
 */
const DiagnosticOverlay = styled.div(({ theme, left, width }) => ({
  pointerEvents: 'none',
  position: 'absolute',
  top: '80px',
  bottom: 0,
  left,
  width,
  borderLeft: '1px solid',
  borderRight: '1px solid',
  borderLeftColor: 'transparent',
  borderImage: `linear-gradient(to bottom, ${theme.color.core.purple.fg.default} 50%, transparent 50%)`,
  borderImageSlice: 20,
  borderImageRepeat: 'repeat',
}))

const Container = styled.div`
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
`

const SelectorsContainer = styled.div(
  ({ theme }) => css`
    display: flex;
    gap: ${theme.spacing.s8};
    align-self: flex-end;
    padding: ${theme.spacing.s} ${theme.spacing.s16};
  `
)

const red = 'rgba(176, 43, 51, 0.30)'
