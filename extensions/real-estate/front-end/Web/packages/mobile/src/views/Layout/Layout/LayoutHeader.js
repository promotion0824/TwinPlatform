import { Children } from 'react'
import { Portal, Spacing } from '@willow/mobile-ui'
import { useLayout } from './LayoutContext'

export default function LayoutHeader({ type, children, ...rest }) {
  const { headerRef, setTitle } = useLayout()

  let nextType = type

  if (type == null) {
    nextType = Children.toArray(children).length === 1 ? 'header' : 'content'
  }

  setTitle(null)

  return (
    <Portal target={headerRef.current}>
      <Spacing
        horizontal
        overflow="hidden"
        height="100%"
        align="middle"
        size="medium"
        {...rest}
        type={nextType}
      >
        {children}
      </Spacing>
    </Portal>
  )
}
