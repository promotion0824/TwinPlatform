import {
  Pill as MantinePill,
  PillProps as MantinePillProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import styled, { css } from 'styled-components'

import { Sizes } from '../../common'
import { WillowStyleProps, useWillowStyleProps } from '../../utils'

export interface PillProps
  extends WillowStyleProps,
    Omit<MantinePillProps, keyof WillowStyleProps | 'size'> {
  disabled?: MantinePillProps['disabled']
  onRemove?: MantinePillProps['onRemove']
  withRemoveButton?: MantinePillProps['withRemoveButton']
  removeButtonProps?: MantinePillProps['removeButtonProps']
  size?: Extract<Sizes, 'sm' | 'md'>
}

/**
 * `Pill` is a tag component.
 */
export const Pill = forwardRef<HTMLDivElement, PillProps>(
  ({ size = 'sm', ...restProps }, ref) => {
    return (
      <StyledPill
        {...restProps}
        {...useWillowStyleProps(restProps)}
        ref={ref}
        $size={size}
      />
    )
  }
)

const StyledPill = styled(MantinePill)<{ $size: PillProps['size'] }>(
  ({ theme, $size }) => {
    if ($size === 'md') {
      // override global default pillStyles
      return css`
        &.mantine-Pill-root {
          height: ${theme.spacing.s24};
          padding: ${theme.spacing.s4} ${theme.spacing.s8};
        }
      `
    }

    return css``
  }
)
