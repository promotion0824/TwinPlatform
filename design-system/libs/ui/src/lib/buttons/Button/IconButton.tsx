import { forwardRef } from 'react'
import { css } from 'styled-components'
import { Icon, IconName } from '../../misc/Icon'
import { Button, ButtonProps } from './Button'

export interface IconButtonProps
  extends Omit<ButtonProps, 'prefix' | 'suffix'> {
  /**
   * The `icon` prop accepts any material symbol name and provides a convenient
   * syntax sugar in place of using `children` to render the icon.
   */
  icon?: IconName
  /**
   * The `children` prop can accept any React node and will override
   * the `icon` prop if both are provided.
   */
  children?: ButtonProps['children']
}

// 'aria-label'?: string // TODO how could we make 'aria-label' required for Customized Icon Only and optional for other cases?
export const IconButton = forwardRef<
  HTMLButtonElement | HTMLAnchorElement,
  IconButtonProps
>(({ icon, size = 'medium', children, ...props }, ref) => (
  <Button
    css={css(({ theme }) => ({
      padding:
        props.background === 'none'
          ? 0
          : size === 'large'
          ? theme.spacing.s6
          : theme.spacing.s4,
    }))}
    ref={ref}
    children={children || (icon && <Icon icon={icon} />)}
    {...props}
  />
))
