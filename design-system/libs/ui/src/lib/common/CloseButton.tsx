import { forwardRef } from 'react'
import { IconButton, IconButtonProps } from '../buttons/Button'

export const CloseButton = forwardRef<HTMLButtonElement, IconButtonProps>(
  ({ children, ...props }, ref) => (
    // TODO: use background = 'none' when available
    <IconButton
      icon="close"
      kind="secondary"
      background="transparent"
      size="medium"
      ref={ref}
      p="s2"
      {...props}
    />
  )
)
