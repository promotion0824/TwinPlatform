import { forwardRef } from 'react'
import { IconButton, IconButtonProps } from '../../../buttons/Button'

const BaseIconButton = forwardRef<HTMLButtonElement, IconButtonProps>(
  (props, ref) => (
    <IconButton
      kind="secondary"
      background="transparent"
      ref={ref}
      {...props}
    />
  )
)

export default BaseIconButton
