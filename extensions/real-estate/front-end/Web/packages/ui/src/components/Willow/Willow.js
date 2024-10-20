import cx from 'classnames'
import WillowLogo from './willowLogo.svg'
import styles from './Willow.css'

export default function Willow({ className, ...rest }) {
  const cxClassName = cx(styles.willow, className)

  return <WillowLogo {...rest} className={cxClassName} />
}
