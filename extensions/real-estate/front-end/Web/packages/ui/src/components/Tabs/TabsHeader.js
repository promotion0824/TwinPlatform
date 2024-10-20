import Portal from '../Portal/Portal'
import { useTabs } from './TabsContext'

export default function TabsHeader({ children }) {
  const tabs = useTabs()

  return <Portal target={tabs.tabsHeaderRef}>{children}</Portal>
}
