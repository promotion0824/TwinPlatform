import { Portal } from '@willow/ui'

import { useLayout } from './LayoutContext'

export default function LayoutHeader({
  children,
  className,
}: {
  children: React.ReactNode
  className?: string
}) {
  const layout = useLayout()

  return (
    <Portal target={layout.headerRef}>
      <div className={className}>{children}</div>
    </Portal>
  )
}
