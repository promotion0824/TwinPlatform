import {
  PillGroup as MantinePillGroup,
  PillGroupProps as MantinePillGroupProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import { css } from 'styled-components'
import {
  SpacingValue,
  WillowStyleProps,
  getSpacing,
  useWillowStyleProps,
} from '../../utils'

export interface PillGroupProps
  extends WillowStyleProps,
    Omit<MantinePillGroupProps, keyof WillowStyleProps> {
  /** Determines whether child Pill components should be disabled */
  disabled?: MantinePillGroupProps['disabled']
  /**
   * Accepts "spacing" properties from the Willow theme (s2, s4, etc.), any valid CSS value,
   * or numbers. Numbers are converted to rem.
   * @default 's4'
   */
  gap?: SpacingValue
  /** List of Pill components. */
  children?: React.ReactNode
}

/**
 * `PillGroup` is the container for a list of `Pill` components.
 */
export const PillGroup = forwardRef<HTMLDivElement, PillGroupProps>(
  ({ gap = 's4', ...restProps }, ref) => {
    return (
      <MantinePillGroup
        {...restProps}
        {...useWillowStyleProps(restProps)}
        ref={ref}
        css={css`
          && {
            gap: ${getSpacing(gap)};
          }
        `}
      />
    )
  }
)
