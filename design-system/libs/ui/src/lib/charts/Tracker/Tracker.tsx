import { HTMLAttributes, forwardRef } from 'react'
import styled from 'styled-components'
import { Group } from '../../layout/Group'
import { Stack } from '../../layout/Stack'
import { Box } from '../../misc/Box'
import { WillowStyleProps } from '../../utils/willowStyleProps'
import { IntentThresholds } from '../utils/chartUtils'
import { TrackerBlock, TrackerBlockProps } from './TrackerBlock'

export interface TrackerProps
  extends WillowStyleProps,
    HTMLAttributes<HTMLDivElement> {
  /**
   * Segments to be displayed on the tracker.
   *
   * Single dimension arrays can be used if labels aren't required,
   * otherwise the labels should be provided alongside the value in the array.
   *
   * If labels are provided, these will be shown in the tooltip along with the value.
   * If not, only the value will be displayed in the tooltip.
   *
   * Use numbers for "intent" themed trackers.
   * Use booleans for "status" themed trackers.
   */
  data:
    | boolean[]
    | number[]
    | Array<{
        label?: string
        value: boolean
      }>
    | Array<{
        label?: string
        value: number
      }>
  /** Description shown above the trakcer. */
  description?: string
  /**
   * Prevents tooltips from being shown when hovering over the tracker.
   * Note that tooltips are only shown on trackers with numeric values.
   * @default false
   */
  disableTooltips?: boolean
  /**
   * Height of the tracker.
   * @default 40
   */
  height?: number
  /**
   * Set the thresholds where the colors should change when using "intent" themed trackers.
   * @default { positiveThreshold: 100, noticeThreshold: 75 }
   */
  intentThresholds?: IntentThresholds
  /** Label shown above the tracker. */
  label?: string
}

const Label = styled(Box<'div'>)(({ theme }) => ({
  ...theme.font.body.sm.regular,
}))

const Track = styled(Group)(({ theme }) => ({
  borderRadius: theme.radius.r4,
  overflow: 'hidden',
}))

const TooltipValue = styled.div(({ theme }) => ({
  ...theme.font.body.sm.semibold,
}))

/**
 * `Tracker` is used to display a series of data points.
 */
export const Tracker = forwardRef<HTMLDivElement, TrackerProps>(
  (
    {
      data,
      description,
      disableTooltips = false,
      height = 40,
      intentThresholds = {
        positiveThreshold: 100,
        noticeThreshold: 75,
      },
      label,
      ...restProps
    },
    ref
  ) => {
    return (
      <Stack gap="s4" ref={ref} {...restProps}>
        {(!!label || !!description) && (
          <Group>
            <Label c="neutral.fg.default">{label}</Label>
            <Label c="neutral.fg.muted" ml="auto">
              {description}
            </Label>
          </Group>
        )}

        <Track gap={1} h={height} wrap="nowrap">
          {data.map((row, index) => {
            const value = typeof row === 'object' ? row.value : row
            const rowLabel = typeof row === 'object' ? row.label : undefined

            const isStatusVariant = typeof value === 'boolean'
            const intent: TrackerBlockProps['intent'] = isStatusVariant
              ? value
                ? 'primary'
                : 'secondary'
              : value >= intentThresholds.positiveThreshold
              ? 'positive'
              : value >= intentThresholds.noticeThreshold
              ? 'notice'
              : 'negative'

            const tooltipLabel =
              disableTooltips || isStatusVariant ? undefined : rowLabel ? (
                <Group>
                  <div>{rowLabel}</div>
                  <TooltipValue>{value}</TooltipValue>
                </Group>
              ) : (
                <TooltipValue>{value}</TooltipValue>
              )

            return (
              <TrackerBlock intent={intent} key={index} label={tooltipLabel} />
            )
          })}
        </Track>
      </Stack>
    )
  }
)
