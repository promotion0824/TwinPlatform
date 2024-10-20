import {
  Progress as MantineProgress,
  ProgressProps as MantineProgressProps,
} from '@mantine/core'
import { useTheme } from '@willowinc/theme'
import { forwardRef } from 'react'
import styled from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { Intent } from '../../common'

type Size = 'xs' | 'sm' | 'md' | 'lg'

// Fix for Storybook ArgTypes not working with Mantine's props.
// See https://willow.atlassian.net/l/cp/40rrHNJp
export interface BaseProps {
  /**
   * Dictates the color of the progress bar.
   * @default 'primary'
   */
  intent?: Intent
  /**
   * Dictates the height of the progress bar.
   * @default 'md'
   */
  size?: Size
  /** Percentage of the progress bar to be filled in. */
  value: number
}

export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

export interface ProgressProps
  extends WillowStyleProps,
    BaseProps,
    Omit<MantineProgressProps, keyof WillowStyleProps | 'color' | 'size'> {}

const sizeMap: Record<Size, number> = {
  xs: 2,
  sm: 4,
  md: 8,
  lg: 24,
}

const StyledProgress = styled(MantineProgress)(({ theme }) => ({
  backgroundColor: theme.color.state.disabled.bg,

  '&, div': {
    borderRadius: '1px',
  },
}))

/**
 * `Progress` gives feedback on progress using a bar.
 */
export const Progress = forwardRef<HTMLDivElement, ProgressProps>(
  ({ intent = 'primary', size = 'md', value, ...restProps }, ref) => {
    const theme = useTheme()

    return (
      <StyledProgress
        color={theme.color.intent[intent].bg.bold.default}
        size={sizeMap[size]}
        ref={ref}
        value={value}
        {...restProps}
        {...useWillowStyleProps(restProps)}
      >
        Progress
      </StyledProgress>
    )
  }
)
