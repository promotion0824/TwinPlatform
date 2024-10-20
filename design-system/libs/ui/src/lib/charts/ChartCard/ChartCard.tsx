import { Box, BoxProps } from '@mantine/core'
import * as echarts from 'echarts'
import EChartsReact from 'echarts-for-react'
import {
  cloneElement,
  forwardRef,
  useEffect,
  useRef,
  useState,
  type ReactElement,
} from 'react'
import styled from 'styled-components'
import { Icon } from '../../misc/Icon'
import { Tooltip } from '../../overlays/Tooltip'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { ChartTable } from '../ChartTable'
import { allChartTypes } from '../utils'
import { ChartCardMenu } from './ChartCardMenu'

export interface ChartCardProps
  extends WillowStyleProps,
    Omit<BoxProps, keyof WillowStyleProps> {
  /** The chart to be displayed. */
  chart: ReactElement
  /** Description of the chart shown in a tooltip in the header. */
  description?: string
  /** Title of the chart to be shown in the header. */
  title: string
}

const CardBody = styled.div(({ theme }) => ({
  height: '100%',
  padding: theme.spacing.s16,
}))

const CardContainer = styled(Box<'div'>)(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.panel.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
}))

const CardHeader = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  justifyContent: 'space-between',
  padding: `${theme.spacing.s4} ${theme.spacing.s8} ${theme.spacing.s4} ${theme.spacing.s16}`,
}))

const CardTitle = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
  color: theme.color.neutral.fg.default,
  display: 'flex',
  gap: theme.spacing.s8,
}))

/**
 * `ChartCard` is a container for any Chart component, adding a common title and toolbar.
 */
export const ChartCard = forwardRef<HTMLDivElement, ChartCardProps>(
  ({ chart, description, title, ...restProps }, ref) => {
    const chartRef = useRef<EChartsReact>(null)
    const [echartsInstance, setEchartsInstance] = useState<echarts.ECharts>()
    const isChartTable = chart.type === ChartTable

    useEffect(() => {
      if (chartRef.current && !echartsInstance) {
        setEchartsInstance(echarts.getInstanceByDom(chartRef.current.ele))
      }
    }, [echartsInstance])

    if (
      !isChartTable &&
      !allChartTypes.find((type) => type.Component === chart.type)
    ) {
      throw new Error(
        `ChartCard only supports the following chart types: ${[
          ...allChartTypes.map((type) => type.name),
          'ChartTable',
        ]
          .sort()
          .join(', ')}`
      )
    }

    return (
      <CardContainer
        ref={ref}
        {...restProps}
        {...useWillowStyleProps(restProps)}
      >
        <CardHeader>
          <CardTitle>
            <div>{title}</div>
            {description && (
              <Tooltip label={description} position="top" withinPortal>
                <Icon filled icon="info" />
              </Tooltip>
            )}
          </CardTitle>
          {isChartTable ? (
            <ChartCardMenu data={chart.props.rows} title={title} />
          ) : (
            <ChartCardMenu echartsInstance={echartsInstance} title={title} />
          )}
        </CardHeader>
        <CardBody>
          {isChartTable ? chart : cloneElement(chart, { ref: chartRef })}
        </CardBody>
      </CardContainer>
    )
  }
)
