import { Box, BoxProps } from '@mantine/core'
import { css } from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface ButtonGroupProps
  extends WillowStyleProps,
    Omit<BoxProps, keyof WillowStyleProps> {
  children?: React.ReactNode
}

/**
 * The `ButtonGroup` component can be used to group related buttons.
 */
export function ButtonGroup({ children, ...restProps }: ButtonGroupProps) {
  return (
    <Box
      css={css(({ theme }) => ({
        display: 'flex',
        flexDirection: 'row',
        gap: theme.spacing.s8,
        width: 'fit-content',
      }))}
      data-testid="buttonGroup"
      {...restProps}
      {...useWillowStyleProps(restProps)}
    >
      {children}
    </Box>
  )
}
