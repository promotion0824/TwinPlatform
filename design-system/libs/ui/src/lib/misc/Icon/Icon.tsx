import { HTMLAttributes, forwardRef } from 'react'
import styled from 'styled-components'
import { MaterialSymbol } from 'material-symbols'

import { WillowStyleProps } from '../../utils/willowStyleProps'
import { Box } from '../Box'

export type IconName = MaterialSymbol
export interface IconProps
  extends WillowStyleProps,
    Omit<HTMLAttributes<HTMLElement>, 'children'> {
  // do not support customized children at the moment
  icon: IconName // could support a svg in the future
  /** @default true */
  filled?: boolean
  /** @default 20 */
  size?: 16 | 20 | 24
}

const StyledBox = styled(Box<'span'>)<{
  $size: Exclude<IconProps['size'], undefined>
  $filled: IconProps['filled']
  $iconOption: 'sharp'
}>(({ theme, $size, $filled, $iconOption }) => ({
  fontFamily: `Material Symbols ${$iconOption}`,
  color: 'inherit',
  fontSize: theme.spacing[`s${$size}`],
  'font-variation-settings': `'FILL' ${
    $filled ? 1 : 0
  }, 'wght' 300, 'GRAD' 0, 'opsz' ${$size}`,
}))

export const Icon = forwardRef<HTMLDivElement, IconProps>(
  ({ icon, filled = true, size = 20, className = '', ...restProps }, ref) => {
    /**
     * options: 'rounded' | 'sharp' | 'outlined'
     *
     * Note: To update the css import in libs/theme/src/lib/globalStyles.ts
     * if you changed the iconOption
     */
    const iconOption = 'sharp'

    return (
      <StyledBox
        ref={ref}
        component="span"
        className={`material-symbols-${iconOption} ${className}`}
        $size={size}
        $filled={filled}
        $iconOption={iconOption}
        {...restProps}
      >
        {icon}
      </StyledBox>
    )
  }
)
