import { ComponentView, ECharts, getInstanceByDom, graphic } from 'echarts'
import EChartsReact from 'echarts-for-react'
import { debounce } from 'lodash'
import { RefObject, useEffect, useLayoutEffect, useState } from 'react'

const LEGEND_MARGIN = 12

/** Returns the height of the chart legend, updated whenever the chart's width changes. */
export default function useLegendHeight(chartRef: RefObject<EChartsReact>) {
  const [echartsInstance, setEchartsInstance] = useState<ECharts>()
  const [legendHeight, setLegendHeight] = useState(0)

  useLayoutEffect(() => {
    if (!chartRef.current || !echartsInstance) return

    const observer = new ResizeObserver(
      debounce(() => {
        const components:
          | (ComponentView & { _backgroundEl?: graphic.Rect; type: string }[])
          | undefined =
          // ECharts doesn't provide a way to access its components other than
          // through this internal method. 😢
          // eslint-disable-next-line @typescript-eslint/ban-ts-comment
          // @ts-ignore
          echartsInstance._componentsViews

        const legend = components?.find((c) => c.type === 'legend.plain')
        const legendHeight = legend?._backgroundEl?.shape.height
        setLegendHeight(legendHeight ? legendHeight + LEGEND_MARGIN : 0)
      }, 200)
    )

    observer.observe(chartRef.current.ele)

    return () => observer.disconnect()
  }, [chartRef, echartsInstance])

  useEffect(() => {
    if (chartRef.current && !echartsInstance) {
      setEchartsInstance(getInstanceByDom(chartRef.current.ele))
    }
  }, [chartRef, echartsInstance])

  return legendHeight
}
