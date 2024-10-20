import { allChartTypes, Group, Icon, Stack, Tooltip } from '@willowinc/ui'
import { forwardRef, ReactElement } from 'react'
import styled from 'styled-components'
import { Tile, TileProps } from '../common'

export interface ChartTileProps extends TileProps {
  /** The chart to be displayed. */
  chart: ReactElement
  /** Description of the chart shown in a tooltip in the header. */
  description?: string
  /** Title of the chart to be shown in the header. */
  title: string
}

const Label = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
}))

export const ChartTile = forwardRef<HTMLDivElement, ChartTileProps>(
  ({ chart, description, title, ...restProps }, ref) => {
    if (!allChartTypes.find((type) => type.Component === chart.type)) {
      throw new Error(
        `ChartCard only supports the following chart types: ${allChartTypes
          .map((type) => type.name)
          .sort()
          .join(', ')}`
      )
    }

    return (
      <Tile h={240} ref={ref} title={title} {...restProps}>
        <Stack gap="s12" h="100%">
          <Group c="neutral.fg.default" gap="s4">
            <Label>{title}</Label>
            {description && (
              <Tooltip label={description} position="top" withinPortal>
                <Icon
                  c="neutral.fg.muted"
                  filled
                  icon="info"
                  style={{ cursor: 'default' }}
                />
              </Tooltip>
            )}
          </Group>

          {chart}
        </Stack>
      </Tile>
    )
  }
)
