import EChartsReact from 'echarts-for-react'
import { debounce } from 'lodash'
import { RefObject, useEffect, useLayoutEffect, useState } from 'react'

const DEBOUNCE_DELAY = 200

/** Ensure that charts keep their width sized correctly when a grid they are placed in is resized. */
export default function useResizeChartInGrid(
  /** A ref to the Chart component. */
  chartRef: RefObject<EChartsReact>,
  /** A ref to the component whose width should be watched for changes. */
  containerRef: RefObject<HTMLDivElement>,
  /** The delay in milliseconds to wait before resizing the chart after the container's width changes. */
  debounceDelay?: number
) {
  const [width, setWidth] = useState(0)

  useLayoutEffect(() => {
    if (!containerRef.current) return

    const observer = new ResizeObserver(
      debounce((entries) => {
        const { contentRect } = entries[0]
        if (contentRect.width) setWidth(contentRect.width)
      }, debounceDelay ?? DEBOUNCE_DELAY)
    )

    observer.observe(containerRef.current)

    return () => observer.disconnect()
  }, [containerRef, debounceDelay])

  useEffect(() => {
    if (width) chartRef.current?.getEchartsInstance()?.resize()
  }, [chartRef, width])
}
