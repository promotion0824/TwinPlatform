import cx from 'classnames'
import { useTabs } from './TabsContext'
import Panel from './Panel/Panel'
import styles from './Tabs.css'

export default function TabsContent({ color, className, ...rest }) {
  const tabs = useTabs()

  const nextColor = color ?? tabs?.color ?? 'dark'
  const cxContentClassName = cx(styles.content, className)

  return (
    <Panel
      {...rest}
      ref={tabs?.contentRef}
      color={nextColor}
      className={cxContentClassName}
    />
  )
}
