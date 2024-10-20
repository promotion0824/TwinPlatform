import { HTMLProps, forwardRef } from 'react'
import { Box, BoxProps } from '../../misc/Box'

export interface PanelContentProps
  extends BoxProps,
    Omit<HTMLProps<HTMLDivElement>, 'style'> {}

/**
 * `PanelContent` is the wrapper container for the content of the panel.
 */
const PanelContent = forwardRef<HTMLDivElement, PanelContentProps>(
  (props, ref) => {
    return <Box css={{ overflow: 'auto' }} {...props} ref={ref} />
  }
)

export default PanelContent
