import Portal from '../Portal/Portal'
import { useTabs } from './TabsContext'

export default function TabsContent({ children }) {
  const tabs = useTabs()

  return <Portal target={tabs.contentRef}>{children}</Portal>
}
