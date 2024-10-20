import cx from 'classnames'
import Link from 'components/Link/Link'
import WillowLogo from './willowLogo.svg'
import styles from './Willow.css'

export default function Willow(props) {
  const { color, className, ...rest } = props

  const cxClassName = cx(
    styles.willow,
    {
      [styles.purple]: color === 'purple',
    },
    className
  )

  return (
    <Link to="/" {...rest} className={cxClassName}>
      <WillowLogo className={styles.logo} />
    </Link>
  )
}
