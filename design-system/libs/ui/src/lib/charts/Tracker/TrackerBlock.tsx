import { forwardRef, ReactNode } from 'react'
import { Box } from '../../misc/Box'
import { Tooltip } from '../../overlays/Tooltip'

export interface TrackerBlockProps {
  /** Defines the color of the block. */
  intent: 'negative' | 'notice' | 'primary' | 'positive' | 'secondary'
  /** The label displayed in a tooltip when the block is hovered. */
  label?: ReactNode
}

export const TrackerBlock = forwardRef<HTMLDivElement, TrackerBlockProps>(
  ({ intent, label, ...restProps }, ref) => (
    <Tooltip disabled={!label} label={label} openDelay={0} withinPortal>
      <Box
        bg={`intent.${intent}.fg.default`}
        h="100%"
        ref={ref}
        w="100%"
        {...restProps}
      />
    </Tooltip>
  )
)
