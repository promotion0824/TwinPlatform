import Portal from 'components/Portal/Portal'
import { useTabs } from './TabsContext'

export default function TabsHeader({ children }) {
  const tabs = useTabs()

  return <Portal target={tabs.headerRef.current}>{children}</Portal>
}
