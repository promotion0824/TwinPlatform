import { useHistory } from 'react-router'
import cx from 'classnames'
import Button from '../Button/Button'
import Portal from '../Portal/Portal'
import { useTabs } from './TabsContext'
import styles from './TabBackButton.css'

export default function TabBackButton({
  to,
  href,
  className,
  onClick,
  ...rest
}) {
  const history = useHistory()
  const tabs = useTabs()

  const isClickable = to != null || href != null || onClick != null

  const cxClassName = cx(styles.tabBackButton, className)

  function handleClick(e) {
    if (!isClickable) {
      history.goBack()
    }

    onClick?.(e)
  }

  return (
    <Portal target={tabs.tabsRef}>
      <Button
        icon="left"
        height="large"
        {...rest}
        to={to}
        href={href}
        className={cxClassName}
        onClick={handleClick}
        data-segment="Back"
      />
    </Portal>
  )
}
